# Mackiloha [![Build status](https://ci.appveyor.com/api/projects/status/toda9bnsi5ur1k4b/branch/master?svg=true)](https://ci.appveyor.com/project/PikminGuts92/mackiloha/branch/master)
A suite of modding software for hacking milo engine based games. This is still very much a work-in-progress project. So don't expect perfection. Although feel free to submit [issues](https://github.com/PikminGuts92/Mackiloha/issues) for any bugs found.

The latest build can be found on [AppVeyor](https://ci.appveyor.com/project/PikminGuts92/mackiloha/branch/master/artifacts).

# System Requirements
You will need at least [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) runtime installed and be using an x64 operating system.

# Build Instructions
Run `./build.sh`

# Overview
## Ark Helper
CLI tool for unpacking/repacking .ark archives from milo. Should work with Amplitude (PS2) up to RB3. For dta/dtb serialization support, download [dtab](https://github.com/mtolly/dtab) and place in same directory as ark helper executable.

Usage:
- Extract ark archive:
  - Everything: `arkhelper ark2dir main.hdr ext_dir -a`
  - Everything + convert scripts: `arkhelper ark2dir main.hdr ext_dir -a -s`
- Repack ark archive:
  - Amp/KR/AntiGrav PS2: `arkhelper dir2ark ext_dir gen_dir -n "MAIN" -v 2`
  - GH1/GH2 PS2: `arkhelper dir2ark ext_dir gen_dir -n "MAIN"`
  - GH2 360: `arkhelper dir2ark ext_dir gen_dir -e`
  - RB2/TBRB/GDRB PS3: `arkhelper dir2ark ext_dir gen_dir -n "main_ps3" -e -v 5`
  - RB3 360: `arkhelper dir2ark ext_dir gen_dir -n "main_xbox" -e -v 6`

## P9 Song Tool
CLI tool to assist in venue authoring for TBRB.

Usage:
- Create new project: `p9songtool newproj -n temporarysec project_temporarysec`
- Create project from milo: `p9songtool.exe milo2proj -m temporarysec.mid temporarysec.milo_xbox project_temporarysec`
- Generate milo from project: `p9songtool.exe proj2milo project_temporarysec temporarysec.milo_xbox`


## SuperFreq (pronounced "Super Freak")
CLI tool for unpacking/packing rnd archives from milo games. These files usually use the extensions: .gh, .kr, .milo, .rnd

**Warning:** Game compatibility is *very* limited. Editing archives for games beyond GH2 will have mixed results.

Usage:
- Extract rnd archive: `superfreq milo2dir test.milo_ps2 ext_test --convertTextures --preset=gh2`
- Create rnd archive: `superfreq dir2milo ext_test test.milo_ps2 --preset=gh2`
  - Note: Any `.png` files in the "Tex" directory will automatically be converted and serialized to milo encoded textures