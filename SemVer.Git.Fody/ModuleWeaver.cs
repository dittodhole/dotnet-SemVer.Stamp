using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Version = System.Version;

// ReSharper disable NonLocalizedString
// ReSharper disable ThrowingSystemException
// ReSharper disable CatchAllClause
// ReSharper disable CheckNamespace
// ReSharper disable EventExceptionNotDocumented
// ReSharper disable ExceptionNotDocumented
// ReSharper disable ExceptionNotDocumentedOptional
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SemVer.Fody
{
  public sealed partial class ModuleWeaver
  {
    private void Prerequisites()
    {
      this.IncludeNativeBinariesFolderInPathEnvironmentVariable();
    }

    private void IncludeNativeBinariesFolderInPathEnvironmentVariable()
    {
      string architectureSubFolder;
      if (Environment.Is64BitProcess)
      {
        architectureSubFolder = "amd64";
      }
      else
      {
        architectureSubFolder = "x86";
      }

      var nativeBinariesPath = Path.Combine(this.AddinDirectoryPath,
                                            "NativeBinaries",
                                            architectureSubFolder);

      this.LogInfo($"NativeBinaries path: {nativeBinariesPath}");

      var existingPath = Environment.GetEnvironmentVariable("PATH");
      var newPath = string.Concat(nativeBinariesPath,
                                  Path.PathSeparator,
                                  existingPath);
      Environment.SetEnvironmentVariable("PATH",
                                         newPath);
    }

    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    public Version GetVersion(string repositoryPath,
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
        this.LogWarning($"found no git repository in {repositoryPath}");
        return null;
      }

      this.LogInfo($"found git repository in {repositoryPath}: {gitDirectory}");

      var relativePath = this.GetRelativePath(gitDirectory,
                                              repositoryPath);

      this.LogInfo($"relative path to {nameof(repositoryPath)}: {relativePath}");

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
          this.LogWarning($"Could not calculate version for {relativePath} due to local uncommitted changes");
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
            throw new WeavingException($"retrieving the commits from a {nameof(relativePath)} and a {nameof(baseRevision)} is currently not implemented");
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

      var version = this.GetVersionAccordingToSemVer(commitMessages,
                                                     baseVersion,
                                                     patchFormat,
                                                     featureFormat,
                                                     breakingChangeFormat);

      return version;
    }

    public string GetRelativePath(string gitDirectoryPath,
                                  string targetPath)
    {
      var repositoryPath = Directory.GetParent(gitDirectoryPath)
                                    .Parent.FullName;
      var result = targetPath.Substring(repositoryPath.Length)
                             .TrimStart(Path.DirectorySeparatorChar);

      return result;
    }
  }
}
