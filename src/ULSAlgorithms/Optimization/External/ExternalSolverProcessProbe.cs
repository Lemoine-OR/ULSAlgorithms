using System.Diagnostics;

namespace ULSAlgorithms.Optimization.External;

/// <summary>
/// Executes short solver command-line probes and captures their output.
/// </summary>
public sealed class ExternalSolverProcessProbe
{
    /// <summary>
    /// Runs one external process and captures stdout, stderr and exit code.
    /// </summary>
    public async ValueTask<ExternalSolverProcessProbeResult> RunAsync(
        string executablePath,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentNullException.ThrowIfNull(arguments);

        var startInfo =
            new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory =
                    Path.GetDirectoryName(executablePath) ??
                    Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process =
            new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException(
                    $"Unable to start '{executablePath}'.");
            }

            Task<string> stdoutTask =
                process.StandardOutput.ReadToEndAsync(
                    cancellationToken);

            Task<string> stderrTask =
                process.StandardError.ReadToEndAsync(
                    cancellationToken);

            try
            {
                await process.WaitForExitAsync(
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                throw;
            }

            string stdout =
                await stdoutTask;

            string stderr =
                await stderrTask;

            return new ExternalSolverProcessProbeResult(
                process.ExitCode,
                stdout,
                stderr);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            TryKill(process);
            throw;
        }
    }

    private static void TryKill(
        Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(
                    entireProcessTree: true);
            }
        }
        catch
        {
            // Best-effort cleanup only.
        }
    }
}
