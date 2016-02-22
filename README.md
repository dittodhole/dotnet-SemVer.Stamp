# ![Icon](https://raw.githubusercontent.com/dittodhole/csharp-Fody.SemVer/master/Icons/package_icon.png) csharp-SemVer.Fody

**This is an add-in for [Fody](https://github.com/Fody/Fody/)**

Version your assemblies according to [SemVer](http://semver.org/) based on your CVS commit messages.

Supported CVS:

### Git [![NuGet Status](http://img.shields.io/nuget/v/SemVer.Git.Fody.svg?style=flat)](https://www.nuget.org/packages/SemVer.Git.Fody/)

    PM > Install-Package SemVer.Git.Fody

### Svn [![NuGet Status](http://img.shields.io/nuget/v/SemVer.Svn.Fody.svg?style=flat)](https://www.nuget.org/packages/SemVer.Svn.Fody/)
    PM > Install-Package SemVer.Svn.Fody

## Commit message formats

The default formats - for parsing the level of a commit - are:

- PatchFormat `^fix(\(.*\))*: `
- FeatureFormat `^feat(\(.*\))*: `
- BreakingChangeFormat `^perf(\(.*\))*: `

These formats can be changed in the *FodyWeavers.xml* file as following:

    <SemVer.??? PatchFormat=""
                FeatureFormat=""
                BreakingChangeFormat="" />

By default, the project directory's history is taken into account when calculating the version. This behaviour can be changed in the *FodyWeavers.xml* file as well:

    <SemVer.??? UseProject="False" />

If you decide to introduce SemVer later on in a project, you can provide a base version:

    <SemVer.??? BaseVersion="17.1.3.5" />

## Icon

[Cyclops](https://thenounproject.com/term/cyclops/60203/) by [Mike Hince](https://thenounproject.com/zer0mike) from the Noun Project.

## License

csharp-Fody.SemVer is published under [WTFNMFPLv3](http://andreas.niedermair.name/introducing-wtfnmfplv3).

## Spotify Playlist

Good in the mood, [listen to the playlist](https://open.spotify.com/user/dittodhole/playlist/0KF2OFBoetcBt59qdHlbx7).