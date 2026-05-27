using BloodTestContext.Domain.Tests.Stubs;
using BloodTestContext.Domain.UseCases;

namespace BloodTestContext.Domain.Tests.UseCases;

public class SubmitBloodSampleTest
{
    private readonly InMemoryBloodSampleEvaluationRepository _repository = new();

    [Fact]
    public async Task Submitting_a_sample_with_a_SHOX2_methylation_value_below_the_biological_range_should_be_refused()
    {
        var command = new SubmitBloodSampleCommand(Shox2MethylationValue: -0.05, Ptger4MethylationValue: 0.5);

        var result = await SubmitBloodSampleHandler.Handle(command, _repository);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Methylation values must be between 0 and 1");
        _repository.SavedEvaluations.Should().BeEmpty();
    }

    [Fact]
    public async Task Submitting_a_sample_with_a_PTGER4_methylation_value_above_the_biological_range_should_be_refused()
    {
        var command = new SubmitBloodSampleCommand(Shox2MethylationValue: 0.5, Ptger4MethylationValue: 1.20);

        var result = await SubmitBloodSampleHandler.Handle(command, _repository);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Methylation values must be between 0 and 1");
        _repository.SavedEvaluations.Should().BeEmpty();
    }

    [Fact]
    public async Task Submitting_a_sample_with_an_undefined_SHOX2_methylation_value_should_be_refused_because_both_biomarkers_are_required()
    {
        var command = new SubmitBloodSampleCommand(Shox2MethylationValue: null, Ptger4MethylationValue: 0.5);

        var result = await SubmitBloodSampleHandler.Handle(command, _repository);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Both biomarkers are required");
        _repository.SavedEvaluations.Should().BeEmpty();
    }
}
