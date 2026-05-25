using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Ports;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class pythonController : MonoBehaviour
{
    public GameObject task;    
    public float gain = 1.5f;
    public float rotationgain = 10f;
    public float[] action;

    void Start()
    {

    }

    
    void Update()
    {
        //grab current action value from tcp script
        action =  task.GetComponent<tcp_client>().action;

        //translation
        float moveX = action[0];
        float moveZ = action[1];

        //rotation
        float rotationY = action[2];

        //apply gains and deltaT
        moveX = moveX * gain * Time.deltaTime;
        moveZ = moveZ * gain * Time.deltaTime;
        Vector3 move = new Vector3(moveX, 0.0f, moveZ) ;
        float yangle = rotationY * rotationgain * Time.deltaTime; // Scale rotation according to rotation gain

        //apply transformation
        transform.Translate(move, Space.Self);
        transform.Rotate(0.0f, yangle, 0.0f, Space.Self);

    }

}
