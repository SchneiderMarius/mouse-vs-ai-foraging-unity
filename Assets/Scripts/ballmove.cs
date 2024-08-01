using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class ballmove : MonoBehaviour
{
    //SerialPort stream = new SerialPort();
    public Vector3 move;
    public float yangle;
    public float gain = 0.05f;
    public GameObject scene;
    string comport = "";
    public float rotationgain = 250f;
    string serinput;
    int x1, x2, y1, y2;
    bool ball_initialized;

    void Start()
    {
        scene = GameObject.Find("Scenemanager");
        // comport = scene.GetComponent<scenes>().ballcom;
        // stream.BaudRate = 115200;
        // stream.PortName = comport;
        // stream.ReadTimeout = 20;
        // try
        // {
        //     stream.Open();
        //     Debug.Log("Serial connection to ball initializing...");
        // }
        // catch (Exception e) { Debug.Log("Couldn't open ballport! " + e.Message); }
        move = new Vector3(0.0f, 0.0f, 0.0f);
        ball_initialized = false; //variable turns to true after connection is established - stops rapid reconnect attempts
    }

    void Update()
    {
        //get info from serial and parse. This was annoying as lengths are variable and c# substrings are stupid.
        try
        {
            // serinput = stream.ReadLine();
            // x1 = int.Parse(serinput.Substring(0, 4));
            // y1 = int.Parse(serinput.Substring(4, 4));
            // x2 = int.Parse(serinput.Substring(8, 4));
            // y2 = int.Parse(serinput.Substring(12, 4));

            //move square
            move.z = (float)-x1 / 127;
            move.x = (float)-x2 / 127;
            

            yangle = (float)(y1) + (y2);
            
            //put model inputs here, then once it's working re-activate ballmove script in editor
            //make them [-1,1] for move.z and move.x and yangle
            //probably you'll have to change rotationgain
            transform.Translate(move * gain, Space.Self);
            transform.Rotate(0.0f, yangle / rotationgain, 0.0f, Space.Self);

            ball_initialized = true;

            // if(Time.frameCount % 40 == 0) //about once per second discard the in buffer to keep input in tight sync with game
            // {
            //     stream.DiscardInBuffer();
            //     Debug.Log("Discarding buffer...");
            // }
        }
        catch (TimeoutException) 
        {
            // Debug.Log("ball read timeout!");
            // if (ball_initialized==true) //reset serial connection if timeout detected and connection isn't already resetting
            //     try
            //     {
            //         stream.Close();
            //         stream.Open();
            //         Debug.Log("Serial connection to ball resetting...");
            //         ball_initialized = false;
            //     }
            //     catch (Exception e) { Debug.Log("Couldn't reset ballport! " + e.Message); }        
        }


    }

}

