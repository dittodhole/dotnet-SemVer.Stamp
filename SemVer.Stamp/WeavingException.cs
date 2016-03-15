using System;
using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace SemVer.Stamp
{
  [Serializable]
  public class WeavingException : Exception
  {
    public WeavingException()
    {
    }

    public WeavingException(string message)
      : base(message)
    {
    }

    public WeavingException(string message,
                            Exception inner)
      : base(message,
             inner)
    {
    }

    /// <exception cref="ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
    /// <exception cref="SerializationException">
    ///   The class name is null or <see cref="P:System.Exception.HResult" /> is zero
    ///   (0).
    /// </exception>
    protected WeavingException(SerializationInfo info,
                               StreamingContext context)
      : base(info,
             context)
    {
    }
  }
}
