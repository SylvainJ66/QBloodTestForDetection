using BloodTestContext.Domain.Models;
using BloodTestContext.Domain.Tests.Stubs;
using BloodTestContext.Domain.UseCases;

namespace BloodTestContext.Domain.Tests.UseCases;

public class SubmitBloodSampleTest
{
    private readonly StubRiskClassifier _classifier = new(probability: 0.0);
    private readonly InMemoryBloodSampleEvaluationRepository _repository = new();

    [Fact]
    public async Task Submitting_a_sample_with_a_SHOX2_methylation_value_below_the_biological_range_should_be_refused()
    {
        var command = new SubmitBloodSampleCommand(
            Shox2MethylationValue: -0.05, Ptger4MethylationValue: 0.5,
            Rassf1aMethylationValue: 0.5, ApcMethylationValue: 0.5, Cdh13MethylationValue: 0.5);

        var result = await SubmitBloodSampleHandler.Handle(command, _classifier, _repository);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("SHOX2 methylation value must be between 0 and 1");
        _repository.SavedEvaluations.Should().BeEmpty();
    }

    [Fact]
    public async Task Submitting_a_sample_with_a_PTGER4_methylation_value_above_the_biological_range_should_be_refused()
    {
        var command = new SubmitBloodSampleCommand(
            Shox2MethylationValue: 0.5, Ptger4MethylationValue: 1.20,
            Rassf1aMethylationValue: 0.5, ApcMethylationValue: 0.5, Cdh13MethylationValue: 0.5);

        var result = await SubmitBloodSampleHandler.Handle(command, _classifier, _repository);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("PTGER4 methylation value must be between 0 and 1");
        _repository.SavedEvaluations.Should().BeEmpty();
    }

    [Fact]
    public async Task Submitting_a_sample_with_an_undefined_SHOX2_methylation_value_should_be_refused_because_both_biomarkers_are_required()
    {
        var command = new SubmitBloodSampleCommand(
            Shox2MethylationValue: null, Ptger4MethylationValue: 0.5,
            Rassf1aMethylationValue: 0.5, ApcMethylationValue: 0.5, Cdh13MethylationValue: 0.5);

        var result = await SubmitBloodSampleHandler.Handle(command, _classifier, _repository);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("SHOX2 methylation value is required");
        _repository.SavedEvaluations.Should().BeEmpty();
    }

    [Fact]
    public async Task Submitting_a_sample_with_a_RASSF1A_methylation_value_above_the_biological_range_should_be_refused()
    {
        var command = new SubmitBloodSampleCommand(
            Shox2MethylationValue: 0.5, Ptger4MethylationValue: 0.5,
            Rassf1aMethylationValue: 1.10, ApcMethylationValue: 0.5, Cdh13MethylationValue: 0.5);

        var result = await SubmitBloodSampleHandler.Handle(command, _classifier, _repository);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("RASSF1A methylation value must be between 0 and 1");
        _repository.SavedEvaluations.Should().BeEmpty();
    }

    [Fact]
    public async Task Submitting_a_sample_with_an_undefined_APC_methylation_value_should_be_refused_because_all_biomarkers_are_required()
    {
        var command = new SubmitBloodSampleCommand(
            Shox2MethylationValue: 0.5, Ptger4MethylationValue: 0.5,
            Rassf1aMethylationValue: 0.5, ApcMethylationValue: null, Cdh13MethylationValue: 0.5);

        var result = await SubmitBloodSampleHandler.Handle(command, _classifier, _repository);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("APC methylation value is required");
        _repository.SavedEvaluations.Should().BeEmpty();
    }

    [Fact]
    public async Task Submitting_a_sample_with_a_CDH13_methylation_value_below_the_biological_range_should_be_refused()
    {
        var command = new SubmitBloodSampleCommand(
            Shox2MethylationValue: 0.5, Ptger4MethylationValue: 0.5,
            Rassf1aMethylationValue: 0.5, ApcMethylationValue: 0.5, Cdh13MethylationValue: -0.01);

        var result = await SubmitBloodSampleHandler.Handle(command, _classifier, _repository);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("CDH13 methylation value must be between 0 and 1");
        _repository.SavedEvaluations.Should().BeEmpty();
    }

