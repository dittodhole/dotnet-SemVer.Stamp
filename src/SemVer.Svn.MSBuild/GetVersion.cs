using System;
using System.Collections.ObjectModel;
using System.Linq;
using SemVer.MSBuild;
using SharpSvn;

namespace SemVer.Svn.MSBuild
{
  public sealed class GetVersion : GetVersionBase
  {
    /// <inheritdoc/>
    protected override string[] GetCommitMessages()
    {
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
          try
          {
            startRevision = int.Parse(this.BaseRevision);
          }
          catch (Exception exception)
          {
            throw new InvalidOperationException($"Could not parse {nameof(this.BaseRevision)} '{this.BaseRevision}'",
                                                exception);
          }

          start = startRevision;
        }

        if (!svnClient.GetLog(this.SourcePath,
                              new SvnLogArgs
                              {
                                StrictNodeHistory = false,
                                Range = new SvnRevisionRange(start,
                                                             SvnRevision.Head)
                              },
                              out logItems))
        {
          throw new InvalidOperationException($"Could not get log for '{this.SourcePath}'");
        }
      }

      var result = logItems.OrderBy(arg => arg.Revision)
                           .Select(arg => arg.LogMessage)
                           .ToArray();

      return result;
    }
  }
}
