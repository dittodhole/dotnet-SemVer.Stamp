using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SemVer.Stamp
{
  public abstract class SemVersionGrabberBase
  {
    protected SemVersionGrabberBase(Action<string> logInfo,
                                    Action<string> logWarning,
                                    Action<string> logError)
    {
      this.LogInfo = logInfo;
      this.LogWarning = logWarning;
      this.LogError = logError;
    }

    protected Action<string> LogError { get; }
    protected Action<string> LogInfo { get; }
    protected Action<string> LogWarning { get; }

    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    public Version GetVersion(Version baseVersion,
                              string patchFormat,
                              string featureFormat,
                              string breakingChangeFormat)
    {
      if (patchFormat == null)
      {
        throw new ArgumentNullException(nameof(patchFormat));
      }
      if (featureFormat == null)
      {
        throw new ArgumentNullException(nameof(featureFormat));
      }
      if (breakingChangeFormat == null)
      {
        throw new ArgumentNullException(nameof(breakingChangeFormat));
      }

      if (baseVersion == null)
      {
        baseVersion = new Version(0,
                                  0);
      }

      var commitMessages = this.GetCommitMessages();
      baseVersion = this.PatchVersionBeforeCalculatingTheSemVersion(baseVersion);
      var version = this.CalculateVersionAccordingToSemVer(commitMessages,
                                                           baseVersion,
                                                           patchFormat,
                                                           featureFormat,
                                                           breakingChangeFormat);

      return version;
    }

    /// <exception cref="ArgumentNullException"><paramref name="baseVersion" /> is <see langword="null" />.</exception>
    protected virtual Version PatchVersionBeforeCalculatingTheSemVersion(Version baseVersion)
    {
      if (baseVersion == null)
      {
        throw new ArgumentNullException(nameof(baseVersion));
      }

      return baseVersion;
    }

    protected abstract IEnumerable<string> GetCommitMessages();

    /// <exception cref="ArgumentNullException"><paramref name="commitMessages" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    private Version CalculateVersionAccordingToSemVer(IEnumerable<string> commitMessages,
                                                      Version baseVersion,
                                                      string patchFormat,
                                                      string featureFormat,
                                                      string breakingChangeFormat)
    {
      if (commitMessages == null)
      {
        throw new ArgumentNullException(nameof(commitMessages));
      }
      if (patchFormat == null)
      {
        throw new ArgumentNullException(nameof(patchFormat));
      }
      if (featureFormat == null)
      {
        throw new ArgumentNullException(nameof(featureFormat));
      }
      if (breakingChangeFormat == null)
      {
        throw new ArgumentNullException(nameof(breakingChangeFormat));
      }

      var patch = Math.Max(baseVersion?.Build ?? 0,
                           0);
      var feature = Math.Max(baseVersion?.Minor ?? 0,
                             0);
      var breakingChange = Math.Max(baseVersion?.Major ?? 0,
                                    0);
      var revision = Math.Max(baseVersion?.Revision ?? 0,
                              0);

      this.LogInfo?.Invoke($"baseVersion: {breakingChange}.{feature}.{patch}.{revision}");

      foreach (var commitMessage in commitMessages)
      {
        if (string.IsNullOrEmpty(commitMessage))
        {
          continue;
        }

        var patchMatches = Regex.Matches(commitMessage,
                                         patchFormat,
                                         RegexOptions.Multiline | RegexOptions.IgnoreCase)
                                .OfType<Match>()
                                .ToArray();
        if (patchMatches.Any())
        {
          patch += patchMatches.Length;
        }

        var featureMatches = Regex.Matches(commitMessage,
                                           featureFormat,
                                           RegexOptions.Multiline | RegexOptions.IgnoreCase)
                                  .OfType<Match>()
                                  .ToArray();
        if (featureMatches.Any())
        {
          patch = 0;
          feature += featureMatches.Length;
        }

        var breakingChangeMatches = Regex.Matches(commitMessage,
                                                  breakingChangeFormat,
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
                                patch,
                                revision);

      return version;
    }
  }
}