using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Version = System.Version;

// ReSharper disable EventExceptionNotDocumented
// ReSharper disable ExceptionNotDocumented
// ReSharper disable ExceptionNotDocumentedOptional

namespace SemVer.Stamp.Git
{
  public sealed class GitSemVersionGrabber : SemVersionGrabberBase
  {
    public GitSemVersionGrabber(Action<string> logInfo,
                                Action<string> logWarning,
                                Action<string> logError)
      : base(logInfo,
             logWarning,
             logError)
    {
    }

    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="NotSupportedException">
    ///   If you try to get a version for <paramref name="repositoryPath" /> equals the
    ///   solution directory and <paramref name="baseVersion" /> equals 0.0.0.?.
    /// </exception>
    public override Version GetVersion(string repositoryPath,
                                       Version baseVersion,
                                       string baseRevision,
                                       string patchFormat,
                                       string featureFormat,
                                       string breakingChangeFormat)
    {
      if (repositoryPath == null)
      {
        throw new ArgumentNullException(nameof(repositoryPath));
      }
      if (patchFormat == null)
      {
        throw new ArgumentNullException(nameof(patchFormat));
      }
      if (featureFormat == null)
      {
        throw new ArgumentNullException(nameof(featureFormat));
      }
      if (breakingChangeFormat == null)
      {
        throw new ArgumentNullException(nameof(breakingChangeFormat));
      }

      var gitDirectory = Repository.Discover(repositoryPath);
      if (gitDirectory == null)
      {
        this.LogWarning?.Invoke($"found no git repository in {repositoryPath}");
        return null;
      }

      this.LogInfo?.Invoke($"found git repository in {repositoryPath}: {gitDirectory}");

      var relativePath = this.GetRelativePath(gitDirectory,
                                              repositoryPath);

      this.LogInfo?.Invoke($"relative path to {nameof(repositoryPath)}: {relativePath}");

      ICollection<string> commitMessages;
      using (var repository = new Repository(gitDirectory))
      {
        var statusOptions = new StatusOptions
                            {
                              ExcludeSubmodules = true,
                              IncludeUnaltered = false,
                              DetectRenamesInIndex = false,
                              DetectRenamesInWorkDir = false,
                              DisablePathSpecMatch = true,
                              PathSpec = new[]
                                         {
                                           relativePath
                                         },
                              RecurseIgnoredDirs = false,
                              Show = StatusShowOption.IndexAndWorkDir
                            };
        var status = repository.RetrieveStatus(statusOptions);
        if (status.IsDirty)
        {
          this.LogWarning?.Invoke($"Could not calculate version for {relativePath} due to local uncommitted changes");
          return new Version();
        }

        var branch = repository.Head;

        IEnumerable<Commit> commits;
        if (string.IsNullOrEmpty(baseRevision))
        {
          commits = repository.Commits.QueryBy(relativePath)
                              .Select(arg => arg.Commit);
        }
        else
        {
          var includeReachableFrom = new List<string>
                                     {
                                       branch.CanonicalName,
                                       baseRevision
                                     };
          if (!string.IsNullOrEmpty(relativePath))
          {
            throw new NotSupportedException($"retrieving the commits from a {nameof(relativePath)} and a {nameof(baseRevision)} is currently not implemented");
          }

          var commitFilter = new CommitFilter
                             {
                               IncludeReachableFrom = includeReachableFrom,
                               SortBy = CommitSortStrategies.Reverse | CommitSortStrategies.Time
                             };
          commits = repository.Commits.QueryBy(commitFilter);
        }

        commitMessages = commits.Select(arg => arg.Message)
                                .ToArray();
      }

      var version = this.CalculateVersionAccordingToSemVer(commitMessages,
                                                           baseVersion,
                                                           patchFormat,
                                                           featureFormat,
                                                           breakingChangeFormat);

      return version;
    }

    private string GetRelativePath(string gitDirectoryPath,
                                   string targetPath)
    {
      var repositoryPath = Directory.GetParent(gitDirectoryPath)
        ?.Parent?.FullName;
      if (string.IsNullOrEmpty(repositoryPath))
      {
        return repositoryPath;
      }

      var result = targetPath.Substring(repositoryPath.Length)
                             .TrimStart(Path.DirectorySeparatorChar);

      return result;
    }
  }
}
