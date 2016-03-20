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
    public Version BaseVersion { get; set; }

    [Required]
    public string BreakingChangeFormat { get; set; }

    [Required]
    public string FeatureFormat { get; set; }

    [Required]
    [Output]
    public Version PatchedVersion { get; set; }

    [Required]
    public string PatchFormat { get; set; }

    [Required]
    public string RepositoryPath { get; set; }

    protected abstract SemVersionGrabberBase GetSemVersionGrabber();

    public sealed override bool Execute()
    {
      var semVersionGrabber = this.GetSemVersionGrabber();
      var version = semVersionGrabber.GetVersion(this.RepositoryPath,
                                                 this.BaseVersion,
                                                 this.BaseRevision,
                                                 this.PatchFormat,
                                                 this.FeatureFormat,
                                                 this.BreakingChangeFormat);

      this.PatchedVersion = version;

      return true;
    }
  }
}
