using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class findTags : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Replace "YourTag" with the actual tag you are looking for
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("decoys");

        foreach (GameObject obj in objectsWithTag)
        {
            Debug.Log("Found object: " + obj.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}