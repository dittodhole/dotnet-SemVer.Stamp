using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

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
    }

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
    protected void Execute(XElement config,
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

      string repositoryLocationLevel;
      if (configuration.UseProject)
      {
        repositoryLocationLevel = "ProjectDir"; // Not L10N
      }
      else
      {
        repositoryLocationLevel = "SolutionDir"; // Not L10N
      }

      this.LogInfo($"Starting search for repository in {repositoryLocationLevel}");

      var version = this.GetVersion(solutionPath,projectPath);
      if (version == null)
      {
        this.LogWarning($"Could not get version for repository in {repositoryLocationLevel} - skipping version patching");
        return;
      }

      this.PatchVersionOfAssembly(assemblyFullFileName,
                                  version,
                                  addinDirectoryPath);
    }

    /// <exception cref="ArgumentNullException"><paramref name="solutionPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="projectPath" /> is <see langword="null" />.</exception>
    protected abstract Version GetVersion(string solutionPath,
                                          string projectPath);

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
  }
}
