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

You can override following properties with `Directory.Build.props`:

- `SemVerStamp_BreakingChangeFormat` (default: *^perf(\(.*\))*: *)
- `SemVerStamp_FeatureFormat` (default: *^feat(\(.*\))*: *)
- `SemVerStamp_PatchFormat` (default: *^fix(\(.*\))*: *)
- `SemVerStamp_BaseRevision`
- `SemVerStamp_BaseVersion` (default: *0.0.0*)
- `SemVerStamp_SourcePath` (default: *$(MSBuildProjectDirectory)*)
- `SemVerStamp_Active` (default: *true* on release builds, otherwise *false*)

## Developing & Building

```cmd
> git clone https://github.com/dittodhole/dotnet-SemVer.Stamp.git
> cd dotnet-SemVer.Stamp
dotnet-SemVer.Stamp> cd build
dotnet-SemVer.Stamp/build> build.bat
```

This will create the following artifacts:

- `artifacts/SemVer.Git.MSBuild.{version}.nupkg`
- `artifacts/SemVer.Svn.MSBuild.{version}.nupkg`

## License

dotnet-SemVer.Stamp is published under [WTFNMFPLv3](https://github.com/dittodhole/WTFNMFPLv3).

## Icon

[Cyclops](https://thenounproject.com/term/cyclops/60203/) by [Mike Hince](https://thenounproject.com/zer0mike) from the Noun Project.
