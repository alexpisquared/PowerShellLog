using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using AAV.Sys.Ext;
using AAV.WPF.Ext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PowerShellLog.Helpers;

public class ConfigHelper
{
  public static IConfigurationRoot AutoInitConfigFromFile(string defaultValues = _defaultAppSetValues, bool enforceCreation = false)
  {
    const int max = 2;

    var appsettingsFiles = new[] {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @$"AppSettings\{AppDomain.CurrentDomain.FriendlyName}\{_appSettingsFileNameOnly}"),
        @$"AppSettings\{_appSettingsFileNameOnly}",
        @$"{_appSettingsFileNameOnly}",
      };

    foreach (var appsettingsFile in appsettingsFiles)
    {
      int i;
      for (i = 0; i < max && TryCreateDefaultFile(appsettingsFile, defaultValues, enforceCreation); i++)
      {
        try
        {
          return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(appsettingsFile)
            .AddUserSecrets<WhatIsThatForType>()
            .Build();
        }
        catch (InvalidOperationException ex)
        {
          ex.Pop(null, optl: "Disaster ...");
        }
        catch (FileNotFoundException ex)
        {
          if (!TryCreateDefaultFile(appsettingsFile, defaultValues, enforceCreation))
            _ = ex.Log("Retrying 3 times ...");
        }
        catch (FormatException ex)
        {
          _ = new Process { StartInfo = new ProcessStartInfo("Notepad.exe", $"\"{appsettingsFile}\"") { RedirectStandardError = true, UseShellExecute = false } }.Start();
          ex.Pop(null, optl: $"Try to edit the errors out from \n\t {appsettingsFile}");
        }
        catch (Exception ex)
        {
          if (!TryCreateDefaultFile(appsettingsFile, defaultValues, enforceCreation))
            ex.Pop(null, optl: "██  ██  ██  Take a look!");
        }
      }

      new Exception().Pop(null, optl: $"Unable to create default  {appsettingsFile}  file  {i}/{max} times.");
    }

    return AutoInitConfigHardcoded(defaultValues, enforceCreation);
  }
  public static IConfigurationRoot AutoInitConfigHardcoded(string defaultValues = _defaultAppSetValues, bool enforceCreation = false)
  {
    var server = Environment.MachineName == "RAZER1" ? @".\SqlExpress" : "mtDEVsqldb,1625";
    var config = new ConfigurationBuilder()
      .AddInMemoryCollection()
      .Build();

    config["WhereAmI"] = "-* In Mem *-";
    config["LogFolder"] = @"C:\g\PowerShellLog\Src\PowerShellLog\bin\Logs\PSL..log";
    config["SqlConStrSansSnD"] = "Server={0};     Database={1};       Trusted_Connection=True;Connection Timeout=52";
    config["ConnectionStrings:LclDb"] = "Data Source=(localdb)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\alexp\\OneDrive\\Public\\AppData\\PowerShellLog\\PowerShellLog.mdf;Integrated Security=True;Connect Timeout=17;";

#if !true // 
      var appConfig = new AppConfig();
      config.Bind(appConfig);
      string json = Newtonsoft.Json.JsonConvert.SerializeObject(appConfig);
      File.WriteAllText("exported-" + _appSettingsFileNameOnly, json);
#endif

    return config;
  }

  const string
    _appSettingsFileNameOnly = "AppSettings.json",
    _defaultAppSetValues = @"{{
      ""WhereAmI"":             ""{0}"",
      ""LogFolder"":            ""C:\\g\\PowerShellLog\\Src\\PowerShellLog\\bin\\Logs\\psl..log"",
      ""ServerList"":           "".\\sqlexpress mtDEVsqldb,1625 mtUATsqldb mtPRDsqldb"",
      ""SqlConStrSansSnD"":     ""Server={{0}};Database={{1}};          Trusted_Connection=True;Connection Timeout=52"",
      ""SqlConStrBR"":          ""Server={{server}};Database=BR;        Trusted_Connection=True;Connection Timeout=52"",

      ""ConnectionStrings:LclDb"": ""Data Source=(localdb)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\alexp\\OneDrive\\Public\\AppData\\PowerShellLog\\PowerShellLog.mdf;Integrated Security=True;Connect Timeout=17;""
}}";
  static bool TryCreateDefaultFile(string appsettingsPathFileExt, string defaultValues, bool enforceCreation)
  {
    try
    {
      var dir = Path.GetDirectoryName(appsettingsPathFileExt);
      if (string.IsNullOrEmpty(dir))
      {
        dir = "appsettings";
        appsettingsPathFileExt = Path.Combine(dir, appsettingsPathFileExt);
      }

      if (Directory.Exists(dir) != true)
        Directory.CreateDirectory(dir);

      if (enforceCreation
        || !File.Exists(appsettingsPathFileExt)
        || (DateTime.Now < new DateTime(2021, 9, 14) && Environment.MachineName != "RAZER1" && Environment.MachineName != "D21-MJ0AWBEV"))
      {
        File.WriteAllText(appsettingsPathFileExt, string.Format(defaultValues, appsettingsPathFileExt.Replace(@"\", @"\\"), Assembly.GetEntryAssembly()?.GetName().Name, DateTimeOffset.Now.ToString()));
        Trace.WriteLine($"TrWL:> ■ WARNING: overwrote this {appsettingsPathFileExt}.");
      }

      return true;
    }
    catch (FormatException ex)
    {
      _ = new Process { StartInfo = new ProcessStartInfo("Notepad.exe", $"\"{appsettingsPathFileExt}\"") { RedirectStandardError = true, UseShellExecute = false } }.Start();
      ex.Pop(null, optl: $"Try to edit the errors out from \n\t {appsettingsPathFileExt}");
      return false;
    }
    catch (Exception ex) { ex.Pop(null); return false; }
  }
  class WhatIsThatForType { public string MyProperty { get; set; } = "<Default Value of Nothing Special>"; }

  public class AppConfig
  {
    public string WhereAmI { get; set; } = "";
    public string LogFolder { get; set; } = "";
    public string SqlConStrSansSnD { get; set; } = "";
    public string SqlConStrBR { get; set; } = "";
  }
}
