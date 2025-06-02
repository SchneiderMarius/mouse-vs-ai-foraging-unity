import subprocess
import os
import time

def test(model_name, model_file="My Behavior.onnx" , test_type="Perturbation"):
    """
    Args:
        model_name: Name of the model to be tested
        model_file: Path to the ONNX model file
        test_type: Type of test to run (e.g., "Perturbation", "Normal", "Random")
    """

    exe_path = r"C:/Users/BionicVisionVR/Documents/Mouse/2D go to target v1/Builds/" + test_type + "/2D go to target v1.exe"
    model_name = model_name + "-test"
    # model_file =

    # write log file's name
    with open("C:/Users/BionicVisionVR/Documents/Mouse/2D go to target v1/Builds/Test/2D go to target v1_Data/StreamingAssets/currentLog.txt", "w") as f:
        f.write(f"{model_name}.txt")
    time.sleep(1)

    """
    call LoadModels in Unity executable
    """
    subprocess.Popen([exe_path, "--model=" + model_file])