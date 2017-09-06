using System;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace SemVer.MSBuild
{
  public interface IVersionCalculator
  {
    /// <exception cref="ArgumentNullException"><paramref name="commitMessages" /> is <see langword="null" /></exception>
    /// <exception cref="Exception" />
    [NotNull]
    Version Process([NotNull] [ItemNotNull] string[] commitMessages);
  }

  public class VersionCalculator : IVersionCalculator
  {
    [NotNull]
    public const string DefaultPatchFormat = @"^fix(\(.*\))*: ";

    [NotNull]
    public const string DefaultFeatureFormat = @"^feat(\(.*\))*: ";

    [NotNull]
    public const string DefaultBreakingChangeFormat = @"^perf(\(.*\))*: ";

    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" /></exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" /></exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" /></exception>
    public VersionCalculator([NotNull] string patchFormat,
                             [NotNull] string featureFormat,
                             [NotNull] string breakingChangeFormat)
    {
      this.PatchFormat = patchFormat ?? throw new ArgumentNullException(nameof(patchFormat));
      this.FeatureFormat = featureFormat ?? throw new ArgumentNullException(nameof(featureFormat));
      this.BreakingChangeFormat = breakingChangeFormat ?? throw new ArgumentNullException(nameof(breakingChangeFormat));
    }

    public VersionCalculator()
    {
      this.PatchFormat = VersionCalculator.DefaultPatchFormat;
      this.FeatureFormat = VersionCalculator.DefaultFeatureFormat;
      this.BreakingChangeFormat = VersionCalculator.DefaultBreakingChangeFormat;
    }

    [NotNull]
    private string PatchFormat { get; }

    [NotNull]
    private string FeatureFormat { get; }

    [NotNull]
    private string BreakingChangeFormat { get; }

    /// <exception cref="ArgumentNullException"><paramref name="commitMessages" /> is <see langword="null" /></exception>
    /// <exception cref="ArgumentException" />
    /// <exception cref="ArgumentOutOfRangeException" />
    /// <exception cref="Exception" />
    public virtual Version Process(string[] commitMessages)
    {
      if (commitMessages == null)
      {
        throw new ArgumentNullException(nameof(commitMessages));
      }

      var patch = 0;
      var feature = 0;
      var breakingChange = 0;

      foreach (var commitMessage in commitMessages)
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

      var version = new Version(breakingChange,
                                feature,
                                patch);

      return version;
    }
  }
}
