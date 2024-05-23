using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelMoves : MonoBehaviour
{
    public GameObject ParentPanel;

    void Start()
    {
       
    }

    public void SlidePanelRight()
    {
        Vector3 currentPos = ParentPanel.transform.position;
        ParentPanel.transform.position = new Vector3(-9, currentPos.y, currentPos.z);
    }

    public void SlidePanelLeft()
    {
        Vector3 currentPos = ParentPanel.transform.position;
        ParentPanel.transform.position = new Vector3(964, currentPos.y, currentPos.z);
    }

    void Update()
    {
       
    }
}
