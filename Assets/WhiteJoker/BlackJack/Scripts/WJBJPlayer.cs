using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WJBJBasePlayer
{
    public enum PlayerStatus
    {
        INITIAL,
        GOING,
        STANDING,
        BUST,
        BLACKJACK,
    }

    public PlayerStatus Status { get; set; }
    public List<int> m_cards;
    public int LastPos { get; set; }
    public int CurrentScore { get; set; }
    public int Betting { get; set; }
    public WJBJBasePlayer()
    {
        m_cards = new List<int>();
        LastPos = 0;
    }
    public void TakeCard(int _card)
    {
        m_cards.Add(_card);
    }
    public void EmptyHand()
    {
        m_cards.Clear();
        CurrentScore = 0;
        LastPos = 0;
        Status = PlayerStatus.INITIAL;
        Betting = 0;
    }
    public bool IsDistributed()
    {
        return LastPos >= m_cards.Count;
    }
    public int DistributeLastCard()
    {
        return m_cards[LastPos++];
    }
}
public class WJBJDealer : WJBJBasePlayer
{
    public WJBJDealer() { }
}
public class WJBJPlayer : WJBJBasePlayer
{

    public int Balance { get; set; }
    public string UserName { get; set; }

    public WJBJPlayer()
    {
        Balance = 0;
        UserName = string.Empty;
        Betting = 0;
    }
    public bool Bet(int _amount)
    {
        if (_amount + Betting <= Balance)
        {
            Betting = Betting + _amount;
            return true;
        }
        return false;
    }

    public bool Deal()
    {
        Balance = Balance - Betting;
        return false;
    }

   
}
