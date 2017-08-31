﻿using JetBrains.Annotations;
using SemVer.MSBuild;

namespace SemVer.Git.MSBuild
{
  [UsedImplicitly]
  public class SemVerStampTask : SemVerStampTaskBase
  {
    public override ICommitMessageProvider CreateCommitMessageProvider()
    {
      return new GitCommitMessageProvider(this.RepositoryPath,
                                          this.BaseRevision);
    }
  }
}
