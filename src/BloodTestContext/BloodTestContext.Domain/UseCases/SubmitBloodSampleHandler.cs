using BloodTestContext.Domain.Models;
using BloodTestContext.Domain.Ports;

namespace BloodTestContext.Domain.UseCases;

public static class SubmitBloodSampleHandler
{
    public static Task<Result> Handle(
        SubmitBloodSampleCommand command,
        IBloodSampleEvaluationRepository repository)
    {
        var shox2Result = MethylationValue.Create(command.Shox2MethylationValue);
        
        if (shox2Result.IsFailure)
            return Task.FromResult(Result.Failure(shox2Result.Error));

        var ptger4Result = MethylationValue.Create(command.Ptger4MethylationValue);
        
        if (ptger4Result.IsFailure)
            return Task.FromResult(Result.Failure(ptger4Result.Error));

        return Task.FromResult(Result.Success());
    }
}
