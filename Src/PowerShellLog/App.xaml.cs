using AAV.Sys.Helpers;
using AAV.WPF.Helpers;
using Microsoft.Extensions.DependencyInjection; // see the links below!!!
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PowerShellLog
{
  public partial class App : Application
  {
    static readonly DateTime _started = DateTime.Now;

    #region DI & Logging:
    /* 
    // https://michaelscodingspot.com/logging-in-dotnet/            //todo: Use SeriLog from here too!
    //
    // https://csharp.christiannagel.com/2018/11/13/iloggertofile/  //todo: Use SeriLog from here too!
    //
    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1
    //
    //Using .NET Core 3.0 Dependency Injection and Service Provider with WPF:
    // https://marcominerva.wordpress.com/2019/03/06/using-net-core-3-0-dependency-injection-and-service-provider-with-wpf/

      Sep 2019:      Dependency Injection for .Net Core Console running in Docker      https://www.youtube.com/watch?v=2TgWRfOnOc0
    */
    ServiceProvider _serviceProvider;
    static TraceListener _traceListener;

    void initDI() //nogo:  App() { ...  <== ctor runs AFTER Startup !!!
    {
      //MessageBox.Show("CTor 1/n");

      var serviceCollection = new ServiceCollection();

      //configureServices(serviceCollection);
      var file = Tracer.GetLogPathFileName("PowerShellLog.c3", false);
      var stream = File.AppendText(file);
      stream.AutoFlush = true;
      _traceListener = new TextWriterTraceListener(stream.BaseStream);

      serviceCollection.AddLogging(configure =>
      {
        configure
          //?.AddSerilog(dispose: true)
          //?.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger(), dispose: true)
          //?.AddConfiguration(configuration.GetSection("Logging"))
          .AddTraceSource(new SourceSwitch("TraceSourceLog", SourceLevels.Verbose.ToString()), _traceListener) // must be unique since "...because it is being used by another process".
          .AddConsole()
#if DEBUG    
          .AddDebug()
#endif
          ;
      });


      serviceCollection.AddSingleton<MainWindow>();
      //?or: serviceCollection.AddTransient<MainWindow>();
      //?or: serviceCollection.AddTransient(typeof(MainWindow));

      _serviceProvider = serviceCollection.BuildServiceProvider();
    }
    void configureServices(IServiceCollection serviceCollection) { }

    //serilog 
    public static IServiceCollection AddSerilogServices(/*this */IServiceCollection services, LoggerConfiguration configuration)
    {
      Log.Logger = configuration.CreateLogger();
      AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
      return services.AddSingleton(Log.Logger);
    }

    //cnf 1/2:
    //Microsoft.Extensions.Configuration
    //Microsoft.Extensions.Configuration.FileExtensions
    //Microsoft.Extensions.Configuration.Json
    //public IConfiguration Configuration { get; private set; }
    //  AppSetting.json:
    //  {
    //    "ConnectionStrings": { "BloggingDatabase": "Server=(localdb)\\mssqllocaldb;Database=EFGetStarted.ConsoleApp.NewDb;Trusted_Connection=True;" }
    //  }    

    #endregion

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      
      //MessageBox.Show("OnStartup 2/n");
      Current.DispatcherUnhandledException += UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
      EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox).SelectAll(); }));
      Tracer.SetupTracingOptions("PowerShellLog", new TraceSwitch("Verbose-ish", "See ScrSvr for the model.") { Level = TraceLevel.Verbose }, false);

#if DI
      //cnf 2/2:
      //var builder = new ConfigurationBuilder()
      //  .SetBasePath(Directory.GetCurrentDirectory())
      //  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
      //Configuration = builder.Build();
      //Console.WriteLine(configuration.GetConnectionString("BloggingDatabase"));

      initDI();
      _serviceProvider.GetService<MainWindow>().Show(); // <= an overkill DI demo of: 
#else
      new MainWindow().Show();
#endif

#if !_DEBUG
#else
                     
            // alreaady there: Trace.Listeners.Add(new TextWriterTraceListener(Console.Out) { Filter = new ErrorFilter() });

            // under construction: <== and has no effect on filtering lower level messages !!!
            Trace.Listeners.Add(new TextWriterTraceListener("__PowerShellLog.Err.txt") { Filter = new ErrorFilter() });
            Trace.Listeners.Add(new TextWriterTraceListener("__PowerShellLog.War.txt") { Filter = new WarngFilter() });
            Trace.Listeners.Add(new TextWriterTraceListener("__PowerShellLog.Inf.txt") { Filter = new InfonFilter() });
            Trace.Listeners.Add(new TextWriterTraceListener("__PowerShellLog.Ver.txt") { Filter = new VerbsFilter() });
            Trace.AutoFlush = true;
            Trace.IndentSize = 12;

            demo(AppTraceLevelCfg, "Cfg");

            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Shutdown(0);
#endif
    }
    protected override void OnExit(ExitEventArgs e) {                           /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - _started):mm\\:ss\\.ff} App.OnExit()          1/3"); base.OnExit(e); }
    protected override void OnDeactivated(EventArgs e) {                        /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - _started):mm\\:ss\\.ff} App.OnDeactivated()   2/3"); base.OnDeactivated(e); }
    protected override void OnSessionEnding(SessionEndingCancelEventArgs e) {   /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - _started):mm\\:ss\\.ff} App.OnSessionEnding() 3/3"); base.OnSessionEnding(e); }

    static void demo(TraceSwitch traceLevel, string src)
    {
      Console.WriteLine($"demo - Trace.WLIf: {src} - Trace switch '{traceLevel.DisplayName}' configured as  '{traceLevel.Level}'.");
      Trace.WriteLineIf(traceLevel.TraceError,     /**/ $"demo - Trace.WLIf: {src} - Error,    , ");
      Trace.WriteLineIf(traceLevel.TraceWarning,   /**/ $"demo - Trace.WLIf: {src} - Warning,  , ");
      Trace.WriteLineIf(traceLevel.TraceInfo,      /**/ $"demo - Trace.WLIf: {src} - Info,     , ");
      Trace.WriteLineIf(traceLevel.TraceVerbose,   /**/ $"demo - Trace.WLIf: {src} - Verbose,  , ");
      Console.WriteLine($"");
    }
  }
}
