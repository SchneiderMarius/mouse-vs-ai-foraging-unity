# Mouse vs. AI: Foraging Environment (Unity)

Unity source project for the [Mouse vs. AI benchmark](https://robustforaging.github.io) — a NeurIPS 2025 competition benchmarking visual robustness and neural alignment in RL agents.

## Requirements

| Dependency | Version |
|---|---|
| Unity Editor | **2019.4.40f1** |
| ML-Agents package | **2.0.1** (`com.unity.ml-agents`) |
| Barracuda package | **3.0.1** (`com.unity.barracuda`) |

Unity 2019.4 LTS is required. Other versions are not tested and may break the ML-Agents integration.

## Setup

1. Install **Unity 2019.4.40f1** via [Unity Hub](https://unity.com/download).
2. Clone this repository:
   ```bash
   git clone https://github.com/SchneiderMarius/mouse-vs-ai-foraging-unity.git
   ```
3. Open the project in Unity Hub: **Add → Select the cloned folder**.
4. Unity will resolve the package dependencies automatically via the Package Manager (ML-Agents 2.0.1 and Barracuda 3.0.1 are fetched from the Unity registry).

## What's included

- Full Unity environment used in the Mouse vs. AI: Robust Foraging Competition
- Agent controller scripts (`Assets/Scripts/`)
- Training configuration (`mouse.yml`)
- Python training entry point (`train.py`, requires `mlagents` Python package)

## Training

Install the ML-Agents Python package:
```bash
pip install mlagents==0.27.0
```

Start training:
```bash
mlagents-learn mouse.yml --run-id=my_run
```

The compiled environment build is required for training — see the [competition website](https://robustforaging.github.io) for pre-built binaries (Windows / macOS / Linux).

## Links

- **Competition website:** [https://robustforaging.github.io](https://robustforaging.github.io)
- **Benchmark evaluation repo (Track 2 — Neural Alignment):** [https://github.com/SchneiderMarius/mouse-vs-ai-benchmark](https://github.com/SchneiderMarius/mouse-vs-ai-benchmark)

## Citation

```bibtex
@inproceedings{schneider2025mousevsai,
  title     = {Mouse vs.\ {AI}: A Benchmark for Visual Robustness and Neural Alignment},
  author    = {Schneider, Marius and Canzano, Joe and Hou, Yuchen and Peng, Jing
               and Smith, Spencer LaVere and Beyeler, Michael},
  booktitle = {NeurIPS --- Evaluations \& Datasets Track},
  year      = {2026},
  url       = {https://robustforaging.github.io}
}
```

## License

[MIT](LICENSE)
