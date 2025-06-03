import sys
from train_per import train_per
from test import test

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python start.py [train|train_per|test]")
        sys.exit(1)
    mode = sys.argv[1].lower()
    if mode == "train":
        train_per()
    elif mode == "test":
        test("Neurips_3", "My Behavior.onnx", "Perturbation", 600)
    else:
        print("Unknown mode:", mode)
        print("Usage: python start.py [train|train_per|test]")
