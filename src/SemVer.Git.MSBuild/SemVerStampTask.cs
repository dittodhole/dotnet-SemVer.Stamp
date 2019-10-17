using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LibGit2Sharp;
using NDepend.Path;
using SemVer.MSBuild;

namespace SemVer.Git.MSBuild
{
  public sealed class SemVerStampTask : SemVerStampTaskBase
  {
    /// <inheritdoc />
    protected override string[] GetCommitMessages()
    {
      var repositoryPath = Repository.Discover(this.RepositoryPath);
      if (repositoryPath == null)
      {
        throw new InvalidOperationException($"Could not find repository for '{this.RepositoryPath}'");
      }

      var commonPath = SemVerStampTask.GetCommonPath(repositoryPath,
                                                     this.RepositoryPath);
      if (commonPath == null)
      {
        throw new InvalidOperationException($"'{this.RepositoryPath}' has no common path with '{repositoryPath}'");
      }

      string relativePath;
      try
      {
        relativePath = this.RepositoryPath.Substring(commonPath.Length);
      }
      catch (Exception exception)
      {
        throw new InvalidOperationException($"Could not get {nameof(relativePath)}",
                                            exception);
      }

      if (!string.IsNullOrEmpty(relativePath))
      {
        relativePath = relativePath.Replace("\\",
                                            "/");
      }

      string[] commitMessages;
      using (var repository = new Repository(repositoryPath))
      {
        var status = repository.RetrieveStatus(new StatusOptions
                                               {
                                                 ExcludeSubmodules = true,
                                                 Show = StatusShowOption.WorkDirOnly,
                                                 PathSpec = new[]
                                                            {
                                                              relativePath
                                                            }
                                               });
        if (status.IsDirty)
        {
          throw new InvalidOperationException($"Path {relativePath} in {commonPath} has uncommitted changes.");
        }

        var excludeReachableFrom = new List<object>
                                   {
                                     this.BaseRevision
                                   };
        var includeReachableFrom = new List<object>
                                   {
                                     repository.Head
                                   };

        // Unfortunately, CommitSortStrategies.Time | CommitSortStrategies.Reverse is not
        // supported, hence we are ordering the LINQ-way afterwards ... #smh
        var commitFilter = new CommitFilter
                           {
                             IncludeReachableFrom = includeReachableFrom,
                             ExcludeReachableFrom = excludeReachableFrom,
                             SortBy = CommitSortStrategies.Time
                           };

        IEnumerable<Commit> commits;
        if (string.IsNullOrEmpty(relativePath))
        {
          commits = repository.Commits.QueryBy(commitFilter);
        }
        else
        {
          commits = repository.Commits.QueryBy(relativePath,
                                               commitFilter)
                              .Select(arg => arg.Commit);
        }

        commitMessages = commits.OrderBy(arg => arg.Author.When)
                                .Select(arg => arg.Message)
                                .ToArray();
      }

      return commitMessages;
    }

    [CanBeNull]
    private static string GetCommonPath([NotNull] [ItemNotNull] params string[] paths)
    {
      paths.Select(arg => arg.ToAbsoluteDirectoryPath())
           .ToArray()
           .TryGetCommonRootDirectory(out var commonRootDirectory);

      return commonRootDirectory?.ToString();
    }
  }
}
