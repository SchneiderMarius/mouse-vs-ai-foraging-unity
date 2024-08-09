using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//this object stays put when title screen switches to main game to hold a few critical global variables
//other scripts directly change the public variables within it - the go button calls go() which switches to the main scene if the logfile has been set

public class scenes : MonoBehaviour
{
    public int phase = 0;
    public string logdirectory = "";
    public bool isLogSet = false;
    public string ballcom = "";
    public string rewardcom = "";
    public int phase0delay = 0;
    public float targetdistance = 0;
    public bool scale_difficulty = false;
    public float difficulty_increment = 0;
    public int num_decoys = 1;
    public string recordingdirectory = "";
    public bool recordtoggle = false;
    public int replay_type;
    public string replay_load_dir;
    public string replay_save_dir;
    public GameObject phaseselectfield;
    public GameObject ballcomfield;
    public GameObject rewardcomfield;
    public GameObject delayfield;
    public GameObject distancefield;
    public GameObject difficultybutton;
    public GameObject incfield;
    public GameObject num_decoys_field;
    public GameObject replay_type_field;
    public GameObject replay_load_directory_field;
    public GameObject replay_log_save_directory_field;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        phase = 0; //all values reset again on start to support multiple sessions per app instance
        isLogSet = false;
        logdirectory = "";
        recordingdirectory = "";
        recordtoggle = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void go() //proceeds to next scene when Go! is pressed (if log file and com ports are set)
    {
        phase = phaseselectfield.GetComponent<Dropdown>().value; 
        ballcom = ballcomfield.GetComponent<InputField>().text;
        rewardcom = rewardcomfield.GetComponent<InputField>().text;
        phase0delay = int.Parse(delayfield.GetComponent<InputField>().text);
        targetdistance = float.Parse(distancefield.GetComponent<InputField>().text);
        scale_difficulty = difficultybutton.GetComponent<Toggle>().isOn;
        difficulty_increment = float.Parse(incfield.GetComponent<InputField>().text);
        num_decoys = num_decoys_field.GetComponent<Dropdown>().value + 1; //index starts at 0, first choice is 1.

        if(isLogSet)
        {
            SceneManager.LoadScene("main");
        }
        else
        {
            Debug.Log("Log file not set!");
        }
    }


    void go_replay() //proceeds to replay scene when Replay! is pressed (if log file and other options are set)
    {
        //phase = phaseselectfield.GetComponent<Dropdown>().value;
        replay_type = replay_type_field.GetComponent<Dropdown>().value; //int encoded: 0 for normal, 1 for depth
        replay_load_dir = replay_load_directory_field.GetComponent<InputField>().text;
        replay_load_dir = replay_load_dir.Substring(1, replay_load_dir.Length - 2);
        replay_save_dir = replay_log_save_directory_field.GetComponent<InputField>().text;
        replay_save_dir = replay_save_dir.Substring(1, replay_save_dir.Length - 2);

        SceneManager.LoadScene("replay");
    }
}

