namespace BloodTestContext.Domain.Models;

public record MethylationValue
{
    public double Value { get; }

    private MethylationValue(double value)
    {
        Value = value;
    }

    public static Result<MethylationValue> Create(double? value, string biomarkerName)
    {
        return value switch
        {
            null => Result.Failure<MethylationValue>($"{biomarkerName} methylation value is required"),
            < 0 or > 1 => Result.Failure<MethylationValue>($"{biomarkerName} methylation value must be between 0 and 1"),
            _ => Result.Success(new MethylationValue(value.Value))
        };
    }
}
