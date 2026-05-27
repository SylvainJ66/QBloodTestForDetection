using BloodTestContext.Domain.Models;
using BloodTestContext.Domain.Ports;

namespace BloodTestContext.Domain.Tests.Stubs;

public class StubRiskClassifier(double probability) : IRiskClassifier
{
    public Task<double> ClassifyAsync(MethylationValue shox2, MethylationValue ptger4)
        => Task.FromResult(probability);
}
