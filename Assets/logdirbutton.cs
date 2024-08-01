using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

public class logdirbutton : MonoBehaviour
{
    public Button mybutton;
    private GameObject scene;
    private GameObject text;
    public string logpath = "";

    void Start()
    {
        scene = GameObject.Find("Scenemanager");
        text = GameObject.Find("directory");
        Button btn = mybutton.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }


    void Update()
    {

    }

    void TaskOnClick()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForSaveDialog(false, null, "Enter log file save directory and filename", "Save");

        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
        logpath = FileBrowser.Result + ".txt";
        scene.GetComponent<scenes>().logdirectory = logpath;
        text.GetComponent<Text>().text = logpath;
        scene.GetComponent<scenes>().isLogSet = true;
    }
}
