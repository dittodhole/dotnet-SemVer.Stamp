using System;
using System.IO;
using SemVer.Fody;
using SemVer.Stamp;
using SemVer.Stamp.Git;

// ReSharper disable NonLocalizedString

namespace SemVer.Git.Fody
{
  public sealed class ModuleWeaver : ModuleWeaverBase
  {
    protected override void Prerequisites()
    {
      this.IncludeNativeBinariesFolderInPathEnvironmentVariable();
    }

    protected override SemVersionGrabberBase GetSemVersionGrabber(string repositoryPath,
                                                                  string baseRevision)
    {
      var gimtSemVersionGrabber = new GitSemVersionGrabber(repositoryPath,
                                                           baseRevision,
                                                           this.LogInfo,
                                                           this.LogWarning,
                                                           this.LogError);
      return gimtSemVersionGrabber;
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

      this.LogInfo?.Invoke($"NativeBinaries path: {nativeBinariesPath}");

      var existingPath = Environment.GetEnvironmentVariable("PATH");
      var newPath = string.Concat(nativeBinariesPath,
                                  Path.PathSeparator,
                                  existingPath);
      Environment.SetEnvironmentVariable("PATH",
                                         newPath);
    }
  }
}
