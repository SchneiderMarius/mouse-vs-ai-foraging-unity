import subprocess
import os
import time
from pathlib import Path
import glob
import replace

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

def train(run_id, env_path, config_path, total_runs=5):
    # Get the next run number for this encoder type
    next_run = get_next_run_number(run_id)
    
    for i in range(total_runs):
        current_run_id = f"{run_id}_{next_run + i}"
        print(f"Starting training: {current_run_id}")
        with open("C:/Users/BionicVisionVR/Documents/Mouse/2D go to target v1/Builds/Grey/2D go to target v1_Data/StreamingAssets/currentLog.txt", "w") as f:
            f.write(f"{run_id}_{next_run + i}.txt")
        time.sleep(1)

        replace.replace_nature_visual_encoder("C:/Users/BionicVisionVR/miniconda3/envs/mouse3/Lib/site-packages/mlagents/trainers/torch/encoders.py", ".Encoders/"+run_id+".py")

        cmd = [
            "mlagents-learn",
            config_path,
            "--env", env_path,
            "--run-id", current_run_id,
            "--force",
            "--env-args", "--screen-width=155", "--screen-height=86",
        ]
        
        subprocess.run(cmd)

        print(f"Completed training: {current_run_id}")
        
        time.sleep(5)

def train_multiple_networks(networks, env_path, base_config_path, runs_per_network=5):
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
    for network in networks:
        if network == "fully_connected":
            config_path = "./Config/fc.yaml"
        elif network == "simple":
            config_path = "./Config/simple.yaml"
        elif network == "resnet":
            config_path = "./Config/resnet.yaml"
        else:
            config_path = "./Config/nature.yaml"
            
        print(f"\nStarting training for network: {network}")
        train(
            run_id=network,
            env_path=env_path,
            config_path=config_path,
            total_runs=runs_per_network
        )
        print(f"Completed all runs for network: {network}\n")

if __name__ == "__main__":
    env_path = "C:/Users/BionicVisionVR/Documents/Mouse/2D go to target v1/Builds/Grey/2D go to target v1.exe"
    base_config_path = "./Config"
    
    # Define your networks here with both built-in and custom encoder types
    networks = ["nature_cnn", "simple", "resnet", "neurips", "alexnet", "fully_connected", "mousenet", "vonenet"]
    
    train_multiple_networks(networks, env_path, base_config_path, runs_per_network=5)