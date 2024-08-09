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
    private GameObject mouse;
    int phase;
    
    Vector3 mouse_start_pos;
    public Vector3 target_start_pos; //target_start_pos.x is what stores the distance, including the circle radius for phase 3
    private Quaternion mouse_start_rot;
    private float targetdistance;
    
    [SerializeField] private GameObject target;

    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody rb;
    Rigidbody mousebody;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        mousebody = mouse.GetComponent<Rigidbody>();
        phase = scene.GetComponent<scenes>().phase;
        targetdistance = (float)scene.GetComponent<scenes>().targetdistance;
        
        trial_history = new int[15]; //used for automated difficulty increase in phases 1 and 2 - defaults to all zeros
        
        //phase 2 adds random position offset to target in z direction 
        if (phase == 2)
        {
            timeout_duration = 5;
            reward_amt = 1;
            destroydecoys();
        }
        
        //initialize random target rotation (in effect in all phases with target)
        target_rotation_index = 0;
        target_y_running_rotations = target_rotation_y_list;
        target_z_running_rotations = target_rotation_z_list;
        ShuffleArray(target_y_running_rotations);
        ShuffleArray(target_z_running_rotations);
        
        //initialize list of mouse speeds
        mouse_speeds = Enumerable.Repeat(0.0f, 30).ToList();

        //initialize L/R reward scaling
        left_bonus_threshold = 20; //so large this is never encountered, thus this function defaults to off
        right_bonus_threshold = 20;
        bias_bonus = 0;

        //frame counter for screen recording filename and overlay
        framenum = 0;
    }

    public override void OnEpisodeBegin()
    {
        //initialize starting mouse and target positions
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
        
        mouse_start_pos = mouse.transform.position; //start_pos variables store reset positions between trials - mouse starts at ~(0,0,0)
        mouse_start_rot = mouse.transform.rotation;
        target_start_pos = new Vector3(targetdistance, 0.5f, 0f); //starting target distance defined by user 
        
        
        // Agent
        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));
        target.transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));
        // }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        // float moveX = actions.ContinuousActions[0];
        // float moveZ = actions.ContinuousActions[1];
        
        // Vector3 velocity = new Vector3(moveX, 0f, moveZ);
        // velocity = velocity.normalized * Time.deltaTime * moveSpeed;
        // transform.localPosition += velocity;
        
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];
        
        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate*moveSpeed, 0f, Space.Self);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
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
    
    void win()
    {
        // Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\th\t" + trial_index.ToString());
        if (trial_missed == false) //if target was the first reached, trial is considered a hit and reward is delivered
        {
            trialoutcome(1);
            deliver_reward();
        }
        else  //if target was reached after a decoy the trial still counts as unsuccessful 
        { 
            trialoutcome(0);
            if (reward_all_hits == true) { deliver_reward(); } //rewarding this anyway is optional
        }
        newtrial(); //target hits always start a new trial
    }
    
    void newtrial()
    {
        trial_index++;

        //timeout timer math
        trial_starttime = Time.timeSinceLevelLoad;
        timeout_time = trial_starttime + timeout_duration; //this is time when timeout will occur - scaled realtime in update()
        // Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tn\t" + trial_index.ToString());

        //running performance and difficulty scaling
        running_performance = trial_history.Average(); //running performance is average across last n trials (set by trial_history length)
        if (running_performance >= difficulty_threshold && scale_difficulty) //distance scaling - increase distance to target if performance reaches threshold.
        {
            target_start_pos.x += difficulty_increment;
            Array.Clear(trial_history, 0, trial_history.Length); //clear array to only consider performance at current difficulty
        }

        //overall performance (for display only)
        if (trials.Count() > 0)
            session_performance = trials.Average();

        //autoreward
        if (autoreward)
            deliver_reward();

        //reset mouse position
        mouse.transform.position = mouse_start_pos;
        mouse.transform.rotation = mouse_start_rot;

        //reset target and decoy positions, deactivate all decoys
        if (target != null)
            target.transform.position = target_start_pos; //reset target position 

        //phase 2 and 3 target movement
        if (phase == 2) //add offsets to target position if phase 2
        {
            float poke = ((UnityEngine.Random.value * 2) - 1) * offset_gain * target_start_pos.x; // target moved L/R randomly up to gain value
            Vector3 temp = target.transform.position; //need to use temporary object here for manual reassignment as transform.positon returns a struct, not a reference
            temp.z += poke;
            target.transform.position = temp;
        }

        //target rotation
        if (phase != 1 && rotate_objects == true) //target rotation deactivated in phase 1 as minimum distance was problematic
        {
            target_rotation_index++;
            if (target_rotation_index > target_y_running_rotations.Length - 1) //target y and z rotation lists are the same length
            {
                ShuffleArray(target_y_running_rotations);
                ShuffleArray(target_z_running_rotations);
                target_rotation_index = 0;
            }
            Quaternion qtemp = target.transform.rotation;
            qtemp.eulerAngles = new Vector3(0f, target_y_running_rotations[target_rotation_index], target_z_running_rotations[target_rotation_index]);
            target.transform.rotation = qtemp;
        }

        //tell decoys to rotate
        if (rotate_objects == true)
        {
            foreach (GameObject decoy in activedecoys)
                decoy.SendMessage("Rotate");
        }

        //chaos trial mode automation
        int stimulus = 0;
        if(stimulus_chaos == true)
        {
            stimulus = Flip_Chaos();
        }

        //reset variable keeping track of whether current trial has been missed (for secondary hit rejection)
        trial_missed = false;

        //increment bonus reward amount if target exceeds L/R thresholds to combat bias
        bias_bonus = 0;
        if (target.transform.position.z < -right_bonus_threshold || target.transform.position.z > left_bonus_threshold) { bias_bonus = bias_bonus_amt; } //allows user to easily set thresholds for each side based on distance to origin

        //log target and decoy positions
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tt\t" + target.transform.position.x.ToString() + "\t" + target.transform.position.y.ToString() + "\t" + target.transform.position.z.ToString() + "\t" + target_y_running_rotations[target_rotation_index].ToString() + "\t" + target_z_running_rotations[target_rotation_index].ToString());
        foreach(GameObject decoy in activedecoys)
            Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\td\t" + decoy.transform.position.x.ToString() + "\t" + decoy.transform.position.y.ToString() + "\t" + decoy.transform.position.z.ToString() + "\t" + decoy.name.ToString().Substring(5,1));

        //log stimulus type as well
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\ts\t" + stimulus.ToString()); //changed to 's' 5/17/24

        //output new trial trigger
        //trialTrigger();
    
    }

}
