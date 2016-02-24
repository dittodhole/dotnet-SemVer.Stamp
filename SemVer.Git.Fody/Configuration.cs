using System;
using System.Xml.Linq;

// ReSharper disable CatchAllClause

namespace SemVer.Fody
{
  public sealed class Configuration
  {
    /// <exception cref="ArgumentNullException"><paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="WeavingException">If 'UseProject' could not be parsed as <see cref="bool" />.</exception>
    /// <exception cref="WeavingException">If 'BaseVersion' could not be parsed as <see cref="Version" />.</exception>
    public Configuration(XElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException(nameof(element));
      }

      {
        var attribute = element.Attribute("UseProject"); // Not L10N
        if (attribute != null)
        {
          try
          {
            this.UseProject = bool.Parse(attribute.Value);
          }
          catch (Exception exception)
          {
            throw new WeavingException($"Unable to parse {attribute.Value} as {typeof (bool).FullName} from configuartion",
                                       exception);
          }
        }
        else
        {
          this.UseProject = true;
        }
      }

      {
        var attribute = element.Attribute("PatchFormat"); // Not L10N
        if (attribute != null)
        {
          this.PatchFormat = attribute.Value;
        }
        if (string.IsNullOrEmpty(this.PatchFormat))
        {
          this.PatchFormat = @"^fix(\(.*\))*: ";
        }
      }

      {
        var attribute = element.Attribute("FeatureFormat"); // Not L10N
        if (attribute != null)
        {
          this.FeatureFormat = attribute.Value;
        }
        if (string.IsNullOrEmpty(this.FeatureFormat))
        {
          this.FeatureFormat = @"^feat(\(.*\))*: ";
        }
      }

      {
        var attribute = element.Attribute("BreakingChangeFormat"); // Not L10N
        if (attribute != null)
        {
          this.BreakingChangeFormat = attribute.Value;
        }
        if (string.IsNullOrEmpty(this.BreakingChangeFormat))
        {
          this.BreakingChangeFormat = @"^perf(\(.*\))*: ";
        }
      }

      {
        var attribute = element.Attribute("BaseVersion"); // Not L10N
        if (attribute != null)
        {
          try
          {
            this.BaseVersion = Version.Parse(attribute.Value);
          }
          catch (Exception exception)
          {
            throw new WeavingException($"Unable to parse {attribute.Value} as {typeof (Version).FullName} from configuartion",
                                       exception);
          }
        }
      }

      {
        var attribute = element.Attribute("BaseRevision");
        if (attribute != null)
        {
          this.BaseRevision = attribute.Value;
        }
      }
    }

    public Version BaseVersion { get; }
    public string BaseRevision { get; }
    public string BreakingChangeFormat { get; }
    public string FeatureFormat { get; }
    public string PatchFormat { get; }
    public bool UseProject { get; }
  }
}
