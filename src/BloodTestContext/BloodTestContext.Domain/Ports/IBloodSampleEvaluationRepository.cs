namespace BloodTestContext.Domain.Ports;

public interface IBloodSampleEvaluationRepository
{
    Task SaveAsync(object evaluation);
}
