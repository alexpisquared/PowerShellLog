
namespace PowerShellLog;

public partial class App : Application
{
  static readonly DateTime _started = DateTime.Now;

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
  readonly ServiceProvider? _serviceProvider;

  public App()
  {
    var services = new ServiceCollection();

    try
    {
      _ = services.AddSingleton<IConfigurationRoot>(ConfigHelper.AutoInitConfigFromFile());

      _ = services.AddSingleton<ILogger<Window>>(sp => SeriLogHelper.InitLoggerFactory( //todo: this allows to override by UserSettings entry: UserSettingsIPM.UserLogFolderFile ??= // if new - store in usersettings for next uses.
        FileSysHelper.GetCreateSafeLogFolderAndFile(new[]
        {
          sp.GetRequiredService<IConfigurationRoot>()["LogFolder"].Replace("..", $"{(Assembly.GetExecutingAssembly().GetName().Name??"Unkwn")[..5]}.{Environment.UserName[..3]}.."), @$"..\Logs\", @$"\Temp\Logs\"
        })).CreateLogger<MainWindow>());

      _ = services.AddSingleton<IAddChild, MainWindow>(); //tu: for troubleshooting the type mismatch: (sp => new MainWindow(sp.GetRequiredService<ILogger<Window>>(), sp.GetRequiredService<A0DbContext>()));

      _serviceProvider = services.BuildServiceProvider();

      _ = services.AddDbContext<A0DbContext>(optionsBuilder =>
        {
          var lgr = _serviceProvider?.GetRequiredService<ILogger<Window>>();
          var cfg = _serviceProvider?.GetRequiredService<IConfigurationRoot>();

          lgr?.LogInformation($"*** WhereAmI: {cfg?["WhereAmI"]}");

          optionsBuilder.UseSqlServer(cfg.GetConnectionString("LclDb") ?? throw new ArgumentNullException(".GetConnectionString('LclDb')"));
        });

      _serviceProvider = services.BuildServiceProvider();
    }
    catch (Exception ex)
    {
      ex.Pop(null, optl: "Shutting down the app ...");
      Current.Shutdown();
    }
  }

  protected override void OnStartup(StartupEventArgs e)
  {
    base.OnStartup(e);

    if (_serviceProvider is null) return;

    Current.DispatcherUnhandledException += UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
    EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox)?.SelectAll(); }));

    MainWindow = (Window)_serviceProvider.GetRequiredService<IAddChild>();
    MainWindow.Show();
  }
  protected override void OnDeactivated(EventArgs e) {                        /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - _started):mm\\:ss\\.ff} App.OnDeactivated()   1/3 - OK   "); base.OnDeactivated(e); }
  protected override void OnExit(ExitEventArgs e) {                           /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - _started):mm\\:ss\\.ff} App.OnExit()          2/3 - OK \n"); base.OnExit(e); }
  protected override void OnSessionEnding(SessionEndingCancelEventArgs e) {   /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - _started):mm\\:ss\\.ff} App.OnSessionEnding() 3/3 - never seen in the log"); base.OnSessionEnding(e); }
}