    [Fact]
    public async Task A_blood_sample_classified_above_70_percent_risk_should_produce_a_high_risk_evaluation_with_urgent_CT_scan_recommendation()
    {
        var command = new SubmitBloodSampleCommand(0.78, 0.85, 0.72, 0.80, 0.68);
        var classifier = new StubRiskClassifier(probability: 0.75);

        var result = await SubmitBloodSampleHandler.Handle(command, classifier, _repository);

        result.IsSuccess.Should().BeTrue();
        var evaluation = _repository.SavedEvaluations.Should().ContainSingle().Subject;
        evaluation.Shox2Methylation.Value.Should().Be(0.78);
        evaluation.Ptger4Methylation.Value.Should().Be(0.85);
        evaluation.Rassf1aMethylation.Value.Should().Be(0.72);
        evaluation.ApcMethylation.Value.Should().Be(0.80);
        evaluation.Cdh13Methylation.Value.Should().Be(0.68);
        evaluation.RiskLevel.Should().Be(RiskLevel.High);
        evaluation.Recommendation.Should().Be("Urgent CT scan recommended");
        evaluation.EvaluatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task A_blood_sample_classified_between_50_and_70_percent_risk_should_produce_a_moderate_risk_evaluation_with_CT_scan_within_30_days()
    {
        var command = new SubmitBloodSampleCommand(0.55, 0.60, 0.50, 0.55, 0.48);
        var classifier = new StubRiskClassifier(probability: 0.60);

        var result = await SubmitBloodSampleHandler.Handle(command, classifier, _repository);

        result.IsSuccess.Should().BeTrue();
        var evaluation = _repository.SavedEvaluations.Should().ContainSingle().Subject;
        evaluation.Shox2Methylation.Value.Should().Be(0.55);
        evaluation.Ptger4Methylation.Value.Should().Be(0.60);
        evaluation.Rassf1aMethylation.Value.Should().Be(0.50);
        evaluation.ApcMethylation.Value.Should().Be(0.55);
        evaluation.Cdh13Methylation.Value.Should().Be(0.48);
        evaluation.RiskLevel.Should().Be(RiskLevel.Moderate);
        evaluation.Recommendation.Should().Be("CT scan within 30 days");
        evaluation.EvaluatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task A_blood_sample_classified_between_30_and_50_percent_risk_should_produce_a_low_risk_evaluation_with_retest_in_6_months()
    {
        var command = new SubmitBloodSampleCommand(0.35, 0.40, 0.32, 0.38, 0.30);
        var classifier = new StubRiskClassifier(probability: 0.40);

        var result = await SubmitBloodSampleHandler.Handle(command, classifier, _repository);

        result.IsSuccess.Should().BeTrue();
        var evaluation = _repository.SavedEvaluations.Should().ContainSingle().Subject;
        evaluation.Shox2Methylation.Value.Should().Be(0.35);
        evaluation.Ptger4Methylation.Value.Should().Be(0.40);
        evaluation.Rassf1aMethylation.Value.Should().Be(0.32);
        evaluation.ApcMethylation.Value.Should().Be(0.38);
        evaluation.Cdh13Methylation.Value.Should().Be(0.30);
        evaluation.RiskLevel.Should().Be(RiskLevel.Low);
        evaluation.Recommendation.Should().Be("Surveillance, retest in 6 months");
        evaluation.EvaluatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task A_blood_sample_classified_below_30_percent_risk_should_produce_a_normal_risk_evaluation_with_standard_annual_screening()
    {
        var command = new SubmitBloodSampleCommand(0.10, 0.15, 0.08, 0.12, 0.10);
        var classifier = new StubRiskClassifier(probability: 0.15);

        var result = await SubmitBloodSampleHandler.Handle(command, classifier, _repository);

        result.IsSuccess.Should().BeTrue();
        var evaluation = _repository.SavedEvaluations.Should().ContainSingle().Subject;
        evaluation.Shox2Methylation.Value.Should().Be(0.10);
        evaluation.Ptger4Methylation.Value.Should().Be(0.15);
        evaluation.Rassf1aMethylation.Value.Should().Be(0.08);
        evaluation.ApcMethylation.Value.Should().Be(0.12);
        evaluation.Cdh13Methylation.Value.Should().Be(0.10);
        evaluation.RiskLevel.Should().Be(RiskLevel.Normal);
        evaluation.Recommendation.Should().Be("Standard annual screening");
        evaluation.EvaluatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}
