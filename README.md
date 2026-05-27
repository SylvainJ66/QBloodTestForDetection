# QBloodTestForDetection

Lung cancer detection from blood tests using quantum circuit classification.

## The Problem

Lung cancer kills 1.8 million people annually worldwide. Despite CT screening being available for over 8 years, only 25% of cases are diagnosed early — 75% are discovered at advanced stages where the 5-year survival rate drops from ~60% to ~6%. Current screening relies on CT scans that are costly, expose patients to radiation, and have strict eligibility criteria (age + smoking history), resulting in only ~10% enrollment among eligible individuals.

## The Approach

This project implements a **liquid biopsy classification pipeline** that analyzes DNA methylation biomarkers from a simple blood draw, enhanced by a quantum circuit for classification. It is based on the research described in [Quantum-Enhanced Lung Cancer Detection](https://mugiwarai.org/blog/quantum-cancer-poumon/), inspired by the collaboration between Cleveland Clinic and IBM Quantum.

### Biomarkers

Cancer cells release cell-free DNA (cfDNA) into the bloodstream carrying abnormal methylation patterns. Five markers are analyzed:

| Biomarker | Role |
|-----------|------|
| **SHOX2 methylation** | Hypermethylated in 60-80% of lung cancers |
| **PTGER4 methylation** | Combined with SHOX2, improves detection accuracy |
| **RASSF1A methylation** | Tumor suppressor, hypermethylated in 30-50% of lung cancers |
| **APC methylation** | Adenomatous polyposis coli, methylation linked to NSCLC |
| **CDH13 methylation** | H-cadherin, methylation associated with lung cancer progression |

Methylation values are normalized to [0, 1]. Normal levels sit below 0.3 (30%); values above indicate hypermethylation — a signal that tumor suppressor genes may be silenced.

### Pipeline

```
Blood sample → cfDNA extraction → SHOX2/PTGER4/RASSF1A/APC/CDH13 methylation measurement → normalization [0,1] → quantum circuit classification (5 qubits) → risk stratification → clinical recommendation
```

### Quantum Circuit (Qiskit Aer)

The quantum classification circuit is implemented in **Python** using **Qiskit** with the **Aer simulator** backend, following the same approach as the original research:

```
        ┌──────────────┐          ┌───┐
q₀ |0⟩ ─┤ RY(π·shox2)  ├───●─────┤ M ├─→ c₀
        └──────────────┘   │     └───┘
        ┌──────────────┐ ┌─┴─┐   ┌───┐
q₁ |0⟩ ─┤ RY(π·ptger4) ├─┤ X ├─●─┤ M ├─→ c₁
        └──────────────┘ └───┘ │ └───┘
        ┌──────────────┐     ┌─┴─┐┌───┐
q₂ |0⟩ ─┤ RY(π·rassf1a)├─────┤ X ├┤ ● ├─→ c₂
        └──────────────┘     └───┘│└───┘
        ┌──────────────┐       ┌──┴┐┌───┐
q₃ |0⟩ ─┤ RY(π·apc)    ├───────┤ X ├┤ ● ├─→ c₃
        └──────────────┘       └───┘│└───┘
        ┌──────────────┐         ┌──┴┐┌───┐
q₄ |0⟩ ─┤ RY(π·cdh13)  ├─────────┤ X ├┤ M ├─→ c₄
        └──────────────┘         └───┘└───┘
         ── Encoding ──   ── Entanglement ──  Meas.
```

1. **Encoding (RY gates)** — each biomarker value is converted to a rotation angle (`angle = value * pi`) and applied as an RY rotation on its dedicated qubit, mapping all five markers onto the Bloch sphere
2. **Correlation (CNOT chain)** — a chain of CNOT gates (q₀→q₁→q₂→q₃→q₄) captures cascading correlations between all five biomarkers via quantum entanglement
3. **Classification (parameterized rotation)** — a trained RY rotation on the control qubit acts as the decision boundary, optimized on labeled data
4. **Measurement (1000 shots)** — qubit collapse produces a probability distribution; the ratio of outcomes containing |1⟩ gives the cancer risk probability

The quantum advantage becomes significant as biomarker count grows: 50 biomarkers produce 2^50 (~10^15) combinations, infeasible classically but handled natively by 50 qubits via superposition.

### Why Quantum is a Game Changer — The Cleveland Clinic Scale

This project uses 5 biomarkers (SHOX2, PTGER4, RASSF1A, APC, CDH13) — a scale where classical computation still handles classification without difficulty. The quantum circuit here is pedagogical: it demonstrates the encoding and classification pattern that becomes essential at larger biomarker scales.

The Cleveland Clinic & IBM Quantum collaboration operates at an entirely different scale. Their liquid biopsy research analyzes:

- **Fragmentomics** — over **40 million DNA fragments** across the genome, measuring cfDNA fragmentation patterns caused by dying cells. Detected 8 out of 10 lung cancers while cutting the number of CT scans needed by more than 50%.
- **Methylomics** — methylation levels at **~6 million genomic loci**, looking for cancer-specific epigenetic signatures. Detected 9 out of 10 lung cancers with the same 50%+ scan reduction.

At this scale, a **variational quantum classifier (VQC)** replaces our 5-qubit circuit with a deep, layered architecture — here illustrated with N biomarkers:

```
              ┌──────────┐┌───┐┌──────────┐       ┌───┐
q₀ (marker₁) ┤ RY(π·m₁) ├┤ ● ├┤ RY(θ₁)  ├─ ··· ─┤ M ├
              └──────────┘└─┬─┘└──────────┘       └───┘
              ┌──────────┐┌─┴─┐┌──────────┐       ┌───┐
q₁ (marker₂) ┤ RY(π·m₂) ├┤ X ├┤ RY(θ₂)  ├─ ··· ─┤ M ├
              └──────────┘└───┘└──────────┘       └───┘
              ┌──────────┐┌───┐┌──────────┐       ┌───┐
q₂ (marker₃) ┤ RY(π·m₃) ├┤ ● ├┤ RY(θ₃)  ├─ ··· ─┤ M ├
              └──────────┘└─┬─┘└──────────┘       └───┘
    ⋮              ⋮         ⋮        ⋮              ⋮
              ┌──────────┐┌─┴─┐┌──────────┐       ┌───┐
qₙ (markerₙ) ┤ RY(π·mₙ) ├┤ X ├┤ RY(θₙ)  ├─ ··· ─┤ M ├
              └──────────┘└───┘└──────────┘       └───┘
               ─ Encoding ─ Entanglement ─ × L ─  Meas.
```

Each biomarker gets its own qubit and RY encoding gate. CNOT chains entangle neighboring qubits, then parameterized RY(θ) rotations form trainable weights — this entanglement + rotation block repeats L times, building a deep classifier. With N qubits in superposition, the circuit explores 2^N states simultaneously: 50 biomarkers → ~10^15 combinations in a single pass; 6 million methylation loci → a state space no classical machine can enumerate.

The combinatorial explosion makes quantum computing genuinely transformative. Classifying across a 6-million-dimensional feature space means the interactions between loci grow combinatorially — classical approaches struggle with both compute cost and generalization. Quantum variational classifiers encode these millions of features into qubit rotations, and superposition lets the circuit explore an exponentially large state space (2^N for N qubits) in a single forward pass. Entanglement captures correlations between distant genomic loci that classical approaches would need explicit feature engineering to represent.

This is why Cleveland Clinic deployed the IBM Quantum System One — the first quantum computer dedicated to healthcare research — on their campus: not as a proof of concept, but because the dimensionality of real liquid biopsy data is where quantum algorithms start to deliver advantages that classical hardware cannot match.

### Risk Stratification

| Probability | Urgency | Recommendation |
|-------------|---------|----------------|
| > 70% | High | Urgent CT scan |
| 50-70% | Medium | CT scan within 30 days |
| 30-50% | Low | Surveillance, retest in 6 months |
| < 30% | Normal | Standard annual screening |

## Tech Stack

- **.NET 10** — domain logic, API, hexagonal architecture
- **Python / Qiskit Aer** — quantum circuit simulation (encoding, entanglement, classification)
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

## API Examples

The API exposes a single endpoint: `POST /api/blood-samples/evaluate`. It accepts five methylation values (normalized to [0, 1]), runs them through the 5-qubit quantum circuit, and returns a risk assessment.

### High risk — all five markers strongly hypermethylated

```http
POST /api/blood-samples/evaluate
Content-Type: application/json

{
  "Shox2MethylationValue": 0.78,
  "Ptger4MethylationValue": 0.85,
  "Rassf1aMethylationValue": 0.72,
  "ApcMethylationValue": 0.80,
  "Cdh13MethylationValue": 0.68
}
```

All five markers are far above the 0.30 normal threshold, indicating strong hypermethylation across multiple tumor suppressor genes — a pattern frequently observed in active lung tumors. The quantum circuit produces a high probability (typically >70%), triggering an **urgent CT scan recommendation**.

### Moderate risk — moderate hypermethylation

```http
POST /api/blood-samples/evaluate
Content-Type: application/json

{
  "Shox2MethylationValue": 0.45,
  "Ptger4MethylationValue": 0.50,
  "Rassf1aMethylationValue": 0.42,
  "ApcMethylationValue": 0.48,
  "Cdh13MethylationValue": 0.40
}
```

All markers are above normal but not dramatically elevated. This could indicate an early-stage cancer or a benign condition causing methylation changes. The quantum circuit returns a probability in the 50-70% range, recommending a **CT scan within 30 days** for further investigation.

### Low risk — mild hypermethylation

```http
POST /api/blood-samples/evaluate
Content-Type: application/json

{
  "Shox2MethylationValue": 0.35,
  "Ptger4MethylationValue": 0.30,
  "Rassf1aMethylationValue": 0.32,
  "ApcMethylationValue": 0.28,
  "Cdh13MethylationValue": 0.25
}
```

Values sit right at the edge of the normal threshold. The slight elevation could be noise, inflammation, or very early changes. The circuit returns a probability in the 30-50% range, recommending **surveillance with a retest in 6 months** rather than immediate imaging.

### Normal — values below threshold

```http
POST /api/blood-samples/evaluate
Content-Type: application/json

{
  "Shox2MethylationValue": 0.10,
  "Ptger4MethylationValue": 0.12,
  "Rassf1aMethylationValue": 0.08,
  "ApcMethylationValue": 0.11,
  "Cdh13MethylationValue": 0.09
}
```

All markers are well within the normal range (<30%). The cfDNA methylation pattern shows no sign of tumor activity. The circuit returns a low probability (<30%), and the patient continues with **standard annual screening**.

### Error — missing marker value

```http
POST /api/blood-samples/evaluate
Content-Type: application/json

{
  "Ptger4MethylationValue": 0.50,
  "Rassf1aMethylationValue": 0.50,
  "ApcMethylationValue": 0.50,
  "Cdh13MethylationValue": 0.50
}
```

All five biomarkers are required. The domain rejects incomplete submissions because the quantum circuit needs all five qubit inputs to produce a meaningful classification. Returns a `400 Bad Request` with an error message.

### Error — value out of biological range

```http
POST /api/blood-samples/evaluate
Content-Type: application/json

{
  "Shox2MethylationValue": 1.5,
  "Ptger4MethylationValue": 0.50,
  "Rassf1aMethylationValue": 0.50,
  "ApcMethylationValue": 0.50,
  "Cdh13MethylationValue": 0.50
}
```

Methylation values must be between 0 and 1 (0% to 100%). A value of 1.5 is biologically impossible — it would mean 150% methylation. The domain validates this invariant and returns a `400 Bad Request` before reaching the quantum circuit.

## Architecture Decision Records

- [ADR-0001: Use Python Qiskit Aer instead of Q#](docs/adr/0001-use-python-qiskit-aer-instead-of-qsharp.md)

## References

- [Quantum-Enhanced Lung Cancer Detection (mugiwarai.org)](https://mugiwarai.org/blog/quantum-cancer-poumon/)
- Cleveland Clinic & IBM Quantum collaboration on lung cancer liquid biopsy classification
- IBM Quantum Heron processor — first private quantum computer in the US dedicated to health research
