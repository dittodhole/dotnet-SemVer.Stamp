![](assets/noun_60203_cc.png)

# dotnet-SemVer.Stamp
> Version your assemblies according to SemVer based on your VCS commit messages.

## Build status

[![](https://img.shields.io/appveyor/ci/dittodhole/dotnet-semver-stamp.svg)](https://ci.appveyor.com/project/dittodhole/dotnet-semver-stamp)

## Installing

### myget.org

[![](https://img.shields.io/myget/dittodhole/vpre/SemVer.Git.MSBuild.svg)](https://www.myget.org/feed/dittodhole/package/nuget/SemVer.Git.MSBuild)

```powershell
PM> Install-Package -Id SemVer.Git.MSBuild -pre --source https://www.myget.org/F/dittodhole/api/v2
```

[![](https://img.shields.io/myget/dittodhole/vpre/SemVer.Svn.MSBuild.svg)](https://www.myget.org/feed/dittodhole/package/nuget/SemVer.Svn.MSBuild)

```powershell
PM> Install-Package -Id SemVer.Svn.MSBuild -pre --source https://www.myget.org/F/dittodhole/api/v2
```

### nuget.org

[![](https://img.shields.io/nuget/v/SemVer.Git.MSBuild.svg)](https://www.nuget.org/packages/SemVer.Git.MSBuild)

```powershell
PM> Install-Package -Id SemVer.Git.MSBuild
```

[![](https://img.shields.io/nuget/v/SemVer.Svn.MSBuild.svg)](https://www.nuget.org/packages/SemVer.Svn.MSBuild)

```powershell
PM> Install-Package -Id SemVer.Svn.MSBuild
```

## Configuration

*SemVer.Stamp.props*-file can be found in your project's root directory:

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
    <SemVerStampActive>False</SemVerStampActive>
    <SemVerStampActive Condition="'$(Configuration)' == 'Release'">True</SemVerStampActive>
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

## Developing & Building

```cmd
> git clone https://github.com/dittodhole/dotnet-SemVer.Stamp.git
> cd dotnet-SemVer.Stamp
dotnet-SemVer.Stamp> cd build
dotnet-SemVer.Stamp/build> build.bat
```

This will create the following artifacts:

- `artifacts/SemVer.Git.MSBuild.{version}.nupkg`
- `artifacts/SemVer.Git.MSBuild.{version}.symbols.nupkg`
- `artifacts/SemVer.Svn.MSBuild.{version}.nupkg`
- `artifacts/SemVer.Svn.MSBuild.{version}.symbols.nupkg`

## License

dotnet-SemVer.Stamp is published under [WTFNMFPLv3](https://github.com/dittodhole/WTFNMFPLv3).

## Icon

[Cyclops](https://thenounproject.com/term/cyclops/60203/) by [Mike Hince](https://thenounproject.com/zer0mike) from the Noun Project.
