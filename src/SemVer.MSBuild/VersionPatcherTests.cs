using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable ExceptionNotDocumented

namespace SemVer.MSBuild
{
  [TestClass]
  public class VersionPatcherTests
  {
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Should_Throw_ArgumentException_If_Invalid_BaseVersion_Provided()
    {
      var baseVersion = "asd";
      var version = new Version();

      var versionPatcher = new VersionPatcher();
      var patchedVersion = versionPatcher.PatchBaseVersionWithVersion(baseVersion,
                                                                      version);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void Should_Throw_FormatException_If_Invalid_BaseVersion_Provided()
    {
      var baseVersion = "1.a.b";
      var version = new Version();

      var versionPatcher = new VersionPatcher();
      var patchedVersion = versionPatcher.PatchBaseVersionWithVersion(baseVersion,
                                                                      version);
    }

    [TestMethod]
    [ExpectedException(typeof(OverflowException))]
    public void Should_Throw_OverflowException_If_Invalid_BaseVersion_Provided()
    {
      var baseVersion = "1.9999999999.99";
      var version = new Version();

      var versionPatcher = new VersionPatcher();
      var patchedVersion = versionPatcher.PatchBaseVersionWithVersion(baseVersion,
                                                                      version);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Should_Throw_ArgumentOutOfRangeException_If_Invalid_BaseVersion_Provided()
    {
      var baseVersion = "-1.2.3";
      var version = new Version();

      var versionPatcher = new VersionPatcher();
      var patchedVersion = versionPatcher.PatchBaseVersionWithVersion(baseVersion,
                                                                      version);
    }



    [TestMethod]
    public void Should_Return_Version_If_No_BaseVersion_Provided()
    {
      var baseVersion = default(string);
      var version = new Version(1,
                                0);

      var versionPatcher = new VersionPatcher();
      var patchedVersion = versionPatcher.PatchBaseVersionWithVersion(baseVersion,
                                                                      version);

      Assert.AreEqual(version,
                      patchedVersion);
    }

    [TestMethod]
    public void Should_Patch_Build()
    {
      var baseVersion = "0.0.1";
      var version = new Version(0,
                                0,
                                2);

      var versionPatcher = new VersionPatcher();
      var patchedVersion = versionPatcher.PatchBaseVersionWithVersion(baseVersion,
                                                                      version);

      var expectedPatchedVersion = new Version(0,
                                               0,
                                               3);

      Assert.AreEqual(expectedPatchedVersion,
                      patchedVersion);
    }

    [TestMethod]
    public void Should_Patch_Minor()
    {
      var baseVersion = "0.0.1";
      var version = new Version(0,
                                1,
                                0);

      var versionPatcher = new VersionPatcher();
      var patchedVersion = versionPatcher.PatchBaseVersionWithVersion(baseVersion,
                                                                      version);

      var expectedPatchedVersion = new Version(0,
                                               1,
                                               0);

      Assert.AreEqual(expectedPatchedVersion,
                      patchedVersion);
    }

    [TestMethod]
    public void Should_Patch_Major()
    {
      var baseVersion = "0.0.1";
      var version = new Version(1,
                                0,
                                0);

      var versionPatcher = new VersionPatcher();
      var patchedVersion = versionPatcher.PatchBaseVersionWithVersion(baseVersion,
                                                                      version);

      var expectedPatchedVersion = new Version(1,
                                               0,
                                               0);

      Assert.AreEqual(expectedPatchedVersion,
                      patchedVersion);
    }
  }
}
