using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using UnityEngine.UI;


public class recordscreenbutton : MonoBehaviour
{

    public Button mybutton;
    private GameObject scene;
    private GameObject text;
    public string recordpath = "";

    // Start is called before the first frame update
    void Start()
    {
       scene = GameObject.Find("Scenemanager");
        text = GameObject.Find("directory2");
        Button btn = mybutton.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TaskOnClick()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForSaveDialog(true, null,"Enter screen recording file save directory", "Save"); //added dummy filename to be removed- easier to implement this way

        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
        recordpath = FileBrowser.Result;
        //recordpath = recordpath.Substring(0,recordpath.Length-4); //remove trailing filename
        scene.GetComponent<scenes>().recordingdirectory = recordpath;
        text.GetComponent<Text>().text = recordpath;
        scene.GetComponent<scenes>().recordtoggle = true;
    }
}
