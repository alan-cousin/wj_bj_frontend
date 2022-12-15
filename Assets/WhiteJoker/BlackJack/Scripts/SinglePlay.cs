using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlay : MonoBehaviour
{
    public static SinglePlay Instance { get; private set; }

    public Table CurrenTable { get; private set; }
    public WJBJPlayer CurrentPlayer { get; private set; }
    public WJBJDealer CurrentDealer { get; private set; }

    
    public WJBJGameLogic CurrentGame;

    public delegate void OnWinnerDeclared();
    public event OnWinnerDeclared winnerDeclared;
    public AudioSource m_objMusic;
    SinglePlay()
    {
        Instance = this;
    }
    void Start()
    {
        _registerHandles();
    }
    // Update is called once per frame
    void Update()
    {

    }
    #region Event Handlers
    void _registerHandles()
    {
        UIController.Instance.newGameStarted += OnNewGameStarted;
        Web3Connector.Instance.wallectConnected += OnWalletConnected;
        Web3Connector.Instance.gameCreatedOnContract += OnDealCreated;
        CardManager.Instance.cardDistributeFinished += OnCardDistributeFinished;
        Web3Connector.Instance.declaredWinnerOnContract += OnDeclaredWinnerOnContract;
        Web3Connector.Instance.doubleBetted += OnDoubleBetted;
    }

    void OnDoubleBetted()
    {
        CurrentPlayer.TakeCard(CurrenTable.PopUp());
        CurrentPlayer.Status = WJBJBasePlayer.PlayerStatus.STANDING;
        CardManager.Instance.Distribute();
    }
    void OnDeclaredWinnerOnContract()
    {
        CurrentGame.Status = WJBJGameLogic.GameStatus.IN_GAME;
        CurrentPlayer?.EmptyHand();
        CurrentDealer?.EmptyHand();
        CurrenTable?.CheckDeck();
        CurrentPlayer.Balance = Web3Connector.Instance.Balance;
    }
    void OnWalletConnected()
    {
        if(!string.IsNullOrEmpty(Web3Connector.Instance.AccountAddress))
        {
            CurrentPlayer = new WJBJPlayer();
            CurrentPlayer.Balance = Web3Connector.Instance.Balance;
            CurrentPlayer.UserName = Web3Connector.Instance.AccountAddress;
        }

    }
    void OnNewGameStarted()
    {
        Debug.Log("SinglePlayer OnNewGameStarted");
        _prepareTable();
    }

    void OnDealCreated()
    {
        CurrentDealer = new WJBJDealer();
        CurrentPlayer.Deal();
        Debug.Log("Deal created" + CurrentPlayer.Balance);
        CurrentPlayer.TakeCard(CurrenTable.PopUp());
        CurrentDealer.TakeCard(CurrenTable.PopUp());
        CurrentPlayer.TakeCard(CurrenTable.PopUp());
        CurrentDealer.TakeCard(CurrenTable.PopUp());
        CardManager.Instance.Distribute();
    }
    
    void OnCardDistributeFinished()
    {
        CurrentGame.Status = CurrentGame.CheckStatus(CurrentDealer, CurrentPlayer);
        if(CurrentGame.Status == WJBJGameLogic.GameStatus.IN_GAME)
        {
            if(CurrentPlayer.Status == WJBJBasePlayer.PlayerStatus.STANDING)
            {
                CurrentDealer.TakeCard(CurrenTable.PopUp());
                CardManager.Instance.Distribute();
            }
        }
        else
        {
            Debug.Log(CurrentGame.Status.ToString());
            if (CurrentDealer.Status == WJBJBasePlayer.PlayerStatus.INITIAL)
                CardManager.Instance.ShowDealerCard();
            else
            {
                winnerDeclared?.Invoke();
                bool is_user_win = CurrentGame.Status == WJBJGameLogic.GameStatus.PLAYER_WIN;
                bool is_draw = CurrentGame.Status == WJBJGameLogic.GameStatus.DRAW;
                Web3Connector.Instance.DeclareWinner(is_user_win,is_draw);
            }
        }
    }
    #endregion // Event Handlers
    #region Internal Methods
    void _prepareTable()
    {
        _createTable();
        CardManager.Instance.ResetNewGame();

    }
    void _createTable()
    {
        CurrenTable  = new Table();
        CurrenTable.Initialize();
    }
    #endregion
    #region Public Methods for Game Play
    public bool Bet(int _amount)
    {
        return CurrentPlayer.Bet(_amount);
    }

    public void Deal()
    {
        // create game contract on chain
        Web3Connector.Instance.CreateGame(CurrentPlayer.Betting);
        
    }
    public void Stand() {
        CurrentPlayer.Status = WJBJBasePlayer.PlayerStatus.STANDING;
        CardManager.Instance.ShowDealerCard();
    }
    public void Hit() {
        CurrentPlayer.Status = WJBJBasePlayer.PlayerStatus.GOING;
        CurrentPlayer.TakeCard(CurrenTable.PopUp());
        CardManager.Instance.Distribute();
    }

    public void Double()
    {
        Web3Connector.Instance.DoubleBet(CurrentPlayer.Betting);
        
    }

    public void MusicOnOff(bool _isOnOff)
    {
        m_objMusic.mute = _isOnOff;
    }
    #endregion
}
