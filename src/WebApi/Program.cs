using BloodTestContext.Domain.Ports;
using BloodTestContext.Domain.UseCases;
using BloodTestContext.Infrastructure.Classifiers;
using BloodTestContext.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

var repoRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".."));
var pythonPath = builder.Configuration["Quantum:PythonPath"] ?? Path.Combine(repoRoot, ".venv", "bin", "python3");
var scriptPath = builder.Configuration["Quantum:ScriptPath"]
    ?? Path.Combine(repoRoot, "src", "BloodTestContext", "BloodTestContext.Quantum", "classify.py");

builder.Services.AddSingleton<IRiskClassifier>(new QuantumRiskClassifier(pythonPath, scriptPath));
builder.Services.AddSingleton<IBloodSampleEvaluationRepository, InMemoryBloodSampleEvaluationRepository>();

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
