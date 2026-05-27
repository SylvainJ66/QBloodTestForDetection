using System.Diagnostics;
using System.Globalization;
using BloodTestContext.Domain.Models;
using BloodTestContext.Domain.Ports;

namespace BloodTestContext.Infrastructure.Classifiers;

public class QuantumRiskClassifier(string pythonPath, string scriptPath) : IRiskClassifier
{
    public CalibrationStatus CalibrationStatus => CalibrationStatus.Experimental;

    public async Task<double> ClassifyAsync(
        MethylationValue shox2, MethylationValue ptger4,
        MethylationValue rassf1a, MethylationValue apc, MethylationValue cdh13)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{scriptPath}\" {shox2.Value.ToString(CultureInfo.InvariantCulture)} {ptger4.Value.ToString(CultureInfo.InvariantCulture)} {rassf1a.Value.ToString(CultureInfo.InvariantCulture)} {apc.Value.ToString(CultureInfo.InvariantCulture)} {cdh13.Value.ToString(CultureInfo.InvariantCulture)}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Quantum classifier failed: {error}");
        }

        return double.Parse(output.Trim(), CultureInfo.InvariantCulture);
    }
}
