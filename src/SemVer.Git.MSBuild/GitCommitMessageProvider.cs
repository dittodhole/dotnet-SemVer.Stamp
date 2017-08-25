using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LibGit2Sharp;
using SemVer.MSBuild;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace SemVer.Git.MSBuild
{
  public class GitCommitMessageProvider : ICommitMessageProvider
  {
    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    public GitCommitMessageProvider([NotNull] string repositoryPath,
                                    [CanBeNull] string baseRevision)
    {
      this.RepositoryPath = repositoryPath ?? throw new ArgumentNullException(nameof(repositoryPath));
      this.BaseRevision = baseRevision;
    }

    [NotNull]
    private string RepositoryPath { get; }

    [CanBeNull]
    private string BaseRevision { get; }

    /// <exception cref="InvalidOperationException" />
    /// <exception cref="Exception" />
    public virtual string[] GetCommitMessages()
    {
      string[] commitMessages;
      using (var repository = new Repository(this.RepositoryPath))
      {
        var statusOptions = new StatusOptions
                            {
                              ExcludeSubmodules = true,
                              DisablePathSpecMatch = true,
                              PathSpec = new[]
                                         {
                                           this.RepositoryPath
                                         },
                              Show = StatusShowOption.WorkDirOnly
                            };
        var status = repository.RetrieveStatus(statusOptions);
        if (status.IsDirty)
        {
          throw new InvalidOperationException($"Path {this.RepositoryPath} has local uncommitted changes.");
        }

        var branch = repository.Head;

        List<object> excludeReachableFrom;
        List<object> includeReachableFrom;
        if (string.IsNullOrEmpty(this.BaseRevision))
        {
          excludeReachableFrom = new List<object>
                                 {
                                   branch.CanonicalName
                                 };
          includeReachableFrom = null;
        }
        else
        {
          excludeReachableFrom = new List<object>
                                 {
                                   this.BaseRevision
                                 };
          includeReachableFrom = new List<object>
                                 {
                                   branch.Tip.Id
                                 };
        }

        var commitFilter = new CommitFilter
                           {
                             IncludeReachableFrom = includeReachableFrom,
                             ExcludeReachableFrom = excludeReachableFrom,
                             SortBy = CommitSortStrategies.Reverse | CommitSortStrategies.Time
                           };
        var commits = repository.Commits.QueryBy(commitFilter);

        commitMessages = commits.Select(arg => arg.Message)
                                .ToArray();
      }

      return commitMessages;
    }
  }
}
