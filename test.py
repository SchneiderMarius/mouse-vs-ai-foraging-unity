import subprocess
import os
import time
import shutil
import train

def test(model_name, model_file="My Behavior.onnx", test_type="Perturbation", duration=600):
    """
    Run a test session for a trained model in the Unity environment.
    
    This function:
    1. Copies the model file to the Unity build directory
    2. Sets up the log file for recording test results
    3. Launches the Unity executable
    4. Runs the test for the specified duration
    5. Terminates the Unity process
    
    Args:
        model_name (str): Name of the model directory (e.g., "Neurips_3")
        model_file (str): Name of the ONNX model file (default: "My Behavior.onnx")
        test_type (str): Type of test environment ("Perturbation", "Normal", or "Random")
        duration (int): How long to run the test in seconds (default: 600 = 10 minutes)
    """
    print(f"\nStarting test for model: {model_name}")
    print(f"Test type: {test_type}")
    print(f"Duration: {duration} seconds")

    # Set up the destination path for the model file in Unity's StreamingAssets
    destination = f"./Builds/{test_type}/2D go to target v1_Data/StreamingAssets/Models/" 
    
    # Create necessary directories if they don't exist
    print(f"\nCreating directory: {destination}")
    os.makedirs(os.path.dirname(destination), exist_ok=True)
    
    # Copy the model file
    source_file = f"./results/{model_name}/{model_file}"
    print(f"Copying model from: {source_file}")
    if not os.path.exists(source_file):
        print(f"Error: Model file not found at {source_file}")
        return
    shutil.copy(source_file, destination)
    print("Model file copied successfully")
    time.sleep(1)

    # Path to the Unity executable for this test type
    exe_path = r"./Builds/" + test_type + "/2D go to target v1.exe"
    if not os.path.exists(exe_path):
        print(f"Error: Unity executable not found at {exe_path}")
        return
    print(f"\nFound Unity executable at: {exe_path}")

    # write log file's name
    # replace the path with where you have your exe file, but keep the rest of the path the same
        # e.g. "whichever folder you save your exe file" + "/2D go to target v1_Data/StreamingAssets/currentLog.txt"

    # TODO: generate the log file name if there are multiple runs

    with open("./Builds/" + test_type + "/2D go to target v1_Data/StreamingAssets/currentLog.txt", "w") as f:
        f.write(f"{model_name}-test.txt")
    time.sleep(1)  # Brief pause to ensure log file is written

    """
    Launch the Unity executable and run the test:
    1. Start Unity with the specified model
    2. Let it run for the specified duration
    3. Terminate the process
    """
    print(f"\nLaunching Unity executable...")
    process = subprocess.Popen([exe_path, "--model=" + model_file])
    print(f"Test running for {duration} seconds...")
    
    # Wait for the specified test duration
    time.sleep(duration)
    
    # Close the process
    print("\nTest complete. Terminating Unity process...")
    process.terminate()
    try:
        process.wait(timeout=5)  # Wait up to 5 seconds for the process to terminate
        print("Unity process terminated successfully")
    except subprocess.TimeoutExpired:
        process.kill()  # Force kill if it doesn't terminate gracefully
        print("Unity process force terminated")