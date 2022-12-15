using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UIController : MonoBehaviour
{
    public GameObject m_uiStartBtn;

    public GameObject m_uiHitbtn;
    public GameObject m_uiStandBtn;
    public GameObject m_uiDoubleBtn;
    public GameObject m_uiDealBtn;

    public GameObject m_uiChip1Btn;
    public GameObject m_uiChip5Btn;
    public GameObject m_uiChip10Btn;
    public GameObject m_uiChip25Btn;
    public GameObject m_uiChip50tn;
    public GameObject m_uiChip100Btn;
    public GameObject m_uiShowMessage;
    public GameObject m_uiSound;
    public TextMeshProUGUI m_uiWalletBalance;
    public TextMeshProUGUI m_uiErrorText;

    public ChipHolder m_uiChipHolder;

    public GameObject m_uiWaitingWindow;

    public GameObject m_uiWinnerWnd;
    public TextMeshProUGUI m_uiWinnerText;

    public TextMeshProUGUI m_uiPlayerResult;
    public TextMeshProUGUI m_uiDealerResult;

    public TextMeshProUGUI m_uiPlayerScore;
    public TextMeshProUGUI m_uiDealerScore;
    public TextMeshProUGUI m_uiTransferText;

    public TextMeshProUGUI m_uiWaitingText;
    public static UIController Instance { get; private set; }
    void Update()
    {
        int user_balance = SinglePlay.Instance.CurrentPlayer != null ? SinglePlay.Instance.CurrentPlayer.Balance : Web3Connector.Instance.Balance;
        m_uiWalletBalance.text = user_balance.ToString();
    }
    #region Initialize
    UIController()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        _initialize();
        _registerEvents();
    }


    void _initialize()
    {
        m_uiStartBtn.SetActive(true);
        _enableBtn(m_uiStartBtn, false);

        _enableBtn(m_uiHitbtn, false);
        _enableBtn(m_uiStandBtn, false);
        _enableBtn(m_uiDoubleBtn, false);
        _enableBtn(m_uiDealBtn, false);

        _enableBtn(m_uiChip1Btn, false);
        _enableBtn(m_uiChip5Btn, false);
        _enableBtn(m_uiChip10Btn, false);
        _enableBtn(m_uiChip25Btn, false);
        _enableBtn(m_uiChip50tn, false);
        _enableBtn(m_uiChip100Btn, false);

        m_uiWalletBalance.text = "";
        m_uiPlayerScore.text = "";
        m_uiDealerScore.text = "";

        m_uiChipHolder.gameObject.SetActive(false);

        ShowWaitingGame(false);
        ShowWinnerWindow(false);
    }

    void _enableBtn(GameObject _btn, bool _enable)
    {
        _btn.GetComponent<Button>().interactable = _enable;
        _btn.GetComponentInChildren<TextMeshProUGUI>().color = _enable? Color.white:Color.gray;
    }
    
    void _waitingForStart()
    {

    }
    void _prepareNewDeal()
    {
        m_uiStartBtn.SetActive(false);
        _enableBtn(m_uiChip1Btn, true);
        _enableBtn(m_uiChip5Btn, true);
        _enableBtn(m_uiChip10Btn, true);
        _enableBtn(m_uiChip25Btn, true);
        _enableBtn(m_uiChip50tn, true);
        _enableBtn(m_uiChip100Btn, true);

        _enableBtn(m_uiDealBtn, true);
        m_uiChipHolder.gameObject.SetActive(true);
        m_uiChipHolder.ResetChipStacks();
        m_uiPlayerScore.text = "";
        m_uiDealerScore.text = "";
        ShowWaitingGame(false);
        ShowWinnerWindow(false);
    }
    void _newDeal()
    {
        _enableBtn(m_uiChip1Btn, false);
        _enableBtn(m_uiChip5Btn, false);
        _enableBtn(m_uiChip10Btn, false);
        _enableBtn(m_uiChip25Btn, false);
        _enableBtn(m_uiChip50tn, false);
        _enableBtn(m_uiChip100Btn, false);

        _enableBtn(m_uiDealBtn, false);
        //ShowWaitingGame(false);
        //ShowWinnerWindow(false);
        m_uiPlayerResult.text = "";
        m_uiDealerResult.text = "";
    }
    async void _showWinnerWind()
    {
        await new WaitForSeconds(1.0f);
        ShowWinnerWindow(true);
    }
    #endregion //initialize

    #region Event Handlers
    void _registerEvents()
    {
        Web3Connector.Instance.wallectConnected += OnWalletConnected;
        CardManager.Instance.cardDistributeFinished += OnCardDistributeFinished;
        Web3Connector.Instance.gameCreatedOnContract += OnDealCreated;
        SinglePlay.Instance.winnerDeclared += OnWinnerDeclared;
        Web3Connector.Instance.declaredWinnerOnContract += OnDeclaredWinnerOnContract;
        Web3Connector.Instance.unexpectedErrorOccured += OnUnexpectedError;
        Web3Connector.Instance.doubleBetted+= OnDoubleBetted;

    }
    void OnDoubleBetted()
    {
        ShowWaitingGame(false);
    }
    
    void OnUnexpectedError()
    {
        Debug.Log("Unexpected error occured");
        //m_uiShowMessage.SetActive(true);
    }
    void OnDeclaredWinnerOnContract()
    {
        Debug.Log("OnDeclaredWinnerOnContract");
        _prepareNewDeal();
    }
    void OnWinnerDeclared()
    {
        m_uiPlayerResult.gameObject.SetActive(true);
        if (SinglePlay.Instance.CurrentPlayer.CurrentScore == WJBJGameLogic.BLACKJACK)
            m_uiPlayerResult.text = "BLACKJACK";
        else if (SinglePlay.Instance.CurrentPlayer.CurrentScore > WJBJGameLogic.BLACKJACK)
            m_uiPlayerResult.text = "BUST";
        else
            m_uiPlayerResult.text = "";

        m_uiDealerResult.gameObject.SetActive(true);
        if (SinglePlay.Instance.CurrentDealer.CurrentScore == WJBJGameLogic.BLACKJACK)
            m_uiDealerResult.text = "BLACKJACK";
        else if (SinglePlay.Instance.CurrentDealer.CurrentScore > WJBJGameLogic.BLACKJACK)
            m_uiDealerResult.text = "BUST";
        else
            m_uiDealerResult.text = "";
        _showWinnerWind();

    }
    void OnDealCreated()
    {
        ShowWaitingGame(false);
    }
    void OnWalletConnected()
    {
        if (!string.IsNullOrEmpty(Web3Connector.Instance.AccountAddress))
        {
            _enableBtn(m_uiStartBtn, true);
            m_uiWalletBalance.text = Web3Connector.Instance.Balance.ToString();
        }
        else
        {

        }
    }
    void OnCardDistributeFinished()
    {
        if(SinglePlay.Instance.CurrentGame.Status == WJBJGameLogic.GameStatus.IN_GAME && SinglePlay.Instance.CurrentPlayer.Status < WJBJBasePlayer.PlayerStatus.STANDING)
        {

            _enableBtn(m_uiHitbtn, true);
            _enableBtn(m_uiStandBtn, true);
            Debug.Log("Current Player Status : " + SinglePlay.Instance.CurrentPlayer.Status.ToString());
            _enableBtn(m_uiDoubleBtn, SinglePlay.Instance.CurrentPlayer.Status == WJBJBasePlayer.PlayerStatus.INITIAL); 
        }

        m_uiPlayerScore.text = SinglePlay.Instance.CurrentPlayer.CurrentScore.ToString();
        m_uiDealerScore.text = SinglePlay.Instance.CurrentDealer.CurrentScore.ToString();

    }


    public void OnNewGameStart()
    {
        _prepareNewDeal();
        newGameStarted?.Invoke();
    }

    public void OnBetChips(int _amount)
    {
        // plus total bet amount
        if (SinglePlay.Instance.Bet(_amount))
        {
            m_uiChipHolder.UpdateAmount(_amount);
        }
        else
        {
            m_uiChipHolder.ShowMessage("Insufficient amount");
        }
    }

    public void OnDeal()
    {
        if (SinglePlay.Instance.CurrentPlayer.Betting < 1)
        {
            m_uiChipHolder.ShowMessage("Please bet chips.");
        }
        else
        {
            ShowWaitingGame(true);
            SinglePlay.Instance.Deal();
            _newDeal();
        }
    }
    public void OnHit() {
        SinglePlay.Instance.Hit();
        _enableBtn(m_uiHitbtn, false);
        _enableBtn(m_uiStandBtn, false);
        _enableBtn(m_uiDoubleBtn, false);
    }
    public void OnStand() {
        SinglePlay.Instance.Stand();
        _enableBtn(m_uiHitbtn, false);
        _enableBtn(m_uiStandBtn, false);
        _enableBtn(m_uiDoubleBtn, false);
    }
    public void OnDouble() {
        ShowWaitingGame(true);
        SinglePlay.Instance.Double();
        _enableBtn(m_uiHitbtn, false);
        _enableBtn(m_uiStandBtn, false);
        _enableBtn(m_uiDoubleBtn, false);
    }

    public void OnSoundOnOff()
    {
        Toggle sound_obj = m_uiSound.GetComponent<Toggle>();
        SinglePlay.Instance.MusicOnOff(!sound_obj.isOn);
    }
    #endregion //Event Handlers
    #region Delegates
    public delegate void OnNewGameStarted();
    public event OnNewGameStarted newGameStarted;

    
    #endregion
    #region UI Handler
    public void ShowWaitingGame(bool _enable)
    {
        m_uiWaitingWindow.SetActive(_enable);
        if(string.IsNullOrEmpty( Web3Connector.Instance.GameContract))
            m_uiWaitingText.text = "Loading Game from Blockchain...";
        else
            m_uiWaitingText.text = "doubling betting amout...";
    }
    public void ShowErrorMessage(string _message)
    {
        m_uiErrorText.text = _message;
        m_uiShowMessage.SetActive(true);
    }
    public void ShowWinnerWindow(bool _enable)
    {
        Debug.Log("ShowWinnerWindow : " + _enable.ToString());
        m_uiWinnerWnd.SetActive(_enable);
        switch(SinglePlay.Instance.CurrentGame.Status)
        {
            case WJBJGameLogic.GameStatus.DEALER_WIN:
                m_uiWinnerText.text = "You Lost";
                m_uiTransferText.text = "Transferring Rewards to Winner";
                break;
            case WJBJGameLogic.GameStatus.PLAYER_WIN:
                m_uiWinnerText.text = "You Won";
                m_uiTransferText.text = "Transferring Your Rewards";
                break;
            case WJBJGameLogic.GameStatus.DRAW:
                m_uiWinnerText.text = "GAME IS DRAWN.";
                m_uiTransferText.text = "Transferring Your Betting";
                break;
        }
        
    }
    #endregion //UI Handler
}
