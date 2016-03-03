using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Mono.Cecil;

// ReSharper disable CheckNamespace
// ReSharper disable NonLocalizedString
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ExceptionNotDocumented
// ReSharper disable UnusedMember.Global
// ReSharper disable ExceptionNotDocumentedOptional
// ReSharper disable EventExceptionNotDocumented
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SemVer.Fody
{
  public sealed partial class ModuleWeaver
  {
    public ModuleWeaver()
    {
      this.LogInfo = s =>
                     {
                     };
      this.LogWarning = s =>
                        {
                        };
      this.LogError = s =>
                      {
                      };
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
    public string SolutionDirectoryPath { get; set; }

    private Assembly HandleAssemblyResolveFailed(object sender,
                                                 ResolveEventArgs args)
    {
      this.LogWarning($"could not resolve assembly for {args.Name}");
      var assemblyName = args.Name.Split(',')
                             .First();
      var assemblyFileName = string.Concat(assemblyName,
                                           ".dll");
      var assemblyFullFileName = Path.Combine(this.AddinDirectoryPath,
                                              assemblyFileName);

      this.LogInfo($"loading assembly {args.Name} from {assemblyFullFileName}");

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

      this.LogInfo($"Starting search for repository in {repositoryLocationLevel}: {repositoryPath}");

      var version = this.GetVersion(repositoryPath,
                                    configuration.BaseVersion,
                                    configuration.BaseRevision,
                                    configuration.PatchFormat,
                                    configuration.FeatureFormat,
                                    configuration.BreakingChangeFormat);
      if (version == null)
      {
        this.LogWarning($"Could not get version for repository in {repositoryLocationLevel} - skipping version patching");
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

      this.LogInfo($"Patching version using: {verpatchPathPath} {arguments}");

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

    /// <exception cref="ArgumentNullException"><paramref name="commitMessages" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    public Version GetVersionAccordingToSemVer(IEnumerable<string> commitMessages,
                                               Version baseVersion,
                                               string patchFormat,
                                               string featureFormat,
                                               string breakingChangeFormat)
    {
      if (commitMessages == null)
      {
        throw new ArgumentNullException(nameof(commitMessages));
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

      var patch = baseVersion?.Build ?? 0;
      var feature = baseVersion?.Minor ?? 0;
      var breakingChange = baseVersion?.Major ?? 0;
      var revision = Math.Max(0,
                              baseVersion?.Revision ?? 0);

      this.LogInfo($"baseVersion: {breakingChange}.{feature}.{patch}.{revision}");

      foreach (var commitMessage in commitMessages)
      {
        if (string.IsNullOrEmpty(commitMessage))
        {
          continue;
        }

        var patchMatches = Regex.Matches(commitMessage,
                                         patchFormat,
                                         RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled)
                                .OfType<Match>()
                                .ToArray();
        if (patchMatches.Any())
        {
          patch += patchMatches.Length;
        }

        var featureMatches = Regex.Matches(commitMessage,
                                           featureFormat,
                                           RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled)
                                  .OfType<Match>()
                                  .ToArray();
        if (featureMatches.Any())
        {
          patch = 0;
          feature += featureMatches.Length;
        }

        var breakingChangeMatches = Regex.Matches(commitMessage,
                                                  breakingChangeFormat,
                                                  RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled)
                                         .OfType<Match>()
                                         .ToArray();
        if (breakingChangeMatches.Any())
        {
          patch = 0;
          feature = 0;
          breakingChange += breakingChangeMatches.Length;
        }
      }

      var version = new Version(breakingChange,
                                feature,
                                patch,
                                revision);

      return version;
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
