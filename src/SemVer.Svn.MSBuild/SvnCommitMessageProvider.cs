using System;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using SemVer.MSBuild;
using SharpSvn;

namespace SemVer.Svn.MSBuild
{
  public class SvnCommitMessageProvider : CommitMessageProviderBase
  {
    /// <inheritdoc />
    public SvnCommitMessageProvider([NotNull] string path,
                                    [CanBeNull] string baseRevision)
      : base(path,
             baseRevision) { }

    /// <exception cref="InvalidOperationException" />
    /// <exception cref="Exception" />
    public override string[] GetCommitMessages()
    {
      SvnWorkingCopyVersion svnWorkingCopyVersion;
      using (var svnWorkingCopyClient = new SvnWorkingCopyClient())
      {
        if (!svnWorkingCopyClient.GetVersion(this.Path,
                                             out svnWorkingCopyVersion))
        {
          throw new InvalidOperationException($"Could not get version for {this.Path}.");
        }
      }

      if (svnWorkingCopyVersion.Modified)
      {
        throw new InvalidOperationException($"{this.Path} has uncommitted changes.");
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
          if (!int.TryParse(this.BaseRevision,
                            out var startRevision))
          {
            throw new InvalidOperationException($"Could not parse {nameof(this.BaseRevision)}: {this.BaseRevision}");
          }
          start = startRevision;
        }

        var svnLogArgs = new SvnLogArgs
                         {
                           StrictNodeHistory = false,
                           Range = new SvnRevisionRange(start,
                                                        SvnRevision.Head)
                         };
        if (!svnClient.GetLog(this.Path,
                              svnLogArgs,
                              out logItems))
        {
          throw new InvalidOperationException($"Could not get log for {this.Path}.");
        }
      }

      var commitMessages = logItems.OrderBy(arg => arg.Revision)
                                   .Select(arg => arg.LogMessage)
                                   .ToArray();

      return commitMessages;
    }
  }
}
