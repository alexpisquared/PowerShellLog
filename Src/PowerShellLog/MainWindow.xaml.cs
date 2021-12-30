using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AAV.Sys.Helpers;
using AAV.WPF.Ext;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerShellLog.Db.Common;
using PowerShellLog.Db.DbModel;

namespace PowerShellLog
{
  public partial class MainWindow : AAV.WPF.Base.WindowBase
  {
    readonly A0DbContext _dbx; // = A0DbContext.GetLclFl; // suspended till cost analysis is over:  .GetAzure;
    readonly IConfigurationRoot _cfg;
    readonly ILogger<Window> _lgr;
    readonly CollectionViewSource _cvsEmails;
    const string _badCmd = "DO NOT USE", _noui_updatedbonly = "NoUi_UpdateDbOnly";
    Cmd? _selectCmd;

    public MainWindow(ILogger<Window> lgr, A0DbContext dbx, IConfigurationRoot cfg) //public MainWindow() : this(new LoggerFactory().CreateLogger<MainWindow>(), A0DbContext.GetLclFl) { }
    {
      _lgr = lgr;                //new LoggerFactory().CreateLogger<Window>();
      _dbx = dbx;
      _cfg = cfg;

      Opacity = Environment.CommandLine.Contains(_noui_updatedbonly) ? 0 : 1;

      InitializeComponent();

      _cvsEmails = ((CollectionViewSource)(FindResource("cmdViewSource")));
      DataContext = this;

      tbkVer.Text = VerHelper.CurVerStr("Net5");

      tbxSearch.Focus();

      themeSelector1.ApplyTheme = ApplyTheme;
      Topmost = Debugger.IsAttached;

#if DI
      try
      {
        _lgr.LogInformation($"*** WhereAmI: {_cfg["WhereAmI"]}   (mainwin.xaml.cs)");
        //_lgr.LogInformation($"*** CS:LclDb: {_cfg.GetConnectionString("LclDb")}");

        //_lgr.LogInformation /**/ (" ~~ 1 / 6   Informa");
        //_lgr.LogDebug       /**/ (" ~~ 2 / 6   Debug  ");
        //_lgr.LogWarning     /**/ (" ~~ 3 / 6   Warning");
        //_lgr.LogError       /**/ (" ~~ 4 / 6   Error  ");
        //_lgr.LogCritical    /**/ (" ~~ 5 / 6   Critica");
        //_lgr.LogTrace       /**/ (" ~~ 6 / 6   Trace");

        //_lgr.Log(LogLevel.None,        /**/ " ~~ 0 / 6   None   ");
        //_lgr.Log(LogLevel.Information, /**/ " ~~ 1 / 6   Informa");
        //_lgr.Log(LogLevel.Debug,       /**/ " ~~ 2 / 6   Debug  ");
        //_lgr.Log(LogLevel.Warning,     /**/ " ~~ 3 / 6   Warning");
        //_lgr.Log(LogLevel.Error,       /**/ " ~~ 4 / 6   Error  ");
        //_lgr.Log(LogLevel.Critical,    /**/ " ~~ 5 / 6   Critica");
        //_lgr.Log(LogLevel.Trace,       /**/ " ~~ 6 / 6   Trace  ");
      }
      catch (Exception ex) { _lgr.LogError(ex, $""); ex.Pop(this); }
#endif
    }

    async Task LoadAsync() // A0DbContext _db, Action doSearch, Func<Task> fsToDbLoad)
    {
      //Debug.WriteLine($"** _db.ServerDatabase(): {_db.ServerDatabase()}");

      try
      {
        await LoadTablesAsync(_dbx);
        tbkRpt.Text = await fsToDbLoad();

        doSearch(); // assigns CVS => must be before the next line:
        CollectionViewSource.GetDefaultView(dg0.ItemsSource).Refresh(); //tu: refresh bound datagrid
      }
      catch (SqlException ex) { var s = @$"Time to run MDB setup scripts \n\n  %OneDrive%\Public\Install\SqlLocalDB [Express 2017]*.* "; _lgr.LogError(ex, s); ex.Pop(this, optl: s); }
      catch (Exception ex) { _lgr.LogError(ex, $""); ex.Pop(this); }

      if (Environment.CommandLine.Contains(_noui_updatedbonly))
        Close();

      themeSelector1.SetCurTheme(Thm);

      Bpr.BeepClk();
    }

