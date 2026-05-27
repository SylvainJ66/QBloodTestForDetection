import sys
import math
from qiskit import QuantumCircuit
from qiskit_aer import AerSimulator

def classify(shox2: float, ptger4: float, rassf1a: float, apc: float, cdh13: float, shots: int = 1000) -> float:
    qc = QuantumCircuit(5, 5)

    qc.ry(math.pi * shox2, 0)
    qc.ry(math.pi * ptger4, 1)
    qc.ry(math.pi * rassf1a, 2)
    qc.ry(math.pi * apc, 3)
    qc.ry(math.pi * cdh13, 4)

    qc.cx(0, 1)
    qc.cx(1, 2)
    qc.cx(2, 3)
    qc.cx(3, 4)

    qc.measure([0, 1, 2, 3, 4], [0, 1, 2, 3, 4])

    simulator = AerSimulator()
    result = simulator.run(qc, shots=shots).result()
    counts = result.get_counts()

    positive = sum(count for outcome, count in counts.items() if "1" in outcome)
    return positive / shots

if __name__ == "__main__":
    shox2 = float(sys.argv[1])
    ptger4 = float(sys.argv[2])
    rassf1a = float(sys.argv[3])
    apc = float(sys.argv[4])
    cdh13 = float(sys.argv[5])
    shots = int(sys.argv[6]) if len(sys.argv) > 6 else 1000
    print(classify(shox2, ptger4, rassf1a, apc, cdh13, shots))
