using AAV.WPF.Ext;
using PowerShellLog.Db.DbModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PowerShellLog.Db.Common;
using AAV.Sys.Helpers;
using Microsoft.Extensions.Logging;

namespace PowerShellLog
{
  public partial class MainWindow : AAV.WPF.Base.WindowBase
  {
    readonly A0DbContext _db = A0DbContext.GetLclFl; // suspended till cost analysis is over:  .GetAzure;
    readonly CollectionViewSource _cvsEmails;
    const string _badCmd = "DO NOT USE", _noui_updatedbonly = "NoUi_UpdateDbOnly";
    Cmd _selectCmd;
    
    readonly ILogger<MainWindow> _logger;
    public MainWindow() : this(new LoggerFactory().CreateLogger<MainWindow>()) { }
    public MainWindow(ILogger<MainWindow> logger)
    {
      _logger = logger;
      Opacity = Environment.CommandLine.Contains(_noui_updatedbonly) ? 0 : 1;

      InitializeComponent();

      _cvsEmails = ((CollectionViewSource)(FindResource("cmdViewSource")));
      DataContext = this;

      tbkVer.Text = VerHelper.CurVerStr("Core3");

      tbxSearch.Focus();

      themeSelector1.ApplyTheme = ApplyTheme;

#if DI
      try
      {
        _logger.LogInformation /**/ (" ~~ 1 / 6   Informa");
        _logger.LogDebug       /**/ (" ~~ 2 / 6   Debug  ");
        _logger.LogWarning     /**/ (" ~~ 3 / 6   Warning");
        _logger.LogError       /**/ (" ~~ 4 / 6   Error  ");
        _logger.LogCritical    /**/ (" ~~ 5 / 6   Critica");
        _logger.LogTrace       /**/ (" ~~ 6 / 6   Trace");

        _logger.Log(LogLevel.None,        /**/ " ~~ 0 / 6   None   ");
        _logger.Log(LogLevel.Information, /**/ " ~~ 1 / 6   Informa");
        _logger.Log(LogLevel.Debug,       /**/ " ~~ 2 / 6   Debug  ");
        _logger.Log(LogLevel.Warning,     /**/ " ~~ 3 / 6   Warning");
        _logger.Log(LogLevel.Error,       /**/ " ~~ 4 / 6   Error  ");
        _logger.Log(LogLevel.Critical,    /**/ " ~~ 5 / 6   Critica");
        _logger.Log(LogLevel.Trace,       /**/ " ~~ 6 / 6   Trace  ");
      }
      catch (Exception ex) { _logger.LogError(ex, $""); ex.Pop(); }
#endif
    }

    async Task load() // A0DbContext _db, Action doSearch, Func<Task> fsToDbLoad)
    {
      //Debug.WriteLine($"** _db.ServerDatabase(): {_db.ServerDatabase()}");

      try
      {
        await LoadTablesAsync(_db);
        tbkRpt.Text = await fsToDbLoad();

        doSearch(); // assigns CVS => must be before the next line:
        CollectionViewSource.GetDefaultView(dg0.ItemsSource).Refresh(); //tu: refresh bound datagrid
      }
      catch (Exception ex) { _logger.LogError(ex, $""); ex.Pop(); }

      if (Environment.CommandLine.Contains(_noui_updatedbonly))
        Close();

      themeSelector1.SetCurTheme(Thm);

      Bpr.BeepClk();
    }

    async Task<string> fsToDbLoad(string srcFileMode = "cc") => await new FsToDbLoader().LoadUpdateDbFromFs(_db, srcFileMode);
    void doSearch(string match = "") => DoSearch(match, _db, _cvsEmails, tbkTtl, _logger);
    public static void DoSearch(string match, A0DbContext _db, CollectionViewSource _cvsEmails, TextBlock tbkTtl, ILogger _logger)
    {
      try
      {
        const int max = 100;
        var lowercaseMatch = match.ToLowerInvariant();
        var qry = _db.Cmd.Local.Where(r => (string.IsNullOrEmpty(r.Note) || !r.Note.StartsWith(_badCmd)) && (string.IsNullOrEmpty(match) || r.CommandText.ToLowerInvariant().Contains(lowercaseMatch) || (r.Note != null && r.Note.ToLowerInvariant().Contains(lowercaseMatch))))
          .OrderByDescending(r => r.Id)
          .Take(max)
          .ToList();
        _cvsEmails.Source = qry;
        tbkTtl.Text = $" => {(qry.Count() == max ? "Over" : "Total")} {qry.Count()} matches to '{match}' found";
      }
      catch (Exception ex) { _logger.LogError(ex, $"DoSearch({match}).."); ex.Pop(); }
    }
    public static async Task LoadTablesAsync(A0DbContext db)
    {
      var lsw = Stopwatch.StartNew();
      await db.Log.OrderByDescending(r => r.Id).LoadAsync();                                                                            /**/  Debug.WriteLine($">>> Loaded  Logs {db.Log.Local.Count,7} rows in {lsw.ElapsedMilliseconds,6:N0} ms");
      await db.Cmd.Where(r => (string.IsNullOrEmpty(r.Note) || !r.Note.StartsWith(_badCmd))).OrderByDescending(r => r.Id).LoadAsync();  /**/  Debug.WriteLine($">>> Loaded  Cmds {db.Cmd.Local.Count,7} rows in {lsw.ElapsedMilliseconds,6:N0} ms");
    }

