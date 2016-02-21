using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

// ReSharper disable EventExceptionNotDocumented
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Fody.SemVer
{
  public abstract class ModuleWeaverBase
  {
    protected ModuleWeaverBase()
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
    }

    public Action<string> LogError { get; set; }

    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }

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
    protected void PatchVersionOfAssemblyTheSemVerWay(XElement config,
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
        repositoryLocationLevel = "ProjectDir"; // Not L10N
        repositoryPath = projectPath;
      }
      else
      {
        repositoryLocationLevel = "SolutionDir"; // Not L10N
        repositoryPath = solutionPath;
      }

      this.LogInfo($"Starting search for repository in {repositoryLocationLevel}");

      var version = this.GetVersion(repositoryPath,
                                    configuration.PatchFormat,
                                    configuration.FeatureFormat,
                                    configuration.BreakingChangeFormat);
      if (version == null)
      {
        this.LogWarning($"Could not get version for repository in {repositoryLocationLevel} - skipping version patching");
        return;
      }

      this.PatchVersionOfAssembly(assemblyFullFileName,
                                  version,
                                  addinDirectoryPath);
    }

    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    protected abstract Version GetVersion(string repositoryPath,
                                          string patchFormat,
                                          string featureFormat,
                                          string breakingChangeFormat);

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

      var verpatchPathPath = Path.Combine(addinDirectoryPath,
                                          "verpatch.exe"); // Not L10N
      var arguments = $@"""{assemblyFullFileName}"" /high {assemblyVersion}";

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

        if (process.ExitCode == 0)
        {
          return;
        }

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        var message = string.Join(Environment.NewLine,
                                  "Failed to SemVer Win32 resources.", // Not L10N
                                  $"Output: {output}",
                                  $"Error: {error}");

        throw new WeavingException(message);
      }
    }

    /// <exception cref="ArgumentNullException"><paramref name="commitMessages" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    protected Version GetVersionAccordingToSemVer(IEnumerable<string> commitMessages,
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

      var patch = 0;
      var feature = 0;
      var breakingChange = 0;

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
          feature = featureMatches.Length;
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
          breakingChange = breakingChangeMatches.Length;
        }
      }

      var version = new Version(breakingChange,
                                feature,
                                patch,
                                0);

      return version;
    }
  }
}
