namespace DiStartupHelper.Helpers;

public static class FileSysHelper
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
    //catch (IOException ex) { ex.Log($"Directory.CreateDirectory({folder})"); }
    //catch (Exception ex) { ex.Log($"Directory.CreateDirectory({folder})"); throw; }
    catch (Exception ex) { System.Diagnostics.Trace.Write($"Directory.CreateDirectory({folder}) failed with {ex}\n"); }

    return false;
  }
}
