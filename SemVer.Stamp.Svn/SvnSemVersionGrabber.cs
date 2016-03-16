using System;
using System.Collections.ObjectModel;
using System.Linq;
using SharpSvn;

// ReSharper disable EventExceptionNotDocumented
// ReSharper disable ExceptionNotDocumentedOptional

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
    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">If <paramref name="baseRevision" /> could not be parsed.</exception>
    public override Version GetVersion(string repositoryPath,
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
          this.LogError?.Invoke($"Could not get working copy version for {repositoryPath}");
          return null;
        }
      }

      if (svnWorkingCopyVersion.Modified)
      {
        this.LogWarning?.Invoke($"Could not calculate version for {repositoryPath} due to local uncomitted changes");
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
            throw new ArgumentException($"could not parse {nameof(baseRevision)} to {typeof (int).FullName}: {baseRevision}",
                                        nameof(baseRevision),
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
          this.LogError?.Invoke($"Could not get log for repository in {repositoryPath}");
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

      var version = this.CalculateVersionAccordingToSemVer(commitMessages,
                                                           baseVersion,
                                                           patchFormat,
                                                           featureFormat,
                                                           breakingChangeFormat);

      return version;
    }
  }
}
