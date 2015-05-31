using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace RunProcessAsTask
{
  /// <summary>
  /// Encapculates asynchronous process' result that contains standard output, 
  /// standard error, an exit code and other properties of the process.
  /// </summary>
  /// <see cref="https://github.com/jamesmanning/RunProcessAsTask"/>
  public class ProcessResults
  {
    private readonly Process _process;
    private readonly IEnumerable<string> _standardOutput;
    private readonly IEnumerable<string> _standardError;

    /// <summary>
    /// Creates an asynchronous process result.
    /// </summary>
    public ProcessResults(
      Process process, 
      IEnumerable<string> standardOutput, 
      IEnumerable<string> standardError)
    {
      _process = process;
      _standardOutput = standardOutput;
      _standardError = standardError;
    }

    /// <summary>
    /// Gets a running process.
    /// </summary>
    public Process Process
    {
      get { return _process; }
    }

    /// <summary>
    /// Gets the process' stdout.
    /// </summary>
    public IEnumerable<string> StandardOutput
    {
      get { return _standardOutput; }
    }

    /// <summary>
    /// Gets the process' stderr.
    /// </summary>
    public IEnumerable<string> StandardError
    {
      get { return _standardError; }
    }
  }
}