using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using BloodTestContext.Domain.Ports;
using WebApi.Tests.Stubs;

namespace WebApi.Tests;

public class SubmitBloodSampleApiTest
{
    private HttpClient CreateClient(IRiskClassifier classifier, IBloodSampleEvaluationRepository repository)
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(classifier);
                    services.AddSingleton(repository);
                });
            });

        return factory.CreateClient();
    }

    [Fact]
    public async Task Submitting_a_blood_sample_via_the_API_should_return_the_risk_level_recommendation_and_evaluation_id()
    {
        var classifier = new StubRiskClassifier(probability: 0.75);
        var repository = new InMemoryBloodSampleEvaluationRepository();
        using var client = CreateClient(classifier, repository);

        var response = await client.PostAsJsonAsync("/api/blood-samples/evaluate", new
        {
            Shox2MethylationValue = 0.78,
            Ptger4MethylationValue = 0.85,
            Rassf1aMethylationValue = 0.72,
            ApcMethylationValue = 0.80,
            Cdh13MethylationValue = 0.68
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<EvaluationResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.RiskLevel.Should().Be("High");
        body.Recommendation.Should().Be("Urgent CT scan recommended");
    }

    [Fact]
    public async Task Submitting_a_biomarker_value_outside_the_valid_range_via_the_API_should_return_a_client_error_specifying_which_biomarker_is_invalid()
    {
        var classifier = new StubRiskClassifier(probability: 0.0);
        var repository = new InMemoryBloodSampleEvaluationRepository();
        using var client = CreateClient(classifier, repository);

        var response = await client.PostAsJsonAsync("/api/blood-samples/evaluate", new
        {
            Shox2MethylationValue = 1.5,
            Ptger4MethylationValue = 0.5,
            Rassf1aMethylationValue = 0.5,
            ApcMethylationValue = 0.5,
            Cdh13MethylationValue = 0.5
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Error.Should().Contain("SHOX2");
        body.Error.Should().Contain("between 0 and 1");
    }
}

public record EvaluationResponse(Guid Id, string RiskLevel, string Recommendation);
public record ErrorResponse(string Error);
