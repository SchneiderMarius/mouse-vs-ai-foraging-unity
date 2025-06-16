using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.MLAgents;
using Unity.Barracuda;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using System.IO;
using Unity.Barracuda.ONNX;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LoadModels : MonoBehaviour
{
    // public AgentController agent;  // Reference to the Agent component
    public BehaviorParameters behaviorParameters;
    string modelFile;

    Agent m_Agent;

    void Start()
    {
        modelFile = GetModelNameFromArgs();
        // agent = GameObject.Find("Mouse").GetComponent<AgentController>();
        Debug.Log("LoadModels Start");
        m_Agent = GetComponent<Agent>();


        // if (agent == null)
        // {
        //     Debug.LogError("Agent component not found on this GameObject.");
        //     return;
        // }
        // else
        // {
        //     if (string.IsNullOrEmpty(modelFile))
        //     {
        // modelFile = GetModelNameFromArgs();
        modelFile = "My Behavior.onnx"; // Default model file name
        // Debug.LogError("No model file specified in command line arguments.");
        // return;
        // }

        // LoadModelFromResources(agent, modelFile);
        LoadModelFromResources(modelFile);
        // }
    }

    string GetModelNameFromArgs()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            if (arg.StartsWith("--model="))
            {
                return arg.Substring("--model=".Length).Trim();
            }
        }
        return null;
    }

    void LoadModelFromResources(string modelFile)
    {
        // byte[] rawModel = null;
        // Get the full path to the model file in StreamingAssets
        string assetPath = Path.Combine(Application.streamingAssetsPath, "Models", modelFile);
        Debug.Log("modelPath " + assetPath);
        // try
        // {
        //     rawModel = File.ReadAllBytes(assetPath);
        //     // isOnnx = overrideExtension.Equals("onnx");
        //     // assetName = "Override - " + Path.GetFileName(assetPath);
        //     // break;
        // }
        // catch (IOException)
        // {
        //     // Do nothing - try the next extension, or we'll exit if nothing loaded.
        // }

        // NNModel model = LoadNNModel(assetPath, modelFile);
        NNModel model = null;
        // m_Agent.SetModel("My Behavior", asset);

        if (model != null)
        {
            behaviorParameters.Model = model;
            behaviorParameters.BehaviorType = BehaviorType.InferenceOnly;
            Debug.Log("Loaded model: " + modelFile);
        }
        else
        {
            Debug.LogError("Model not found in Resources/Models/: " + modelFile);
        }

        // if (File.Exists(modelPath))
        // {
        //     // Load the model using Agent's SetModel method
        //     NNModel model = Resources.Load<NNModel>("Models/" + modelFile);
        //     Agent.SetModel("My Behavior", model);
        //     // behaviorParameters.BehaviorType = BehaviorType.InferenceOnly;
        //     Debug.Log("Loaded model: " + modelFile);
        // }
        // else
        // {
        //     Debug.LogError("Model not found at path: " + modelPath);
        // }
    }

    // NNModel LoadOnnxModel(byte[] rawModel)
    // {
    //     var converter = new ONNXModelConverter(true);
    //     var onnxModel = converter.Convert(rawModel);

    //     NNModelData assetData = ScriptableObject.CreateInstance<NNModelData>();
    //     using (var memoryStream = new MemoryStream())
    //     using (var writer = new BinaryWriter(memoryStream))
    //     {
    //         ModelWriter.Save(writer, onnxModel);
    //         assetData.Value = memoryStream.ToArray();
    //     }
    //     assetData.name = "Data";
    //     assetData.hideFlags = HideFlags.HideInHierarchy;

    //     var asset = ScriptableObject.CreateInstance<NNModel>();
    //     asset.modelData = assetData;
    //     return asset;
    // }

        NNModel LoadNNModel(string modelPath, string modelName)
    {
        print("Loading model: " + modelPath);
        print("Model Name: " + modelName);
        var converter = new ONNXModelConverter(true);
        if (!File.Exists(modelPath))
        {
            Debug.LogError("Model file does not exist at: " + modelPath);
            return null;
        }
        Model model = converter.Convert(modelPath);
        NNModelData modelData = ScriptableObject.CreateInstance<NNModelData>();
        using (var memoryStream = new MemoryStream())
        using (var writer = new BinaryWriter(memoryStream))
        {
            ModelWriter.Save(writer, model);
            modelData.Value = memoryStream.ToArray();
        }
        modelData.name = "Data";
        modelData.hideFlags = HideFlags.HideInHierarchy;
        NNModel result = ScriptableObject.CreateInstance<NNModel>();
        result.modelData = modelData;
        result.name = modelName;
        return result;
    }
}