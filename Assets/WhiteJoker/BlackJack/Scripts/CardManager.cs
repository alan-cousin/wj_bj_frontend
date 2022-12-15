using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CardManager : MonoBehaviour
{
    public GameObject m_objPlayer;
    public GameObject m_objDealer;
    public GameObject m_objDummy;
    public GameObject m_objLast3;

    public GameObject m_prefabCard;


    public delegate void OnCardDistributeFinished();
    public event OnCardDistributeFinished cardDistributeFinished;
    public List<Card> m_dealerCards;
    public List<Card> m_playerCards;
    public List<Card> m_Lst3Cards;
    public static CardManager Instance { get; private set; }
    CardManager()
    {
        Instance = this;
        m_dealerCards = new List<Card>();
        m_playerCards = new List<Card>();
    }


    private void Awake()
    {
        _registerHandles();
    }
    #region Event Handlers
    void _registerHandles()
    {
        Web3Connector.Instance.gameCreatedOnContract += OnStartNewTurn;
        SinglePlay.Instance.winnerDeclared += OnWinnerDeclared;
        Web3Connector.Instance.declaredWinnerOnContract += OnDeclaredWinnerOnContract;
    }
    void OnDeclaredWinnerOnContract()
    {
        ResetNewGame();


    }
    void OnWinnerDeclared()
    {

    }
    void OnStartNewTurn()
    {

    }
    #endregion // Event Handlers

    public async void ShowDealerCard()
    {

        await new WaitForSeconds(0.4f);
        m_dealerCards[1].ShowFront();
        SinglePlay.Instance.CurrentDealer.Status = WJBJBasePlayer.PlayerStatus.BLACKJACK;
        cardDistributeFinished?.Invoke();
    }
    public async void Distribute()
    {
        WJBJPlayer player = SinglePlay.Instance.CurrentPlayer;
        WJBJDealer dealer = SinglePlay.Instance.CurrentDealer;
        bool finished = false;
        bool is_player = true;
        while (!finished)
        {
            int card = -1;
            WJBJBasePlayer taker = is_player ? (WJBJBasePlayer)player : (WJBJBasePlayer)dealer;
            if (!taker.IsDistributed())
            {
                if (!is_player && taker.LastPos == 2 && !m_dealerCards[1].IsShowFront())
                {
                    m_dealerCards[1].ShowFront();
                }
                else
                {
                    card = taker.DistributeLastCard();

                    Card new_card_obj = GameObject.Instantiate(m_prefabCard).GetComponent<Card>();
                    new_card_obj.Initialize(card, is_player ? true : !(dealer.LastPos == 2));
                    new_card_obj.transform.SetParent(m_objDummy.transform);
                    new_card_obj.transform.localPosition = Vector3.zero;
                    if (is_player)
                        MoveToPlayer(new_card_obj);
                    else
                        MoveToDealer(new_card_obj);
                }
                
            }
            
            await new WaitForSeconds(0.4f);
            is_player = !is_player;
            finished = player.IsDistributed() && dealer.IsDistributed();
        }
        await new WaitForSeconds(1.0f);
        cardDistributeFinished?.Invoke();
    }
    void MoveToPlayer(Card _card) {
        _card.transform.SetParent(m_objPlayer.transform);
        _card.SetOrderInLayer(10 + SinglePlay.Instance.CurrentPlayer.LastPos);
        _card.SetDestination(new Vector3(0.3f * SinglePlay.Instance.CurrentPlayer.LastPos, 0.2f * SinglePlay.Instance.CurrentPlayer.LastPos,0));
        m_playerCards.Add(_card);
    }
    void MoveToDealer(Card _card) {
        _card.transform.SetParent(m_objDealer.transform);
        _card.SetOrderInLayer(10 + SinglePlay.Instance.CurrentDealer.LastPos);
        _card.SetDestination( new Vector3(0.2f * SinglePlay.Instance.CurrentDealer.LastPos, 0.2f * SinglePlay.Instance.CurrentDealer.LastPos, 0));
        m_dealerCards.Add(_card);
    }
    void MoveToLast3(Card _card) { }
    async void DistributeCard()
    {
        await new WaitForSeconds(1f);
    }
    public void ResetNewGame()
    {
        m_objPlayer.SetActive(true);
        m_objDealer.SetActive(true);
        m_objDummy.SetActive(true);
        m_objLast3.SetActive(true);
        _resetPlayer();
        _resetDealer();
        _resetLast3();
    }
    void _removeAllChild(GameObject _parent)
    {
        foreach (Transform child in _parent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    void _resetPlayer() {
        m_playerCards.Clear();
        _removeAllChild(m_objPlayer);
    }
    void _resetDealer() {
        m_dealerCards.Clear();
        _removeAllChild(m_objDealer);
    }
    void _resetLast3() {
        _removeAllChild(m_objLast3);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void _initialize()
    {
        m_objPlayer.SetActive(false);
        m_objDealer.SetActive(false);
        m_objDummy.SetActive(false);
        m_objLast3.SetActive(false);
    }
}
