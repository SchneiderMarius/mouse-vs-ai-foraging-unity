using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class positionlogger : MonoBehaviour
{

    private GameObject task;
    private float delay = 0.066f; //15 Hz

    // Start is called before the first frame update
    void Start()
    {
        task = GameObject.Find("task");
        //StartCoroutine(streamPosition());
    }

    // Update is called once per frame
    void Update()
    {
        task.SendMessage("Write_log", DateTime.Now.ToString("HH:mm:ss.fff") + "\tp\t" + transform.position.x.ToString() + "\t" + transform.position.y.ToString() + "\t" + transform.position.z.ToString() + "\t" + transform.eulerAngles.y.ToString());
    }

}
