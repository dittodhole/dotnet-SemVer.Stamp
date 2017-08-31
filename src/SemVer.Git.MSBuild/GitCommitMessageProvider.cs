using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LibGit2Sharp;
using NDepend.Path;
using SemVer.MSBuild;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace SemVer.Git.MSBuild
{
  public class GitCommitMessageProvider : ICommitMessageProvider
  {
    /// <exception cref="ArgumentNullException"><paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="path" /> is not rooted.</exception>
    public GitCommitMessageProvider([NotNull] string path,
                                    [CanBeNull] string baseRevision)
    {
      if (path == null)
      {
        throw new ArgumentNullException(nameof(path));
      }
      if (!System.IO.Path.IsPathRooted(path))
      {
        throw new ArgumentException($"{nameof(path)} must be rooted.",
                                    nameof(path));
      }
      this.Path = this.PathAddBackslash(path);
      this.BaseRevision = baseRevision;
    }

    [NotNull]
    private string Path { get; }

    [CanBeNull]
    private string BaseRevision { get; }

    /// <exception cref="InvalidOperationException" />
    /// <exception cref="Exception" />
    public virtual string[] GetCommitMessages()
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

    /// <remarks>https://stackoverflow.com/a/20406065</remarks>
    [NotNull]
    public virtual string PathAddBackslash([NotNull] string path)
    {
      // They're always one character but EndsWith is shorter than
      // array style access to last path character. Change this
      // if performance are a (measured) issue.
      string separator1 = System.IO.Path.DirectorySeparatorChar.ToString();
      string separator2 = System.IO.Path.AltDirectorySeparatorChar.ToString();

      // Trailing white spaces are always ignored but folders may have
      // leading spaces. It's unusual but it may happen. If it's an issue
      // then just replace TrimEnd() with Trim(). Tnx Paul Groke to point this out.
      path = path.TrimEnd();

      // Argument is always a directory name then if there is one
      // of allowed separators then I have nothing to do.
      if (path.EndsWith(separator1) || path.EndsWith(separator2))
        return path;

      // If there is the "alt" separator then I add a trailing one.
      // Note that URI format (file://drive:\path\filename.ext) is
      // not supported in most .NET I/O functions then we don't support it
      // here too. If you have to then simply revert this check:
      // if (path.Contains(separator1))
      //     return path + separator1;
      //
      // return path + separator2;
      if (path.Contains(separator2))
        return path + separator2;

      // If there is not an "alt" separator I add a "normal" one.
      // It means path may be with normal one or it has not any separator
      // (for example if it's just a directory name). In this case I
      // default to normal as users expect.
      return path + separator1;
    }
  }
}
