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
                // Ensure the model is in StreamingAssets
                string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "Models");
                string targetPath = Path.Combine(streamingAssetsPath, Path.GetFileName(modelPath));
                
                // Create Models directory in StreamingAssets if it doesn't exist
                if (!Directory.Exists(streamingAssetsPath))
                {
                    Directory.CreateDirectory(streamingAssetsPath);
                }

                // Copy the model to StreamingAssets if it's not already there
                if (!File.Exists(targetPath) || File.GetLastWriteTime(modelPath) > File.GetLastWriteTime(targetPath))
                {
                    File.Copy(modelPath, targetPath, true);
                    Debug.Log($"Copied model to StreamingAssets: {targetPath}");
                }

                // Load the model from StreamingAssets
                if (File.Exists(targetPath))
                {
                    byte[] modelData = File.ReadAllBytes(targetPath);
                    model = ScriptableObject.CreateInstance<NNModel>();
                    var modelDataObj = ScriptableObject.CreateInstance<NNModelData>();
                    modelDataObj.Value = modelData;
                    model.modelData = modelDataObj;
                }
                else
                {
                    Debug.LogError($"Failed to copy model to StreamingAssets: {targetPath}");
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
            if (e.Message.Contains("Format version not supported"))
            {
                Debug.LogError("This error typically occurs when the model file was created with a different version of Unity or Barracuda than what you're currently using. Please ensure you're using compatible versions.");
            }
        }
    }
}
