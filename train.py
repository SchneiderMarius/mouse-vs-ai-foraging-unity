import subprocess
import os
import time

def train(run_id, env_path, config_path, total_runs=5):
    for i in range(total_runs):
        current_run_id = f"{run_id}_{i+1}"
        print(f"Starting training: {current_run_id}")
        with open("C:/Users/BionicVisionVR/Documents/Mouse/2D go to target v1/Builds/Grey/2D go to target v1_Data/StreamingAssets/currentLog.txt", "w") as f:
            f.write(f"{run_id}_{i+1}.txt")
        time.sleep(1)

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

if __name__ == "__main__":
    env_path = "C:/Users/BionicVisionVR/Documents/Mouse/2D go to target v1/Builds/Grey/2D go to target v1.exe"
    config_path = "./Config/visualtutor.yaml"
    run_id = "batch_run_a"
    total_runs = 3

    train(run_id, env_path, config_path, total_runs)

# from mlagents_envs.environment import UnityEnvironment
# from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
# from mlagents.trainers import learn
# import os

# def main():
#     env_path = "C:/Users/BionicVisionVR/Documents/Mouse/2D go to target v1/Builds/NoTitle/2D go to target v1.exe"
#     run_id = "test_run-head"
#     config_path = "./Config/visual.yaml"

#     engine_channel = EngineConfigurationChannel()
#     engine_channel.set_configuration_parameters(
#         width=155, height=86, quality_level=1, time_scale=20, target_frame_rate=-1
#     )


#     env = UnityEnvironment(file_name=env_path, side_channels=[engine_channel])

#     try:
#         learn.run_training(
#             run_seed=1,
#             run_id=run_id,
#             trainer_config_path=config_path,
#             env=env,
#             resume=False,
#             force=True,
#         )
#     finally:
#         env.close()

# if __name__ == "__main__":
#     main()


# def main():

#     build_path = "C:/Users/BionicVisionVR/Documents/Mouse/2D go to target v1/Builds/NoTitle/2D go to target v1.exe"  # <- change if needed
#     unity_proc = launch_unity(build_path)

#     time.sleep(10)  # wait for Unity to launch

#     # Connect ML-Agents
#     channel = EngineConfigurationChannel()
#     channel.set_configuration_parameters(width=155, height=86, quality_level=1, time_scale=20)

#     env = UnityEnvironment(file_name=None, side_channels=[channel], base_port=5005)
#     env.reset()

#     # Now start PPO or any trainer (example: PPO)
#     # You need to have a config.yaml ready
#     from mlagents.trainers.learn import run_training
#     run_training(run_options="./Config/visual.yaml")

#     env.close()

#     unity_proc.terminate()

# if __name__ == "__main__":
#     main()
