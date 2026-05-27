using BloodTestContext.Domain.Models;
using BloodTestContext.Domain.UseCases;
using BloodTestContext.Infrastructure.Classifiers;
using BloodTestContext.Infrastructure.Tests.Stubs;

namespace BloodTestContext.Infrastructure.Tests;

public class QuantumClassifierTest
{
    private static string FindRepositoryRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, ".git")))
            dir = Directory.GetParent(dir)?.FullName;
        return dir ?? throw new InvalidOperationException("Repository root not found");
    }

    private static QuantumRiskClassifier CreateClassifier()
    {
        var root = FindRepositoryRoot();
        var pythonPath = Path.Combine(root, ".venv", "bin", "python3");
        var scriptPath = Path.Combine(root, "src", "BloodTestContext", "BloodTestContext.Quantum", "classify.py");
        return new QuantumRiskClassifier(pythonPath, scriptPath);
    }

    [Fact]
    public async Task Submitting_a_blood_sample_to_the_quantum_classifier_should_produce_a_coherent_risk_evaluation()
    {
        var classifier = CreateClassifier();
        var repository = new InMemoryBloodSampleEvaluationRepository();
        var command = new SubmitBloodSampleCommand(
            Shox2MethylationValue: 0.78, Ptger4MethylationValue: 0.85,
            Rassf1aMethylationValue: 0.72, ApcMethylationValue: 0.80, Cdh13MethylationValue: 0.68);

        var result = await SubmitBloodSampleHandler.Handle(command, classifier, repository);

        result.IsSuccess.Should().BeTrue();
        var evaluation = repository.SavedEvaluations.Should().ContainSingle().Subject;
        evaluation.Shox2Methylation.Value.Should().Be(0.78);
        evaluation.Ptger4Methylation.Value.Should().Be(0.85);
        evaluation.Rassf1aMethylation.Value.Should().Be(0.72);
        evaluation.ApcMethylation.Value.Should().Be(0.80);
        evaluation.Cdh13Methylation.Value.Should().Be(0.68);
        evaluation.RiskLevel.Should().NotBe(RiskLevel.Normal,
            "elevated biomarkers should produce elevated risk");
    }

    [Fact]
    public async Task The_quantum_classifier_should_produce_the_same_risk_level_for_identical_biomarker_submissions()
    {
        var classifier = CreateClassifier();
        var command = new SubmitBloodSampleCommand(
            Shox2MethylationValue: 0.55, Ptger4MethylationValue: 0.60,
            Rassf1aMethylationValue: 0.50, ApcMethylationValue: 0.55, Cdh13MethylationValue: 0.48);

        var evaluations = new List<BloodSampleEvaluation>();
        for (var i = 0; i < 3; i++)
        {
            var repository = new InMemoryBloodSampleEvaluationRepository();
            var result = await SubmitBloodSampleHandler.Handle(command, classifier, repository);
            result.IsSuccess.Should().BeTrue();
            evaluations.Add(repository.SavedEvaluations.Single());
        }

        evaluations.Should().AllSatisfy(e =>
        {
            e.RiskLevel.Should().Be(evaluations[0].RiskLevel);
            e.Recommendation.Should().Be(evaluations[0].Recommendation);
        });
    }

    [Fact]
    public async Task Higher_biomarker_values_should_produce_a_risk_level_equal_to_or_higher_than_lower_biomarker_values()
    {
        var classifier = CreateClassifier();

        var lowRepository = new InMemoryBloodSampleEvaluationRepository();
        var lowCommand = new SubmitBloodSampleCommand(
            Shox2MethylationValue: 0.10, Ptger4MethylationValue: 0.15,
            Rassf1aMethylationValue: 0.08, ApcMethylationValue: 0.12, Cdh13MethylationValue: 0.10);
        var lowResult = await SubmitBloodSampleHandler.Handle(lowCommand, classifier, lowRepository);
        lowResult.IsSuccess.Should().BeTrue();

        var highRepository = new InMemoryBloodSampleEvaluationRepository();
        var highCommand = new SubmitBloodSampleCommand(
            Shox2MethylationValue: 0.78, Ptger4MethylationValue: 0.85,
            Rassf1aMethylationValue: 0.72, ApcMethylationValue: 0.80, Cdh13MethylationValue: 0.68);
        var highResult = await SubmitBloodSampleHandler.Handle(highCommand, classifier, highRepository);
        highResult.IsSuccess.Should().BeTrue();

        var lowEvaluation = lowRepository.SavedEvaluations.Single();
        var highEvaluation = highRepository.SavedEvaluations.Single();

        ((int)highEvaluation.RiskLevel).Should().BeGreaterThanOrEqualTo((int)lowEvaluation.RiskLevel);
    }

    [Fact]
    public async Task An_assessment_produced_by_the_quantum_classifier_should_be_marked_as_not_clinically_calibrated()
    {
        var classifier = CreateClassifier();
        var repository = new InMemoryBloodSampleEvaluationRepository();
        var command = new SubmitBloodSampleCommand(
            Shox2MethylationValue: 0.50, Ptger4MethylationValue: 0.50,
            Rassf1aMethylationValue: 0.50, ApcMethylationValue: 0.50, Cdh13MethylationValue: 0.50);

        var result = await SubmitBloodSampleHandler.Handle(command, classifier, repository);

        result.IsSuccess.Should().BeTrue();
        var evaluation = repository.SavedEvaluations.Should().ContainSingle().Subject;
        evaluation.CalibrationStatus.Should().Be(CalibrationStatus.Experimental);
    }
}
