namespace BloodTestContext.Domain.Models;

public record BloodSampleEvaluation(
    Guid Id,
    MethylationValue Shox2Methylation,
    MethylationValue Ptger4Methylation,
    MethylationValue Rassf1aMethylation,
    MethylationValue ApcMethylation,
    MethylationValue Cdh13Methylation,
    RiskLevel RiskLevel,
    string Recommendation,
    CalibrationStatus CalibrationStatus,
    DateTimeOffset EvaluatedAt);
