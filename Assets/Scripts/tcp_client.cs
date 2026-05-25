using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;

public class tcp_client : MonoBehaviour
{
    
    NetworkStream stream;
    TcpClient client;
    bool open;
    int loops = 0;
    public float[] action;
    public float reward;

    void Start()
    {
        TcpClient client = new TcpClient("127.0.0.1", 12345);
        stream = client.GetStream();
        if(stream != null){
            open = true;
        }

        action = new float[3]; //vector holding the actions. forward/back, left/right, rotation.
        reward = 0; //reward signal in first loop is 0, then updated every frame.
        loops=0; //blocking loop iteration counter. used for validation.

    }

    void Update()
    {

        //this if block BLOCKS when waiting for messages from python
        if(open && Time.frameCount >= 3) //plot a few frames to output an image before starting the blocking loop
        {
            
            // reward = ...
            //!!!! UPDATE reward VALUE HERE BASED ON THE OUTCOME OF LAST FRAME !!!!
            
            //Tell python I have plotted a new frame by sending last frame's reward. (on windows it's encoded as little endian single, might have to modify for cross platform)
            byte[] message = BitConverter.GetBytes(reward); //unity loop count
            stream.Write(message, 0, message.Length);

            //Python processes frame here and sends its next action.

            try 
            {
                //Get the next action from python.
                byte[] buffer = new byte[1024]; // 3 float * 4 bytes each = only 12, but read a bunch to make sure the buffer is cleared between messages
                stream.Read(buffer,0,buffer.Length); //blocking happens here

                //decode the byte[] to float vector
                for (int i = 0; i < 3; i++) {
                    action[i] = BitConverter.ToSingle(buffer, i * 4);
                }

            }
            catch (Exception e)
            {
                Debug.Log(e);
                open = false;
                stream.Close();
                client.Close();
            }

            loops = loops+1;

        }

    }

    void OnApplicationQuit() {
        stream.Close();
        client.Close();
    }

}
