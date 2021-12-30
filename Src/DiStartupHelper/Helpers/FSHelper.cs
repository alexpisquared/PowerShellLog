using System;
using System.IO;
using AAV.Sys.Ext;

namespace PowerShellLog.Helpers;

public static class FSHelper
{
  public static string GetCreateSafeLogFolderAndFile(string[] fullPaths)
  {
    foreach (var fp in fullPaths)
      if (ExistsOrCreated(Path.GetDirectoryName(fp) ?? throw new ArgumentNullException(nameof(fullPaths))))
        return fp;

    return "__FallbackName__.log";
  }

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
}
