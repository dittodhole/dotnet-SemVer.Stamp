using SemVer.Stamp.Svn;

// ReSharper disable CheckNamespace

namespace SemVer.Fody
{
  public sealed partial class ModuleWeaver
  {
    private void Prerequisites()
    {
      this.SemVersionGrabber = new SvnSemVersionGrabber(this.LogInfo,
                                                        this.LogWarning,
                                                        this.LogError);
    }
  }
}
