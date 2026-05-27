using BloodTestContext.Domain.Models;

namespace BloodTestContext.Domain.Ports;

public interface IRiskClassifier
{
    CalibrationStatus CalibrationStatus { get; }
    Task<double> ClassifyAsync(MethylationValue shox2, MethylationValue ptger4);
}
