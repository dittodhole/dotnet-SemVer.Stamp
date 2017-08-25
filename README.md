![Icon](assets/package_icon.png)

# dotnet-SemVer.Stamp

**master** branch status
[![Build status](//ci.appveyor.com/api/projects/status/pfhxvmnefxmga46k?svg=true)](//ci.appveyor.com/project/dittodhole/dotnet-semver-stamp)

**develop** branch status
[![Build status](//ci.appveyor.com/api/projects/status/pfhxvmnefxmga46k/branch/develop?svg=true)](//ci.appveyor.com/project/dittodhole/dotnet-semver-stamp/branch/develop)

## Installing

### Releases

[![NuGet Status](//img.shields.io/nuget/v/SemVer.Git.MSBuild.svg?style=flat)](//www.nuget.org/packages/SemVer.Git.MSBuild/)

    PM > Install-Package SemVer.Git.MSBuild

[![NuGet Status](//img.shields.io/nuget/v/SemVer.Svn.MSBuild.svg?style=flat)](//www.nuget.org/packages/SemVer.Svn.MSBuild/)

    PM > Install-Package SemVer.Svn.MSBuild

### Pre Releases

    PM> nuget sources Add "dittodhole" https://www.myget.org/F/dittodhole/api/v3/index.json

[![MyGet Pre Release](//img.shields.io/myget/dittodhole/vpre/SemVer.Git.MSBuild.svg?style=flat-square)](//www.myget.org/feed/dittodhole/package/nuget/SemVer.Git.MSBuild)
https://www.myget.org/feed/dittodhole/package/nuget/SemVer.Git.MSBuild

    PM> Install-Package SemVer.Git.MSBuild -pre

[![MyGet Pre Release](//img.shields.io/myget/dittodhole/vpre/SemVer.Svn.MSBuild.svg?style=flat-square)](//www.myget.org/feed/dittodhole/package/nuget/SemVer.Svn.MSBuild)
https://www.myget.org/feed/dittodhole/package/nuget/SemVer.Svn.MSBuild

    PM> Install-Package SemVer.Svn.MSBuild -pre

## Configuration

[SemVer.MSBuild.props](src/SemVer.MSBuild/content/SemVer.MSBuild.props)-file can be found in your project's root directory:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <BreakingChangeFormat>^perf(\(.*\))*: </BreakingChangeFormat>
    <FeatureFormat>^feat(\(.*\))*: </FeatureFormat>
    <PatchFormat>^fix(\(.*\))*: </PatchFormat>
    <BaseRevision></BaseRevision>
    <BaseVersion>0.0.0</BaseVersion>
    <RepositoryPath>$(ProjectDir)</RepositoryPath>
    <SemVerStampActive>True</SemVerStampActive>
  </PropertyGroup>
</Project>
```

### Commit message formats

The default formats - for parsing the level of a commit - are:

- PatchFormat `^fix(\(.*\))*: `
- FeatureFormat `^feat(\(.*\))*: `
- BreakingChangeFormat `^perf(\(.*\))*: `

### Baseline your version

Hey, awesome ... You have introduced SemVer'sioning. Somewhere after several releases. That's no problem, just set a `BaseVersion` (which may be combined with a `BaseRevision` to ignore any commits before that very revision for parsing) which is then used as the baseline for SemVer.

### Source of commits

Depending on the chosen injection technology, you can set different properties to switch the source of commits from the **repository**'s path (default) to the **solution**'s path:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <!-- other properties -->
    <RepositoryPath>$(SolutionDir)</RepositoryPath>
    <!-- other properties -->
  </PropertyGroup>
</Project>
```

### Active-Switch

To disable stamping in DEBUG builds, one can adapt the *.props*-file like:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <!-- other properties -->
    <SemVerStampActive>False</SemVerStampActive>
    <SemVerStampActive Condition="'$(Configuration)' == 'Release'">True</SemVerStampActive>
    <!-- other properties -->
  </PropertyGroup>
</Project>
```

## Icon

[Cyclops](//thenounproject.com/term/cyclops/60203/) by [Mike Hince](//thenounproject.com/zer0mike) from the Noun Project.

## License

dotnet-SemVer.Stamp is published under [WTFNMFPLv3](//github.com/dittodhole/WTFNMFPLv3).
