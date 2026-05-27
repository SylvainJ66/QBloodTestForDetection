namespace BloodTestContext.Domain.Models;

public record RiskAssessment(RiskLevel RiskLevel, string Recommendation)
{
    public static RiskAssessment FromProbability(double probability) => probability switch
    {
        > 0.70 => new RiskAssessment(RiskLevel.High, "Urgent CT scan recommended"),
        > 0.50 => new RiskAssessment(RiskLevel.Moderate, "CT scan within 30 days"),
        > 0.30 => new RiskAssessment(RiskLevel.Low, "Surveillance, retest in 6 months"),
        _ => new RiskAssessment(RiskLevel.Normal, "Standard annual screening")
    };
}
