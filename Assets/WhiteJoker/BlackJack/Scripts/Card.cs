using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public GameObject Front;
    public GameObject Back;
    public int CardNumber;
    private float m_distance;
    public Vector3 Destination;
    private bool m_isMoving;
    Card()
    {
        m_isMoving = false;
    }
    public void SetDestination(Vector3 _dest)
    {
        Vector3 offset = _dest - transform.localPosition;
        m_distance = offset.magnitude;
        m_isMoving = true;
        Destination = _dest;
    }

    // Start is called before the first frame update
    void Start()
    {
       // m_isMoving = false;
        //Destination = 
    }

    // Update is called once per frame
    void Update()
    {
        if (m_isMoving)
        {
            Vector3 dir = Destination - transform.localPosition;
            Vector3 move_offset = dir.normalized * m_distance * Time.deltaTime;
            transform.localPosition = transform.localPosition + move_offset;
            if (dir.magnitude < 0.03f)
            {
                transform.localPosition = Destination;
                m_isMoving = false;
                
            }
        }
    }
    public void SetOrderInLayer(int _order)
    {
        Front.GetComponent<SpriteRenderer>().sortingOrder = _order;
        Back.GetComponent<SpriteRenderer>().sortingOrder = _order;
    }
    public static string GetCardName(int _number)
    {
        if (_number == 52) return "black_joker";
        if (_number == 53) return "black_joker";
        string type = "";
        switch (_number % 4)
        {
            case 0:
                type = "clubs";
                break;
            case 1:
                type = "spades";
                break;
            case 2:
                type = "hearts";
                break;
            case 3:
                type = "diamonds";
                break;
            default:
                type = "hearts";
                break;
        }

        string card_name = "";
        switch(_number % 13)
        {
            case 0:
                card_name = "ace";
                break;
            case 10:
                card_name = "jack";
                break;
            case 11:
                card_name = "queen";
                break;
            case 12:
                card_name = "king";
                break;
            default:
                card_name = ((_number % 13) + 1).ToString();
                break;
        }
        
        string sprite_name = card_name + "_of_"+type;

        return sprite_name;
    }
    public void Initialize(int _number, bool _show)
    {
        Front.SetActive(_show);
        Back.SetActive(!_show);
        CardNumber = _number;
        string card_name = GetCardName(_number);
        Sprite card_img = Resources.Load<Sprite>("cards/" + card_name) ;
        SpriteRenderer renderer = Front.GetComponent<SpriteRenderer>();
        renderer.sprite = card_img;
    }
    public bool IsShowFront()
    {
        return Front.activeSelf;
    }
    public void ShowFront()
    {
        Front.SetActive(true);
        Back.SetActive(false);
    }
}
