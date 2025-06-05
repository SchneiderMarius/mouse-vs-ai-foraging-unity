import sys
import argparse
from train import train
from test import test

def print_usage():
    """Print usage instructions for the script."""
    print("Usage: python start.py [train|test] [options]")
    print("\nTraining options:")
    print("  --runs-per-network R    Number of runs per network (default: 5)")
    print("  --run-id ID            Run identifier (default: Normal)")
    print("  --networks N1,N2,N3    Comma-separated list of networks to train (default choices: ['fully_connected', 'nature_cnn', 'simple', 'resnet'])")
    print("\nTesting options:")
    print("  test                   Run testing with default parameters")

def parse_args():
    parser = argparse.ArgumentParser(description='Run training or testing')
    parser.add_argument('mode', choices=['train', 'test'], help='Mode to run')
    
    # Training arguments
    parser.add_argument('--runs-per-network', type=int, default=5,
                      help='Number of runs per network')
    parser.add_argument('--run-id', type=str, default='Normal',
                      help='Run identifier')
    parser.add_argument('--networks', type=str, default='Normal',
                      help='Comma-separated list of networks to train')
    
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
            train(args.run_id, args.runs_per_network, networks)
        elif args.mode == "test":
            # TODO: add option for user to specify model name, model file, test type, duration
            test("neurips_3", "My Behavior.onnx", "Perturbation", 600)
        else:
            print(f"Error: Unknown mode '{args.mode}'")
            print_usage()
            sys.exit(1)
    except Exception as e:
        print(f"Error occurred while running {args.mode}: {str(e)}")
        sys.exit(1)
