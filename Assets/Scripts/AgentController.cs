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
        
    [SerializeField] public float gain = 1.5f;
    
    [SerializeField] public float rotationgain = 10f;
    
    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    Vector3 mouse_start_pos;
    public Vector3 target_start_pos; //target_start_pos.x is what stores the distance, including the circle radius for phase 3
    private Quaternion mouse_start_rot;
    private float targetdistance;
    public double session_performance;

    [SerializeField] private GameObject target;

    // [SerializeField] private float moveSpeed = 4f;

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
        
        
    }

    // public override void OnEpisodeBegin()
    // {
    //     var statsRecorder = Academy.Instance.StatsRecorder;
    //     statsRecorder.Add("Session Performance", (float)session_performance);
    //     
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

        Vector3 move = new Vector3(moveX, 0.0f, moveZ);
        float yangle = rotationY * rotationgain * Time.deltaTime; // Scale rotation according to rotation gain

        // Apply the movement and rotation
        transform.Translate(move, Space.Self);
        transform.Rotate(0.0f, yangle, 0.0f, Space.Self);

        // penalty for each action (including moving and rotation)
        SetReward(-1f);

        //RequestDecision();
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
