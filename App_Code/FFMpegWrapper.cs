using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using RunProcessAsTask;

namespace FFMPEGWrapper
{
  /// <summary>
  /// A ffmpeg.exe open source utility wrapper.
  /// </summary>
  /// <remarks>
  /// An idea was taken from 
  /// https://jasonjano.wordpress.com/2010/02/09/a-simple-c-wrapper-for-ffmpeg/
  /// </remarks>
  public class FFMpegWrapper
  {
    /// <summary>
    /// Creates a wrapper for ffmpeg utility.
    /// </summary>
    /// <param name="ffmpegexe">a real path to ffmpeg.exe</param>
    public FFMpegWrapper(string ffmpegexe)
    {
      if (!string.IsNullOrEmpty(ffmpegexe) && File.Exists(ffmpegexe))
      {
        this.ffmpegexe = ffmpegexe;
      }
    }

    /// <summary>
    /// Runs ffmpeg asynchronously.
    /// </summary>
    /// <param name="args">determines command line arguments for ffmpeg.exe</param>
    /// <returns>
    /// asynchronous result with ProcessResults instance that contains 
    /// stdout, stderr and process exit code.
    /// </returns>
    public Task<ProcessResults> Run(string args)
    {
      if (string.IsNullOrEmpty(ffmpegexe))
      {
        throw new InvalidOperationException("Cannot find FFMPEG.exe");
      }

      //create a process info object so we can run our app
      var info = new ProcessStartInfo 
      { 
        FileName = ffmpegexe,
        Arguments = args,
        CreateNoWindow = true
      };

      return ProcessEx.RunAsync(info);
    }

    private string ffmpegexe;
  }
}