using BloodTestContext.Domain.Models;
using BloodTestContext.Domain.Ports;

namespace BloodTestContext.Domain.UseCases;

public static class SubmitBloodSampleHandler
{
    public static async Task<Result<BloodSampleEvaluation>> Handle(
        SubmitBloodSampleCommand command,
        IRiskClassifier classifier,
        IBloodSampleEvaluationRepository repository)
    {
        var shox2Result = MethylationValue.Create(command.Shox2MethylationValue, "SHOX2");

        if (shox2Result.IsFailure)
            return Result.Failure<BloodSampleEvaluation>(shox2Result.Error);

        var ptger4Result = MethylationValue.Create(command.Ptger4MethylationValue, "PTGER4");

        if (ptger4Result.IsFailure)
            return Result.Failure<BloodSampleEvaluation>(ptger4Result.Error);

        var probability = await classifier.ClassifyAsync(shox2Result.Value, ptger4Result.Value);
        var assessment = RiskAssessment.FromProbability(probability);

        var evaluation = new BloodSampleEvaluation(
            Guid.NewGuid(),
            shox2Result.Value,
            ptger4Result.Value,
            assessment.RiskLevel,
            assessment.Recommendation,
            classifier.CalibrationStatus,
            DateTimeOffset.UtcNow);

        await repository.SaveAsync(evaluation);

        return Result.Success(evaluation);
    }
}
