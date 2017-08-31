using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SemVer.Git.MSBuild
{
  [TestClass]
  public class GitCommitMessageProviderTests
  {
    [NotNull]
    public const string BasePath = @"C:\work\dotnet-SemVer.Stamp";

    [TestMethod]
    public void Should_Provide_CommitMessages_For_BasePath()
    {
      var path = System.IO.Path.Combine(GitCommitMessageProviderTests.BasePath,
                                        string.Empty);
      var baseRevision = default(string);

      var gitCommitMessageProvider = new GitCommitMessageProvider(path,
                                                                  baseRevision);

      var commitMessages = gitCommitMessageProvider.GetCommitMessages();

      Assert.IsTrue(commitMessages.Count() > 0);
    }

    [TestMethod]
    public void Should_Provide_CommitMessages_For_BasePath_And_BaseRevision()
    {
      var path = System.IO.Path.Combine(GitCommitMessageProviderTests.BasePath,
                                        string.Empty);
      var baseRevision = "27eed99e0c8a58fb1d9a556e8b0c24534a777b2d";

      var gitCommitMessageProvider = new GitCommitMessageProvider(path,
                                                                  baseRevision);

      var commitMessages = gitCommitMessageProvider.GetCommitMessages();

      Assert.IsTrue(commitMessages.Count() > 0);
    }

    [TestMethod]
    public void Should_Provide_CommitMessages_For_SubPath()
    {
      var path = System.IO.Path.Combine(GitCommitMessageProviderTests.BasePath,
                                        "src");
      var baseRevision = default(string);

      var gitCommitMessageProvider = new GitCommitMessageProvider(path,
                                                                  baseRevision);

      var commitMessages = gitCommitMessageProvider.GetCommitMessages();

      Assert.IsTrue(commitMessages.Count() > 0);
    }

    [TestMethod]
    public void Should_Provide_CommitMessages_For_SubPath_And_BaseRevision()
    {
      var path = System.IO.Path.Combine(GitCommitMessageProviderTests.BasePath,
                                        "src");
      var baseRevision = "27eed99e0c8a58fb1d9a556e8b0c24534a777b2d";

      var gitCommitMessageProvider = new GitCommitMessageProvider(path,
                                                                  baseRevision);

      var commitMessages = gitCommitMessageProvider.GetCommitMessages();

      Assert.IsTrue(commitMessages.Count() > 0);
    }

  }
}
