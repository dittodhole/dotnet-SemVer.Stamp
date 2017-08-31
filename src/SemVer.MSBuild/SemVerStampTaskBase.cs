using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using JetBrains.Annotations;

namespace SemVer.MSBuild
{
  public abstract class SemVerStampTaskBase : Task
  {
    /// <remarks>TODO: rename</remarks>
    [CanBeNull]
    public string BaseRevision { get; set; }

    /// <remarks>TODO: rename</remarks>
    [CanBeNull]
    public string BaseVersion { get; set; }

    [Required]
    [NotNull]
    public string BreakingChangeFormat { get; set; }

    [Required]
    [NotNull]
    public string FeatureFormat { get; set; }

    [Output]
    public string PatchedVersion { get; set; }

    [Required]
    [NotNull]
    public string PatchFormat { get; set; }

    /// <remarks>TODO: rename</remarks>
    [Required]
    [NotNull]
    public string RepositoryPath { get; set; }

    public override bool Execute()
    {
      var commitMessageProvider = this.CreateCommitMessageProvider();

      string[] commitMessages;
      try
      {
        commitMessages = commitMessageProvider.GetCommitMessages();
      }
      catch (Exception exception)
      {
        this.Log.LogErrorFromException(exception);
        return false;
      }

      var versionCalculator = this.CreateVersionCalculator();

      Version version;
      try
      {
        version = versionCalculator.Process(commitMessages);
      }
      catch (Exception exception)
      {
        this.Log.LogErrorFromException(exception);
        return false;
      }

      var versionPatcher = this.CreateVersionPatcher();
      try
      {
        version = versionPatcher.PatchBaseVersionWithVersion(this.BaseVersion,
                                                             version);
      }
      catch (Exception exception)
      {
        this.Log.LogErrorFromException(exception);
        return false;
      }

      this.PatchedVersion = version.ToString();

      return true;
    }

    [NotNull]
    public abstract ICommitMessageProvider CreateCommitMessageProvider();

    [NotNull]
    public virtual IVersionCalculator CreateVersionCalculator()
    {
      return new VersionCalculator(this.PatchFormat,
                                   this.FeatureFormat,
                                   this.BreakingChangeFormat);
    }

    [NotNull]
    public virtual IVersionPatcher CreateVersionPatcher()
    {
      return new VersionPatcher();
    }
  }
}
