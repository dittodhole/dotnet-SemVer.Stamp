using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Mono.Cecil;

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

    public string AddinDirectoryPath { get; set; }
    public string AssemblyFilePath { get; set; }
    public XElement Config { get; set; }
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public string ProjectDirectoryPath { get; set; }
    public string SolutionDirectoryPath { get; set; }

    public abstract void Execute();

    public void AfterWeaving()
    {
    }

    /// <exception cref="ArgumentNullException"><paramref name="assemblyVersion" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="assemblyVersionInfo" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">
    ///   If the assembly defined in <see cref="AssemblyFilePath" /> cannot be
    ///   SemVer'ed.
    /// </exception>
    private void PatchVersionOfAssembly(string assemblyVersion,
                                        string assemblyVersionInfo)
    {
      if (assemblyVersion == null)
      {
        throw new ArgumentNullException(nameof(assemblyVersion));
      }
      if (assemblyVersionInfo == null)
      {
        throw new ArgumentNullException(nameof(assemblyVersionInfo));
      }

      var verpatchPathPath = Path.Combine(this.AddinDirectoryPath,
                                          "verpatch.exe"); // Not L10N
      var arguments = $@"""{this.AssemblyFilePath}"" /pv ""{assemblyVersionInfo}"" /high {assemblyVersion}";

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
          throw new InvalidOperationException($"{nameof(typeof (Process).FullName)} could not be created");
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

        throw new InvalidOperationException(message);
      }
    }
  }
}
