using System.IO;
using System.Xml.Linq;
using Mono.Cecil;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

// ReSharper disable NonLocalizedString

namespace SemVer.Fody
{
  public class ModuleWeaver
  {
    public XElement Config { get; set; }
    public string ProjectDirectoryPath { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }

    public void Execute()
    {
      var configuration = new Configuration(this.Config);
      var msBuildConfigurationTemplate = new MSBuildConfigurationTemplate();
      msBuildConfigurationTemplate.Session["configuration"] = configuration;
      msBuildConfigurationTemplate.Initialize();
      var msbuildConfiguration = msBuildConfigurationTemplate.TransformText();

      var fileName = "SemVer.MSBuild.props";
      var fullFileName = Path.Combine(this.ProjectDirectoryPath,
                                      fileName);

      File.WriteAllText(fullFileName,
                        msbuildConfiguration);
    }
  }
}
