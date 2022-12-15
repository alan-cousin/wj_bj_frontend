using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Numerics;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Networking;
using TMPro;
public class Web3Connector : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Web3Connect();

    [DllImport("__Internal")]
    private static extern string ConnectAccount();

    [DllImport("__Internal")]
    private static extern void SetConnectAccount(string value);

    public static Web3Connector Instance{ get; private set; }
    public string AccountAddress { get; private set; }
    public int Balance { get; private set; }

    public string GameContract { get; set; }
    public TextMeshProUGUI m_uiAccountAddress;
    #region delegates
    public delegate void OnWalletConnected();
    public event OnWalletConnected wallectConnected;

    public delegate void OnCreateGameOnConract();
    public event OnCreateGameOnConract gameCreatedOnContract;

    public delegate void OnUnexpectedErrorOccured();
    public event OnUnexpectedErrorOccured unexpectedErrorOccured;

    public delegate void OnDeclareWinnered();
    public event OnDeclareWinnered declaredWinnerOnContract;


    public delegate void OnDoubleBetted();
    public event OnDoubleBetted doubleBetted;
    #endregion
    Web3Connector()
    {
        Instance = this;
    }
    private void Awake()
    {
        AccountAddress = string.Empty;
    }
    public void OnLogin()
    {
        
        if (string.IsNullOrEmpty(AccountAddress))
        {

#if UNITY_WEBGL && !UNITY_EDITOR
         Debug.Log("try connect metamask");
        Web3Connect();
       
#endif
            OnConnected();
        }
    }

    async private void OnConnected()
    {
        m_uiAccountAddress.text = "connecting...";
#if UNITY_WEBGL && !UNITY_EDITOR
        
        AccountAddress = ConnectAccount();
        while (AccountAddress == "")
        {
            await new WaitForSeconds(1f);
            AccountAddress = ConnectAccount();
        };
        
        // reset login message
        SetConnectAccount("");
#else
        AccountAddress = "0x2E1Ba247AD65f5B929D106FFD86f9121aB2d54eA";
#endif
#if UNITY_WEBGL //&& !UNITY_EDITOR
        // save account for next scene
        if (AccountAddress.Contains("metamask"))
        {
            UIController.Instance.ShowErrorMessage("Please install Metamask.");
            m_uiAccountAddress.text = "CONNECT";
            AccountAddress = string.Empty;
        }
        else
        {
            PlayerPrefs.SetString("Account", AccountAddress);
        }
        
        
#else
        AccountAddress = "0x2E1Ba247AD65f5B929D106FFD86f9121aB2d54eA";
#endif


#if UNITY_WEBGL //&& !UNITY_EDITOR
        bool success_get_balance = false;
         while (!success_get_balance && !string.IsNullOrEmpty(AccountAddress))
         {
             try
             {
                 Debug.Log("WalletAddress =" + AccountAddress);
                 BigInteger balanceOf = await ERC20.BalanceOf(Config.CHAIN, Config.NETWORK_NAME, Config.TOKEN_CONTRACT, AccountAddress, Config.RPC_URL);
                 Balance = (int)(balanceOf / Config.ETHERS);
                 success_get_balance = true;
                 string theString = AccountAddress.Remove(4, 34);
                 m_uiAccountAddress.text = theString.Insert(4, "....");
             }
             catch (Exception e)
             {
                 Debug.Log("Reading balance failed");
             }
             await new WaitForSeconds(.1f);
         }
#else
        Balance = 1000;
#endif
        Debug.Log("User Wallet Balance = " + Balance);
        
        wallectConnected?.Invoke();
        //GameObject.Find("SingleGameManager").GetComponent<SingleGameManager>().PlayerJoinRoom();
    }

