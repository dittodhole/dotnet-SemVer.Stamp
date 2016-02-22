using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Version = System.Version;

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
        architectureSubFolder = "amd64"; // Not L10N
      }
      else
      {
        architectureSubFolder = "x86"; // Not L10N
      }

      var nativeBinariesPath = Path.Combine(this.AddinDirectoryPath,
                                            "NativeBinaries", // Not L10N
                                            architectureSubFolder);

      this.LogInfo($"NativeBinaries path: {nativeBinariesPath}");

      var existingPath = Environment.GetEnvironmentVariable("PATH"); // Not L10N
      var newPath = string.Concat(nativeBinariesPath,
                                  Path.PathSeparator,
                                  existingPath);
      Environment.SetEnvironmentVariable("PATH", // Not L10N
                                         newPath);
    }

    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    private Version GetVersion(string repositoryPath,
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

      string gitDirectory;
      try
      {
        gitDirectory = Repository.Discover(repositoryPath);
      }
      catch (Exception exception)
      {
        if (exception.Message.Contains("LibGit2Sharp.Core.NativeMethods") // Not L10N
            || exception.Message.Contains("FilePathMarshaler")) // Not L10N
        {
          throw new WeavingException("Restart of Visual Studio required due to update of SemVer.Git.Fody", // Not L10N
                                     exception);
        }
        throw;
      }

      if (gitDirectory == null)
      {
        this.LogWarning($"found no git repository in {repositoryPath}");
        return null;
      }

      this.LogInfo($"found git repository in {repositoryPath}");

      ICollection<string> commitMessages;
      using (var repository = new Repository(gitDirectory))
      {
        var commitFilter = new CommitFilter
                           {
                             FirstParentOnly = false,
                             SortBy = CommitSortStrategies.Reverse | CommitSortStrategies.Topological | CommitSortStrategies.Time
                           };
        var commits = repository.Commits.QueryBy(commitFilter);
        commitMessages = commits.Select(arg => string.Join(Environment.NewLine,
                                                           arg.Message,
                                                           arg.MessageShort))
                                .ToArray();
      }

      var version = this.GetVersionAccordingToSemVer(commitMessages,
                                                     patchFormat,
                                                     featureFormat,
                                                     breakingChangeFormat);

      return version;
    }
  }
}
