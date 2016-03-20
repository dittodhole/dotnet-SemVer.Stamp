using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SharpSvn;

// ReSharper disable EventExceptionNotDocumented

namespace SemVer.Stamp.Svn
{
  public sealed class SvnSemVersionGrabber : SemVersionGrabberBase
  {
    public SvnSemVersionGrabber(Action<string> logInfo,
                                Action<string> logWarning,
                                Action<string> logError)
      : base(logInfo,
             logWarning,
             logError)
    {
    }

    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    protected override IEnumerable<string> GetCommitMessages(string repositoryPath,
                                                             string baseRevision)
    {
      if (repositoryPath == null)
      {
        throw new ArgumentNullException(nameof(repositoryPath));
      }

      SvnWorkingCopyVersion svnWorkingCopyVersion;
      using (var svnWorkingCopyClient = new SvnWorkingCopyClient())
      {
        if (!svnWorkingCopyClient.GetVersion(repositoryPath,
                                             out svnWorkingCopyVersion) ||
            svnWorkingCopyVersion == null)
        {
          this.LogError?.Invoke($"Could not get working copy version for {repositoryPath}");
          return Enumerable.Empty<string>();
        }
      }

      if (svnWorkingCopyVersion.Modified)
      {
        this.LogError?.Invoke($"Could not calculate version for {repositoryPath} due to local uncomitted changes");
        return Enumerable.Empty<string>();
      }

      Collection<SvnLogEventArgs> logItems;
      using (var svnClient = new SvnClient())
      {
        SvnRevision start;
        if (baseRevision == null)
        {
          start = SvnRevision.Zero;
        }
        else
        {
          int startRevision;
          if (!int.TryParse(baseRevision,
                            out startRevision))
          {
            this.LogError?.Invoke($"could not parse {nameof(baseRevision)} to {typeof (int).FullName}: {baseRevision}");
            return Enumerable.Empty<string>();
          }
          start = startRevision;
        }

        var svnLogArgs = new SvnLogArgs
                         {
                           StrictNodeHistory = true,
                           Range = new SvnRevisionRange(start,
                                                        SvnRevision.Head)
                         };
        if (!svnClient.GetLog(repositoryPath,
                              svnLogArgs,
                              out logItems) ||
            logItems == null)
        {
          this.LogError?.Invoke($"Could not get log for repository in {repositoryPath}");
          return null;
        }
      }

      var commitMessages = logItems.OrderBy(arg => arg.Revision)
                                   .Select(arg => arg.LogMessage);

      return commitMessages;
    }

    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="baseVersion" /> is <see langword="null" />.</exception>
    protected override Version PatchVersionBeforeCalculatingTheSemVersion(string repositoryPath,
                                                                          Version baseVersion)
    {
      if (repositoryPath == null)
      {
        throw new ArgumentNullException(nameof(repositoryPath));
      }
      if (baseVersion == null)
      {
        throw new ArgumentNullException(nameof(baseVersion));
      }

      SvnWorkingCopyVersion svnWorkingCopyVersion;
      using (var svnWorkingCopyClient = new SvnWorkingCopyClient())
      {
        if (!svnWorkingCopyClient.GetVersion(repositoryPath,
                                             out svnWorkingCopyVersion) ||
            svnWorkingCopyVersion == null)
        {
          this.LogError?.Invoke($"Could not get working copy version for {repositoryPath}");
          return baseVersion;
        }
      }

      var patchedBaseVersion = new Version(baseVersion.Major,
                                           baseVersion.Minor,
                                           baseVersion.Build,
                                           (int) svnWorkingCopyVersion.End);

      return patchedBaseVersion;
    }
  }
}
