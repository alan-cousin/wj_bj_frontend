using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoHide : MonoBehaviour
{
    public float m_fTime;
    float m_fCounter;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        m_fCounter = 0;
    }
    // Update is called once per frame
    void Update()
    {
        m_fCounter += Time.deltaTime;
        if(m_fCounter > m_fTime)
        {
            gameObject.SetActive(false);
        }
    }
}
