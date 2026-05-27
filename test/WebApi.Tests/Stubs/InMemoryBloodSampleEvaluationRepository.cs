using BloodTestContext.Domain.Models;
using BloodTestContext.Domain.Ports;

namespace WebApi.Tests.Stubs;

public class InMemoryBloodSampleEvaluationRepository : IBloodSampleEvaluationRepository
{
    public List<BloodSampleEvaluation> SavedEvaluations { get; } = [];

    public Task SaveAsync(BloodSampleEvaluation evaluation)
    {
        SavedEvaluations.Add(evaluation);
        return Task.CompletedTask;
    }
}
