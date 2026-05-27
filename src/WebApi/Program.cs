using BloodTestContext.Domain.Ports;
using BloodTestContext.Domain.UseCases;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapPost("/api/blood-samples/evaluate", async (
    SubmitBloodSampleRequest request,
    IRiskClassifier classifier,
    IBloodSampleEvaluationRepository repository) =>
{
    var command = new SubmitBloodSampleCommand(request.Shox2MethylationValue, request.Ptger4MethylationValue);
    var result = await SubmitBloodSampleHandler.Handle(command, classifier, repository);

    if (result.IsFailure)
        return Results.BadRequest(new { Error = result.Error });

    var evaluation = result.Value;
    return Results.Ok(new
    {
        evaluation.Id,
        RiskLevel = evaluation.RiskLevel.ToString(),
        evaluation.Recommendation
    });
});

app.Run();

public record SubmitBloodSampleRequest(double? Shox2MethylationValue, double? Ptger4MethylationValue);
