# csharp-Fody.SemVer

## This is an add-in for [Fody](https://github.com/Fody/Fody/) 

![Icon](https://raw.githubusercontent.com/dittodhole/csharp-Fody.SemVer/master/Icons/package_icon.png)

Versions your assemblies according to SemVer based on your CVS commit messages.

Supported CVS:
- Git [![NuGet Status](http://img.shields.io/nuget/v/Fody.SemVer.Git.svg?style=flat)](https://www.nuget.org/packages/Fody.SemVer.Git/)
    PM > Install-Package Fody.SemVer.Git
- Svn [![NuGet Status](http://img.shields.io/nuget/v/Fody.SemVer.Svn.svg?style=flat)](https://www.nuget.org/packages/Fody.SemVer.Svn/)
    PM > Install-Package Fody.SemVer.Svn

## Commit message formats

The default commit message formats are:

- PatchFormat `^fix(\(.*\))*: `
- FeatureFormat `^feat(\(.*\))*: `
- BreakingChangeFormat `^perf(\(.*\))*: `

The can be changed in the *FodyWeavers.xml* as following:

    <Fody.SemVer.Git PatchFormat=""
                     FeatureFormat=""
                     BreakingChangeFormat="" />

By default, the project directorie's history is taken into account when versioning. This behaviour can be changed in the *FodyWeavers.xml* as well:

    <Fody.SemVer.Git UseProject="False" />

## Icon

[Cyclops](https://thenounproject.com/term/cyclops/60203/) by [Mike Hince](https://thenounproject.com/zer0mike) from the Noun Project.

## License

csharp-Fody.SemVer is published under [WTFNMFPLv3](http://andreas.niedermair.name/introducing-wtfnmfplv3).

## Spotify Playlist

Good in the mood, [listen to the playlist](https://open.spotify.com/user/dittodhole/playlist/0KF2OFBoetcBt59qdHlbx7).