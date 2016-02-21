using System;
using System.Xml.Linq;

// ReSharper disable CatchAllClause

namespace Fody.SemVer
{
  public sealed class Configuration
  {
    /// <exception cref="ArgumentNullException"><paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="WeavingException">
    ///   If 'UseProject' could not be read from <see cref="XElement.Attributes" /> of
    ///   <see cref="element" />.
    /// </exception>
    /// <exception cref="WeavingException">If 'UseProject' could not be parsed as <see cref="bool" />.</exception>
    public Configuration(XElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException(nameof(element));
      }

      var attribute = element.Attribute("UseProject"); // Not L10N
      if (attribute == null)
      {
        throw new WeavingException($"Unable to read UseProject from configuration");
      }

      try
      {
        this.UseProject = bool.Parse(attribute.Value);
      }
      catch (Exception exception)
      {
        throw new WeavingException($"Unable to parse {attribute.Value} as boolean from configuartion",
                                   exception);
      }
    }

    public bool UseProject { get; }
  }
}
