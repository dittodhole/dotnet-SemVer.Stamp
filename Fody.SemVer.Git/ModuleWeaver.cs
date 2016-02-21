using System;
using System.Xml.Linq;
using Mono.Cecil;

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

    public void Execute()
    {
      this.Execute(this.Config,
                   this.AssemblyFilePath,
                   this.AddinDirectoryPath,
                   this.SolutionDirectoryPath,
                   this.ProjectDirectoryPath);
    }

    protected override Version GetVersion(string solutionPath,
                                          string projectPath)
    {
      throw new NotImplementedException();
    }
  }
}
