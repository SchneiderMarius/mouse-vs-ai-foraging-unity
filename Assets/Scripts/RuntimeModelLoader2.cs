/*
 * RuntimeModelLoader.cs  
 * (Unity 2019.4 LTS + ML-Agents 2.x + Barracuda ≥3.0.1)
 */
using System;
using System.IO;
using System.Text;
using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;   // ONNXModelConverter
using Unity.Barracuda;         // ModelWriter

public class RuntimeModelLoader2 : MonoBehaviour
{
    [Tooltip("Fallback model baked into the build")]
    public NNModel defaultModel;

    void Start()
    {
        // 0️⃣ Debug log all args
        Debug.Log("Args → " + string.Join(" | ", Environment.GetCommandLineArgs()));

        // Grab your BehaviorParameters on this same GameObject
        var bp = GetComponent<BehaviorParameters>();
        if (bp == null)
        {
            Debug.LogError("RuntimeModelLoader requires a BehaviorParameters on the same GameObject!");
            return;
        }

        // 1️⃣ Parse --model= flag
        string path = null;
        foreach (var arg in Environment.GetCommandLineArgs())
            if (arg.StartsWith("--model="))
                path = arg.Substring("--model=".Length).Trim('"');

        // We'll default to whatever you set in the Inspector
        NNModel nnAsset = defaultModel;

        // 2️⃣ If they pointed at a real ONNX file, convert it!
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            Debug.Log($"RuntimeModelLoader | converting ONNX → Barracuda Model from:\n    {path}");

            // a) Read ONNX bytes
            var onnxBytes = File.ReadAllBytes(path);

            // b) Instantiate the converter (you can tweak those flags if you like)
            var converter = new ONNXModelConverter(
                optimizeModel: true,
                treatErrorsAsWarnings: false,
                forceArbitraryBatchSize: true
            );

            // c) Convert into a Barracuda Model object
            Model convertedModel = converter.Convert(onnxBytes);

            // d) Serialize that Model object into Barracuda flatbuffer bytes
            byte[] barracudaBytes;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                ModelWriter.Save(bw, convertedModel, verbose: false);
                barracudaBytes = ms.ToArray();
            }

            // e) Wrap those bytes in a temporary NNModel ScriptableObject
            var tempNN = ScriptableObject.CreateInstance<NNModel>();
            tempNN.name = Path.GetFileNameWithoutExtension(path);


            var data    = ScriptableObject.CreateInstance<NNModelData>();
            data.Value  = barracudaBytes;
            tempNN.modelData = data;
            nnAsset = tempNN;

            Debug.Log($"RuntimeModelLoader | conversion complete, created in-memory NNModel ({barracudaBytes.Length:N0} bytes)");
        }
        else
        {
            Debug.Log("RuntimeModelLoader | no --model= found or file missing; using default baked-in model");
        }

        // 3️⃣ Finally, shove it into ML-Agents and switch to InferenceOnly
        bp.Model         = nnAsset;
        bp.BehaviorType  = BehaviorType.InferenceOnly;
        Debug.Log("RuntimeModelLoader | BehaviorParameters updated, ready for inference");
    }
}
