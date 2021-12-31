using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CI.Standard.Lib.Helpers;
using PowerShellLog.Db.DbModel;

namespace PowerShellLog.Db.Common;

public class FsToDbLoader
{
  public static async Task<string> LoadUpdateDbFromFs(A0DbContext db, string srcFileMode)
  {
    var _sw = Stopwatch.StartNew();
    var _now = DateTime.Today.AddHours(DateTime.Now.Hour);
    var _report = "";

    if (srcFileMode == "cc")
    {
      var appDataRoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
;
      _report += await DoOneFile($@"{appDataRoamingPath}\Microsoft\Windows\PowerShell\PSReadline\ConsoleHost_history.txt", _now, db);
    }
    else if (srcFileMode == "ia")
    {
      foreach (var file in new string[] { "Nymi", "ASUS2", "RAZER1", "MAC1" }) { _report += await DoOneFile($@"{OneDrive.Folder("c\\PowerShellLog")}\{file}.txt", _now, db); _now = _now.AddMinutes(10); }
    }
    else
      return $"{srcFileMode} is not known here.";

    return $"{_report} => took {_sw.Elapsed.TotalSeconds:N1} sec on {Environment.MachineName}.";
  }

  static async Task<string> DoOneFile(string pathfile, DateTime now, A0DbContext db)
  {
    var ttlNewCmds = 0;
    var ttlNewLogs = 0;
    var report = "";
    try
    {
      var lineNum = 0;
      var machine = Path.GetFileNameWithoutExtension(pathfile);
      if (machine.Equals("ConsoleHost_history", StringComparison.OrdinalIgnoreCase))
        machine = Environment.MachineName;

      var fsLines = File.ReadAllLines(pathfile).Where(r => !string.IsNullOrEmpty(r) && !string.IsNullOrWhiteSpace(r.Trim())).ToArray();

      //redundant:                (Jul-2019)
      //await db.Cmd.LoadAsync();
      //await db.Log.LoadAsync();
      var rowss = db.Log.Local.Where(r => r.Machine.Equals(machine, StringComparison.OrdinalIgnoreCase)).Count();
      var lines = fsLines.Length;

      report += $"{pathfile} :   {db.Log.Count()} {db.Log.Local.Count} log      {db.Cmd.Count()} {db.Cmd.Local.Count} cmd     fs.lines={lines}  db.rows={rowss}\r\n";

      for (var i = rowss; i < lines; i++)
      {
        var cmdTxt = CleanCmd(fsLines, i);

        Debug.Write($"{++lineNum,5}  ");

        var cmd = db.Cmd.Local.FirstOrDefault(r => !string.IsNullOrEmpty(cmdTxt) && r.CommandText.Equals(cmdTxt, StringComparison.OrdinalIgnoreCase));
        if (cmd == null)
        {
          db.Cmd.Local.Add(cmd = new Cmd { CommandText = cmdTxt.Length < 900 ? cmdTxt : cmdTxt[..900], AddedAt = now });
          Debug.Write($" ++");
          ttlNewCmds++;
        }
        else
          Debug.Write($" ··");

        if (!db.Log.Local.Any(r => r.LineNum == i && r.CommandId == cmd.Id && r.Machine.Equals(machine, StringComparison.OrdinalIgnoreCase)))
        {
          Debug.Write($" ++");
          db.Log.Local.Add(new Log
          {
            Command = cmd,
            LineNum = i,
            Machine = machine,
            AddedAt = now
          });

          ttlNewLogs++;

          //break; // one at a time ...for debugging only
        }
        else
          Debug.Write($" ··");

        Debug.Write($"   {cmdTxt,-64} \r");
      }

      var rs = await db.SaveChangesAsync();
      report += ($"\r\n\n{rs} rows saved\n");

      //System.Media.SystemSounds.Beep.Play();

      return $"{lines,5:N0}:lines - rows:{rowss,-5:N0} = {(lines - rowss),4}:  {ttlNewLogs} new uses,  {ttlNewCmds} new cmds from  '{Path.GetFileNameWithoutExtension(pathfile)}'\r\n";
    }
    catch (Exception ex) { report += ($"\r\n\n{ex}\n"); }

    return report;
  }

  static string CleanCmd(string[] fsLines, int i) => fsLines[i].Replace("     ", " ").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ").Trim();
}
