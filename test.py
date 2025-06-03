import subprocess
import os
import time
import shutil

def test(model_name, model_file="My Behavior.onnx", test_type="Perturbation", duration=600):
    """
    Args:
        model_name: Name of the model to be tested
        model_file: Path to the ONNX model file
        test_type: Type of test to run (e.g., "Perturbation", "Normal", "Random")
        duration: Duration of the test in seconds (default: 60)
    """

    # replace the path with where you have your exe file
    exe_path = r"./Builds/" + test_type + "/2D go to target v1.exe"
    model_file = model_name +"/"+ model_file
    model_name = model_name + "-test"

    destination = f"./Builds/{test_type}/2D go to target v1_Data/StreamingAssets/" + model_file

    
    shutil.copy("./results/" + model_file, destination)
    time.sleep(1)

    # write log file's name
    # replace the path with where you have your exe file, but keep the rest of the path the same
        # e.g. "whichever folder you save your exe file" + "/2D go to target v1_Data/StreamingAssets/currentLog.txt"
    with open("./Builds/" + test_type + "/2D go to target v1_Data/StreamingAssets/currentLog.txt", "w") as f:
        f.write(f"{model_name}.txt")
    time.sleep(1)

    """
    call LoadModels in Unity executable and close it after duration
    """
    process = subprocess.Popen([exe_path, "--model=" + model_file])
    
    # Wait for the specified duration
    time.sleep(duration)
    
    # Close the process
    process.terminate()
    try:
        process.wait(timeout=5)  # Wait up to 5 seconds for the process to terminate
    except subprocess.TimeoutExpired:
        process.kill()  # Force kill if it doesn't terminate gracefully