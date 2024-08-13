using System.Collections;
using UnityEngine;
using System.IO;
using System.IO.Ports;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class task_control : MonoBehaviour
{
    private GameObject scene;
    private GameObject target;
    private GameObject mouse;
    public GameObject mousecam;
    public GameObject mousecam_L;
    public GameObject mousecam_R;
    public GameObject audioobject;
    public GameObject sunobject;
    Light sun;
    private float baseline_brightness;
    AudioSource beep;
    string logpath = "C:/Users/Joe Canzano/Documents/Unity Projects/2D go to target v1/logtests/defaultlog.txt"; //defaults to a test directory; is updated to user input game is run from title scene
    int phase; //1,2,3 are the same, 4 is 2R, 5 is 3R.
    public int rewardfreq = 0;
    private float delay = 0;
    public StreamWriter log;
    public int reward_amt = 1; //works with hardware for any positive value between 1 and 10.
    int reward_delivered;
    public Vector3 mouse_start_pos;
    public Vector3 target_start_pos; //target_start_pos.x is what stores the distance, including the circle radius for phase 3
    private float targetdistance;
    public Quaternion mouse_start_rot;
    public int trial_index = 0;
    public float offset_gain = 0.5f;
    public float difficulty_increment;
    string lickcom = "";
    //SerialPort lickport = new SerialPort();
    public int reward_given = 0;
    public float timeout_duration = 5; //this is default trial duration before fail
    public float timeout_time; //this is time when timeout will occur - update() slides this around
    private float trial_starttime; //container for start time of individual trials
    int[] trial_history; //container for last few trial outcomes used for running performance calculation
    public float timeout_travel_threshold = 0.5f;
    float timeout_scaling_gain = 1; //used for timeout scaling, gain is fraction of full time to add back (1 means time is stopped, 0 is unaffected)
    bool mouse_at_origin;
    bool target_in_fov;
    public double running_performance;
    public bool scale_difficulty;
    double difficulty_threshold = 0.7; //70% running performance required to increase target distance
    bool autoreward = false;
    private int[] phase3_rotation_list = { 0, 45, 90, 135, 180, 225, 270, 315 }; //list of all possible rotations for phase 3
    int[] phase3_running_rotations; //shuffled list of rotations used to keep trial distribution uniform
    private int running_rotation_index;
    public int[] target_rotation_y_list = { -180, -150, -120, -90, -60, -30, 0 }; //list of all possible out-of-plane target rotations
    public int[] target_rotation_z_list = { -90, -60, -30, 0, 30, 60, 90 };  //list of all possible in-plane target rotations
    int[] target_y_running_rotations;
    int[] target_z_running_rotations;
    private int target_rotation_index;
    int[] phase2r_running_target_position; //keeps track of target L/R position for window of trials to enforce uniform distribution
    int phase2r_target_position_index;
    int num_decoys;
    int[] running_decoylist; //determines which decoys are randomly drawn for each trial to enforce uniform frequency
    int decoylist_index;
    int[] phase3r_rotation_list = { 0, 90, 180, 270 }; //phase 3R gets only 4 rotation slots so one object is visible at once
    private GameObject[] decoys; //stores reference to all decoys
    List<GameObject> activedecoys = new List<GameObject>(); //stores reference to currently active decoys
    public bool rotate_objects;
    bool newtrial_on_miss;
    public double session_performance;
    List<int> trials = new List<int>(); //holds all trial outcomes for the session for realtime display
    bool trial_missed;
    public float left_bonus_threshold, right_bonus_threshold; //used to scale rewards with L/R difficulty and discourage bias
    int bias_bonus; //used to store L/R bonus amount
    public int bias_bonus_amt; //amount for bonus rewards
    public bool reward_all_hits;
    List<float> mouse_speeds = new List<float>(); //holds recent mouse speeds for running avg calculation
    public float mouse_avg_speed; //holds average mouse speed used for newtrial() in phases 2+
    Rigidbody mousebody;
    public bool newtrial_requires_stop; //optional requirement for mouse to stop running before new trial starts 
    public bool recordtoggle;
    string screenrecordpath = "";
    string framepath = "";
    public int framenum;
    public bool start_blackout;
    public float blackout_duration;
    public float blackout_frequency; //in seconds, +/- 50% this value from uniform distribution
    public bool random_blackouts_activated;
    public int total_blackouts;
    Camera cam;
    Camera cam_L;
    Camera cam_R;
    public RawImage blackout_img;
    public RawImage blackout_img_L;
    public RawImage blackout_img_R;
    public bool stimulus_chaos;
    int[] running_chaos; //shuffled list of stimuli used to keep trial distribution uniform
    private int running_chaos_index;
    public GameObject fog, blobs, terrain, outerterrain, RDKmask;
    Terrain[] outerterrains;
    public bool chaos_blackouts; //toggles blob and fog added to blackouts
    public float blobs_blackout_prob; //probability of randomly switching stimulus to blobs. 
    public float RDK_blackout_prob; //same for fog. fog + blob + normal blackouts = 1 when chaos blackouts activated.
    public bool lickport_initialized;
    public bool manual_lickport_reset;

    void Start()
    {
        //get references etc
        scene = GameObject.Find("Scenemanager");
        target = GameObject.Find("target");
        mouse = GameObject.Find("Mouse");
        decoys = GameObject.FindGameObjectsWithTag("decoys");
        logpath = scene.GetComponent<scenes>().logdirectory;
        phase = scene.GetComponent<scenes>().phase;
        rewardfreq = scene.GetComponent<scenes>().phase0delay;
        targetdistance = (float)scene.GetComponent<scenes>().targetdistance;
        scale_difficulty = scene.GetComponent<scenes>().scale_difficulty;
        difficulty_increment = (float)scene.GetComponent<scenes>().difficulty_increment;
        num_decoys = scene.GetComponent<scenes>().num_decoys;
        cam = mousecam.GetComponent<Camera>();
        cam_L = mousecam_L.GetComponent<Camera>();
        cam_R = mousecam_R.GetComponent<Camera>();
        blackout_img = blackout_img.GetComponent<RawImage>();
        blackout_img_L = blackout_img_L.GetComponent<RawImage>();
        blackout_img_R = blackout_img_R.GetComponent<RawImage>();

        beep = audioobject.GetComponent<AudioSource>();
        mousebody = mouse.GetComponent<Rigidbody>();
        sun = sunobject.GetComponent<Light>();
        baseline_brightness = sun.intensity;
        screenrecordpath = scene.GetComponent<scenes>().recordingdirectory;
        recordtoggle = scene.GetComponent<scenes>().recordtoggle;

        //initialize starting mouse and target positions
        mouse_start_pos = mouse.transform.position; //start_pos variables store reset positions between trials - mouse starts at ~(0,0,0)
        mouse_start_rot = mouse.transform.rotation;
        target_start_pos = new Vector3(targetdistance, 0.5f, 0f); //starting target distance defined by user 

        //setup lickport io
        lickcom = scene.GetComponent<scenes>().rewardcom;
        //lickport.BaudRate = 9600;
        //lickport.WriteTimeout = 2000;
        //lickport.ReadTimeout = 2000;
        //lickport.PortName = lickcom;
        //try
        //{
        //    lickport.Open();
        //    Debug.Log("Serial port for lickport open");
        //}
        //catch (Exception e) { Debug.Log("Couldn't open lickport! " + e.Message); }
        lickport_initialized = false; //keeps track of whether lickport is actively working. If it fails, this briefly turns false to prevent it from reconnecting multiple times.
        //StartCoroutine(licklogger()); //lick logging is asynchronous 
        manual_lickport_reset = false;

        //set up logging and prints startup stuff
        if (File.Exists(logpath))
        {
            Debug.Log("File already exists! Appending with numeric.");
            logpath += "1";
        }
        log = new StreamWriter(logpath);
        Write_log((string)"Starting new session " + DateTime.Now);
        Write_log((string)"phase:" + phase);
        Write_log((string)"phase 0 reward frequency:" + rewardfreq);
        Write_log((string)"Log format is\nSessionTime(seconds)\tEventType\t(when relevant)x\ty\tz\tr\t");
        Write_log((string)"Event types: p=mouseposition \t u=user initiated reward \t r=reward \t n=new trial \t t=target position \t h=target hit \t d=decoy \t m=miss \t e=session ended");
        trial_history = new int[15]; //used for automated difficulty increase in phases 1 and 2 - defaults to all zeros
        running_performance = 0;
        session_performance = 0;

        //phase 0 removes target and delivers reward at random intervals
        if (phase == 0)
        {
            reward_amt = 2;
            Destroy(target);
            destroydecoys();
            Write_log((string)"phase 0 - target hidden");
            StartCoroutine(rewardRandomizer()); //reward delivery is random and asynchronous 
        }

        //phase 1 spawns target in fixed position requiring no change here
        if (phase == 1)
        {
            timeout_duration = 5;
            reward_amt = 1;
            destroydecoys();
        }

        //phase 2 adds random position offset to target in z direction 
        if (phase == 2)
        {
            timeout_duration = 5;
            reward_amt = 1;
            destroydecoys();
        }

        //phase 3 spawns target on a circle of radius target_start_pos.x around the spawn point
        if (phase == 3)
        {
            reward_amt = 3;
            timeout_duration = 60;
            running_rotation_index = 0;
            phase3_running_rotations = phase3_rotation_list;
            ShuffleArray(phase3_running_rotations);
            destroydecoys();
        }

        //phase 4 here is phase 2R. Similar to phase 2, but there's only three potential target positions (L/M/R) and there's decoys now.
        if (phase == 4)
        {
            timeout_duration = 20;
            reward_amt = 3;

            //shuffle target L/R position list to enforce uniform distribution
            if (num_decoys == 1)
            {
                int[] temp = { 1, -1, 1, -1, 1, -1, 1, -1 }; //two positions if 1 decoy
                phase2r_running_target_position = temp; //window is size 8
                ShuffleArray(phase2r_running_target_position);
            }
            if (num_decoys == 2)
            {
                int[] temp = { -1, 0, 1, -1, 0, 1, -1, 0, 1 }; //3 positions if 2 decoys
                phase2r_running_target_position = temp; //window is size 9
                ShuffleArray(phase2r_running_target_position);
            }
            phase2r_target_position_index = 0;

            offset_gain = offset_gain * 0.85f; //max offset is smaller in 2R so side trials don't require extreme turns
        }

        //phase 5 here is phase 3R. Phase 3, but there's decoys now.
        if (phase == 5)
        {
            timeout_duration = 60;
            reward_amt = 3;
            running_rotation_index = 0;
            phase3_running_rotations = phase3r_rotation_list; //smaller list used
            ShuffleArray(phase3_running_rotations);
        }

        //initialize decoylist to keep track of frequency each is shown
        if (phase == 4 || phase == 5)
        {
            int[] decoynums = { 0, 1, 2, 0, 1, 2, 0, 1, 2 };
            running_decoylist = decoynums; //uniform sampling of decoys is enforced every 9 trials
            ShuffleArray(running_decoylist);
            decoylist_index = 0;
        }

        //initialize stimulus chaos (can be activated during session without issue)
        int[] chaosnums = { 0, 1, 2, 4, 5, 0, 1, 2, 4, 5 }; //all possible stimulus types. From 0: normal, fog, clutter, contrastmod w/o terrain.
        running_chaos = chaosnums; //uniform sampling of stimuli enforced every 8 trials
        ShuffleArray(running_chaos);
        running_chaos_index = 0;

        //initialize random target rotation (in effect in all phases with target)
        target_rotation_index = 0;
        target_y_running_rotations = target_rotation_y_list;
        target_z_running_rotations = target_rotation_z_list;
        ShuffleArray(target_y_running_rotations);
        ShuffleArray(target_z_running_rotations);

        //initalize a few optional parameters
        rotate_objects = false; //object rotation off by default, activate manually once mouse ready
        newtrial_on_miss = false; //miss() does not start a new trial by default
        reward_all_hits = false; //option to reward secondary hits on the target after decoys are reached first. Deactivate for cheating mice.
        newtrial_requires_stop = false; //option to require mouse to stop before newtrial()

        //initialize variable keeping track of whether a miss event has occured in 2R or 3R
        trial_missed = false;

        //initialize list of mouse speeds
        mouse_speeds = Enumerable.Repeat(0.0f, 30).ToList();

        //initialize L/R reward scaling
        left_bonus_threshold = 20; //so large this is never encountered, thus this function defaults to off
        right_bonus_threshold = 20;
        bias_bonus = 0;

        //frame counter for screen recording filename and overlay
        framenum = 0;

        //initialize blackout variables
        start_blackout = false;
        random_blackouts_activated = false;
        total_blackouts = 0;
        StartCoroutine(blackoutRandomizer());

        //initialize array of outer terrain objects needed for stimulus chaos mode
        outerterrains = outerterrain.GetComponentsInChildren<Terrain>();

        //begin first trial
        Write_log((string)"Time zero at: " + DateTime.Now.ToString("HH:mm:ss.fff"));
        // newtrial();
    }

    void Update()
    {
        framenum += 1;

        //start blackout on user command
        if (start_blackout)
        {
            blackout(blackout_duration);
            start_blackout = false;
        }

        //calculate avg mouse speed (used for timeout management in phases 2+)
        mouse_speeds.Add(mouse.GetComponent<ballmove>().move.magnitude);
        mouse_speeds.RemoveAt(0);
        mouse_avg_speed = mouse_speeds.Average();

        //timeout and timeout scaling
        if (phase > 0)
        {
            mouse_at_origin = Vector3.Distance(mouse.transform.position, new Vector3(0.0f, 0.5f, 0.0f)) < timeout_travel_threshold; //if mouse is stationary within certain distance of origin
            Vector3 viewPos = cam.WorldToViewportPoint(target.transform.position);
            target_in_fov = viewPos.x < 1 && viewPos.x > 0 && viewPos.y < 1 && viewPos.y > 0; //if target visible to mouse camera (checking if target renderer is active is sensitive to scene cam in unity editor)
            float time_increment = Time.deltaTime * timeout_scaling_gain; //now timeout scaling gain is fraction of full time delay since last frame
            if (phase < 3)
            {
                timeout_time += time_increment * 0.5f * ((mouse_at_origin ? 1 : 0) + (target_in_fov ? 1 : 0)); //adds more time to trial if mouse at origin and/or when target in FOV. 0.5 caps maximum time addition to full increment (stops time).
            }
            else
            {
                timeout_time += time_increment * 1f * (mouse_at_origin ? 1 : 0); //all other phase timeouts only depend on mouse at origin.
            }

            //timeout if timeout was reached
            if (Time.timeSinceLevelLoad > timeout_time)
            {
                if (newtrial_requires_stop == false)
                    timeout();
                else if (mouse_avg_speed < 0.1)
                    timeout();
            }
        }

        //keyboard inputs
        if (Input.GetKeyDown(KeyCode.R)) //manual reward delivery
        {
            Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tu\t");
            deliver_reward(manual:true);
        }

        if(manual_lickport_reset)
        {
            manual_lickport_reset = false;
            //reset_lickport();
        }

    }

    void LateUpdate()
    {
        if(recordtoggle)
        {
            framepath = Path.Combine(screenrecordpath, framenum.ToString() + " " + DateTime.Now.ToString("HH:mm:ss.fff") + ".png");
            ScreenCapture.CaptureScreenshot(framepath);
        }

    }

    //functions
    public void win()
    {
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\th\t" + trial_index.ToString());
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

    public void timeout()
    {
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tf\t" + trial_index.ToString());
        trialoutcome(0);
        if(phase > 0)
            beep.Play();
        newtrial(); 
    }

    public void miss(string decoy)
    {
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tm\t" + trial_index.ToString() + "\t" + decoy);
        //timeout() and hit() ultimately call trialoutcome(0), so this function doesn't
        beep.Play();
        StartCoroutine(flash());
        trial_missed = true;
        if (newtrial_on_miss == true) //keeping this false (default) enables essentially a timeout punishment
            newtrial();
    }

    public void newtrial()
    {
        trial_index++;

        //timeout timer math
        trial_starttime = Time.timeSinceLevelLoad;
        timeout_time = trial_starttime + timeout_duration; //this is time when timeout will occur - scaled realtime in update()
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tn\t" + trial_index.ToString());

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
        if (phase == 4 || phase == 5)
        {
            foreach (GameObject decoy in decoys)
            {
                decoy.transform.position = target_start_pos; //reset position of all decoys (these are subsequently moved)
                decoy.SetActive(false);
            }
            activedecoys.Clear();
        }

        //phase 2 and 3 target movement
        if (phase == 2) //add offsets to target position if phase 2
        {
            float poke = ((UnityEngine.Random.value * 2) - 1) * offset_gain * target_start_pos.x; // target moved L/R randomly up to gain value
            Vector3 temp = target.transform.position; //need to use temporary object here for manual reassignment as transform.positon returns a struct, not a reference
            temp.z += poke;
            target.transform.position = temp;
        }
        if (phase == 3) //move target to circular position if phase 3
        {
            running_rotation_index++;
            float radius = target_start_pos.x;
            if (running_rotation_index > phase3_running_rotations.Length - 1)
            {
                ShuffleArray(phase3_running_rotations);
                running_rotation_index = 0;
            }
            int rot = phase3_running_rotations[running_rotation_index];
            Vector3 temp = target.transform.position; //need to use temporary object here for manual reassignment as transform.positon returns a struct, not a reference
            temp.x = radius * (float)Math.Sin(rot * Math.PI / 180);
            temp.z = radius * (float)Math.Cos(rot * Math.PI / 180);
            target.transform.position = temp;
        }

        //select decoys for next trial: uniform selection between all classes is enforced in all cases
        if (phase == 4 || phase == 5)
        {
            //if only one decoy, decoylist is used to determine the active decoy
            if (num_decoys == 1)
            {
                if (decoylist_index > running_decoylist.Length - 1)
                {
                    ShuffleArray(running_decoylist);
                    decoylist_index = 0;
                }
                decoys[running_decoylist[decoylist_index]].SetActive(true);
                activedecoys.Add(decoys[running_decoylist[decoylist_index]]); //decoys are added to the active list in random order
                decoylist_index++;
            }

            //if 2 decoys, decoylist determines the inactive decoy (thus all are omitted with same frequency)
            if (num_decoys == 2)
            {
                if (decoylist_index > running_decoylist.Length - 1)
                {
                    ShuffleArray(running_decoylist);
                    decoylist_index = 0;
                }
                foreach (GameObject decoy in decoys) //all decoys activated and added to the list
                {
                    decoy.SetActive(true);
                    activedecoys.Add(decoy);
                }
                decoys[running_decoylist[decoylist_index]].SetActive(false); //chosen decoy is removed
                activedecoys.Remove(decoys[running_decoylist[decoylist_index]]); //and removed from the active list
                ShuffleList(activedecoys); //list is shuffled to ensure random decoy placement
                decoylist_index++;
            }

            //if 3 decoys, all are present in every trial (but the list order is shuffled)
            if (num_decoys == 3)
            {
                foreach (GameObject decoy in decoys) //all decoys activated and added to the list
                {
                    decoy.SetActive(true);
                    activedecoys.Add(decoy);
                }
                ShuffleList(activedecoys);
            }

        }

        //phase 2R target and decoy movement
        if (phase == 4)
        {
            //TARGET MOVEMENT 
            //L/R position set to +/- phase 2 maximum, distribution is forced to uniform every 8 or 9 trials dep on # decoys             
            if (phase2r_target_position_index > phase2r_running_target_position.Length - 1)
            {
                ShuffleArray(phase2r_running_target_position);
                phase2r_target_position_index = 0;
            }
            int tarpos = phase2r_running_target_position[phase2r_target_position_index]; //either -1, 0, or 1
            float poke = tarpos * offset_gain * target_start_pos.x;
            Vector3 temp = target.transform.position;
            temp.z += poke;
            target.transform.position = temp;

            //DECOY MOVEMENT
            //when only L/R positions are available, the decoy is set to opposite the target position
            if (num_decoys == 1)
            {
                temp.z = temp.z * -1;
                activedecoys[0].transform.position = temp;
            }
            //when L/Middle/R are available, decoys are randomly assigned to those not occupied by the target
            if (num_decoys == 2)
            {
                List<int> positions = new List<int>() { -1, 0, 1 }; //create list of all available positions
                positions.RemoveAll(position => position == tarpos); //remove target position from list

                foreach (GameObject decoy in activedecoys) //assign each (randomly ordered) activedecoy one available position
                {
                    temp.z = (float)positions[0] * offset_gain * target_start_pos.x;
                    decoy.transform.position = temp;
                    positions.RemoveAt(0);
                }
            }
            phase2r_target_position_index++; //iterated last as the decoy positions depend on the target position
        }

        //phase 3R target and decoy movement
        if (phase == 5)
        {
            //TARGET MOVEMENT
            float radius = target_start_pos.x;
            if (running_rotation_index > phase3_running_rotations.Length - 1)
            {
                ShuffleArray(phase3_running_rotations);
                running_rotation_index = 0;
            }
            int tarpos = phase3_running_rotations[running_rotation_index];
            Vector3 temp = target.transform.position; //need to use temporary object here for manual reassignment as transform.positon returns a struct, not a reference
            temp.x = radius * (float)Math.Sin(tarpos * Math.PI / 180);
            temp.z = radius * (float)Math.Cos(tarpos * Math.PI / 180);
            target.transform.position = temp;

            //DECOY MOVEMENT        
            List<int> positions = phase3r_rotation_list.ToList();
            positions.RemoveAll(position => position == tarpos); //remove target position from list
            foreach (GameObject decoy in activedecoys) //assign each (randomly ordered) activedecoy one available position
            {
                int pos = positions[0];
                Vector3 tempy = decoy.transform.position; //need to use temporary object here for manual reassignment as transform.positon returns a struct, not a reference
                tempy.x = radius * (float)Math.Sin(pos * Math.PI / 180);
                tempy.z = radius * (float)Math.Cos(pos * Math.PI / 180);
                decoy.transform.position = tempy;
                positions.RemoveAt(0);
            }
            running_rotation_index++;
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

    void Write_log(string message)
    {
        log.WriteLine(message);
        //Debug.Log(message);
    }

    IEnumerator rewardRandomizer() //delivers rewards with randomized delays between 0.5 and 1.5*rewardfreq
    {
        for(; ;) //loops forever with period of delay
        {
            delay = (rewardfreq / 2) + (rewardfreq * UnityEngine.Random.value);
            deliver_reward();
            yield return new WaitForSeconds(delay);
        }

    }


    // IEnumerator licklogger() //asynchronous lick logger
    // {
    //     int islick = 0;
    //     for(; ;) //loops forever at frequency set by WaitForSeconds delay
    //     {
    //         if (lickport.BytesToRead > 0) 
    //         { 
    //             islick = lickport.ReadByte();
    //             lickport.DiscardInBuffer();
    //         }
    //         if (islick > 0) { Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tl\t"); } //log lick 
    //         islick = 0; //reset lick variable
    //         yield return new WaitForSeconds(0.1f);
    //     }
    // }

    IEnumerator flash()
    {
        sun.intensity = 2;
        yield return new WaitForSeconds(0.25f);
        sun.intensity = baseline_brightness;
    }

    // void trialTrigger()
    // {
    //     try
    //     {
    //         //lickport.Write('t'.ToString());
    //         //lickport_initialized = true;
    //     }
    //     catch (TimeoutException) 
    //     { 
    //         Debug.Log("Trial trigger output timeout!");
    //         //reset_lickport();
    //     }
    //     catch (InvalidOperationException) { Debug.Log("COM port not open! Trigger output failed!"); }
    //     catch (Exception e) { Debug.Log("Lickport error!" + e.Message); }
    // }


    // void reset_lickport()
    // {
    //     if (lickport_initialized == true) //reset serial connection if timeout detected and connection isn't already resetting
    //         try
    //         {
    //             lickport_initialized = false;
    //             lickport.Close();
    //             lickport.Open();
    //             lickport_initialized = true;
    //
    //             Debug.Log("Serial connection to lickport resetting...");
    //         }
    //         catch (Exception e) { Debug.Log("Couldn't reset lickport! " + e.Message); }
    // }

    void deliver_reward(bool manual = false) //outputs serial command to reward hardware when called. reward_delivered works for any positive int value between 1 and 10, which are multiples of 5 ul for the current hardware. 
    {
        bool include_bonus = false;
        if (trial_missed == true || manual == true)
            reward_delivered = 1; //secondary hits and manual rewards only get reward=1
        else
        {
            reward_delivered = reward_amt; //primary hits get rewarded based on reward_amt and side bonuses, latter is added in a few lines
            include_bonus = true;
        }

        //send base reward
        try
        {
            //lickport.Write(reward_delivered.ToString());
            //lickport_initialized = true;
        }
        catch (TimeoutException) { 
            Debug.Log("Reward delivery failed!");
            //reset_lickport();
        }
        catch (InvalidOperationException) { 
            Debug.Log("COM port not open! Reward delivery failed!");
            //reset_lickport();
        }

        //send bias bonus- this clicks the relay twice to make it extra apparent to the animal
        if (include_bonus == true)
        {   
            try
            {
                //lickport.Write(bias_bonus.ToString());
                reward_delivered += bias_bonus; //add to the reward delivered total
            }
            catch (TimeoutException) { 
                Debug.Log("Reward delivery failed!");
            }
            catch (InvalidOperationException) { 
                Debug.Log("COM port not open! Reward delivery failed!");
            }
        }

        reward_given += reward_delivered;
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tr\t" + reward_delivered.ToString());
    }

    void trialoutcome(int outcome)
    {
        Array.Copy(trial_history, 1, trial_history, 0, trial_history.Length - 1);
        trial_history[trial_history.Length - 1] = outcome;
        trials.Add(outcome);
    }

    void blackout(float duration)
    {
        
        //get meangray value from screens
        float gray_front = mousecam.GetComponent<cameracontrol>().getMeanGray();
        float gray_left = mousecam_L.GetComponent<cameracontrol>().getMeanGray();
        float gray_right = mousecam_R.GetComponent<cameracontrol>().getMeanGray();
        Debug.Log("Blackout for " + duration + " seconds!\nFront: " + gray_front + " Left: " + gray_left + " Right: " + gray_right);

        //start the blackout with timer
        StartCoroutine(blackout_timer(gray_front, gray_left, gray_right, duration));

    }


    IEnumerator blackout_timer(float front,float left,float right,float duration)
    {
        
        //activate gray image on each screen with proper value (game starts with them transparent)
        blackout_img.color = new Color(front, front, front, 1f);
        blackout_img_L.color = new Color(left, left, left, 1f);
        blackout_img_R.color = new Color(right, right, right, 1f);

        total_blackouts += 1; //keep track of total blackouts
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tB\t"); //log start time

        //wait
        yield return new WaitForSeconds(duration);

        //set all images back to transparent
        blackout_img.color = new Color(front, front, front, 0f);
        blackout_img_L.color = new Color(left, left, left, 0f);
        blackout_img_R.color = new Color(right, right, right, 0f);

        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tb\t"); //log end time

    }

    IEnumerator Blobout(float duration)
    {

        blobs.SetActive(true);
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tC\t"); //log start time

        //wait
        yield return new WaitForSeconds(duration);

        blobs.SetActive(false);
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tc\t"); //log end time
    }

    IEnumerator RDKout(float duration)
    {

        RDKmask.SetActive(true);
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tG\t"); //log start time

        //wait
        yield return new WaitForSeconds(duration);

        RDKmask.SetActive(false);
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\tg\t"); //log end time

    }

    IEnumerator blackoutRandomizer() //delivers rewards with randomized delays between 0.5 and 1.5*rewardfreq
    {
        for (; ; ) //loops forever with period of delay
        {
            delay = (blackout_frequency / 2) + (blackout_frequency * UnityEngine.Random.value);
            
            if(random_blackouts_activated==true) //if blackouts activated, select one and activate it
            {
                if (chaos_blackouts == false) { blackout(blackout_duration); }
                else
                {
                    float draw = UnityEngine.Random.value; //draw random number used as single switch between possibilities
                    if (draw < blobs_blackout_prob) { StartCoroutine(Blobout(blackout_duration)); } //blob blackout case
                    else if (draw > (1 - RDK_blackout_prob)) { StartCoroutine(RDKout(blackout_duration)); } //fog blackout case
                    else { blackout(blackout_duration); } //regular blackout is remainder of probability
                }

            }
            yield return new WaitForSeconds(delay); //wait
        }

    }

    void ShuffleArray<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int r = UnityEngine.Random.Range(0, i+1);
            T tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }
    }

    void ShuffleList<T>(List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;

        for (int i = list.Count - 1; i > 1; i--)
        {
            int rnd = random.Next(i + 1);

            T value = list[rnd];
            list[rnd] = list[i];
            list[i] = value;
        }
    }

    int Flip_Chaos()
    {

        //shuffle list if we've reached the end
        if (running_chaos_index > running_chaos.Length - 1)
        {
            ShuffleArray(running_chaos);
            running_chaos_index = 0;
        }

        //--------activate stimuli based on list position
        int stimulus = running_chaos[running_chaos_index]; //get current stimulus
        
        mousecam.GetComponent<customcamshader>().enabled = false; //deactivate everything
        mousecam_L.GetComponent<customcamshader>().enabled = false; //deactivate everything
        mousecam_R.GetComponent<customcamshader>().enabled = false; //deactivate everything

        terrain.GetComponent<Terrain>().enabled = true;
        foreach (Terrain thisterrain in outerterrains)
        {
            thisterrain.enabled = true;
        }
        fog.SetActive(false);
        blobs.SetActive(false);
        RDKmask.SetActive(false);


        //turn on stuff for this stimulus
        switch (stimulus)
        {
            case 0: //normal- do nothing
                break;
            case 1: //activate fog
                fog.SetActive(true);
                break;
            case 2: //activate clutter
                blobs.SetActive(true);
                break;
            case 3: //contrastmod with terrain
                mousecam.GetComponent<customcamshader>().enabled = true;
                mousecam_L.GetComponent<customcamshader>().enabled = true;
                mousecam_R.GetComponent<customcamshader>().enabled = true;
                break;
            case 4: //contrastmod without terrain
                mousecam.GetComponent<customcamshader>().enabled = true;
                mousecam_L.GetComponent<customcamshader>().enabled = true;
                mousecam_R.GetComponent<customcamshader>().enabled = true;
                terrain.GetComponent<Terrain>().enabled = false;
                foreach (Terrain thisterrain in outerterrains)
                {
                    thisterrain.enabled = false;
                }
                break;
            case 5: //RDK
                RDKmask.SetActive(true);
                break;
        }
        running_chaos_index++; //increment index in running list

        //log trial state
        return stimulus;
    }

    void destroydecoys()
    {
        GameObject[] decoys = GameObject.FindGameObjectsWithTag("decoys");
        foreach (GameObject decoy in decoys)
            GameObject.Destroy(decoy);
            // Debug.Log("Found object: " + decoy.name);
    }

    void Restart()
    {
        Write_log(DateTime.Now.ToString("HH:mm:ss.fff") + "\te\t");
        log.Close(); //close log fi le when application is ended
        Destroy(scene);
        SceneManager.LoadScene("Title");
    }
}
