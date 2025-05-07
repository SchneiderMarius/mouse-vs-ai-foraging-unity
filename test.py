import subprocess
import os
import time

exe_path = r"C:/Users/BionicVisionVR/Documents/Mouse/2D go to target v1/Builds/Test/2D go to target v1.exe"
model_name = "My Behavior-99979"
model_file = "My Behavior-99979.onnx"  # 也可以是 "models/run1/model.onnx"

with open("C:/Users/BionicVisionVR/Documents/Mouse/2D go to target v1/Builds/Grey/2D go to target v1_Data/StreamingAssets/currentLog.txt", "w") as f:
# f.write(f"{model_file}_{i+1}.txt")
    f.write(f"{model_name}.txt")
time.sleep(1)

subprocess.Popen([exe_path, "--model=" + model_file])

# import subprocess
# import os
# import time



# def test(run_id, env_path, total_runs=5):
#     for i in range(total_runs):
#         current_run_id = f"{run_id}_{i+1}"
#         print(f"Starting testing: {current_run_id}")
#         with open("C:/Users/BionicVisionVR/Documents/Mouse/2D go to target v1/Builds/Grey/2D go to target v1_Data/StreamingAssets/currentLog.txt", "w") as f:
#             f.write(f"{run_id}_{i+1}.txt")
#         time.sleep(3)

#         subprocess.Popen(env_path)

#         print(f"Completed testing: {current_run_id}")
        
#         time.sleep(5)

# if __name__ == "__main__":
#     env_path = "C:/Users/BionicVisionVR/Documents/Mouse/2D go to target v1/Builds/Test/2D go to target v1.exe"
#     # config_path = "./Config/visualtutor.yaml"
#     run_id = "test_run"
#     total_runs = 3  # 总训练模型数目

#     test(run_id, env_path, total_runs)