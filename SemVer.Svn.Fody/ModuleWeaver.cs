using System;
using System.Collections.ObjectModel;
using System.Linq;
using SharpSvn;

// ReSharper disable CatchAllClause
// ReSharper disable CheckNamespace
// ReSharper disable ExceptionNotDocumented
// ReSharper disable ExceptionNotDocumentedOptional
// ReSharper disable EventExceptionNotDocumented
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace SemVer.Fody
{
  public sealed partial class ModuleWeaver
  {
    private void Prerequisites()
    {
    }

    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="WeavingException">If <paramref name="baseRevision" /> could not be parsed to <see cref="int"/>.</exception>
    private Version GetVersion(string repositoryPath,
                               Version baseVersion,
                               string baseRevision,
                               string patchFormat,
                               string featureFormat,
                               string breakingChangeFormat)
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
          this.LogError($"Could not get working copy version for {repositoryPath}");
          return null;
        }
      }

      if (svnWorkingCopyVersion.Modified)
      {
        this.LogWarning($"Could not calculate version for {repositoryPath} due to local changes");
        return new Version();
      }

      Collection<SvnLogEventArgs> logItems;
      //SvnInfoEventArgs svnInfoEventArgs;
      using (var svnClient = new SvnClient())
      {
        SvnRevision start;
        if (baseRevision == null)
        {
          start = SvnRevision.Zero;
        }
        else
        {
          try
          {
            start = int.Parse(baseRevision);
          }
          catch (Exception excpetion)
          {
            throw new WeavingException($"could not parse {nameof(Configuration.BaseRevision)} to {typeof (int).FullName}: {baseRevision}",
                                       excpetion);
          }
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
          this.LogError($"Could not get log for repository in {repositoryPath}");
          return null;
        }

        //if (!svnClient.GetInfo(repositoryPath,
        //                       out svnInfoEventArgs) ||
        //    svnInfoEventArgs == null)
        //{
        //  this.LogError($"Could not get info for repository in {repositoryPath}");
        //  return null;
        //}
      }

      if (baseVersion == null)
      {
        baseVersion = new Version(0,
                                  0,
                                  0,
                                  (int) svnWorkingCopyVersion.End);
      }
      else
      {
        baseVersion = new Version(baseVersion.Major,
                                  baseVersion.Minor,
                                  baseVersion.Build,
                                  (int) svnWorkingCopyVersion.End);
      }

      var commitMessages = logItems.OrderBy(arg => arg.Revision)
                                   .Select(arg => arg.LogMessage);
      var version = this.GetVersionAccordingToSemVer(commitMessages,
                                                     baseVersion,
                                                     patchFormat,
                                                     featureFormat,
                                                     breakingChangeFormat);

      return version;
    }
  }
}
