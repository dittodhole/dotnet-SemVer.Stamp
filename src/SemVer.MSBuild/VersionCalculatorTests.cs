using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SemVer.MSBuild
{
  [TestClass]
  public class VersionCalculatorTests
  {
    [TestMethod]
    public void Version_Should_Be_Calculated_For_One_Fix()
    {
      this.Get_Version_According_To_SemVer_Base(new[]
                                                {
                                                  "fix: foobar"
                                                },
                                                "0.0.1");
    }

    [TestMethod]
    public void Version_Should_Be_Calculated_For_Two_Fixes()
    {
      this.Get_Version_According_To_SemVer_Base(new[]
                                                {
                                                  "fix: foobar",
                                                  "fix: foobar2"
                                                },
                                                "0.0.2");
    }

    [TestMethod]
    public void Version_Should_Be_Calculated_For_One_Feature()
    {
      this.Get_Version_According_To_SemVer_Base(new[]
                                                {
                                                  "feat: foobar"
                                                },
                                                "0.1.0");
    }

    [TestMethod]
    public void Version_Should_Be_Calculated_For_Two_Features()
    {
      this.Get_Version_According_To_SemVer_Base(new[]
                                                {
                                                  "feat: foobar",
                                                  "feat: foobar2"
                                                },
                                                "0.2.0");
    }

    [TestMethod]
    public void Version_Should_Be_Calculated_For_One_Breaking_Change()
    {
      this.Get_Version_According_To_SemVer_Base(new[]
                                                {
                                                  "perf: foobar"
                                                },
                                                "1.0.0");
    }

    [TestMethod]
    public void Version_Should_Be_Calculated_For_Two_Breaking_Changes()
    {
      this.Get_Version_According_To_SemVer_Base(new[]
                                                {
                                                  "perf: foobar",
                                                  "perf: foobar2"
                                                },
                                                "2.0.0");
    }

    [TestMethod]
    public void Version_Should_Be_Calculated_For_Complex_Commits()
    {
      this.Get_Version_According_To_SemVer_Base(new[]
                                                {
                                                  "fix: foobar",
                                                  "feat: foobar"
                                                },
                                                "0.1.0");
      this.Get_Version_According_To_SemVer_Base(new[]
                                                {
                                                  "feat: foobar",
                                                  "fix: foobar"
                                                },
                                                "0.1.1");
      this.Get_Version_According_To_SemVer_Base(new[]
                                                {
                                                  "fix: foobar",
                                                  "feat: foobar",
                                                  "perf: foobar"
                                                },
                                                "1.0.0");
      this.Get_Version_According_To_SemVer_Base(new[]
                                                {
                                                  "perf: foobar",
                                                  "fix: foobar",
                                                  "feat: foobar"
                                                },
                                                "1.1.0");
      this.Get_Version_According_To_SemVer_Base(new[]
                                                {
                                                  "perf: foobar",
                                                  "feat: foobar",
                                                  "fix: foobar"
                                                },
                                                "1.1.1");
    }

    private void Get_Version_According_To_SemVer_Base(string[] commitMessages,
                                                      string expectedVersion)
    {
      var versionCalculator = new VersionCalculator();

      var version = versionCalculator.Process(commitMessages);

      Assert.AreEqual(expectedVersion,
                      version.ToString(3));
    }
  }
}
