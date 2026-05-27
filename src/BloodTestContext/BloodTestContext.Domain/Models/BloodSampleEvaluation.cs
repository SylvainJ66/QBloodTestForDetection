namespace BloodTestContext.Domain.Models;

public record BloodSampleEvaluation(
    Guid Id,
    MethylationValue Shox2Methylation,
    MethylationValue Ptger4Methylation,
    RiskLevel RiskLevel,
    string Recommendation,
    DateTimeOffset EvaluatedAt);
