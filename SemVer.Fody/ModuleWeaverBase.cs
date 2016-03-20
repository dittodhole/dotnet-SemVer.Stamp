using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Mono.Cecil;
using SemVer.Stamp;

// ReSharper disable NonLocalizedString

namespace SemVer.Fody
{
  public abstract class ModuleWeaverBase
  {
    protected ModuleWeaverBase()
    {
      AppDomain.CurrentDomain.AssemblyResolve += this.HandleAssemblyResolveFailed;
    }

    public string AddinDirectoryPath { get; set; }
    public string AssemblyFilePath { get; set; }
    public XElement Config { get; set; }
    public Action<string> LogError { get; set; }
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public string ProjectDirectoryPath { get; set; }
    protected SemVersionGrabberBase SemVersionGrabber { get; set; }
    public string SolutionDirectoryPath { get; set; }

    private Assembly HandleAssemblyResolveFailed(object sender,
                                                 ResolveEventArgs args)
    {
      this.LogWarning?.Invoke($"could not resolve assembly for {args.Name}");
      var assemblyName = args.Name.Split(',')
                             .First();
      var assemblyFileName = string.Concat(assemblyName,
                                           ".dll");
      var assemblyFullFileName = Path.Combine(this.AddinDirectoryPath,
                                              assemblyFileName);

      this.LogInfo?.Invoke($"loading assembly {args.Name} from {assemblyFullFileName}");

      var assembly = Assembly.LoadFrom(assemblyFullFileName);

      return assembly;
    }

    public void Execute()
    {
    }

    public void AfterWeaving()
    {
      this.Prerequisites();

      var version = this.PatchVersionOfAssemblyTheSemVerWay(this.Config,
                                                            this.AssemblyFilePath,
                                                            this.AddinDirectoryPath,
                                                            this.SolutionDirectoryPath,
                                                            this.ProjectDirectoryPath);
      if (version != null)
      {
        this.PatchAssemblyAttribution(version);
      }
    }

    protected abstract void Prerequisites();

    /// <exception cref="ArgumentNullException"><paramref name="config" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="assemblyFullFileName" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="addinDirectoryPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="solutionPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="projectPath" /> is <see langword="null" />.</exception>
    /// <exception cref="WeavingException">
    ///   If 'UseProject' could not be read from <see cref="XElement.Attributes" /> of
    ///   <see cref="config" />.
    /// </exception>
    /// <exception cref="WeavingException">If 'UseProject' could not be parsed as <see cref="bool" />.</exception>
    /// <exception cref="WeavingException">
    ///   If the assembly defined in <see cref="AssemblyFilePath" /> cannot be
    ///   SemVer'ed.
    /// </exception>
    /// <exception cref="WeavingException">If a <see cref="Process" /> instance for verpatch.exe cannot be created.</exception>
    private Version PatchVersionOfAssemblyTheSemVerWay(XElement config,
                                                       string assemblyFullFileName,
                                                       string addinDirectoryPath,
                                                       string solutionPath,
                                                       string projectPath)
    {
      if (config == null)
      {
        throw new ArgumentNullException(nameof(config));
      }
      if (assemblyFullFileName == null)
      {
        throw new ArgumentNullException(nameof(assemblyFullFileName));
      }
      if (addinDirectoryPath == null)
      {
        throw new ArgumentNullException(nameof(addinDirectoryPath));
      }
      if (projectPath == null)
      {
        throw new ArgumentNullException(nameof(projectPath));
      }
      if (solutionPath == null)
      {
        throw new ArgumentNullException(nameof(solutionPath));
      }

      var configuration = new Configuration(config);

      string repositoryPath;
      string repositoryLocationLevel;
      if (configuration.UseProject)
      {
        repositoryLocationLevel = "ProjectDir";
        repositoryPath = projectPath;
      }
      else
      {
        repositoryLocationLevel = "SolutionDir";
        repositoryPath = solutionPath;
      }

      this.LogInfo?.Invoke($"Starting search for repository in {repositoryLocationLevel}: {repositoryPath}");

      var version = this.SemVersionGrabber.GetVersion(repositoryPath,
                                                      configuration.BaseVersion,
                                                      configuration.BaseRevision,
                                                      configuration.PatchFormat,
                                                      configuration.FeatureFormat,
                                                      configuration.BreakingChangeFormat);
      if (version == null)
      {
        this.LogWarning?.Invoke($"Could not get version for repository in {repositoryLocationLevel} - skipping version patching");
        return null;
      }

      this.PatchVersionOfAssembly(assemblyFullFileName,
                                  version,
                                  addinDirectoryPath);

      return version;
    }

