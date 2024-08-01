using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class mainui : MonoBehaviour
{

    public GameObject task;
    public Text rewardObject;
    public Text targetObject;
    public Text trialObject;
    public Text perfObject;
    public Text timeObject;
    public Text sessionperfObject;
    public Text sessionTimeObject;
    public Text sessionFrameObject;
    public Text fpsObject;
    private int reward;
    private float targetdistance;
    private int trial;
    private double performance;
    private float time_remaining;
    private double session_performance;
    private int frame;
    private float fps;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        reward = task.GetComponent<task_control>().reward_given;
        rewardObject.text = "Cumulative Reward: " + reward.ToString();
        targetdistance = task.GetComponent<task_control>().target_start_pos.x;
        targetObject.text = "Target Distance: " + targetdistance.ToString();
        trial = task.GetComponent<task_control>().trial_index;
        trialObject.text = "Trial: " + trial;
        performance = task.GetComponent<task_control>().running_performance;
        perfObject.text = "Diff. Performance: " + performance.ToString("0.###");
        time_remaining = task.GetComponent<task_control>().timeout_time - Time.timeSinceLevelLoad;
        timeObject.text = "Trial time left: " + time_remaining.ToString("0.##");
        session_performance = task.GetComponent<task_control>().session_performance;
        sessionperfObject.text = "Session Performance: " + session_performance.ToString("0.###");
        frame = task.GetComponent<task_control>().framenum;
        sessionFrameObject.text = "Frame: " + frame.ToString();
        sessionTimeObject.text = "Time: " + DateTime.Now.ToString("HH:mm:ss.fff");
        fps = 1 / Time.smoothDeltaTime;
        fpsObject.text = "FPS: " + fps.ToString();

    }
}
