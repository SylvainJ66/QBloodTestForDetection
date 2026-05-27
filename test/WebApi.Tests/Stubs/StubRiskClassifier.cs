using BloodTestContext.Domain.Models;
using BloodTestContext.Domain.Ports;

namespace WebApi.Tests.Stubs;

public class StubRiskClassifier(double probability) : IRiskClassifier
{
    public CalibrationStatus CalibrationStatus => CalibrationStatus.Calibrated;

    public Task<double> ClassifyAsync(MethylationValue shox2, MethylationValue ptger4)
        => Task.FromResult(probability);
}
