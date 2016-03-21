# ![Icon](Icons/package_icon.png) csharp-SemVer.Stamp

This is a solution for various plugins to achieve SemVer'sioning your assemblies.

## Commit message formats

The default formats - for parsing the level of a commit - are:

- PatchFormat `^fix(\(.*\))*: `
- FeatureFormat `^feat(\(.*\))*: `
- BreakingChangeFormat `^perf(\(.*\))*: `

## Source of commits

Depending on the chosen injection technology, you can set different properties to switch the source of commits from the **repository**'s path (default) to the **solution**'s path.

## Base version

Hey, awesome ... You have introduced SemVer'sioning. Somewhere after several releases. That's no problem, just set a `BaseVersion` (which may be combined with a `BaseRevision` to ignore any commits before that very revision for parsing) which is then used as the baseline for SemVer.

## ![](https://raw.github.com/DanielTheCoder/MSBuild.MSBNuget/master/media/MSBuild.ico) SemVer.MSBuild

After installing one of the following nugets, you can adapt any of the mentioned options in the *SemVer.MSBuild.props*-file in your project's root directory:

    <?xml version="1.0" encoding="utf-8"?>
    <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
      <PropertyGroup>
        <BaseRevision></BaseRevision>
        <BaseVersion>0.0.0</BaseVersion>
        <BreakingChangeFormat>^perf(\(.*\))*: </BreakingChangeFormat>
        <FeatureFormat>^feat(\(.*\))*: </FeatureFormat>
        <PatchFormat>^fix(\(.*\))*: </PatchFormat>
        <RepositoryPath>$(ProjectDir)</RepositoryPath>
      </PropertyGroup>
    </Project>

### SemVer.Git.MSBuild [![NuGet Status](https://img.shields.io/nuget/v/SemVer.Git.MSBuild.svg?style=flat)](https://www.nuget.org/packages/SemVer.Git.MSBuild/)

    PM > Install-Package SemVer.Git.MSBuild

###  SemVer.Svn.MSBuild [![NuGet Status](https://img.shields.io/nuget/v/SemVer.Svn.MSBuild.svg?style=flat)](https://www.nuget.org/packages/SemVer.Svn.MSBuild/)
    PM > Install-Package SemVer.Svn.MSBuild


## [![Fody](https://camo.githubusercontent.com/5765643b25e9e30770ce1b9a7719e36f82739c9f/68747470733a2f2f7261772e6769746875622e636f6d2f466f64792f466f64792f6d61737465722f49636f6e732f7061636b6167655f69636f6e2e706e67)](https://github.com/Fody/Fody/) SemVer.Fody

After installing one of the following nugets, you can adapt any of the mentioned options in the *FodyWeavers.xml*-file in your project's root directory:

    <?xml version="1.0" encoding="utf-8"?>
    <Weavers>
      <SemVer.??? UseProjectDir="True"
                  PatchFormat="^fix(\(.*\))*: "
                  FeatureFormat="^feat(\(.*\))*: "
                  BreakingChangeFormat="^perf(\(.*\))*: "
                  BaseVersion="17.1.3"
                  BaseRevision="BaseVersion got introduced in" />
    </Weavers>

> SemVer.Svn.Fody and SemVer.Git.Fody leverage their respective SemVer.MSBuild project since version 1.3.0 to calculate and inject a SemVersion in your assembly. Using the MSBuild task is more robust - Fody integration is just to keep the dev in their used and known environment.
> In short: SemVer.Fody converts the `SemVer.???`-element in *FodyWeavers.xml* to a *SemVer.MSBuild.props*-file upon every build.

### SemVer.Git.Fody [![NuGet Status](https://img.shields.io/nuget/v/SemVer.Git.Fody.svg?style=flat)](https://www.nuget.org/packages/SemVer.Git.Fody/)

    PM > Install-Package SemVer.Git.Fody

###  SemVer.Svn.Fody [![NuGet Status](https://img.shields.io/nuget/v/SemVer.Svn.Fody.svg?style=flat)](https://www.nuget.org/packages/SemVer.Svn.Fody/)
    PM > Install-Package SemVer.Svn.Fody

## Icon

[Cyclops](https://thenounproject.com/term/cyclops/60203/) by [Mike Hince](https://thenounproject.com/zer0mike) from the Noun Project.

## License

csharp-SemVer.Stamp is published under [WTFNMFPLv3](https://andreas.niedermair.name/introducing-wtfnmfplv3).

## Spotify Playlist

Good in the mood, [listen to the playlist](https://open.spotify.com/user/dittodhole/playlist/0KF2OFBoetcBt59qdHlbx7).
