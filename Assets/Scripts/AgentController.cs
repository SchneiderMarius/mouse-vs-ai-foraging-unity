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
    public float rotationgain = 250f;
    
    public float gain = 15;
    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;
    
    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    public bool useVecObs;
    
    Vector3 mouse_start_pos;
    public Vector3 target_start_pos; //target_start_pos.x is what stores the distance, including the circle radius for phase 3
    private Quaternion mouse_start_rot;
    private float targetdistance;
    
    // private float trial_starttime; //container for start time of individual trials
    // public float timeout_time; //this is time when timeout will occur - update() slides this around
    // public float timeout_duration = 5;
    // public int reward_amt = 1;
    //
    // public int trial_index = 0;
    // int[] trial_history; //container for last few trial outcomes used for running performance calculation
    // public float difficulty_increment;
    // double difficulty_threshold = 0.7; //70% running performance required to increase target distance
    // public bool scale_difficulty;
    // public double running_performance;
    //
    // public float offset_gain = 0.5f;

    
    [SerializeField] private GameObject target;

    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody rb;
    // Rigidbody mousebody;
    
    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        
        Debug.Log("initialize!!");
        rb = GetComponent<Rigidbody>();
        
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        scene = GameObject.Find("Scenemanager");
        target = GameObject.Find("target");

        taskControl = GameObject.Find("task").GetComponent<task_control>();
        
        if (rb == null)
            Debug.LogError("mouseRigidbody is null!");
        if (target == null)
            Debug.LogError("target is null!");
        if (taskControl == null)
            Debug.LogError("taskControl is null!");
        
        // // difficulty_increment = (float)scene.GetComponent<scenes>().difficulty_increment;
        // difficulty_increment = 1;
        // // scale_difficulty = scene.GetComponent<scenes>().scale_difficulty;
        // scale_difficulty = true;
        // // targetdistance = (float)scene.GetComponent<scenes>().targetdistance;
        // targetdistance = 2;
        //
        // timeout_duration = 5;
        // reward_amt = 1;
        // // destroydecoys();
        // running_performance = 0;
        
        //initialize starting mouse and target positions
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
        
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
    public override void CollectObservations(VectorSensor sensor)
    {
        
        Debug.Log("collectObservation!!");
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.transform.position);
        
        // sensor.AddObservation(transform.rotation.z);
        // sensor.AddObservation(transform.rotation.x);
        sensor.AddObservation(transform.localRotation);
        
        // sensor.AddObservation(target.transform.position - transform.position);// 3
        // sensor.AddObservation(rb.angularVelocity);
        // sensor.AddObservation(rb.velocity); // 3
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // float moveX = actions.ContinuousActions[0];
        // float moveZ = actions.ContinuousActions[1];
        
        // Vector3 velocity = new Vector3(moveX, 0f, moveZ);
        // velocity = velocity.normalized * Time.deltaTime * moveSpeed;
        // transform.localPosition += velocity;
        Debug.Log("OnActionReceived!!");

        float moveX = actionBuffers.ContinuousActions[0]; // Movement along the X axis (left/right)
        float moveZ = actionBuffers.ContinuousActions[1]; // Movement along the Z axis (forward/backward)

        // moveZ = Mathf.Clamp(moveZ, 0, 1); // Only allow forward movement (0 = no movement, 1 = full forward)

        // Rotation actions
        // float rotationX = actionBuffers.ContinuousActions[2]; // Rotation around the X axis (pitch)
        float rotationY = actionBuffers.ContinuousActions[2]; // Rotation around the Y axis (yaw)

        Vector3 move = new Vector3(moveX /127, 0.0f, moveZ/127);
        float yangle = rotationY * rotationgain; // Scale rotation according to rotation gain

        // Apply the movement and rotation
        transform.Translate(move * gain, Space.Self);
        transform.Rotate(0.0f, yangle / rotationgain, 0.0f, Space.Self);
        
        // Debug.Log("actionReceived" + moveX + "  " + moveZ + "  " + rotationX + "   " + rotationY);
        // Debug.Log("actionReceived" + moveX + "  " + moveZ + "  " + rotationX);
        Debug.Log("actionReceived" + moveX + "  " + moveZ + "  " + rotationY);

        // Apply movement forces
        // rb.AddRelativeForce(Vector3.right * moveX * gain);
        // rb.AddRelativeForce(Vector3.forward * moveZ * gain);

        // Apply rotation
        // rotY += rotationY * mouseSensitivity * Time.deltaTime;
        // rotX += rotationX * mouseSensitivity * Time.deltaTime;
        // rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        // Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        // Quaternion localRotation = Quaternion.Euler(rotX, 0.0f, 0.0f);
        // Quaternion localRotation = Quaternion.Euler(0.0f, rotY, 0.0f);
        // transform.rotation = localRotation;
        
        // # TODO: how to set the penalty?(everytime receive a action including moving and rotation? or just the position changes)
        // # TODO: reward and penalty
        
        // penalty for each action (including moving and rotation)
        SetReward(-1f);
        
        // float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        // if (distanceToTarget < taskControl.timeout_travel_threshold)
        // {
        //     SetReward(10f); // Reward the agent for getting close to the target
        //     EndEpisode();
        //     taskControl.win(); // Call the win function to deliver the reward and reset for a new trial
        //     
        // }
        // else if (Time.timeSinceLevelLoad > taskControl.timeout_time)
        // {
        //     // Penalize the agent for timing out
        //     SetReward(-5f);
        //     taskControl.timeout(); // Handle timeout behavior
        //     EndEpisode();
        // }
        
        // # TODO: take timeout into consideration
        // framenum += 1;
        //
        // //start blackout on user command
        // if (start_blackout)
        // {
        //     blackout(blackout_duration);
        //     start_blackout = false;
        // }
        //
        // //calculate avg mouse speed (used for timeout management in phases 2+)
        // mouse_speeds.Add(mouse.GetComponent<ballmove>().move.magnitude);
        // mouse_speeds.RemoveAt(0);
        // mouse_avg_speed = mouse_speeds.Average();
        //
        // //timeout and timeout scaling
        // if (phase > 0)
        // {
        //     mouse_at_origin = Vector3.Distance(mouse.transform.position, new Vector3(0.0f, 0.5f, 0.0f)) < timeout_travel_threshold; //if mouse is stationary within certain distance of origin
        //     Vector3 viewPos = cam.WorldToViewportPoint(target.transform.position);
        //     target_in_fov = viewPos.x < 1 && viewPos.x > 0 && viewPos.y < 1 && viewPos.y > 0; //if target visible to mouse camera (checking if target renderer is active is sensitive to scene cam in unity editor)
        //     float time_increment = Time.deltaTime * timeout_scaling_gain; //now timeout scaling gain is fraction of full time delay since last frame
        //     if (phase < 3)
        //     {
        //         timeout_time += time_increment * 0.5f * ((mouse_at_origin ? 1 : 0) + (target_in_fov ? 1 : 0)); //adds more time to trial if mouse at origin and/or when target in FOV. 0.5 caps maximum time addition to full increment (stops time).
        //     }
        //     else
        //     {
        //         timeout_time += time_increment * 1f * (mouse_at_origin ? 1 : 0); //all other phase timeouts only depend on mouse at origin.
        //     }
        //
        //     //timeout if timeout was reached
        //     if (Time.timeSinceLevelLoad > timeout_time)
        //     {
        //         if (newtrial_requires_stop == false)
        //             timeout();
        //         else if (mouse_avg_speed < 0.1)
        //             timeout();
        //     }
        // }
        

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Debug.Log("Heuristic being called");
        var continuousActions = actionsOut.ContinuousActions;
        // Debug.Log(continuousActions.ToString());
        continuousActions[0] = Input.GetAxis("Horizontal"); // X axis (A/D or Left/Right Arrow)
        // Debug.Log(continuousActions[0]);
        continuousActions[1] = Input.GetAxis("Vertical");   // Z axis (W/S or Up/Down Arrow)
        // Debug.Log(continuousActions[1]+"vertical");

        // continuousActions[2] = -Input.GetAxis("Mouse Y");   // Mouse Y axis for pitch
        continuousActions[2] = Input.GetAxis("Mouse X");    // Mouse X axis for yaw

    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.gameObject.tag == "Pellet")
    //     {
    //         AddReward(10f);
    //         EndEpisode();
    //     }
    //     if (other.gameObject.tag == "Wall")
    //     {
    //         AddReward(-5f);
    //         EndEpisode();
    //     }
    //     
    // }
    
    // void destroydecoys()
    // {
    //     GameObject[] decoys = GameObject.FindGameObjectsWithTag("decoys");
    //     foreach (GameObject decoy in decoys)
    //         GameObject.Destroy(decoy);
    //     // Debug.Log("Found object: " + decoy.name);
    // }
    // void timeout()
    // {
    //     // Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tf\t" + trial_index.ToString());
    //     trialoutcome(0);
    //     // if(phase > 0)
    //     //     beep.Play();
    //     EndEpisode();
    // }
    //
    // void trialoutcome(int outcome)
    // {
    //     Array.Copy(trial_history, 1, trial_history, 0, trial_history.Length - 1);
    //     trial_history[trial_history.Length - 1] = outcome;
    //     // trials.Add(outcome);
    // }
    // void win()
    // {
    //     // Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\th\t" + trial_index.ToString());
    //     if (trial_missed == false) //if target was the first reached, trial is considered a hit and reward is delivered
    //     {
    //         trialoutcome(1);
    //         deliver_reward();
    //     }
    //     else  //if target was reached after a decoy the trial still counts as unsuccessful 
    //     { 
    //         trialoutcome(0);
    //         if (reward_all_hits == true) { deliver_reward(); } //rewarding this anyway is optional
    //     }
    //     newtrial(); //target hits always start a new trial
    // }
    //
    // void newtrial()
    // {
    //     trial_index++;
    //
    //     //timeout timer math
    //     trial_starttime = Time.timeSinceLevelLoad;
    //     timeout_time = trial_starttime + timeout_duration; //this is time when timeout will occur - scaled realtime in update()
    //     // Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tn\t" + trial_index.ToString());
    //
    //     //running performance and difficulty scaling
    //     running_performance = trial_history.Average(); //running performance is average across last n trials (set by trial_history length)
    //     if (running_performance >= difficulty_threshold && scale_difficulty) //distance scaling - increase distance to target if performance reaches threshold.
    //     {
    //         target_start_pos.x += difficulty_increment;
    //         Array.Clear(trial_history, 0, trial_history.Length); //clear array to only consider performance at current difficulty
    //     }
    //
    //     //overall performance (for display only)
    //     if (trials.Count() > 0)
    //         session_performance = trials.Average();
    //
    //     //autoreward
    //     if (autoreward)
    //         deliver_reward();
    //
    //     //reset mouse position
    //     mouse.transform.position = mouse_start_pos;
    //     mouse.transform.rotation = mouse_start_rot;
    //
    //     //reset target and decoy positions, deactivate all decoys
    //     if (target != null)
    //         target.transform.position = target_start_pos; //reset target position 
    //
    //     //phase 2 and 3 target movement
    //     if (phase == 2) //add offsets to target position if phase 2
    //     {
    //         float poke = ((UnityEngine.Random.value * 2) - 1) * offset_gain * target_start_pos.x; // target moved L/R randomly up to gain value
    //         Vector3 temp = target.transform.position; //need to use temporary object here for manual reassignment as transform.positon returns a struct, not a reference
    //         temp.z += poke;
    //         target.transform.position = temp;
    //     }
    //
    //     //target rotation
    //     if (phase != 1 && rotate_objects == true) //target rotation deactivated in phase 1 as minimum distance was problematic
    //     {
    //         target_rotation_index++;
    //         if (target_rotation_index > target_y_running_rotations.Length - 1) //target y and z rotation lists are the same length
    //         {
    //             ShuffleArray(target_y_running_rotations);
    //             ShuffleArray(target_z_running_rotations);
    //             target_rotation_index = 0;
    //         }
    //         Quaternion qtemp = target.transform.rotation;
    //         qtemp.eulerAngles = new Vector3(0f, target_y_running_rotations[target_rotation_index], target_z_running_rotations[target_rotation_index]);
    //         target.transform.rotation = qtemp;
    //     }
    //
    //     //tell decoys to rotate
    //     if (rotate_objects == true)
    //     {
    //         foreach (GameObject decoy in activedecoys)
    //             decoy.SendMessage("Rotate");
    //     }
    //
    //     //chaos trial mode automation
    //     int stimulus = 0;
    //     if(stimulus_chaos == true)
    //     {
    //         stimulus = Flip_Chaos();
    //     }
    //
    //     //reset variable keeping track of whether current trial has been missed (for secondary hit rejection)
    //     trial_missed = false;
    //
    //     //increment bonus reward amount if target exceeds L/R thresholds to combat bias
    //     bias_bonus = 0;
    //     if (target.transform.position.z < -right_bonus_threshold || target.transform.position.z > left_bonus_threshold) { bias_bonus = bias_bonus_amt; } //allows user to easily set thresholds for each side based on distance to origin
    //
    //     //log target and decoy positions
    //     Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tt\t" + target.transform.position.x.ToString() + "\t" + target.transform.position.y.ToString() + "\t" + target.transform.position.z.ToString() + "\t" + target_y_running_rotations[target_rotation_index].ToString() + "\t" + target_z_running_rotations[target_rotation_index].ToString());
    //     foreach(GameObject decoy in activedecoys)
    //         Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\td\t" + decoy.transform.position.x.ToString() + "\t" + decoy.transform.position.y.ToString() + "\t" + decoy.transform.position.z.ToString() + "\t" + decoy.name.ToString().Substring(5,1));
    //
    //     //log stimulus type as well
    //     Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\ts\t" + stimulus.ToString()); //changed to 's' 5/17/24
    //
    //     //output new trial trigger
    //     //trialTrigger();
    //
    // }

}
