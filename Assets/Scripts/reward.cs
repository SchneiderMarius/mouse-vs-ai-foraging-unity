using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;
using System;


public class reward : MonoBehaviour
{
    SerialPort outport = new SerialPort("COM4", 9600);

    void Start()
    {
        try
        {
            outport.Open();
            Debug.Log("Serial port for output open");
        }
        catch (Exception e) { Debug.Log("Couldn't open output serial port! " + e.Message); }
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(Click);
    }

    void Update()
    {
    }

    void Click()
    {
        if(outport.IsOpen == false) { outport.Open(); }
        outport.Write("0");
        Debug.Log("Reward command sent");
    }
}
