using SemVer.Fody;
using SemVer.Stamp;
using SemVer.Stamp.Svn;

namespace SemVer.Svn.Fody
{
  public sealed class ModuleWeaver : ModuleWeaverBase
  {
    protected override SemVersionGrabberBase GetSemVersionGrabber(string repositoryPath,
                                                                  string baseRevision)
    {
      var semVersionGrabber = new SvnSemVersionGrabber(repositoryPath,
                                                       baseRevision,
                                                       this.LogInfo,
                                                       this.LogWarning,
                                                       this.LogError);

      return semVersionGrabber;
    }
  }
}
