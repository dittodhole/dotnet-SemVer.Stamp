using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SemVer.Stamp;

namespace SemVer.MSBuild
{
  public abstract class SemVerStampTaskBase : Task
  {
    /// <exception cref="ArgumentNullException"><paramref name="semVersionGrabberBase" /> is <see langword="null" />.</exception>
    protected SemVerStampTaskBase(SemVersionGrabberBase semVersionGrabberBase)
    {
      if (semVersionGrabberBase == null)
      {
        throw new ArgumentNullException(nameof(semVersionGrabberBase));
      }

      this.SemVersionGrabberBase = semVersionGrabberBase;
    }

    [Required]
    public string BaseRevision { get; set; }

    [Required]
    public Version BaseVersion { get; set; }

    [Required]
    public string BreakingChangeFormat { get; set; }

    [Required]
    public string FeatureFormat { get; set; }

    [Required]
    public string PatchFormat { get; set; }

    [Required]
    public string RepositoryPath { get; set; }

    private SemVersionGrabberBase SemVersionGrabberBase { get; }

    [Required]
    [Output]
    public Version Version { get; set; }

    public sealed override bool Execute()
    {
      this.Version = this.SemVersionGrabberBase.GetVersion(this.RepositoryPath,
                                                           this.BaseVersion,
                                                           this.BaseRevision,
                                                           this.PatchFormat,
                                                           this.FeatureFormat,
                                                           this.BreakingChangeFormat);

      return true;
    }
  }
}
