using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RunProcessAsTask
{
  /// <summary>
  /// Executes external processes asynchronously.
  /// Includes support for cancellation, timeout (via cancellation), 
  /// and exposes the standard output, standard error, and exit code of the process.
  /// </summary>
  /// <see cref="https://github.com/jamesmanning/RunProcessAsTask"/>
  public static partial class ProcessEx
  {
    /// <summary>
    /// Runs an external processes asynchronously.
    /// </summary>
    /// <param name="processStartInfo">a set of parameters to run the process.</param>
    /// <returns>
    /// a Task&lt;ProcessResults&gt; that encapculates asynchronous process result,
    /// which includes the standard output, standard error, and exit 
    /// code of the process.
    /// </returns>
    public static Task<ProcessResults> RunAsync(ProcessStartInfo processStartInfo)
    {
      return RunAsync(processStartInfo, CancellationToken.None);
    }

    /// <summary>
    /// Runs an external processes asynchronously with support for cancellation 
    /// and/or timeout (via cancellation).
    /// </summary>
    /// <param name="processStartInfo">a set of parameters to run the process.</param>
    /// <param name="cancellationToken">a canceletion token.</param>
    /// <returns>
    /// a Task&lt;ProcessResults&gt; that encapculates asynchronous process result,
    /// which includes the standard output, standard error, and exit 
    /// code of the process.
    /// </returns>
    public static Task<ProcessResults> RunAsync(
      ProcessStartInfo processStartInfo, 
      CancellationToken cancellationToken)
    {
      // force some settings in the start info so we can capture the output
      processStartInfo.UseShellExecute = false;
      processStartInfo.RedirectStandardOutput = true;
      processStartInfo.RedirectStandardError = true;

      var tcs = new TaskCompletionSource<ProcessResults>();

      var standardOutput = new List<string>();
      var standardError = new List<string>();

      var process = new Process
      {
        StartInfo = processStartInfo,
        EnableRaisingEvents = true
      };

      process.OutputDataReceived += (sender, args) =>
      {
        if (args.Data != null)
        {
          standardOutput.Add(args.Data);
        }
      };

      process.ErrorDataReceived += (sender, args) =>
      {
        if (args.Data != null)
        {
          standardError.Add(args.Data);
        }
      };

      process.Exited += (sender, args) => tcs.TrySetResult(new ProcessResults(process, standardOutput, standardError));

      cancellationToken.Register(() =>
      {
        tcs.TrySetCanceled();
        process.CloseMainWindow();
      });

      cancellationToken.ThrowIfCancellationRequested();

      if (process.Start() == false)
      {
        tcs.TrySetException(new InvalidOperationException("Failed to start process"));
      }

      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      return tcs.Task;
    }
  }
}