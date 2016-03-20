using System.IO;
using System.Linq;
using System.Xml.Linq;
using SemVer.Stamp.Git;

// ReSharper disable NonLocalizedString

namespace SemVer.Fody.Console
{
  public static class Program
  {
    static void Main(string[] args)
    {
      var solutionPath = args.ElementAtOrDefault(0);
      if (solutionPath == null)
      {
        return;
      }

      var projectPath = args.ElementAtOrDefault(1);
      if (projectPath == null)
      {
        return;
      }

      var configFullFileName = Path.Combine(projectPath,
                                            "FodyWeavers.xml");

      var xdocument = XDocument.Load(configFullFileName);
      var xweavers = xdocument.Element("Weavers");
      if (xweavers == null)
      {
        return;
      }

      var xelement = xweavers.Element("SemVer.Git") ?? xweavers.Element("SemVer.Svn");
      if (xelement == null)
      {
        return;
      }

      var configuration = new Configuration(xelement);

      string repositoryPath;
      if (configuration.UseProject)
      {
        repositoryPath = projectPath;
      }
      else
      {
        repositoryPath = solutionPath;
      }

      var gitSemVersionGrabber = new GitSemVersionGrabber(null,
                                                          null,
                                                          null);

      var version = gitSemVersionGrabber.GetVersion(repositoryPath,
                                                    configuration.BaseVersion,
                                                    configuration.BaseRevision,
                                                    configuration.PatchFormat,
                                                    configuration.FeatureFormat,
                                                    configuration.BreakingChangeFormat);

      System.Console.WriteLine(version);
    }
  }
}
