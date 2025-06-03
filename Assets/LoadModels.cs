using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.Barracuda;
using System.IO;

public class LoadModels : MonoBehaviour
{
    public BehaviorParameters behaviorParameters;

    [SerializeField]
    private string fallbackModelName = "./results/neurips_3/My Behavior.onnx";

    void Start()
    {
        string modelName = GetModelNameFromArgs();
        if (string.IsNullOrEmpty(modelName))
        {
    #if UNITY_EDITOR
            modelName = fallbackModelName;
            Debug.LogWarning("No --model= passed. Using fallbackModelName in editor.");
    #else
            Debug.LogWarning("No --model= passed. Using default.");
            return;
    #endif
        }

        LoadModel(modelName);
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
