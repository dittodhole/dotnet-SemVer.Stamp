using System;
using JetBrains.Annotations;

namespace SemVer.MSBuild
{
  public interface ICommitMessageProvider
  {
    /// <exception cref="Exception" />
    [NotNull]
    [ItemNotNull]
    string[] GetCommitMessages();
  }
}
