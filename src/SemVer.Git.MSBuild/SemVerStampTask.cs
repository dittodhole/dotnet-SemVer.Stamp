using SemVer.MSBuild;

namespace SemVer.Git.MSBuild
{
  public class SemVerStampTask : SemVerStampTaskBase
  {
    public override ICommitMessageProvider CreateCommitMessageProvider()
    {
      return new GitCommitMessageProvider(this.RepositoryPath,
                                          this.BaseRevision);
    }
  }
}
