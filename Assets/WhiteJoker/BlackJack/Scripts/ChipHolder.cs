using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChipHolder : MonoBehaviour
{
    public Text m_uiStackAmount;
    public Text m_uiLastChip;
    public GameObject m_uiChipSet;
    public TMPro.TextMeshProUGUI m_uiMsg;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetChipStacks()
    {
        m_uiChipSet.SetActive(false);
        m_uiStackAmount.text = "";
    }
    public void ShowMessage(string _msg)

    {
        m_uiMsg.text = _msg;
        m_uiMsg.gameObject.SetActive(true);
    }
    public void UpdateAmount( int _current)
    {
        m_uiChipSet.SetActive(SinglePlay.Instance.CurrentPlayer.Betting > 0);
        //m_uiStackAmount.gameObject.SetActive(SinglePlay.Instance.CurrenPlayer.Betting > 0);
        m_uiStackAmount.text = SinglePlay.Instance.CurrentPlayer.Betting > 0 ? "+ " + SinglePlay.Instance.CurrentPlayer.Betting.ToString() : "";
        m_uiLastChip.text = _current.ToString();
    }
}
