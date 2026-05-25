import sys
import argparse
from train import train
from test import test
import subprocess
import os
import math
import pandas as pd

def summarize_log(log_path: str):
    """
    Reads the Unity log at log_path, then prints:
      • Overall success rate (%)
      • Success rate per trial type
      • Max target distance (units)
    """
    # 1) load into DataFrame
    df = pd.read_csv(
        log_path,
        sep=r'\s+',                # whitespace-separated
        comment='#',               # skip any comment lines
        header=None,
        names=['SessionTime','EventType','x','y','z','r','extra'],
        usecols=[0,1,2,4,5],       # we only need x, z for distance
        engine='python'
    )
    # only keep the “n”, “t”, “h”, “f” events
    df = df[df.EventType.isin(['n','t','s','h','f'])].reset_index(drop=True)

    # 2) find indices of each new trial
    new_trial_idxs = df.index[df.EventType=='n'].tolist()
    trial_type_idx = df.index[df.EventType=='s'].tolist()

    successes = []
    by_type = {}
    distances = []

    for ti, start_idx in enumerate(new_trial_idxs):
        end_idx = new_trial_idxs[ti+1] if ti+1 < len(new_trial_idxs) else len(df)
        trial = df.iloc[start_idx:end_idx]

        # trial type code is in the 'x' column of the 'n' line
        ttype = int(trial.loc[trial.EventType=='s','x'].iat[0])

        # find the target distance (first 't' row)
        trow = trial[trial.EventType=='t']
        if trow.empty:
            continue
        dx, dz = float(trow.x.iat[0]), float(trial.loc[trow.index,'z'].iat[0])
        dist = math.hypot(dx, dz)
        distances.append(dx)

        # did we hit? (any 'h' in the slice)
        hit = 1 if ('h' in trial.EventType.values) else 0
        successes.append(hit)
        by_type.setdefault(ttype, []).append(hit)

    # 3) summarize
    if successes:
        overall = sum(successes)/len(successes)*100
        print(f"\n=== EVALUATION RESULTS ===")
        print(f"Overall success rate: {overall:.1f}% ({sum(successes)}/{len(successes)})")
        for ttype, hits in by_type.items():
            rate = sum(hits)/len(hits)*100
            print(f"  • Trial type {ttype}: {rate:.1f}% ({sum(hits)}/{len(hits)})")
    if distances:
        print(f"Max target distance reached: {max(distances):.3f}/5.00")
    print("==========================\n")


def print_usage():
    """Print usage instructions for the script."""
    print("Usage: python start.py [train|test] [options]")
    print("\nTraining options:")
    print("  --runs-per-network R    Number of runs per network (default: 5)")
    print("  --envs ID            Run identifier (default: Normal)")
    print("  --networks N1,N2,N3    Comma-separated list of networks to train (default choices: ['fully_connected', 'nature_cnn', 'simple', 'resnet'])")
    print("\nTesting options:")
    print("  test                   Run testing with default parameters")

def parse_args():
    parser = argparse.ArgumentParser(description='Run training or testing')
    parser.add_argument('mode', choices=['train', 'test'], help='Mode to run')
    
    # Training arguments
    parser.add_argument('--runs-per-network', type=int, default=5,
                      help='Number of runs per network')
    parser.add_argument('--env', type=str, default='Normal',
                      help='Run identifier')
    parser.add_argument('--networks', type=str, default='fully_connected',
                      help='Comma-separated list of networks to train')
    
    parser.add_argument('--onnx_file', type=str, default='example_model.onnx',
                    help='only used for testing, specify the .onnx model file to use for testing')
    parser.add_argument('--filename', type=str,
                    help='file name - if not specified it will be set to same as networks parameter')   
    
    return parser.parse_args()

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print_usage()
        sys.exit(1)

    args = parse_args()

    try:
        # TODO: add option for customized model
        if args.mode == "train":
            # Convert comma-separated networks string to list
            networks = [net.strip() for net in args.networks.split(',')]
            if not args.log_name:
                train(args.env, args.runs_per_network, networks)
            else:
                train(args.env, args.runs_per_network, networks,args.log_name)

        elif args.mode == "test":
            
            if not args.model:
                print("--model PATH missing - evaluating example model!")

            if not args.log_name:
                print("--log_name Name for test run missing")
                sys.exit(1)
            
            log_name = f"{args.log_name}_test.txt"

            env_path = f"./Builds/{env}"
            with open("./Builds/train-test/2D go to target v1_Data/StreamingAssets/currentLog.txt", "w") as f:
                f.write(log_name)


            # change this to environment with only max path
            env_path = f"./Builds/RandomTest/2D go to target v1.exe"
            cmd = [
                env_path,
                f"--model={args.model}",
                f"--episodes={args.episodes}"
            ]
            # Create 
            #"C:\Users\BionicVisionVR\Documents\Mouse\exeFile\Builds\RandomDebug5\2D go to target v1.exe" --model=C:\Users\BionicVisionVR\Documents\Mouse\models\1.3-simple-pertur\My_Behavior.onnx
            print(f"[TEST] Running: {' '.join(cmd)}")
            ret = subprocess.run(cmd, check=False)

            print(f"Test Complete")

            logpath = './Auto-mouseTest/'+log_name
            print(logpath)
            summarize_log(dir=logpath)


        else:
            print(f"Error: Unknown mode '{args.mode}'")
            print_usage()
            sys.exit(1)
    except Exception as e:
        print(f"Error occurred while running {args.mode}: {str(e)}")
        sys.exit(1)






