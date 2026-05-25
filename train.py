import subprocess
import os
import time
from pathlib import Path
import glob
import replace
import test

def get_next_run_number(base_name, results_dir="./results"):
    """Get the next run number for a given base name by checking existing results."""
    # Create results directory if it doesn't exist
    os.makedirs(results_dir, exist_ok=True)
    
    # Get all existing runs for this base name
    pattern = os.path.join(results_dir, f"{base_name}_*")
    existing_runs = glob.glob(pattern)
    
    if not existing_runs:
        return 1
    
    # Extract run numbers and find the maximum
    run_numbers = []
    for run_path in existing_runs:
        try:
            # Extract the number after the last underscore
            run_num = int(run_path.split('_')[-1])
            run_numbers.append(run_num)
        except (ValueError, IndexError):
            continue
    
    return max(run_numbers) + 1 if run_numbers else 1

def train_solo(run_id, env_path, config_path, total_runs=5,log_name=None):
    # Get the next run number for this encoder type
    next_run = get_next_run_number(run_id)
    

    run_id_list = []
    for i in range(total_runs):
        current_run_id = f"{run_id}_{next_run + i}"
        
        # replace the path with where you have your exe file, but keep the rest of the path the same
        # e.g. "whichever folder you save your exe file" + "/2D go to target v1_Data/StreamingAssets/currentLog.txt"
        if not log_name: 
            with open("./Builds/train-test/2D go to target v1_Data/StreamingAssets/currentLog.txt", "w") as f:
                f.write(f"{run_id}_{next_run + i}_train.txt")
        else:
            with open("./Builds/train-test/2D go to target v1_Data/StreamingAssets/currentLog.txt", "w") as f:
                f.write(f"{log_name}_train.txt")         
            if total_runs > 1:
                current_run_id = f"{log_name}_{next_run + i}"
            else:
                current_run_id = log_name
        print(f"Starting training: {current_run_id}")

        time.sleep(1)
        
        cmd = [
            "mlagents-learn",
            config_path,
            "--env", env_path,
            "--run-id", current_run_id,
            "--force",
            "--env-args", "-logFile - --screen-width=155 --screen-height=86",
#            "--env-args", "--screen-width=155", "--screen-height=86", "-logfile -",
        ]
#            "--env-args", "-logFile - --screen-width=155 --screen-height=86",

        subprocess.run(cmd)

        print(f"Completed training: {current_run_id}")
        run_id_list.append(current_run_id)

    return run_id_list
        # time.sleep(5)

        # # Call the test function to evaluate the trained model
        # """
        # Args:
        #     model_name: Name of the model to be tested (with id)
        #     model_file: Path to the ONNX model file
        #     test_type: Type of test to run (e.g., "Perturbation"(default), "Normal", "Random")
        # """
        # test.test(
        #     model_name = current_run_id
        # )

def train_multiple_networks(networks, env_path, runs_per_network=2,log_name=None):
    """
    Train multiple visual networks, running each one multiple times.
    
    Args:
        networks (list): List of network configurations, each containing:
            - name: Name of the network
            - config_path: Path to the network's config file
            - encoder_type: Type of encoder to use
        env_path (str): Path to the Unity environment executable
        base_config_path (str): Base path for config files
        runs_per_network (int): Number of times to run each network
    """

    run_id_list2 = []
    
    for network in networks:
        if network == "fully_connected":
            config_path = "./Config/fc.yaml"
        elif network == "simple":
            config_path = "./Config/simple.yaml"
        elif network == "resnet":
            config_path = "./Config/resnet.yaml"
        else:
            config_path = "./Config/nature.yaml"
            if network != "nature_cnn":
                # Replace the path with where your conda environment is located
                replace.replace_nature_visual_encoder("C:/Users/BionicVisionVR/miniconda3/envs/mouse/Lib/site-packages/mlagents/trainers/torch/encoders.py", "./Encoders/" + network + ".py")

        print(f"\nStarting training for network: {network}")
        run_id_list = train_solo(
                        run_id=network,
                        env_path=env_path,
                        config_path=config_path,
                        total_runs=runs_per_network,
                        log_name = log_name
                    )
        print(f"Completed all runs for network: {network}\n")

        run_id_list2.extend(run_id_list)

    return run_id_list2

def train(env, runs_per_network, networks,log_name=None):
    """
    Args:
        env: Type of environment to train on  (e.g., "Perturbation", "Normal", "Random")
    """
    env_path = f"./Builds/{env}/2D go to target v1.exe"
    
    # Define your networks here with both built-in and custom encoder types

    # , "alexnet", "fully_connected", "mousenet", "vonenet"
    # networks = ["nature_cnn", "simple", "resnet", "neurips"]
    # networks = ["neurips"]
    
    run_ids = train_multiple_networks(networks, env_path, runs_per_network,log_name)

    return run_ids