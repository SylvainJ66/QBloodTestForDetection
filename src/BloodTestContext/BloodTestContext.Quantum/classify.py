import sys
import math
from qiskit import QuantumCircuit
from qiskit_aer import AerSimulator

def classify(shox2: float, ptger4: float, shots: int = 1000) -> float:
    qc = QuantumCircuit(2, 2)

    qc.ry(math.pi * shox2, 0)
    qc.ry(math.pi * ptger4, 1)
    qc.cx(0, 1)

    qc.measure([0, 1], [0, 1])

    simulator = AerSimulator()
    result = simulator.run(qc, shots=shots).result()
    counts = result.get_counts()

    positive = sum(count for outcome, count in counts.items() if "1" in outcome)
    return positive / shots

if __name__ == "__main__":
    shox2 = float(sys.argv[1])
    ptger4 = float(sys.argv[2])
    shots = int(sys.argv[3]) if len(sys.argv) > 3 else 1000
    print(classify(shox2, ptger4, shots))
