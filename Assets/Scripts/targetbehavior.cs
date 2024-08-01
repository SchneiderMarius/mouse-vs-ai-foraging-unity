using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class targetbehavior : MonoBehaviour
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

    private void OnTriggerEnter(Collider other)
    {
        task.SendMessage("win");
    }
}
