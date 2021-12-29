using AAV.Sys.Ext;
using System;
using System.IO;

namespace PowerShellLog.Helpers
{
  public static class FSHelper
  {
    public static bool ExistsOrCreated(string folder) // true if created or exists; false if unable to create.
    {
      try
      {
        if (Directory.Exists(folder))
          return true;

        Directory.CreateDirectory(folder);

        return Directory.Exists(folder);
      }
      catch (IOException ex) { ex.Log($"Directory.CreateDirectory({folder})"); }
      catch (Exception ex) { ex.Log($"Directory.CreateDirectory({folder})"); throw; }

      return false;
    }

    public static string GetCreateSafeLogFolderAndFile(string[] fullPaths)
    {
      //if (Environment.UserName == "apigida")
      //  return @$"..\..\Logs\{Path.GetFileName(fullPaths[0])}";

      foreach (var fp in fullPaths)
        if (ExistsOrCreated(Path.GetDirectoryName(fp)))
          return fp;

      return "__FallbackName__.log";
    }
  }
}
