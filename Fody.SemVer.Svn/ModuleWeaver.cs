using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Mono.Cecil;
using SharpSvn;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Fody.SemVer.Svn
{
  public sealed class ModuleWeaver : ModuleWeaverBase
  {
    public string AddinDirectoryPath { get; set; }
    public string AssemblyFilePath { get; set; }
    public XElement Config { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public string ProjectDirectoryPath { get; set; }
    public string SolutionDirectoryPath { get; set; }

    public void Execute()
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

      Collection<SvnLogEventArgs> logItems;
      using (var svnClient = new SvnClient())
      {
        var svnLogArgs = new SvnLogArgs
                         {
                           StrictNodeHistory = true
                         };
        if (!svnClient.GetLog(repositoryPath,
                              svnLogArgs,
                              out logItems) ||
            logItems == null)
        {
          this.LogError($"Could not get log for repository in {repositoryPath}");
          return null;
        }
      }

      var commitMessages = logItems.Select(arg => arg.LogMessage);
      var version = this.GetVersionAccordingToSemVer(commitMessages,
                                                     patchFormat,
                                                     featureFormat,
                                                     breakingChangeFormat);

      return version;
    }
  }
}
