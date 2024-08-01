using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class dropdown : MonoBehaviour
{
    Dropdown m_Dropdown;
    public int value;
    private GameObject scene;

    void Start()
    {
        scene = GameObject.Find("Scenemanager");
        m_Dropdown = GetComponent<Dropdown>();
        m_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(m_Dropdown);
        });

    }

    //Ouput the new value of the Dropdown into Text
    void DropdownValueChanged(Dropdown change)
    {
        value = change.value;
        scene.GetComponent<scenes>().phase = change.value;
    }
}
