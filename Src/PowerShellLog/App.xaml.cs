using AAV.Sys.Helpers;
using AAV.WPF.AltBpr;
using AAV.WPF.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; // see the links below!!!
using Microsoft.Extensions.Logging;
using PowerShellLog.Db.DbModel;
using PowerShellLog.Helpers;
//using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace PowerShellLog
{
  public partial class App : Application
  {
    static readonly DateTime _started = DateTime.Now;

    //#region DI & Logging:
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

    public App()
    {
      var services = new ServiceCollection();

      services.AddSingleton<MainWindow>();
      //?or:  .AddTransient<MainWindow>();
      //?or:  .AddTransient(typeof(MainWindow));

      _ = services.AddSingleton<IConfigurationRoot>(ConfigHelper.AutoInitConfigHardcoded());

      _ = services.AddSingleton<ILogger>(sp => SeriLogHelper.InitLoggerFactory( //todo: this allows to override by UserSettings entry: UserSettingsIPM.UserLogFolderFile ??= // if new - store in usersettings for next uses.
        Helpers.FSHelper.GetCreateSafeLogFolderAndFile(new[]
        {
            sp.GetRequiredService<IConfigurationRoot>()["LogFolder"].Replace("..", $"{(Assembly.GetExecutingAssembly().GetName().Name??"Unkwn")[..5]}.{Environment.UserName[..3]}.."),
          @$"..\Logs\",@$"\Temp\Logs\"})).CreateLogger<MainWindow>());

      //_ = services.AddSingleton<IAddChild, MainWindow>(); // (sp => new MainView(sp.GetRequiredService<ILogger>(), sp.GetRequiredService<IConfigurationRoot>(), sp.GetRequiredService<InventoryContext>(), _started));

      _serviceProvider = services.BuildServiceProvider();

      services.AddDbContext<A0DbContext>(optionsBuilder =>
      {
        IConfigurationRoot? Configuration = _serviceProvider?.GetRequiredService<IConfigurationRoot>();
        optionsBuilder.UseSqlServer(Configuration.GetConnectionString("ad"));
      });
    }
    void configureServices(IServiceCollection serviceCollection) { }

    ////serilog 
    //public static IServiceCollection AddSerilogServices(/*this */IServiceCollection services, LoggerConfiguration configuration)
    //{
    //  Log.Logger = configuration.CreateLogger();
    //  AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
    //  return services.AddSingleton(Log.Logger);
    //}

    //cnf 1/2:
    //Microsoft.Extensions.Configuration
    //Microsoft.Extensions.Configuration.FileExtensions
    //Microsoft.Extensions.Configuration.Json
    //public IConfiguration Configuration { get; private set; }
    //  AppSetting.json:
    //  {
    //    "ConnectionStrings": { "BloggingDatabase": "Server=(localdb)\\mssqllocaldb;Database=EFGetStarted.ConsoleApp.NewDb;Trusted_Connection=True;" }
    //  }    

    //#endregion

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

      // app initDI();
      _serviceProvider.GetService<MainWindow>()?.Show(); // <= an overkill DI demo of: 
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
    protected override void OnDeactivated(EventArgs e) {                        /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - _started):mm\\:ss\\.ff} App.OnDeactivated()   1/3 - OK   "); base.OnDeactivated(e); }
    protected override void OnExit(ExitEventArgs e) {                           /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - _started):mm\\:ss\\.ff} App.OnExit()          2/3 - OK \n"); base.OnExit(e); }
    protected override void OnSessionEnding(SessionEndingCancelEventArgs e) {   /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - _started):mm\\:ss\\.ff} App.OnSessionEnding() 3/3 - never seen in the log"); base.OnSessionEnding(e); }

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
