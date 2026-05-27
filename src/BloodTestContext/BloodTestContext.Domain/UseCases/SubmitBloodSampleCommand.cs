namespace BloodTestContext.Domain.UseCases;

public record SubmitBloodSampleCommand(
    double? Shox2MethylationValue,
    double? Ptger4MethylationValue,
    double? Rassf1aMethylationValue,
    double? ApcMethylationValue,
    double? Cdh13MethylationValue);
