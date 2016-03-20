using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SemVer.Stamp;

namespace SemVer.MSBuild
{
  public abstract class SemVerStampTaskBase : Task
  {
    [Required]
    public string BaseRevision { get; set; }

    [Required]
    public string BaseVersion { get; set; }

    [Required]
    public string BreakingChangeFormat { get; set; }

    [Required]
    public string FeatureFormat { get; set; }

    [Required]
    [Output]
    public string PatchedVersion { get; set; }

    [Required]
    public string PatchFormat { get; set; }

    [Required]
    public string RepositoryPath { get; set; }

    protected abstract SemVersionGrabberBase GetSemVersionGrabber();

    public sealed override bool Execute()
    {
      var baseVersion = Version.Parse(this.BaseVersion);

      var semVersionGrabber = this.GetSemVersionGrabber();
      var version = semVersionGrabber.GetVersion(this.RepositoryPath,
                                                 baseVersion,
                                                 this.BaseRevision,
                                                 this.PatchFormat,
                                                 this.FeatureFormat,
                                                 this.BreakingChangeFormat);

      var patchedVersion = version.ToString();

      this.PatchedVersion = patchedVersion;

      return true;
    }
  }
}
