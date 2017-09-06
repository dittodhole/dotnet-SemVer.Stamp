using JetBrains.Annotations;
using SemVer.MSBuild;

namespace SemVer.Svn.MSBuild
{
  [UsedImplicitly]
  public class SemVerStampTask : SemVerStampTaskBase
  {
    /// <inheritdoc />
    public override ICommitMessageProvider CreateCommitMessageProvider()
    {
      return new SvnCommitMessageProvider(this.RepositoryPath,
                                          this.BaseRevision);
    }
  }
}
