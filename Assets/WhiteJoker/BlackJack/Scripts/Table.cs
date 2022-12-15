using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Table
{
    Stack<int> m_cards;
    public Table()
    {

    }
     void _arrayCards()
    {
        Random rng = new Random((int)DateTime.Now.Ticks);
        m_cards = new Stack<int>();
        while (m_cards.Count < 52)
        {
            int new_card = rng.Next() % 52;
            if (!m_cards.Contains(new_card))
            {
                m_cards.Push(new_card);
            }
        }
    }
    public void Initialize()
    {
        _arrayCards();
    }
    ~Table()
    {

    }

    public int PopUp()
    {
        int cur_card = m_cards.Pop();
        Debug.Log("Current Count : " + m_cards.Count);

        return cur_card;
    }

    public void CheckDeck()
    {
        if (m_cards.Count < 17)
            _arrayCards();
    }
}
