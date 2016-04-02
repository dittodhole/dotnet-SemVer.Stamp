using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SemVer.Stamp;

namespace SemVer.MSBuild
{
  public abstract class SemVerStampTaskBase : Task
  {
    public string BaseRevision { get; set; }

    [Required]
    public string BaseVersion { get; set; }

    [Required]
    public string BreakingChangeFormat { get; set; }

    [Required]
    public string FeatureFormat { get; set; }

    [Output]
    public string PatchedVersion { get; set; }

    [Required]
    public string PatchFormat { get; set; }

    [Required]
    public string RepositoryPath { get; set; }

    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    protected abstract SemVersionGrabberBase GetSemVersionGrabber(string repositoryPath,
                                                                  string baseRevision);

    public sealed override bool Execute()
    {
      Version baseVersion;
      try
      {
        baseVersion = Version.Parse(this.BaseVersion);
      }
      catch (ArgumentException argumentException)
      {
        this.Log.LogErrorFromException(argumentException);
        return false;
      }
      catch (FormatException formatException)
      {
        this.Log.LogErrorFromException(formatException);
        return false;
      }
      catch (OverflowException overflowException)
      {
        this.Log.LogErrorFromException(overflowException);
        return false;
      }

      Version version;
      var semVersionGrabber = this.GetSemVersionGrabber(this.RepositoryPath,
                                                        this.BaseRevision);
      try
      {
        version = semVersionGrabber.GetVersion(baseVersion,
                                               this.PatchFormat,
                                               this.FeatureFormat,
                                               this.BreakingChangeFormat);
      }
      catch (ArgumentNullException argumentNullException)
      {
        this.Log.LogErrorFromException(argumentNullException);
        return false;
      }

      var patchedVersion = version.ToString();

      this.PatchedVersion = patchedVersion;

      return true;
    }
  }
}
