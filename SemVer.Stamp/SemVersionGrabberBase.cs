using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable NonLocalizedString

namespace SemVer.Stamp
{
  public abstract class SemVersionGrabberBase
  {
    public Action<string> LogError { get; set; }
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }

    /// <exception cref="ArgumentNullException"><paramref name="commitMessages" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    public Version GetVersionAccordingToSemVer(IEnumerable<string> commitMessages,
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

      var patch = baseVersion?.Build ?? 0;
      var feature = baseVersion?.Minor ?? 0;
      var breakingChange = baseVersion?.Major ?? 0;
      var revision = Math.Max(0,
                              baseVersion?.Revision ?? 0);

      this?.LogInfo($"baseVersion: {breakingChange}.{feature}.{patch}.{revision}");

      foreach (var commitMessage in commitMessages)
      {
        if (string.IsNullOrEmpty(commitMessage))
        {
          continue;
        }

        var patchMatches = Regex.Matches(commitMessage,
                                         patchFormat,
                                         RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled)
                                .OfType<Match>()
                                .ToArray();
        if (patchMatches.Any())
        {
          patch += patchMatches.Length;
        }

        var featureMatches = Regex.Matches(commitMessage,
                                           featureFormat,
                                           RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled)
                                  .OfType<Match>()
                                  .ToArray();
        if (featureMatches.Any())
        {
          patch = 0;
          feature += featureMatches.Length;
        }

        var breakingChangeMatches = Regex.Matches(commitMessage,
                                                  breakingChangeFormat,
                                                  RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled)
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
