# QBloodTestForDetection

Lung cancer detection from blood tests using quantum machine learning classification.

## The Problem

Lung cancer kills 1.8 million people annually worldwide. Despite CT screening being available for over 8 years, only 25% of cases are diagnosed early — 75% are discovered at advanced stages where the 5-year survival rate drops from ~60% to ~6%. Current screening relies on CT scans that are costly, expose patients to radiation, and have strict eligibility criteria (age + smoking history), resulting in only ~10% enrollment among eligible individuals.

## The Approach

This project implements a **liquid biopsy classification pipeline** that analyzes DNA methylation biomarkers from a simple blood draw, enhanced by a quantum circuit for classification. It is based on the research described in [Quantum-Enhanced Lung Cancer Detection](https://mugiwarai.org/blog/quantum-cancer-poumon/), inspired by the collaboration between Cleveland Clinic and IBM Quantum.

### Biomarkers

Cancer cells release cell-free DNA (cfDNA) into the bloodstream carrying abnormal methylation patterns. Two markers are analyzed:

| Biomarker | Role |
|-----------|------|
| **SHOX2 methylation** | Hypermethylated in 60-80% of lung cancers |
| **PTGER4 methylation** | Combined with SHOX2, improves detection accuracy |

Methylation values are normalized to [0, 1]. Normal levels sit below 0.3 (30%); values above indicate hypermethylation — a signal that tumor suppressor genes may be silenced.

### Pipeline

```
Blood sample → cfDNA extraction → SHOX2/PTGER4 methylation measurement → normalization [0,1] → quantum circuit classification → risk stratification → clinical recommendation
```

### Quantum Circuit (Q#)

The original research uses Qiskit (Python). This project reimplements the quantum classification circuit in **Q#** with the following phases:

1. **Encoding (RY gates)** — biomarker values are converted to rotation angles (`angle = value * pi`) and applied as RY rotations on qubits, mapping each biomarker onto the Bloch sphere
2. **Correlation (CNOT entanglement)** — a CNOT gate captures the interdependency between SHOX2 and PTGER4, leveraging quantum entanglement instead of classical matrix multiplication
3. **Classification (parameterized rotation)** — a trained RY rotation on the control qubit acts as the decision boundary, optimized on labeled data
4. **Measurement (1000 shots)** — qubit collapse produces a probability distribution; the ratio of |1> outcomes gives the cancer risk probability

The quantum advantage becomes significant as biomarker count grows: 50 biomarkers produce 2^50 (~10^15) combinations, infeasible classically but handled natively by 50 qubits via superposition.

### Risk Stratification

| Probability | Urgency | Recommendation |
|-------------|---------|----------------|
| > 70% | High | Urgent CT scan |
| 50-70% | Medium | CT scan within 30 days |
| 30-50% | Low | Surveillance, retest in 6 months |
| < 30% | Normal | Standard annual screening |

## Tech Stack

- **.NET 10** — domain logic, API, hexagonal architecture
- **Q#** — quantum circuit operations (encoding, entanglement, classification)
- **xUnit + FluentAssertions** — BDD-style testing
- **Stryker** — mutation testing with 100% kill threshold

## Getting Started

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run mutation testing
dotnet stryker -f src/BloodTestContext/BloodTestContext.Domain.Tests/stryker-config.json
```

## References

- [Quantum-Enhanced Lung Cancer Detection (mugiwarai.org)](https://mugiwarai.org/blog/quantum-cancer-poumon/)
- Cleveland Clinic & IBM Quantum collaboration on lung cancer liquid biopsy classification
- IBM Quantum Heron processor — first private quantum computer in the US dedicated to health research
