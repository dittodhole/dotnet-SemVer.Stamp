using System;
using JetBrains.Annotations;

namespace SemVer.MSBuild
{
  public interface IVersionPatcher
  {
    /// <exception cref="ArgumentNullException"><paramref name="version" /> is <see langword="null" />.</exception>
    /// <exception cref="Exception" />
    [NotNull]
    Version PatchBaseVersionWithVersion([CanBeNull] string baseVersion,
                                        [NotNull] Version version);
  }

  public class VersionPatcher : IVersionPatcher
  {
    /// <exception cref="ArgumentNullException"><paramref name="version" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException" />
    /// <exception cref="ArgumentOutOfRangeException" />
    /// <exception cref="FormatException" />
    /// <exception cref="OverflowException" />
    public virtual Version PatchBaseVersionWithVersion(string baseVersion,
                                                       Version version)
    {
      if (version == null)
      {
        throw new ArgumentException(nameof(version));
      }

      if (baseVersion == null)
      {
        return version;
      }

      int major;
      int minor;
      int build;

      {
        var baseline = Version.Parse(baseVersion);
        major = baseline.Major;
        minor = baseline.Minor;
        build = baseline.Build;
      }

      if (version.Build > 0)
      {
        build += version.Build;
      }
      if (version.Minor > 0)
      {
        minor += version.Minor;
        build = Math.Max(version.Build,
                         0);
      }
      if (version.Major > 0)
      {
        major += version.Major;
        minor = Math.Max(version.Minor,
                         0);
        build = Math.Max(version.Build,
                         0);
      }

      var patchedVersion = new Version(major,
                                       minor,
                                       build);

      return patchedVersion;
    }
  }
}
