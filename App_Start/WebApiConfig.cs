namespace FFMPEGWrapper
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Linq;
  using System.Web.Http;
  using System.Web.Http.Dependencies;
  using System.Web.Http.Dispatcher;

  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;
  using Newtonsoft.Json.Serialization;
    
  public static class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {
      config.Routes.MapHttpRoute(
        name: "DefaultApi",
        routeTemplate: "api/{controller}/{action}",
        defaults: new { action = RouteParameter.Optional });
    }
  }
}