    public static readonly DependencyProperty SrchFProperty = DependencyProperty.Register("SrchF", typeof(string), typeof(MainWindow), new PropertyMetadata("", new PropertyChangedCallback(callbk)));

    public string SrchF { get => (string)GetValue(SrchFProperty); set => SetValue(SrchFProperty, value); }
    static void callbk(DependencyObject d, DependencyPropertyChangedEventArgs e) { if (e.NewValue is string) (d as MainWindow).doSearch((e.NewValue as string).ToLowerInvariant()); }

    async void onLoaded(object s, RoutedEventArgs e) => await load();// _db, doSearch, fsToDbLoad);
    async void onClose(object s, RoutedEventArgs e)
    {
      var rowsSaved = await _db.SaveChangesAsync();
      Debug.WriteLine($"{rowsSaved} rows saved");
      Hide();
      var dly = 400;
      //System.Media.SystemSounds.Asterisk.Play();    /**/ await Task.Delay(dly); // the same      
      //System.Media.SystemSounds.Beep.Play();        /**/ await Task.Delay(dly); // the same      
      //System.Media.SystemSounds.Exclamation.Play(); /**/ await Task.Delay(dly); // the same      
      System.Media.SystemSounds.Hand.Play();          /**/ await Task.Delay(dly); // different     
                                                                                  //System.Media.SystemSounds.Question.Play();    /**/ await Task.Delay(dly); // no sound      
      Close();
      await Task.Delay(dly);
    }
    //void onAzurePoc0(object sender, RoutedEventArgs e) => AzureSqlPoc0.QueryUsingTSqlExample();
    async void onFsToDbLoad(object s, RoutedEventArgs e)
    {
      tbkRpt.Text = "Wait ...";
      ((Button)s).IsEnabled = false;
      System.Media.SystemSounds.Beep.Play();
      await Task.Delay(250);
      try
      {
        //MessageBox.Show("Only for local SQL - NOT for Azure!!!");      return;
        tbkRpt.Text = await fsToDbLoad(((Button)s)?.Tag?.ToString() ?? "No tag :(");
      }
      catch (Exception ex) { _logger.LogError(ex, $""); ex.Pop(); }      
      finally { ((Button)s).IsEnabled = true; System.Media.SystemSounds.Hand.Play(); }
    }
    void onChangeTheme(object s, RoutedEventArgs e) => ApplyTheme(((Button)s).Tag.ToString());
    void onCopy(object s, RoutedEventArgs e) { Debug.WriteLine("onCopy"); Clipboard.SetText(((Button)s).Tag.ToString()); System.Media.SystemSounds.Beep.Play(); }
    void onHide(object s, RoutedEventArgs e)
    {
      Debug.WriteLine("onHide");
      System.Media.SystemSounds.Hand.Play();
      _selectCmd.Note = _badCmd + _selectCmd.Note;

      doSearch(tbxSearch.Text); // assigns CVS => must be before the next line:
      CollectionViewSource.GetDefaultView(dg0.ItemsSource).Refresh(); //tu: refresh bound datagrid
    }

    private void dg0_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    void onSelChd(object s, SelectionChangedEventArgs e)
    {
      Debug.WriteLine("onSelChd");
      _selectCmd = e.AddedItems.Count > 0 ? (Cmd)e.AddedItems[0] : null;
      if (_selectCmd != null)
        dghist.ItemsSource = _db.Log.Local.Where(r => r.CommandId == _selectCmd.Id);
    }
  }
}
