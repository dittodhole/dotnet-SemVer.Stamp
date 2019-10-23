using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LibGit2Sharp;
using NDepend.Path;
using SemVer.MSBuild;

namespace SemVer.Git.MSBuild
{
  public sealed class GetVersion : GetVersionBase
  {
    /// <inheritdoc />
    protected override string[] GetCommitMessages()
    {
      var repositoryPath = Repository.Discover(this.SourcePath) ?? throw new InvalidOperationException($"Could not find repository for '{this.SourcePath}'");
      var commonPath = GetVersion.GetCommonPath(repositoryPath,
                                                this.SourcePath) ?? throw new InvalidOperationException($"'{this.SourcePath}' has no common path with '{repositoryPath}'");

      string relativePath;
      try
      {
        relativePath = this.SourcePath.Substring(commonPath.Length);
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

      string[] result;
      using (var repository = new Repository(repositoryPath))
      {
        // Unfortunately, CommitSortStrategies.Time | CommitSortStrategies.Reverse is not
        // supported, hence we are ordering the LINQ-way afterwards ... #smh
        var commitFilter = new CommitFilter
                           {
                             IncludeReachableFrom = repository.Head,
                             ExcludeReachableFrom = this.BaseRevision,
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

        result = commits.OrderBy(arg => arg.Author.When)
                        .Select(arg => arg.Message)
                        .ToArray();
      }

      return result;
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
