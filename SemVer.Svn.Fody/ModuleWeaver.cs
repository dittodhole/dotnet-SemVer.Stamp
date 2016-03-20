using SemVer.Fody;
using SemVer.Stamp.Svn;

namespace SemVer.Svn.Fody
{
  public sealed class ModuleWeaver : ModuleWeaverBase
  {
    protected override void Prerequisites()
    {
      this.SemVersionGrabber = new SvnSemVersionGrabber(this.LogInfo,
                                                        this.LogWarning,
                                                        this.LogError);
    }
  }
}
