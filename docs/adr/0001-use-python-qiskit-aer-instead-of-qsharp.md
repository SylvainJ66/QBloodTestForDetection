# ADR-0001: Use Python Qiskit Aer instead of Q# for quantum classification

**Date:** 2026-05-27

**Status:** Accepted

## Context

The quantum classification circuit encodes SHOX2 and PTGER4 methylation biomarkers into qubit rotations (RY gates), captures their correlation via entanglement (CNOT), applies a parameterized decision boundary, and measures over 1000 shots to produce a cancer risk probability.

The original research from Cleveland Clinic and IBM Quantum uses Qiskit (Python). Our initial plan was to reimplement the circuit in Q#, Microsoft's quantum programming language.

## Decision

We will use **Python with Qiskit and the Aer simulator backend** instead of Q# for all quantum circuit operations.

## Consequences

### Advantages

- **Direct alignment with the reference research** — the Cleveland Clinic / IBM Quantum collaboration uses Qiskit. Using the same framework avoids translation errors and lets us validate our circuit against published results.
- **Ecosystem maturity** — Qiskit has the largest quantum computing community, the most tutorials, and the broadest hardware backend support (IBM Quantum, simulators, third-party providers).
- **Aer simulator** — provides high-performance local simulation with noise models, statevector inspection, and shot-based measurement, sufficient for development and testing without access to real quantum hardware.
- **Hardware path** — when the project scales beyond 2 biomarkers, Qiskit circuits can run on IBM Quantum hardware with minimal changes. Q# targets Azure Quantum, which has a smaller set of available backends.
- **Python ML ecosystem** — easier integration with NumPy, SciPy, and scikit-learn for hybrid classical-quantum optimization of the parameterized rotation layer.

### Trade-offs

- **Two runtimes** — the domain logic runs on .NET while the quantum circuit runs on Python. The infrastructure layer must bridge this boundary (e.g., via a Python subprocess call or a REST microservice).
- **No Q# type safety** — Q# provides compile-time guarantees around qubit allocation and measurement that Python does not. We accept this trade-off given the circuit's small size (2 qubits).
