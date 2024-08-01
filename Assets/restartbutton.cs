using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class restartbutton : MonoBehaviour
{
    private GameObject task;
    // Start is called before the first frame update
    void Start()
    {
        task = GameObject.Find("task");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void restart()
    {
        task.SendMessage("restart");
    }
}
