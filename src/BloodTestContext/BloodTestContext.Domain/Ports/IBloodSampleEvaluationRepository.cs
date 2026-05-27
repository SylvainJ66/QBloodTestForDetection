using BloodTestContext.Domain.Models;

namespace BloodTestContext.Domain.Ports;

public interface IBloodSampleEvaluationRepository
{
    Task SaveAsync(BloodSampleEvaluation evaluation);
}
