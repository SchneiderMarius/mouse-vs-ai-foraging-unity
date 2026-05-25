import argparse
import os
import onnx
import numpy as np

def convert_onnx_to_nn(onnx_path: str, nn_output_path: str):
    # Load and check the ONNX model
    print(f"Loading ONNX model from: {onnx_path}")
    model = onnx.load(onnx_path)
    onnx.checker.check_model(model)

    # Serialize ONNX to bytes
    model_bytes = model.SerializeToString()

    # Save as .nn format
    with open(nn_output_path, "wb") as f:
        f.write(model_bytes)

    print(f"✅ Conversion complete: Saved .nn file to {nn_output_path}")

def main():
    parser = argparse.ArgumentParser(description="Convert ONNX model to Barracuda-compatible .nn format")
    parser.add_argument("--onnx", type=str, required=True, help="Path to .onnx model")
    parser.add_argument("--output", type=str, required=True, help="Path to save .nn model")
    args = parser.parse_args()

    if not os.path.exists(args.onnx):
        raise FileNotFoundError(f"ONNX file not found: {args.onnx}")

    output_dir = os.path.dirname(args.output)
    if output_dir and not os.path.exists(output_dir):
        os.makedirs(output_dir)

    convert_onnx_to_nn(args.onnx, args.output)

if __name__ == "__main__":
    main()


# # ✅ Example usage
# if __name__ == "__main__":
#     ONNX_PATH = "./results/neurips_3/My Behavior.onnx"
#     OUTPUT_DIR = "converted_models"
#     STREAMING_ASSETS = "./Builds/Perturbation1/Assets/StreamingAssets"

#     # os.makedirs(OUTPUT_DIR, exist_ok=True)
#     convert_onnx_to_nn(ONNX_PATH, OUTPUT_DIR)
