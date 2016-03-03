using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

// ReSharper disable EmptyGeneralCatchClause
// ReSharper disable CatchAllClause
// ReSharper disable NonLocalizedString

namespace SemVer.Fody.Console
{
  public static class Program
  {
    static void Main(string[] args)
    {
      AppDomain.CurrentDomain.AssemblyResolve += (sender,
                                                  eventArgs) =>
                                                 {
                                                   var fileNames = new[]
                                                                   {
                                                                     "SemVer.Git.Fody.dll",
                                                                     "SemVer.Svn.Fody.dll"
                                                                   };

                                                   foreach (var fileName in fileNames)
                                                   {
                                                     try
                                                     {
                                                       var assembly = Assembly.LoadFrom(fileName);
                                                       if (assembly == null)
                                                       {
                                                         continue;
                                                       }

                                                       return assembly;
                                                     }
                                                     catch
                                                     {
                                                     }
                                                   }

                                                   return null;
                                                 };

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
      var xelement = xdocument.Element("SemVer.Git") ?? xdocument.Element("SemVer.Svn");
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

      var moduleWeaver = new ModuleWeaver();
      var version = moduleWeaver.GetVersion(repositoryPath,
                                            configuration.BaseVersion,
                                            configuration.BaseRevision,
                                            configuration.PatchFormat,
                                            configuration.FeatureFormat,
                                            configuration.BreakingChangeFormat);

      System.Console.WriteLine(version);
    }
  }
}