    /// <exception cref="ArgumentNullException"><paramref name="assemblyFullFileName" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="assemblyVersion" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="addinDirectoryPath" /> is <see langword="null" />.</exception>
    /// <exception cref="WeavingException">
    ///   If the assembly defined in <paramref name="assemblyFullFileName" /> cannot be
    ///   SemVer'ed.
    /// </exception>
    /// <exception cref="WeavingException">If a <see cref="Process" /> instance for verpatch.exe cannot be created.</exception>
    private void PatchVersionOfAssembly(string assemblyFullFileName,
                                        Version assemblyVersion,
                                        string addinDirectoryPath)
    {
      if (assemblyFullFileName == null)
      {
        throw new ArgumentNullException(nameof(assemblyFullFileName));
      }
      if (assemblyVersion == null)
      {
        throw new ArgumentNullException(nameof(assemblyVersion));
      }
      if (addinDirectoryPath == null)
      {
        throw new ArgumentNullException(nameof(addinDirectoryPath));
      }

      string versionString;
      if (assemblyVersion.Revision == 0)
      {
        versionString = assemblyVersion.ToString(3);
      }
      else
      {
        versionString = assemblyVersion.ToString();
      }

      var verpatchPathPath = Path.Combine(addinDirectoryPath,
                                          "verpatch.exe");
      var arguments = $@"""{assemblyFullFileName}"" /pv {versionString} /high /va {versionString}";

      this.LogInfo?.Invoke($"Patching version using: {verpatchPathPath} {arguments}");

      var processStartInfo = new ProcessStartInfo
                             {
                               FileName = verpatchPathPath,
                               Arguments = arguments,
                               CreateNoWindow = true,
                               UseShellExecute = false,
                               RedirectStandardOutput = true,
                               RedirectStandardError = true,
                               WorkingDirectory = Path.GetTempPath()
                             };

      using (var process = Process.Start(processStartInfo))
      {
        if (process == null)
        {
          throw new WeavingException($"{typeof (Process).FullName} could not be created");
        }

        process.WaitForExit();

        if (process.ExitCode == 0)
        {
          return;
        }

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        var message = string.Join(Environment.NewLine,
                                  "Failed to SemVer Win32 resources.",
                                  $"Output: {output}",
                                  $"Error: {error}");

        throw new WeavingException(message);
      }
    }

    private void PatchAssemblyAttribution(Version version)
    {
      var versionString = version.ToString();
      var customAttributes = this.ModuleDefinition.CustomAttributes;
      var customAttribute = customAttributes.FirstOrDefault(arg => arg.AttributeType.Name == "AssemblyVersionAttribute");
      if (customAttribute == null)
      {
        var mscorlib = this.ModuleDefinition.AssemblyResolver.Resolve("mscorlib");
        var versionAttribute = mscorlib.MainModule.Types.FirstOrDefault(arg => arg.Name == "AssemblyVersionAttribute");
        if (versionAttribute == null)
        {
          var systemRuntime = this.ModuleDefinition.AssemblyResolver.Resolve("System.Runtime");
          versionAttribute = systemRuntime.MainModule.Types.First(arg => arg.Name == "AssemblyInformationalVersionAttribute");
        }

        var constructor = this.ModuleDefinition.ImportReference(versionAttribute.Methods.First(arg => arg.IsConstructor));
        customAttribute = new CustomAttribute(constructor);
        customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(this.ModuleDefinition.TypeSystem.String,
                                                                             versionString));
        customAttributes.Add(customAttribute);
      }
      else
      {
        customAttribute.ConstructorArguments[0] = new CustomAttributeArgument(this.ModuleDefinition.TypeSystem.String,
                                                                              versionString);
      }
    }
  }
}
