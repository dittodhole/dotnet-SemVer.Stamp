using System;
using System.Linq;
using System.Text.RegularExpressions;
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

    /// <inheritdoc />
    public override bool Execute()
    {
      Version version;
      try
      {
        version = this.CalculateVersion();
      }
      catch (Exception exception)
      {
        this.Log.LogErrorFromException(exception);

        return false;
      }

      this.PatchedVersion = version.ToString();

      return true;
    }

    /// <exception cref="Exception"/>
    [NotNull]
    private Version CalculateVersion()
    {
      int breakingChange;
      int feature;
      int patch;

      if (this.BaseVersion == null)
      {
        breakingChange = 0;
        feature = 0;
        patch = 0;
      }
      else
      {
        // TODO parse as semVer
        Version baseline;
        try
        {
          baseline = Version.Parse(this.BaseVersion);
        }
        catch (Exception exception)
        {
          throw new ArgumentOutOfRangeException($"Failed to parse {nameof(this.BaseVersion)} '{this.BaseVersion}'",
                                                exception);
        }

        breakingChange = baseline.Major;
        feature = baseline.Minor;
        patch = baseline.Build;
      }

      foreach (var commitMessage in this.GetCommitMessages())
      {
        var patchMatches = Regex.Matches(commitMessage,
                                         this.PatchFormat,
                                         RegexOptions.Multiline | RegexOptions.IgnoreCase)
                                .OfType<Match>()
                                .ToArray();
        if (patchMatches.Any())
        {
          patch += patchMatches.Length;
        }

        var featureMatches = Regex.Matches(commitMessage,
                                           this.FeatureFormat,
                                           RegexOptions.Multiline | RegexOptions.IgnoreCase)
                                  .OfType<Match>()
                                  .ToArray();
        if (featureMatches.Any())
        {
          patch = 0;
          feature += featureMatches.Length;
        }

        var breakingChangeMatches = Regex.Matches(commitMessage,
                                                  this.BreakingChangeFormat,
                                                  RegexOptions.Multiline | RegexOptions.IgnoreCase)
                                         .OfType<Match>()
                                         .ToArray();
        if (breakingChangeMatches.Any())
        {
          patch = 0;
          feature = 0;
          breakingChange += breakingChangeMatches.Length;
        }
      }

      var result = new Version(breakingChange,
                               feature,
                               patch);

      return result;
    }

    /// <exception cref="Exception"/>
    [NotNull]
    [ItemNotNull]
    protected abstract string[] GetCommitMessages();
  }
}
