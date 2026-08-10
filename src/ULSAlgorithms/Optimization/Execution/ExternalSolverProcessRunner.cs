using System.Diagnostics;

namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// Runs a solver command-line process with cancellation and captured output.
/// </summary>
internal sealed class ExternalSolverProcessRunner
{
    internal async ValueTask<ExternalSolverProcessResult> RunAsync(
        string executablePath,
        IEnumerable<string> arguments,
        string workingDirectory,
        string? standardInput,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            executablePath);

        ArgumentNullException.ThrowIfNull(arguments);

        var startInfo =
            new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory =
                    string.IsNullOrWhiteSpace(workingDirectory)
                        ? Environment.CurrentDirectory
                        : workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput =
                    standardInput is not null,
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

        if (!process.Start())
        {
            throw new InvalidOperationException(
                $"Unable to start solver executable '{executablePath}'.");
        }

        Task<string> stdoutTask =
            process.StandardOutput.ReadToEndAsync(
                cancellationToken);

        Task<string> stderrTask =
            process.StandardError.ReadToEndAsync(
                cancellationToken);

        if (standardInput is not null)
        {
            await process.StandardInput.WriteAsync(
                standardInput);

            process.StandardInput.Close();
        }

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

        return new ExternalSolverProcessResult(
            process.ExitCode,
            await stdoutTask,
            await stderrTask);
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
            // Best-effort cancellation cleanup.
        }
    }
}

internal sealed record ExternalSolverProcessResult(
    int ExitCode,
    string StandardOutput,
    string StandardError)
{
    internal string CombinedOutput =>
        string.Join(
            Environment.NewLine,
            new[]
            {
                StandardOutput,
                StandardError
            }.Where(
                static text =>
                    !string.IsNullOrWhiteSpace(text)));
}
