using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.IO;
using System.IO.Ports;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AgentController : Agent
{
    private GameObject scene;

    private task_control taskControl;
    // private GameObject mouse;
    
    int phase;
    
    public float yangle;
    // public float gain = 0.05f;
    
    [SerializeField] public float gain = 1.5f;
    
    [SerializeField] public float rotationgain = 10f;
    
    // public float gain = 15;
    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;
    
    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    Vector3 mouse_start_pos;
    public Vector3 target_start_pos; //target_start_pos.x is what stores the distance, including the circle radius for phase 3
    private Quaternion mouse_start_rot;
    private float targetdistance;
    public double session_performance;

    [SerializeField] private GameObject target;

    [SerializeField] private float moveSpeed = 4f;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        scene = GameObject.Find("Scenemanager");
        target = GameObject.Find("target");

        taskControl = GameObject.Find("task").GetComponent<task_control>();
        session_performance = taskControl.GetComponent<task_control>().session_performance;
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
        
        // if (rb == null)
        //     Debug.LogError("mouseRigidbody is null!");
        // if (target == null)
        //     Debug.LogError("target is null!");
        // if (taskControl == null)
        //     Debug.LogError("taskControl is null!");
        
        // mouse_start_pos = transform.position; //start_pos variables store reset positions between trials - mouse starts at ~(0,0,0)
        // mouse_start_rot = transform.rotation;
        // target_start_pos = new Vector3(targetdistance, 0.5f, 0f); //starting target distance defined by user 

        // taskControl.newtrial();
        // mousebody = mouse.GetComponent<Rigidbody>();
        // phase = scene.GetComponent<scenes>().phase;
        
        //
        // trial_history = new int[15]; //used for automated difficulty increase in phases 1 and 2 - defaults to all zeros
        
        //
        // //initialize random target rotation (in effect in all phases with target)
        // target_rotation_index = 0;
        // target_y_running_rotations = target_rotation_y_list;
        // target_z_running_rotations = target_rotation_z_list;
        // ShuffleArray(target_y_running_rotations);
        // ShuffleArray(target_z_running_rotations);
        //
        // //initialize list of mouse speeds
        // mouse_speeds = Enumerable.Repeat(0.0f, 30).ToList();
        //
        // //initialize L/R reward scaling
        // left_bonus_threshold = 20; //so large this is never encountered, thus this function defaults to off
        // right_bonus_threshold = 20;
        // bias_bonus = 0;
        //
        // //frame counter for screen recording filename and overlay
        // framenum = 0;
    }

    // public override void OnEpisodeBegin()
    // {
    //     var statsRecorder = Academy.Instance.StatsRecorder;
    //     statsRecorder.Add("Session Performance", (float)session_performance);
    //     
    // }
    // {
    //     // # TODO: take phase 2 into consideration
    //     // trial_index++;
    //     //
    //     // //timeout timer math
    //     // trial_starttime = Time.timeSinceLevelLoad;
    //     // timeout_time = trial_starttime + timeout_duration; //this is time when timeout will occur - scaled realtime in update()
    //     // // Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tn\t" + trial_index.ToString());
    //     //
    //     // //running performance and difficulty scaling
    //     // running_performance = trial_history.Average(); //running performance is average across last n trials (set by trial_history length)
    //     // if (running_performance >= difficulty_threshold && scale_difficulty) //distance scaling - increase distance to target if performance reaches threshold.
    //     // {
    //     //     target_start_pos.x += difficulty_increment;
    //     //     Array.Clear(trial_history, 0, trial_history.Length); //clear array to only consider performance at current difficulty
    //     // }
    //     //
    //     //reset mouse position
    //     
    //     Debug.Log("episode begins!!");
    //     
    //     // transform.position = mouse_start_pos;
    //     // transform.rotation = mouse_start_rot;
    //     
    //     // Reset the mouse position and rotation
    //     transform.position = taskControl.mouse_start_pos;
    //     transform.rotation = taskControl.mouse_start_rot;
    //
    //     // if (taskControl == null)
    //     // {
    //     //     Debug.LogError("taskcontrol is empty");
    //     // }
    //     // else
    //     // {
    //     // taskControl.update();
    //     // }
    //
    //     // Agent
    //     // transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));
    //     // target.transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f))
    // }
    // public override void CollectObservations(VectorSensor sensor)
    // {
    //     
    //     // Debug.Log("collectObservation!!");
    //     sensor.AddObservation(transform.localPosition);
    //     sensor.AddObservation(target.transform.position);
    //     sensor.AddObservation(transform.localRotation);
    //     
    //     // sensor.AddObservation(transform.rotation.z);
    //     // sensor.AddObservation(transform.rotation.x);
    //     
    //     // sensor.AddObservation(target.transform.position - transform.position);// 3
    //     // sensor.AddObservation(rb.angularVelocity);
    //     // sensor.AddObservation(rb.velocity); // 3
    // }
    // public override void CollectObservations(VectorSensor sensor)
    // {
    //     // Debug.Log("collectObservation!!");
    //     sensor.AddObservation(transform.localPosition);
    //     // sensor.AddObservation(target.transform.position);
    //     sensor.AddObservation(transform.localRotation);
    //     
    //     // sensor.AddObservation(transform.rotation.z);
    //     // sensor.AddObservation(transform.rotation.x);
    //     
    
    
    //     // sensor.AddObservation(target.transform.position - transform.position);// 3
    //     // sensor.AddObservation(rb.angularVelocity);
    //     // sensor.AddObservation(rb.velocity); // 3
    // }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float moveX = actionBuffers.ContinuousActions[0]; // Movement along the X axis (left/right)
        float moveZ = actionBuffers.ContinuousActions[1]; // Movement along the Z axis (forward/backward)

        // Rotation actions
        // float rotationX = actionBuffers.ContinuousActions[2]; // Rotation around the X axis (pitch)
        float rotationY = actionBuffers.ContinuousActions[2]; // Rotation around the Y axis (yaw)

        moveX = moveX * gain * Time.deltaTime;
        moveZ = moveZ * gain * Time.deltaTime;

        Vector3 move = new Vector3(moveX, 0.0f, moveZ) ;
        float yangle = rotationY * rotationgain * Time.deltaTime; // Scale rotation according to rotation gain

        // Apply the movement and rotation
        transform.Translate(move, Space.Self);
        transform.Rotate(0.0f, yangle, 0.0f, Space.Self);

        // penalty for each action (including moving and rotation)
        SetReward(-1f);
        // float thr
        // eshold = 0.0001f;
        // if (Mathf.Abs(moveX) > threshold || Mathf.Abs(moveZ) > threshold)
        // {
        //     AddReward(-1f); // penalize for movement
        // }
        //
        // // If there's nontrivial rotation
        // if (Mathf.Abs(rotationY) > threshold)
        // {
        //     AddReward(-1f); // penalize for rotation
        // }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Debug.Log("Heuristic being called");
        var continuousActions = actionsOut.ContinuousActions;
        // Debug.Log(continuousActions.ToString());
        continuousActions[0] = Input.GetAxis("Horizontal"); // X axis (A/D or Left/Right Arrow)
        // Debug.Log(continuousActions[0]);
        continuousActions[1] = Input.GetAxis("Vertical");   // Z axis (W/S or Up/Down Arrow)
        // Debug.Log(continuousActions[1]+"vertical");
        // continuousActions[2] = -Input.GetAxis("Mouse Y");   // Mouse Y axis for pitch
        continuousActions[2] = Input.GetAxis("Mouse X");    // Mouse X axis for yaw
    }
}
