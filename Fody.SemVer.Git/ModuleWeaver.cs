using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LibGit2Sharp;
using Mono.Cecil;
using Version = System.Version;

// ReSharper disable EventExceptionNotDocumented
// ReSharper disable ExceptionNotDocumented
// ReSharper disable ExceptionNotDocumentedOptional
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Fody.SemVer.Git
{
  public sealed class ModuleWeaver : ModuleWeaverBase
  {
    public string AddinDirectoryPath { get; set; }
    public string AssemblyFilePath { get; set; }
    public XElement Config { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public string ProjectDirectoryPath { get; set; }
    public string SolutionDirectoryPath { get; set; }

    public void AfterWeaving()
    {
      this.PatchVersionOfAssemblyTheSemVerWay(this.Config,
                                              this.AssemblyFilePath,
                                              this.AddinDirectoryPath,
                                              this.SolutionDirectoryPath,
                                              this.ProjectDirectoryPath);
    }

    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    protected override Version GetVersion(string repositoryPath,
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
