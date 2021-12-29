using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace PowerShellLog.Helpers
{
  public class SeriLogHelper
  {
    public static ILoggerFactory InitLoggerFactory(string logFolder) => LoggerFactory.Create(builder =>
    {
      Trace.WriteLine($"TrWL:/> {logFolder}\nTrWL:/> {logFolder.Replace("..", ".ERR..")}");

      var loggerConfiguration =
        Debugger.IsAttached ?
          new LoggerConfiguration().WriteTo.Debug().MinimumLevel.Information() :
          new LoggerConfiguration()
              .MinimumLevel.Verbose()
              .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
              .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
              .Enrich.FromLogContext() // .Enrich.WithMachineName().Enrich.WithThreadId()                                       
#if DEBUG
              .WriteTo.File(path: @$"{logFolder.Replace("..", ".Dbg.Infi..")}", rollingInterval: RollingInterval.Infinite)
#else
              .WriteTo.File(path: @$"{logFolder.Replace("..", ".Lite..")}", rollingInterval: RollingInterval.Day)
            //.WriteTo.File(path: @$"{logFolder.Replace("..", ".Verb..")}", outputTemplate: _template, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose, rollingInterval: RollingInterval.Day)
            //.WriteTo.File(path: @$"{logFolder.Replace("..", ".Warn..")}", outputTemplate: _template, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning, rollingInterval: RollingInterval.Day)
              .WriteTo.File(path: @$"{logFolder.Replace("..", ".Er▄▀..")}", outputTemplate: _template, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error, rollingInterval: RollingInterval.Day)
            //.WriteTo.File(path: @$"{logFolder.Replace("..", ".11mb..").Replace(".log", ".json")}", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose, rollOnFileSizeLimit: true, fileSizeLimitBytes: 11000000, formatter: new Serilog.Formatting.Json.JsonFormatter()) - useful only with log aggregators.
#endif
              ;

      _ = builder.AddSerilog(loggerConfiguration.CreateLogger());
    });
    public static ILoggerFactory InitLoggerFactory() => LoggerFactory.Create(builder => // :mostly for unit testing.
    {
      var loggerConfiguration = new LoggerConfiguration().WriteTo.Debug().MinimumLevel.Information();

      _ = builder.AddSerilog(loggerConfiguration.CreateLogger());
    });

    const string _template = "{Timestamp:HH:mm:ss.fff}\tMessage:{Message}\tLevel:{Level:w3}\tSourceContext:{SourceContext}{NewLine}{Exception}";
  }
}
/*  static Logger ConfigSerilogger()
    {
      #region Serilog -- https://stackoverflow.com/questions/59362461/logging-in-net-core-wpf-application
      //main: https://github.com/serilog/serilog/blob/dev/README.md
      //iatc: https://www.youtube.com/watch?v=_iryZxv8Rxw
      //todo: cool idea to sink into UI: https://stackoverflow.com/questions/35567814/is-it-possible-to-display-serilog-log-in-the-programs-gui
      //also https://dzone.com/articles/serilog-tutorial-for-net-logging-16-best-practices
      //also https://github.com/sstorie/SerilogDemo.Wpf/blob/develop/SerilogDemo.Wpf/App.xaml.cs

      var serilogILogger = new LoggerConfiguration()
        //-- .ReadFrom.Configuration(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build())
        .MinimumLevel.Verbose()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext() // .Enrich.WithMachineName().Enrich.WithThreadId()
        .WriteTo.Debug(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
        .WriteTo.File(path: @"C:\temp\logs\log-.txt", outputTemplate: _template, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Minute)
        .WriteTo.File(path: @"C:\temp\logs\MaxLen-11k.json", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, rollOnFileSizeLimit: true, fileSizeLimitBytes: 11000, formatter: new Serilog.Formatting.Json.JsonFormatter())
        //.WriteTo.ColoredConsole(outputTemplate: outputTemplate)
        .CreateLogger();
      Log.Logger = serilogILogger; // for Log.Fatal("...");
      #endregion
      return serilogILogger;
    }
 * ...or this:
{
  "Serilog": {
    "Using": [],
    "MinimumLevel": "Verbose",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    },
    "Enrich": [ "FromLogContext" ], //, "WithMachineName", "WithThreadId"
    "WriteTo": [
      {
        "Name": "Debug",
        "Args": {
          "restrictedToMinimumLevel": "Error" // show errors+fatals in the output window.
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "C:\\temp\\logs\\log-.txt",
          "rollingInterval": "Minute",
          "outputTemplate": "{Timestamp:HH:mm:ss.fff} {Level:w3} {Message:j}\t{Properties}\t{NewLine:1}{Exception:1}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "C:\\temp\\logs\\MaxLen-11k.json",
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog",
          "rollOnFileSizeLimit": true,
          "fileSizeLimitBytes": 11000
        }
      }
    ]
  }
}
 
 */