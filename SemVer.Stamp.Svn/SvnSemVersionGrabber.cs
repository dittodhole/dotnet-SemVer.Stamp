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
    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    public SvnSemVersionGrabber(string repositoryPath,
                                string baseRevision,
                                Action<string> logInfo,
                                Action<string> logWarning,
                                Action<string> logError)
      : base(logInfo,
             logWarning,
             logError)
    {
      if (repositoryPath == null)
      {
        throw new ArgumentNullException(nameof(repositoryPath));
      }

      this.RepositoryPath = repositoryPath;
      this.BaseRevision = baseRevision;
    }

    private string BaseRevision { get; }

    private string RepositoryPath { get; }

    protected override IEnumerable<string> GetCommitMessages()
    {
      SvnWorkingCopyVersion svnWorkingCopyVersion;
      using (var svnWorkingCopyClient = new SvnWorkingCopyClient())
      {
        if (!svnWorkingCopyClient.GetVersion(this.RepositoryPath,
                                             out svnWorkingCopyVersion) ||
            svnWorkingCopyVersion == null)
        {
          this.LogError?.Invoke($"Could not get working copy version for {this.RepositoryPath}");
          return Enumerable.Empty<string>();
        }
      }

      if (svnWorkingCopyVersion.Modified)
      {
        this.LogError?.Invoke($"Could not calculate version for {this.RepositoryPath} due to local uncomitted changes");
        return Enumerable.Empty<string>();
      }

      Collection<SvnLogEventArgs> logItems;
      using (var svnClient = new SvnClient())
      {
        SvnRevision start;
        if (this.BaseRevision == null)
        {
          start = SvnRevision.Zero;
        }
        else
        {
          int startRevision;
          if (!int.TryParse(this.BaseRevision,
                            out startRevision))
          {
            this.LogError?.Invoke($"could not parse {nameof(this.BaseRevision)} to {typeof (int).FullName}: {this.BaseRevision}");
            return Enumerable.Empty<string>();
          }
          start = startRevision;
        }

        this.LogInfo?.Invoke($"retrieving commits from {this.RepositoryPath} since {start}");

        var svnLogArgs = new SvnLogArgs
                         {
                           StrictNodeHistory = false,
                           Range = new SvnRevisionRange(start,
                                                        SvnRevision.Head)
                         };
        if (!svnClient.GetLog(this.RepositoryPath,
                              svnLogArgs,
                              out logItems) ||
            logItems == null)
        {
          this.LogError?.Invoke($"Could not get log for repository in {this.RepositoryPath}");
          return null;
        }
      }

      var commitMessages = logItems.OrderBy(arg => arg.Revision)
                                   .Select(arg => arg.LogMessage);

      return commitMessages;
    }

    /// <exception cref="ArgumentNullException"><paramref name="baseVersion" /> is <see langword="null" />.</exception>
    protected override Version PatchVersionBeforeCalculatingTheSemVersion(Version baseVersion)
    {
      if (baseVersion == null)
      {
        throw new ArgumentNullException(nameof(baseVersion));
      }

      SvnWorkingCopyVersion svnWorkingCopyVersion;
      using (var svnWorkingCopyClient = new SvnWorkingCopyClient())
      {
        if (!svnWorkingCopyClient.GetVersion(this.RepositoryPath,
                                             out svnWorkingCopyVersion) ||
            svnWorkingCopyVersion == null)
        {
          this.LogError?.Invoke($"Could not get working copy version for {this.RepositoryPath}");
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
