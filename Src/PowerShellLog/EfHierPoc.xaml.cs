namespace PowerShellLog
{
  public partial class EfHierPoc : Window
  {
    readonly CollectionViewSource _cvsEmails;
    readonly ILogger<Window> _lgr;
    readonly A0DbContext _dbx; // = A0DbContext.GetLclFl; // suspended till cost analysis is over:  .GetAzure;
    readonly IConfigurationRoot _cfg;

    public EfHierPoc(ILogger<Window> logger, A0DbContext dbContext, IConfigurationRoot cfg)
    {
      InitializeComponent();

      _cvsEmails = ((CollectionViewSource)(FindResource("cmdViewSource")));
      DataContext = this;

      tbxSearch.Focus();
      _lgr = logger;
      _dbx = dbContext;
      _cfg = cfg;
    }

    async void onLoaded(object s, RoutedEventArgs e) => await load();
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

    async Task load()
    {
      //var lsw = Stopwatch.StartNew();
      tbkInfo.Text = "//todo: _db.ServerDatabase()";

      try
      {
        var mw = new MainWindow(_lgr, _dbx, _cfg);
        await mw.LoadTablesAsync(_dbx);
        mw.DoSearch("", _dbx, _cvsEmails, tbkTtl, _lgr);
      }
      catch (Exception ex) { _lgr.LogError(ex, $""); ex.Pop((Window?)null); }
    }
  }
}
