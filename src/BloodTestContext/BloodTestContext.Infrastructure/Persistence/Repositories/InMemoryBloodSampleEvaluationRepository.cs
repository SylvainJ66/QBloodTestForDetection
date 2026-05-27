using BloodTestContext.Domain.Models;
using BloodTestContext.Domain.Ports;

namespace BloodTestContext.Infrastructure.Persistence.Repositories;

public class InMemoryBloodSampleEvaluationRepository : IBloodSampleEvaluationRepository
{
    private readonly List<BloodSampleEvaluation> _evaluations = [];

    public Task SaveAsync(BloodSampleEvaluation evaluation)
    {
        _evaluations.Add(evaluation);
        return Task.CompletedTask;
    }
}
