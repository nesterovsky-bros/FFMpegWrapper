namespace FFMPEGWrapper
{
  using System.Threading;
  using System.Web;
  using System.Web.Http;

  using System.Configuration;

  public class WebApiApplication : HttpApplication
  {
    protected void Application_Start()
    {
      GlobalConfiguration.Configure(WebApiConfig.Register);
    }

    /// <summary>
    /// Gets application level semaphore that controls number of running 
    /// in parallel FFMPEG utilities.
    /// </summary>
    public static SemaphoreSlim Semaphore
    {
      get { return semaphore; }
    }

    private static SemaphoreSlim semaphore;

    static WebApiApplication()
    {
      var value =
        ConfigurationManager.AppSettings["NumberOfConcurentFFMpegProcesses"];

      int intValue = 10;

      if (!string.IsNullOrEmpty(value))
      {
        try
        {
          intValue = System.Convert.ToInt32(value);
        }
        catch
        {
          // use the default value
        }
      }

      semaphore = new SemaphoreSlim(intValue, intValue);
    }
  }
}
