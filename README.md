# ![Icon](Icons/package_icon.png) csharp-SemVer.Stamp

This is a solution for various plugins to achieve SemVer'sioning your assemblies.

## Commit message formats

The default formats - for parsing the level of a commit - are:

- PatchFormat `^fix(\(.*\))*: `
- FeatureFormat `^feat(\(.*\))*: `
- BreakingChangeFormat `^perf(\(.*\))*: `

## Source of commits

tbd

## Base version

tbd

## SemVer.MSBuild
----------

tbd

## [![Fody](https://camo.githubusercontent.com/5765643b25e9e30770ce1b9a7719e36f82739c9f/68747470733a2f2f7261772e6769746875622e636f6d2f466f64792f466f64792f6d61737465722f49636f6e732f7061636b6167655f69636f6e2e706e67)](https://github.com/Fody/Fody/) SemVer.Fody
----------

### SemVer.Git.Fody [![NuGet Status](https://img.shields.io/nuget/v/SemVer.Git.Fody.svg?style=flat)](https://www.nuget.org/packages/SemVer.Git.Fody/)

    PM > Install-Package SemVer.Git.Fody

###  SemVer.Svn.Fody [![NuGet Status](https://img.shields.io/nuget/v/SemVer.Svn.Fody.svg?style=flat)](https://www.nuget.org/packages/SemVer.Svn.Fody/)
    PM > Install-Package SemVer.Svn.Fody

### Commit message formats

    <?xml version="1.0" encoding="utf-8"?>
    <Weavers>
      <SemVer.??? PatchFormat="^fix(\(.*\))*: "
                  FeatureFormat="^feat(\(.*\))*: "
                  BreakingChangeFormat="^perf(\(.*\))*: " />
    </Weavers>

### Source of commits

    <?xml version="1.0" encoding="utf-8"?>
    <Weavers>
      <SemVer.??? UseProject="False" />
    </Weavers>

### Base version

    <?xml version="1.0" encoding="utf-8"?>
    <Weavers>
      <SemVer.??? BaseVersion="17.1.3.5"
                  BaseRevision="BaseVersion got introduced in" />
    </Weavers>

## Icon

[Cyclops](https://thenounproject.com/term/cyclops/60203/) by [Mike Hince](https://thenounproject.com/zer0mike) from the Noun Project.

## License

csharp-SemVer.Stamp is published under [WTFNMFPLv3](https://andreas.niedermair.name/introducing-wtfnmfplv3).

## Spotify Playlist

Good in the mood, [listen to the playlist](https://open.spotify.com/user/dittodhole/playlist/0KF2OFBoetcBt59qdHlbx7).
