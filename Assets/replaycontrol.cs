using System.Collections;
using UnityEngine;
using System.IO;
using System.IO.Ports;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class replaycontrol : MonoBehaviour
{
    private GameObject scene;
    private GameObject target;
    private GameObject mouse;
    public string logpath; //defaults to a test directory; is updated to user input game is run from title scene
    public string replaylogpath;
    int replaytype;
    Vector3 mouse_pos, target_pos;
    Quaternion mouse_rot, target_rot;
    StreamReader sr;
    StreamWriter log;
    public GameObject cam1, cam2;

    DateTime start_time_log,start_time_realtime, time_log, time_realtime;
    TimeSpan session_time_log, session_time_real;
    double session_seconds_log, session_seconds_real;
    bool first;

    private void Start()
    {
        //get references etc
        scene = GameObject.Find("Scenemanager");
        target = GameObject.Find("target");
        mouse = GameObject.Find("Mouse");

        //get values from title scene
        logpath = scene.GetComponent<scenes>().replay_load_dir;
        replaylogpath = scene.GetComponent<scenes>().replay_save_dir;
        replaytype = scene.GetComponent<scenes>().replay_type;

        //if replaytype = depth (1), turn on the filters on the cameras
        if(replaytype == 1)
        {
            cam1.GetComponent<depthshader>().enabled = true;
            cam2.GetComponent<depthshader>().enabled = true;
        }

        //open log file for reading
        try
        {
            sr = new StreamReader(logpath);
        }
        catch (Exception e)
        {
            Debug.Log("Exception: " + e.Message);
        }

        //open replay logger
        replaylogpath += @"\replaylog.txt";
        if (File.Exists(replaylogpath))
        {
            Debug.LogError("Overwriting replay log.");
        }
        log = new StreamWriter(replaylogpath);

        first = true;

        //reproduceable stimulus
        //UnityEngine.Random.InitState(25);
        
        //initialize starting mouse and target positions
        mouse_pos = mouse.transform.position; 
        mouse_rot = mouse.transform.rotation;

        target_pos = target.transform.position;
        target_rot = target.transform.rotation;

        start_time_realtime = DateTime.Now;

    }


    private void Update()
    {

        string[] line;
        //string[] time_string;

        //-------------reads lines until it finds a p line, updates internal position variables, then breaks
        while (true) {
            
            //grab next line from log
            line = sr.ReadLine().Split('\t');

            //deal with the first few lines: if the first column isn't a date, skip it.
            if (! DateTime.TryParse(line[0], out DateTime result))
            {
                continue;
            }

            //if this is the first time in the log, record that time
            if (first)
            {
                start_time_log = DateTime.Parse(line[0]);
                Debug.Log("Starting log time: " + start_time_log);
                first = false; //deactivate this for subsequent lines
            }

            //end of log reached, stops game
            if (line[1] == "e")
            {
                log.Close(); 
                Destroy(scene);
            }

            //if it's a t line, update that variable
            if (line[1] == "t")
            {
                //update the internal target position
                Debug.Log("Target position: " + line[2] + "\t" + line[3] + "\t" + line[4]);
                target_pos = new Vector3(float.Parse(line[2]), float.Parse(line[3]), float.Parse(line[4]));
                //target_rot.eulerAngles = new Vector3(0f, float.Parse(line[5]), 0f);

            } 

            //if it's a p line, update that variable, then break
            if (line[1] == "p")
            {
                //update internal mouse position and rotation 
                mouse_pos = new Vector3(float.Parse(line[2]), float.Parse(line[3]), float.Parse(line[4]));
                mouse_rot.eulerAngles = new Vector3(0f, float.Parse(line[5]), 0f);

                //move on
                break;
            }

        }

        //-------------wait for the current p line time (blocking, might have to make it a manual while loop)
        //calculate time since start in game
        time_log = DateTime.Parse(line[0]);
        session_time_log = time_log - start_time_log;
        session_seconds_log = session_time_log.TotalSeconds;

        //calculate time in the session
        time_realtime = DateTime.Now;
        session_time_real = time_realtime - start_time_realtime;
        session_seconds_real = session_time_real.TotalSeconds;

        //hold until real session time meets log session time
        while (session_seconds_real < session_seconds_log)
        {
            time_realtime = DateTime.Now;
            session_time_real = time_realtime - start_time_realtime;
            session_seconds_real = session_time_real.TotalSeconds;
        }  

        //update target position
        target.transform.position = target_pos; //reset target position
        //target.transform.rotation = target_rot;

        //update mouse position        
        mouse.transform.position = mouse_pos;
        mouse.transform.rotation = mouse_rot;

        //log some stuff and frame is released to be rendered
        if (Time.frameCount % 15 == 0)
        {
            Debug.Log("Rendering frame with replay latency (ms) of: " + (session_seconds_real - session_seconds_log) * 1000);
        }
        Write_log(time_log.ToString("HH:mm:ss.fff") + "\t" + time_realtime.ToString("HH:mm:ss.fff") + "\t" + session_seconds_log.ToString() + "\t" + session_seconds_real.ToString());

    }

    void Write_log(string message)
    {
        log.WriteLine(message);
        //Debug.Log(message);
    }
}
