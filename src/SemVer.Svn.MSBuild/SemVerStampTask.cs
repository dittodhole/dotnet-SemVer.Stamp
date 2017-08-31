using JetBrains.Annotations;
using SemVer.MSBuild;

namespace SemVer.Svn.MSBuild
{
  [UsedImplicitly]
  public class SemVerStampTask : SemVerStampTaskBase
  {
    public override ICommitMessageProvider CreateCommitMessageProvider()
    {
      return new SvnCommitMessageProvider();
    }
  }
}
