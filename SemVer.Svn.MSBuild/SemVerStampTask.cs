using SemVer.MSBuild;
using SemVer.Stamp;
using SemVer.Stamp.Svn;

namespace SemVer.Svn.MSBuild
{
  public class SemVerStampTask : SemVerStampTaskBase
  {
    protected override SemVersionGrabberBase GetSemVersionGrabber(string repositoryPath,
                                                                  string baseRevision)
    {
      var semVersionGrabber = new SvnSemVersionGrabber(repositoryPath,
                                                       baseRevision,
                                                       arg => this.Log?.LogMessage(arg),
                                                       arg => this.Log?.LogWarning(arg),
                                                       arg => this.Log?.LogError(arg));

      return semVersionGrabber;
    }
  }
}
