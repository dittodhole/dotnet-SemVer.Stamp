# ![Icon](Icons/package_icon.png) SemVer.Stamp

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

## Active-Switch

The switch `SemVerStampActive` controls the execution of stamping - this can be especially useful, if you do not want DEBUG builds to be stamped (as it requires a change-free working-copy).

To disable stamping in DEBUG builds, one can adapt the *.props*-file like:

    <?xml version="1.0" encoding="utf-8"?>
    <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
      <PropertyGroup>
        <SemVerStampActive>False</SemVerStampActive>
        <SemVerStampActive Condition="'$(Configuration)' == 'Release'">True</SemVerStampActive>
        <!-- other properties -->
      </PropertyGroup>
    </Project>


## ![](assets/MSBuild.ico) [MSBuild](https://github.com/Microsoft/msbuild) integration

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
        <SemVerStampActive>True</SemVerStampActive>
      </PropertyGroup>
    </Project>

### SemVer.Git.MSBuild [![NuGet Status](https://img.shields.io/nuget/v/SemVer.Git.MSBuild.svg?style=flat)](https://www.nuget.org/packages/SemVer.Git.MSBuild/)

    PM > Install-Package SemVer.Git.MSBuild

###  SemVer.Svn.MSBuild [![NuGet Status](https://img.shields.io/nuget/v/SemVer.Svn.MSBuild.svg?style=flat)](https://www.nuget.org/packages/SemVer.Svn.MSBuild/)
    PM > Install-Package SemVer.Svn.MSBuild

## Icon

[Cyclops](https://thenounproject.com/term/cyclops/60203/) by [Mike Hince](https://thenounproject.com/zer0mike) from the Noun Project.

## License

dotnet-SemVer.Stamp is published under [WTFNMFPLv3](//github.com/dittodhole/WTFNMFPLv3).
