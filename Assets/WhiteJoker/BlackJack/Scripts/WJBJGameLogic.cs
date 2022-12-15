using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WJBJGameLogic : MonoBehaviour
{
    public enum GameStatus{
        PLAYER_WIN,
        DRAW,
        DEALER_WIN,
        IN_GAME,
    }
    public GameStatus Status { get; set; }

    public const int BLACKJACK = 21;
    public const int STANDING_POINT= 17;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public int calculateOnHand(WJBJBasePlayer _player)
    {
        int total_score = 0;
        int ace_count = 0;
        for(int i = 0; i < _player.m_cards.Count; i++)
        {
            int card_num = _player.m_cards[i] % 13;
            if (card_num >= 9)
                total_score += 10;
            else if (card_num != 0)
                total_score += (card_num + 1);
            else if(card_num == 0)//ace
            {
                ace_count++;
            }
        }
        for (int j = 0; j < ace_count; j++)
            total_score += total_score <= 10 ? 11 : 1;
        return total_score;
    }
    public int caculateDealer(WJBJBasePlayer _player)
    {
        int total_score = 0;
        int card_num = _player.m_cards[0] % 13;
        if (card_num >= 9)
            total_score = 10;
        else if (card_num != 0)
            total_score = card_num+ 1;
        else if (card_num == 0)//ace
        {
            total_score = 11;
        }
        return total_score;
    }
    public GameStatus CheckStatus(WJBJBasePlayer _dealer, WJBJBasePlayer _player)
    {
        int player_score = calculateOnHand(_player);
        int dealer_score = calculateOnHand(_dealer);
        if( player_score == BLACKJACK)
        {
            if(_player.Status == WJBJPlayer.PlayerStatus.INITIAL)//blackjack
            {
                if( dealer_score == BLACKJACK)
                {
                    _player.CurrentScore = player_score;
                    _dealer.CurrentScore = dealer_score;
                    return GameStatus.DRAW;
                }
                else
                {
                    return GameStatus.PLAYER_WIN;
                }
                
            }
            else // soft hand
            {
                _player.Status = WJBJBasePlayer.PlayerStatus.STANDING;
                _player.CurrentScore = player_score;
                if(dealer_score == BLACKJACK)
                {
                    _dealer.Status = WJBJBasePlayer.PlayerStatus.STANDING;
                    _dealer.CurrentScore = dealer_score;
                    return GameStatus.DRAW;
                }else if( dealer_score > BLACKJACK)
                {
                    _dealer.Status = WJBJBasePlayer.PlayerStatus.BUST;
                    _dealer.CurrentScore = dealer_score;
                    return GameStatus.PLAYER_WIN;
                } else if(dealer_score < STANDING_POINT)
                {
                    _dealer.Status = WJBJBasePlayer.PlayerStatus.GOING;
                    _dealer.CurrentScore = dealer_score;
                    return GameStatus.IN_GAME;
                }
                else
                {
                    _dealer.Status = WJBJBasePlayer.PlayerStatus.STANDING;
                    _dealer.CurrentScore = dealer_score;
                    return GameStatus.PLAYER_WIN;
                }
            }
        }
        else if( player_score > BLACKJACK)
        {
            _player.CurrentScore = player_score;
            _player.Status = WJBJBasePlayer.PlayerStatus.BUST;
            return GameStatus.DEALER_WIN;
        }
        else
        {
            if(dealer_score == BLACKJACK && _dealer.Status == WJBJBasePlayer.PlayerStatus.INITIAL)
            {
                _dealer.CurrentScore = BLACKJACK;
                return GameStatus.DEALER_WIN;
            }
            if(_player.Status < WJBJBasePlayer.PlayerStatus.STANDING) // no stand
            {
                
                //_player.Status = WJBJBasePlayer.PlayerStatus.GOING;
                _player.CurrentScore = player_score;
                _dealer.CurrentScore = caculateDealer(_dealer);
                _dealer.Status = WJBJBasePlayer.PlayerStatus.INITIAL;
                return GameStatus.IN_GAME;
            } else // player standing
            {
                _player.Status = WJBJBasePlayer.PlayerStatus.STANDING;
                _player.CurrentScore = player_score;
                if (dealer_score < STANDING_POINT) {
                    _dealer.CurrentScore = dealer_score;
                    _dealer.Status = WJBJBasePlayer.PlayerStatus.GOING;
                    return GameStatus.IN_GAME;
                }
                else if(dealer_score > BLACKJACK) {
                    _dealer.CurrentScore = dealer_score;
                    _dealer.Status = WJBJBasePlayer.PlayerStatus.BUST;
                    return GameStatus.PLAYER_WIN;
                }
                else {
                    _dealer.CurrentScore = dealer_score;
                    _dealer.Status = WJBJBasePlayer.PlayerStatus.STANDING;
                    if (player_score == dealer_score) return GameStatus.DRAW;
                    else if (player_score > dealer_score) return GameStatus.PLAYER_WIN;
                    else return GameStatus.DEALER_WIN;
                }
            }

        }
    }
}
