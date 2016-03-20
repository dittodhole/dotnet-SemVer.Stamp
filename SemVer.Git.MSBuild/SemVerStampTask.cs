using SemVer.MSBuild;
using SemVer.Stamp;
using SemVer.Stamp.Git;

namespace SemVer.Git.MSBuild
{
  public class SemVerStampTask : SemVerStampTaskBase
  {
    protected override SemVersionGrabberBase GetSemVersionGrabber()
    {
      var svnSemVersionGrabber = new GitSemVersionGrabber(arg => this.Log?.LogMessage(arg),
                                                          arg => this.Log?.LogWarning(arg),
                                                          arg => this.Log?.LogError(arg));

      return svnSemVersionGrabber;
    }
  }
}