#region Game Contract
    public async void DoubleBet(int _amount)
    {
        string res_result = "";
        try
        {
            res_result = await reqDoubleBet(AccountAddress, GameContract, _amount);

            Debug.Log("Double bet response : " + res_result);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            res_result = "Unexpected Error";
        }
        if (res_result.Contains("Unexpected"))
        {
            UIController.Instance.ShowErrorMessage("Double Betting failed due to unexpected error. ");
            //unexpectedErrorOccured?.Invoke();
            doubleBetted?.Invoke();
        }
        else
        {
            Debug.Log("Double Bet result : " + res_result);
            doubleBetted?.Invoke();
        }
    }
    public async void CreateGame(int _amount)
    {
        GameContract = string.Empty;
        string value = "0";
        // gas limit OPTIONAL
        string gasLimit = "";
        // gas price OPTIONAL
        string gasPrice = "";
        bool has_enough_allowance = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        bool success_get_allowance = false;
        BigInteger allowance = 0;


        while (!success_get_allowance)
        {
            try
            {
                Debug.Log("read allowance from " + AccountAddress);
                string method = "allowance";
                string[] read_allowance_params = { AccountAddress,  Config.BLACKJACK_FACTORY_CONTRACT};
                string method_args = JsonConvert.SerializeObject(read_allowance_params);

                string allowance_response = await EVM.Call(Config.CHAIN, Config.NETWORK_NAME, Config.TOKEN_CONTRACT, Config.TOKEN_ABI , method, method_args,  Config.RPC_URL);
                success_get_allowance = true;
                allowance = BigInteger.Parse(allowance_response);
                
                Debug.Log("allowance is  " + allowance.ToString());

            }
            catch (Exception e)
            {
                Debug.Log("Reading allowance failed");
            }
            await new WaitForSeconds(.1f);
        }
        if(allowance < ((BigInteger)Balance) * Config.ETHERS)
        {
            while(!has_enough_allowance)
            {
                string[] ob1j = { Config.BLACKJACK_FACTORY_CONTRACT, "100000" + Config.DECIMALS };
                string args1 = JsonConvert.SerializeObject(ob1j);
                // value in wei

                // connects to user's browser wallet (metamask) to send a transaction
                try
                {
                    string response1 = await Web3GL.SendContract(Config.CM_APPROVE, Config.TOKEN_ABI, Config.TOKEN_CONTRACT, args1, value, gasLimit, gasPrice);
                    Debug.Log(response1);
                    has_enough_allowance = true;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                await new WaitForSeconds(.1f);
            }
            
        }
        else
        {
            has_enough_allowance = true;
        }
        // array of arguments for contract
#else
        has_enough_allowance = true;
#endif

#if UNITY_WEBGL && NO_BACKEND//&& !UNITY_EDITOR
        Debug.Log("Start Create new game on contract");
        // private key of account
        // account of player 
        try
        {
            Debug.Log("Account Address " + AccountAddress);
            Debug.Log("Amount " + _amount.ToString() + Config.DECIMALS);

            string[] obj = { AccountAddress, _amount.ToString()+Config.DECIMALS };
            string args = JsonConvert.SerializeObject(obj);

            value = "0";
            gasLimit = "7500000";
            gasPrice = "";
            bool success_create_contract = false;
            string data = string.Empty;
            while (!success_create_contract)
            {
                try
                {
                    Debug.Log("create ContractData ");
                    data = await EVM.CreateContractData(Config.BLACKJACK_ABI, Config.CM_CREATE_GAME, args);
                    success_create_contract = true;
                    Debug.Log("ContractData is  " + data);
                }
                catch (Exception e)
                {
                    Debug.Log("ContractData failed");
                }
                await new WaitForSeconds(.1f);
            }

            gasPrice = await EVM.GasPrice(Config.CHAIN, Config.NETWORK_NAME, Config.RPC_URL);
            Debug.Log("gasPrice : " + gasPrice);
            gasLimit = "2000000";
            string transaction = await EVM.CreateTransaction(Config.CHAIN, Config.NETWORK_NAME, Config.OWNER_ADDRESS, Config.BLACKJACK_FACTORY_CONTRACT, value, data, gasPrice, gasLimit, Config.RPC_URL);
            Debug.Log("created transacion : " + transaction);
            string signature = Web3PrivateKey.SignTransaction(Config.PRIVATE_KEY, transaction, Config.CHAINID);
            Debug.Log("signature : " + signature);
            bool succes_broad_cast = false;
            while (!succes_broad_cast)
            {
                string response = await EVM.BroadcastTransaction(Config.CHAIN, Config.NETWORK_NAME, Config.OWNER_ADDRESS, Config.BLACKJACK_FACTORY_CONTRACT, value, data, signature, gasPrice, gasLimit, Config.RPC_URL);
                Debug.Log("Broad Cast result : " + response);
                if (response.StartsWith("0x"))
                {
                    succes_broad_cast = true;
                    GameContract = response;
                    
                }
                await new WaitForSeconds(.1f);

            }
            bool trans_success = false;
            while (!trans_success)
            {
                string response = await EVM.TxStatus(Config.CHAIN, Config.NETWORK_NAME, GameContract, Config.RPC_URL);
                Debug.Log("Transaction Result : " + response);
                if( response == "success")
                    trans_success = true;
                await new WaitForSeconds(.3f);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }

        if (!string.IsNullOrEmpty(GameContract ))
#endif
        if (has_enough_allowance)
        {
            GameContract = await reqNewDeal(AccountAddress, _amount.ToString());
            if(GameContract.Contains("Unexpected"))
            {
                GameContract = string.Empty;
                unexpectedErrorOccured?.Invoke();
                declaredWinnerOnContract?.Invoke();
                UIController.Instance.ShowErrorMessage("New Deal is failed because of unexpected error. Please check allowance for game contract. ");
                Debug.Log("New Deal is failed because of unexpected error. Please check allowance for game contract.");
            }
            else
            {
                Debug.Log("New Create Game :" + GameContract);

                gameCreatedOnContract?.Invoke();
            }
            
        }
        else
        {
            UIController.Instance.ShowErrorMessage("Token allowance is lower than betting amount, please approve ");
            GameContract = string.Empty;
            unexpectedErrorOccured?.Invoke();
            declaredWinnerOnContract?.Invoke();
        }
    }

    public async void DeclareWinner(bool _isUserWin, bool _isDraw)
    {
        string value = "0";
        // gas limit OPTIONAL
        string gasLimit = "";
        // gas price OPTIONAL
        string gasPrice = "";


#if UNITY_WEBGL && NO_BACKEND//&& !UNITY_EDITOR
        Debug.Log("Start DeclareWinner");
        // private key of account
        // account of player 
        try
        {
            Debug.Log("Account Address " + AccountAddress);

            string[] obj = { GameContract, _isUserWin.ToString(), _isDraw.ToString() };
            string args = JsonConvert.SerializeObject(obj);

            value = "0";
            gasPrice = "";
            bool success_create_contract = false;
            string data = string.Empty;
            while (!success_create_contract)
            {
                try
                {
                    Debug.Log("create ContractData ");
                    data = await EVM.CreateContractData(Config.BLACKJACK_ABI, Config.CM_WINNER, args);
                    success_create_contract = true;
                    Debug.Log("ContractData is  " + data);
                }
                catch (Exception e)
                {
                    Debug.Log("ContractData failed");
                }
                await new WaitForSeconds(.1f);
            }

            gasPrice = await EVM.GasPrice(Config.CHAIN, Config.NETWORK_NAME, Config.RPC_URL);
            Debug.Log("gasPrice : " + gasPrice);
            gasLimit = "2000000";
            string transaction = await EVM.CreateTransaction(Config.CHAIN, Config.NETWORK_NAME, Config.OWNER_ADDRESS, Config.BLACKJACK_FACTORY_CONTRACT, value, data, gasPrice, gasLimit, Config.RPC_URL);
            Debug.Log("created transacion : " + transaction);
            string signature = Web3PrivateKey.SignTransaction(Config.PRIVATE_KEY, transaction, Config.CHAINID);
            Debug.Log("signature : " + signature);
            bool succes_broad_cast = false;
            string trans_hax = string.Empty;
            while (!succes_broad_cast)
            {
                trans_hax = await EVM.BroadcastTransaction(Config.CHAIN, Config.NETWORK_NAME, Config.OWNER_ADDRESS, Config.BLACKJACK_FACTORY_CONTRACT, value, data, signature, gasPrice, gasLimit, Config.RPC_URL);
                Debug.Log("Broad Cast result : " + trans_hax);
                if (trans_hax.StartsWith("0x"))
                {
                    succes_broad_cast = true;
                    GameContract = string.Empty;
                    
                }
                await new WaitForSeconds(.1f);

            }
            bool trans_success = false;
            while (!trans_success)
            {
                string response = await EVM.TxStatus(Config.CHAIN, Config.NETWORK_NAME, trans_hax, Config.RPC_URL);
                Debug.Log("Transaction Result : " + response);
                if (response == "success")
                    trans_success = true;
                await new WaitForSeconds(.3f);
            }
            
            bool success_get_balance = false;
            while (!success_get_balance)
            {
                try
                {
                    Debug.Log("WalletAddress =" + AccountAddress);
                    BigInteger balanceOf = await ERC20.BalanceOf(Config.CHAIN, Config.NETWORK_NAME, Config.TOKEN_CONTRACT, AccountAddress, Config.RPC_URL);
                    Balance = (int)(balanceOf / 1000000000000000000);
                    success_get_balance = true;

                }
                catch (Exception e)
                {
                    Debug.Log("Reading balance failed");
                }
                await new WaitForSeconds(.1f);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
#endif

#if UNITY_WEBGL //&& !UNITY_EDITOR
        if (!string.IsNullOrEmpty(GameContract))
        {
            string balance_data = "";
            try
            {
                balance_data = await reqDeclareWinner(AccountAddress, GameContract, _isUserWin, _isDraw);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                balance_data = "Unexpected Error";
            }
            
            Debug.Log("Current Balance :" + balance_data);
            if(balance_data.Contains("Unexpected"))
            {
                UIController.Instance.ShowErrorMessage("Declare winner failed, please send administrator below info.\n Wallet Address :  "+AccountAddress + " \n Game Address:"+GameContract);
                GameContract = string.Empty;
                unexpectedErrorOccured?.Invoke();
                declaredWinnerOnContract?.Invoke();
            }
            else
            {
                balance_data = balance_data.Replace("\"", "");
                Balance = (int)(BigInteger.Parse(balance_data) / 1000000000000000000);
                SinglePlay.Instance.CurrentPlayer.Balance = Balance;
                declaredWinnerOnContract?.Invoke();
                GameContract = string.Empty;

            }


        }
#else
        await new WaitForSeconds(3f);
        Balance = Balance - SinglePlay.Instance.CurrentPlayer.Betting;
#endif
        
    }
    public static async Task<string> reqDoubleBet(string _account, string _game, int _amount)
    {
        WWWForm form = new WWWForm();
        form.AddField("address", _game);
        form.AddField("amount", _amount.ToString());
        form.AddField("user", _account);
        string url = Config.SERVER_URL + "/doublebet";
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
        {
            await webRequest.SendWebRequest();
            string data = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
            return data;
        }
    }

    public static async Task<string> reqDeclareWinner(string _account, string _game,bool _isPlayerWin, bool _isDraw)
    {
        WWWForm form = new WWWForm();
        form.AddField("game_address", _game);
        form.AddField("playerwin", _isPlayerWin.ToString());
        form.AddField("isdraw", _isDraw.ToString());
        form.AddField("user", _account);
        string url = Config.SERVER_URL + "/declare_winner";
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
        {
            await webRequest.SendWebRequest();
            string data = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
            return data;
        }
    }

    public static async Task<string> reqNewDeal(string _address, string _amount)
    {
        WWWForm form = new WWWForm();
        form.AddField("address", _address);
        form.AddField("amount", _amount);
        string url = Config.SERVER_URL + "/create_game";
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
        {
            await webRequest.SendWebRequest();
            string data = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
            data = data.Replace("\"", "");
            return data;
        }
    }

    #endregion
}
