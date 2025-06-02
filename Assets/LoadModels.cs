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
            LoadModel(modelName);
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

    void LoadModel(string modelPath)
    {
        try
        {
            // First try to load from Resources folder
            string modelName = Path.GetFileNameWithoutExtension(modelPath);
            NNModel model = Resources.Load<NNModel>("Models/" + modelName);
            
            if (model == null)
            {
                // If not in Resources, try to load from file system
                if (File.Exists(modelPath))
                {
                    byte[] modelData = File.ReadAllBytes(modelPath);
                    model = ScriptableObject.CreateInstance<NNModel>();
                    model.modelData = new NNModelData { Value = modelData };
                }
                else
                {
                    Debug.LogError($"Model not found at path: {modelPath}");
                    return;
                }
            }

            behaviorParameters.Model = model;
            behaviorParameters.BehaviorType = BehaviorType.InferenceOnly;
            Debug.Log("Loaded model: " + modelPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading model: {e.Message}");
        }
    }
}
