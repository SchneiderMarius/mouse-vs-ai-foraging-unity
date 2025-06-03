using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.Barracuda;
using System.IO;

public class LoadModels : MonoBehaviour
{
    public BehaviorParameters behaviorParameters;

    void Start()
    {
        string modelName = GetModelNameFromArgs();
        if (!string.IsNullOrEmpty(modelName))
        {
            LoadModelFromResources(modelName);
        }
        else
        {
            Debug.LogWarning("No --model= passed. Using default.");
        }

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

    void LoadModelFromResources(string modelName)
    {
        modelName = Path.GetFileNameWithoutExtension(modelName);

        NNModel model = Resources.Load<NNModel>("Models/" + modelName);
        if (model != null)
        {
            behaviorParameters.Model = model;
            behaviorParameters.BehaviorType = BehaviorType.InferenceOnly;
            Debug.Log("Loaded model: " + modelName);
        }
        else
        {
            Debug.LogError("Model not found in Resources/Models/: " + modelName);
        }
    } 
}