    async Task<string> fsToDbLoad(string srcFileMode = "cc") => await new FsToDbLoader().LoadUpdateDbFromFs(_dbx, srcFileMode);
    void doSearch(string match = "") => DoSearch(match, _dbx, _cvsEmails, tbkTtl, _lgr);
    public void DoSearch(string match, A0DbContext _db, CollectionViewSource _cvsEmails, TextBlock tbkTtl, Microsoft.Extensions.Logging.ILogger _logger)
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
      catch (Exception ex) { _logger.LogError(ex, $"DoSearch({match}).."); ex.Pop(this); }
    }
    public async Task LoadTablesAsync(A0DbContext db)
    {
      try
      {
        var lsw = Stopwatch.StartNew();
        await db.Log.OrderByDescending(r => r.Id).LoadAsync();
        await db.Cmd.Where(r => (string.IsNullOrEmpty(r.Note) || !r.Note.StartsWith(_badCmd))).OrderByDescending(r => r.Id).LoadAsync();

        Trace.WriteLine($">>> Loaded  Cmds/Logs {db.Cmd.Local.Count,7} / {db.Log.Local.Count,-7} rows in  {lsw.Elapsed.TotalSeconds:N2} s:");

        var l = db.Log.Local.GroupBy(p => p.Machine).Select(g => new { name = g.Key, count = g.Count(), last = g.Max(x => x.AddedAt) }).OrderByDescending(r => r.last);
        l.ToList().ForEach(q => Trace.WriteLine($"   {q.last:yyyy-MM-dd HH:mm}   {q.name,-14}{q.count,5}  "));

        tbkHist.Text = string.Join("\n", l.Take(3).Select(q => $"   {q.last:yyyy-MM-dd HH:mm}   {q.name,-14}{q.count,5}  ").ToList());

        //l.ToList().ForEach(q => tbkhist.Text += $"   {q.last:yyyy-MM-dd HH:mm}   {q.name,-14}{q.count,5}  \n");
      }
      catch (Exception ex) { _lgr.LogError(ex, $"{nameof(LoadTablesAsync)}({db}).."); ex.Pop(this); }
    }

    public static readonly DependencyProperty SrchFProperty = DependencyProperty.Register("SrchF", typeof(string), typeof(MainWindow), new PropertyMetadata("", new PropertyChangedCallback(callbk)));

    public string SrchF { get => (string)GetValue(SrchFProperty); set => SetValue(SrchFProperty, value); }
    static void callbk(DependencyObject d, DependencyPropertyChangedEventArgs e) { if (e.NewValue is string) (d as MainWindow)?.doSearch((e.NewValue as string)?.ToLowerInvariant() ?? "<null>"); }

    async void onLoaded(object s, RoutedEventArgs e) => await LoadAsync();// _db, doSearch, fsToDbLoad);
    async void onClose(object s, RoutedEventArgs e)
    {
      var rowsSaved = await _dbx.SaveChangesAsync();
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
      catch (Exception ex) { _lgr.LogError(ex, $""); ex.Pop(this); }
      finally { ((Button)s).IsEnabled = true; System.Media.SystemSounds.Hand.Play(); }
    }
    void onChangeTheme(object s, RoutedEventArgs e) => ApplyTheme(((Button)s)?.Tag?.ToString() ?? "Default");
    void onCopy(object s, RoutedEventArgs e) { Debug.WriteLine("onCopy"); Clipboard.SetText(((Button)s).Tag.ToString()); System.Media.SystemSounds.Beep.Play(); }
    void onHide(object s, RoutedEventArgs e)
    {
      Debug.WriteLine("onHide");
      System.Media.SystemSounds.Hand.Play();
      _selectCmd.Note = _badCmd + _selectCmd.Note;

      doSearch(tbxSearch.Text); // assigns CVS => must be before the next line:
      CollectionViewSource.GetDefaultView(dg0.ItemsSource).Refresh(); //tu: refresh bound datagrid
    }

    void dg0_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    void onThrow1(object s, RoutedEventArgs e) { try { throw new Exception("Testing Owned window popup ... "); } catch (Exception ex) { ex.Pop(this); } }
    void onThrow2(object s, RoutedEventArgs e) { try { throw new Exception("Testing Center window popup ... "); } catch (Exception ex) { ex.Pop(this); } }

    void onSelChd(object s, SelectionChangedEventArgs e)
    {
      Debug.WriteLine("onSelChd");
      _selectCmd = e.AddedItems.Count > 0 ? (Cmd)e.AddedItems[0] : null;
      if (_selectCmd != null)
        dghist.ItemsSource = _dbx.Log.Local.Where(r => r.CommandId == _selectCmd.Id);
    }
  }
}
