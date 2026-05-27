using BloodTestContext.Domain.Ports;

namespace BloodTestContext.Domain.Tests.Stubs;

public class InMemoryBloodSampleEvaluationRepository : IBloodSampleEvaluationRepository
{
    public List<object> SavedEvaluations { get; } = [];

    public Task SaveAsync(object evaluation)
    {
        SavedEvaluations.Add(evaluation);
        return Task.CompletedTask;
    }
}
