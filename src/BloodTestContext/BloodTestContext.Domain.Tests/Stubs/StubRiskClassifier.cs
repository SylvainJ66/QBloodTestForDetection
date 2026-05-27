using BloodTestContext.Domain.Models;
using BloodTestContext.Domain.Ports;

namespace BloodTestContext.Domain.Tests.Stubs;

public class StubRiskClassifier(double probability) : IRiskClassifier
{
    public CalibrationStatus CalibrationStatus => CalibrationStatus.Calibrated;

    public Task<double> ClassifyAsync(
        MethylationValue shox2,
        MethylationValue ptger4,
        MethylationValue rassf1a,
        MethylationValue apc,
        MethylationValue cdh13)
        => Task.FromResult(probability);
}
