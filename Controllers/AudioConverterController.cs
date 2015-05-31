namespace FFMPEGWrapper
{
  using System;
  using System.Net;
  using System.Text;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Net.Http;
  using System.Web.Http;
  using System.IO;
  using System.Threading;
  using System.Configuration;
  using System.Web;

  /// <summary>
  /// A controller to convert audio.
  /// </summary>
  public class AudioConverterController : ApiController
  {
    /// <summary>
    /// Gets ffmpeg utility wrapper.
    /// </summary>
    public FFMpegWrapper FFMpeg
    {
      get
      {
        if (ffmpeg == null)
        {
          ffmpeg = new FFMpegWrapper(
            HttpContext.Current.Server.MapPath("~/lib/ffmpeg.exe"));
        }

        return ffmpeg;
      }
    }

    /// <summary>
    /// Converts an audio in WAV, OGG, MP3 or other formats 
    /// to AAC format (MP4 audio).
    /// </summary>
    /// <returns>A data URI as a  string.</returns>
    [HttpPost]
    public async Task<string> ConvertAudio([FromBody]string audio)
    {
      if (string.IsNullOrEmpty(audio))
      {
        throw new ArgumentException(
          "Invalid audio stream (probably the input audio is too big).");
      }

      //------------------ For demo purposes only -----------------------
      var context = HttpContext.Current;
      var address = context.Request.UserHostAddress;

      if (string.IsNullOrEmpty(address) || (context.Cache.Get(address) != null))
      {
        await Task.Delay(trialDelay);
      }
      else
      {
        context.Cache.Insert(address, new object());
      }
      //-----------------------------------------------------------------

      var tmp = Path.GetTempFileName();
      var root = tmp + ".dir";

      Directory.CreateDirectory(root);
      File.Delete(tmp);

      try
      {
        var start = audio.IndexOf(':');
        var end = audio.IndexOf(';');
        var mimeType = audio.Substring(start + 1, end - start - 1);
        var ext = mimeType.Substring(mimeType.IndexOf('/') + 1);
        var source = Path.Combine(root, "audio." + ext);
        var target = Path.Combine(root, "audio.m4a");

        await WriteToFileAsync(audio, source);

        switch (ext)
        {
          case "mpeg":
          case "mp3":
          case "wav":
          case "wma":
          case "ogg":
          case "3gp":
          case "amr":
          case "aif":
          case "au":
          case "mid":
          {
            await WebApiApplication.Semaphore.WaitAsync();

            // see https://trac.ffmpeg.org/wiki/Encode/AAC
            // ffmpeg -i input.ogg -c:a libvo_aacenc -b:a 96k output.m4a
            var result = await FFMpeg.Run(
              string.Format
              (
                "-i {0} -c:a libvo_aacenc -b:a 96k {1}",
                source,
                target
              ));

            WebApiApplication.Semaphore.Release();

            if (result.Process.ExitCode != 0)
            {
              throw new InvalidDataException(
                "Cannot convert this audio file to audio/mp4.");
            }

            break;
          }
          default:
          {
            throw new InvalidDataException(
              "Mime type: '" + mimeType + "' is not supported.");
          }
        }

        var buffer = await ReadAllBytes(target);
        var response = "data:audio/mp4;base64," + System.Convert.ToBase64String(buffer);

        return response;
      }
      finally
      {
        Directory.Delete(root, true);
      }
    }

    /// <summary>
    /// Reads file into a byte array.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <returns>A task that returns a content in a byte array.</returns>
    private async Task<byte[]> ReadAllBytes(string path)
    {
      using (var stream = File.OpenRead(path))
      {
        var length = checked((int)stream.Length);
        var offset = 0;
        var buffer = new byte[length];

        while (offset < length)
        {
          var count = await stream.ReadAsync(buffer, offset, length - offset);

          if (count <= 0)
          {
            throw new EndOfStreamException();
          }

          offset += count;
        }

        return buffer;
      }
    }

    /// <summary>
    /// Writes asynchronously the an Audio instance to the specified file.
    /// </summary>
    private static async Task WriteToFileAsync(
      string audio,
      string file)
    {
      byte[] buffer;

      if (string.IsNullOrEmpty(audio))
      {
        throw new ArgumentException("audio");
      }
      else if (audio.StartsWith("data:"))
      {
        var pos = audio.IndexOf(',');

        buffer = Convert.FromBase64String(audio.Substring(pos + 1));
      }
      else
      {
        throw new ArgumentException("audio");
      }

      using (var stream = File.OpenWrite(file))
      {
        await stream.WriteAsync(buffer, 0, buffer.Length);
      }
    }

    private static FFMpegWrapper ffmpeg = null;
    private static int trialDelay = 30000;

    static AudioConverterController()
    {
      var value = ConfigurationManager.AppSettings["TrialDelay"];

      if (!string.IsNullOrEmpty(value))
      {
        try
        {
          trialDelay = System.Convert.ToInt32(value);
        }
        catch
        {
          // use the default value
        }
      }
    }
  }
}
