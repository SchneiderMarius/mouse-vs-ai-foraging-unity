/*
 * RuntimeModelLoader.cs  (Unity 2019.4 LTS + ML-Agents 1.x + Barracuda 1.0.x)
 */
using System;
using System.IO;
using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.Barracuda;


public class RuntimeModelLoader : MonoBehaviour
{
//    [Tooltip("Fallback model baked into the build")]
    public NNModel defaultModel;


    void Start()
    {
        Debug.Log("Args → " + string.Join(" | ", Environment.GetCommandLineArgs()));
        var bp = GetComponent<BehaviorParameters>();


        // -------- 1. grab --model= flag -----------------------------------
        string path = null;
        foreach (string arg in Environment.GetCommandLineArgs())
            if (arg.StartsWith("--model="))
            {
                path = arg.Substring("--model=".Length);
                break;
            }


        NNModel nnAsset = defaultModel;


        // -------- 2. load user model if provided --------------------------
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            byte[] bytes = File.ReadAllBytes(path);      // Barracuda 1.0 API
            Model   _     = ModelLoader.Load(bytes);     // parses & validates


            // wrap bytes in a temporary NNModel asset
            nnAsset = ScriptableObject.CreateInstance<NNModel>();
            var data     = ScriptableObject.CreateInstance<NNModelData>();
            data.Value   = bytes;                        // expects byte[ ]
            nnAsset.modelData = data;


            Debug.Log($"RuntimeModelLoader | loaded model bytes from {path}");
        }
        else
        {
            Debug.Log("RuntimeModelLoader | using default baked-in model");
        }


        // -------- 3. plug into Agent & freeze -----------------------------
        bp.Model        = nnAsset;
        bp.BehaviorType = BehaviorType.InferenceOnly;
    }
}





