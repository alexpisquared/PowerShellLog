using AAV.WPF.Ext;
using Microsoft.Extensions.Logging;
using PowerShellLog.Db.DbModel;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PowerShellLog
{
  public partial class EfHierPoc : Window
  {
    readonly CollectionViewSource _cvsEmails;
    readonly ILogger<Window> _logger;
    readonly A0DbContext _db; // = A0DbContext.GetLclFl; // suspended till cost analysis is over:  .GetAzure;

    public EfHierPoc(ILogger<Window> logger, A0DbContext dbContext)
    {
      InitializeComponent();

      _cvsEmails = ((CollectionViewSource)(FindResource("cmdViewSource")));
      DataContext = this;

      tbxSearch.Focus();
      _logger = logger;
      _db = dbContext;
    }

    async void onLoaded(object s, RoutedEventArgs e) => await load();
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

    async Task load()
    {
      var lsw = Stopwatch.StartNew();
      tbkInfo.Text = "//todo: _db.ServerDatabase()";

      try
      {
        var mw = new MainWindow(_logger, _db);
        await mw.LoadTablesAsync(_db);
        mw.DoSearch("", _db, _cvsEmails, tbkTtl, _logger);
      }
      catch (Exception ex) { _logger.LogError(ex, $""); ex.Pop(null); }
    }
  }
}
