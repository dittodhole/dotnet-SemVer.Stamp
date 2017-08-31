using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LibGit2Sharp;
using NDepend.Path;
using SemVer.MSBuild;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MemberCanBeProtected.Global

namespace SemVer.Git.MSBuild
{
  public class GitCommitMessageProvider : CommitMessageProviderBase
  {
    /// <inheritdoc />
    public GitCommitMessageProvider([NotNull] string path,
                                    [CanBeNull] string baseRevision)
      : base(path,
             baseRevision) { }

    /// <exception cref="InvalidOperationException" />
    /// <exception cref="Exception" />
    public override string[] GetCommitMessages()
    {
      var repositoryPath = Repository.Discover(this.Path);
      if (repositoryPath == null)
      {
        throw new InvalidOperationException($"Could not find repository for {this.Path}.");
      }

      var commonPath = this.GetCommonPath(repositoryPath,
                                          this.Path);
      if (commonPath == null)
      {
        throw new InvalidOperationException($"{this.Path} has no common path with {repositoryPath}.");
      }

      commonPath = this.PathAddBackslash(commonPath);

      var relativePath = this.Path.Substring(commonPath.Length);
      if (!string.IsNullOrEmpty(relativePath))
      {
        relativePath = relativePath.Replace("\\",
                                            "/");
      }

      //Debug.WriteLine($"{nameof(this.Path)}: {this.Path}");
      //Debug.WriteLine($"{nameof(repositoryPath)}: {repositoryPath}");
      //Debug.WriteLine($"{nameof(commonPath)}: {commonPath}");
      //Debug.WriteLine($"{nameof(relativePath)}: {relativePath}");
      //Debug.WriteLine($"{nameof(this.BaseRevision)}: {this.BaseRevision}");

      string[] commitMessages;
      using (var repository = new Repository(repositoryPath))
      {
        var statusOptions = new StatusOptions
                            {
                              ExcludeSubmodules = true,
                              Show = StatusShowOption.WorkDirOnly,
                              PathSpec = new[]
                                         {
                                           relativePath
                                         }
                            };

        var status = repository.RetrieveStatus(statusOptions);
        if (status.IsDirty)
        {
          throw new InvalidOperationException($"Path {relativePath} in {commonPath} has local uncommitted changes.");
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

    /// <exception cref="ArgumentNullException"><paramref name="paths" /> is <see langword="null" />.</exception>
    [CanBeNull]
    public virtual string GetCommonPath([NotNull] [ItemNotNull] params string[] paths)
    {
      if (paths == null)
      {
        throw new ArgumentNullException(nameof(paths));
      }

      paths.Select(arg => arg.ToAbsoluteDirectoryPath())
           .ToArray()
           .TryGetCommonRootDirectory(out var commonRootDirectory);

      return commonRootDirectory?.ToString();
    }
  }
}
