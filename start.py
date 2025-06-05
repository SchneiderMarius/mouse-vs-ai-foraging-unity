import sys
from train_old import train
from test import test

def print_usage():
    """Print usage instructions for the script."""
    print("Usage: python start.py [train|test]")
    print("  train     - Run standard training")
    print("  test      - Run testing with default parameters")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print_usage()
        sys.exit(1)

    mode = sys.argv[1].lower()
    
    try:
        # TODO: add option for customized model
        if mode == "train":
            train("Normal")
        elif mode == "test":
            # TODO: add option for user to specify model name, model file, test type, duration
            test("neurips_3", "My Behavior.onnx", "Perturbation", 600)
        else:
            print(f"Error: Unknown mode '{mode}'")
            print_usage()
            sys.exit(1)
    except Exception as e:
        print(f"Error occurred while running {mode}: {str(e)}")
        sys.exit(1)
