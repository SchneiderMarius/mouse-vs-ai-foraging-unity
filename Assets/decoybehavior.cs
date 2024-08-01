using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class decoybehavior : MonoBehaviour
{
    private GameObject task;
    public int[] target_rotation_y_list = { -180, -150, -120, -90, -60, -30, 0 }; //list of all possible out-of-plane target rotations
    public int[] target_rotation_z_list = { -90, -60, -30, 0, 30, 60, 90 };  //list of all possible in-plane target rotations
    private int[] target_y_running_rotations;
    private int[] target_z_running_rotations;
    private int target_rotation_index;
    private bool triggered;

    // Start is called before the first frame update
    void Start()
    {
        task = GameObject.Find("task");

        //initialize random target rotation (in effect in all phases with target)
        target_rotation_index = 0;
        target_y_running_rotations = target_rotation_y_list;
        target_z_running_rotations = target_rotation_z_list;
        ShuffleArray(target_y_running_rotations);
        ShuffleArray(target_z_running_rotations);

        triggered = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered == false)
        {
            task.SendMessage("miss", name.Substring(5, 1));
            triggered = true;
        }
    }

    void Rotate()
    {
        target_rotation_index++;
        if (target_rotation_index > target_y_running_rotations.Length - 1) //target y and z rotation lists are the same length
        {
            ShuffleArray(target_y_running_rotations);
            ShuffleArray(target_z_running_rotations);
            target_rotation_index = 0;
        }
        Quaternion qtemp = this.transform.rotation;
        qtemp.eulerAngles = new Vector3(0f, target_y_running_rotations[target_rotation_index], target_z_running_rotations[target_rotation_index]);
        this.transform.rotation = qtemp;
    }

    private void OnEnable()
    {
        GetComponent<Collider>().isTrigger = true;
        triggered = false;
    }

    void ShuffleArray<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int r = UnityEngine.Random.Range(0, i + 1);
            T tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }
    }
}
