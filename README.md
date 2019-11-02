# Mackiloha [![Build status](https://ci.appveyor.com/api/projects/status/toda9bnsi5ur1k4b/branch/master?svg=true)](https://ci.appveyor.com/project/PikminGuts92/mackiloha/branch/master)
A suite of modding software for hacking milo engine based games. This is still very much a work-in-progress project. So don't expect perfection. Although feel free to submit [issues](https://github.com/PikminGuts92/Mackiloha/issues) for any bugs found.

The latest build can be found on [AppVeyor](https://ci.appveyor.com/project/PikminGuts92/mackiloha/branch/master/artifacts)

# System Requirements
You will need at least [.NET Core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0) runtime installed and be using an x64 operating system (come on, it's 2019).

# Overview
## SuperFreq (name not final)
CLI tool for unpacking/packing rnd archives from milo games. These files usually use the extensions: .gh, .kr, .milo, .rnd

- Publish: `dotnet publish -c=Release --self-contained=false -r=win-x64`
- Usage Examples:
  - Extract rnd archive: `superfreq milo2dir test.milo_ps2 ext_test --convertTextures --preset=gh2`
  - Create rnd archive: `superfreq dir2milo ext_test test.milo_ps2 --preset=gh2`
    - Note: Any `.png` files in the "Tex" directory will automatically be converted and serialized to milo encoded textures
