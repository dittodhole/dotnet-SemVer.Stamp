using System;
using System.IO;
using SemVer.Stamp.Git;

// ReSharper disable CheckNamespace
// ReSharper disable NonLocalizedString

namespace SemVer.Fody
{
  public sealed partial class ModuleWeaver
  {
    private void Prerequisites()
    {
      this.SemVersionGrabber = new GitSemVersionGrabber(this.LogInfo,
                                                        this.LogWarning,
                                                        this.LogError);

      this.IncludeNativeBinariesFolderInPathEnvironmentVariable();
    }

    private void IncludeNativeBinariesFolderInPathEnvironmentVariable()
    {
      string architectureSubFolder;
      if (Environment.Is64BitProcess)
      {
        architectureSubFolder = "amd64";
      }
      else
      {
        architectureSubFolder = "x86";
      }

      var nativeBinariesPath = Path.Combine(this.AddinDirectoryPath,
                                            "NativeBinaries",
                                            architectureSubFolder);

      this.LogInfo($"NativeBinaries path: {nativeBinariesPath}");

      var existingPath = Environment.GetEnvironmentVariable("PATH");
      var newPath = string.Concat(nativeBinariesPath,
                                  Path.PathSeparator,
                                  existingPath);
      Environment.SetEnvironmentVariable("PATH",
                                         newPath);
    }
  }
}
