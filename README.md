# Mackiloha
A suite of modding software for hacking milo engine based games. This is still a very much a work-in-progress project. So don't expect perfection. Feel free to submit [Issues](https://github.com/PikminGuts92/Mackiloha/issues) for any bugs found.

# Overview
## SuperFreq (name pending)
CLI tool for unpacking/packing rnd archives for milo games. There are many known extensions used such as .gh., .kr, .milo, and .rnd.

- Publish: `dotnet publish -c=Release --self-contained=false -r=win-x64`
- Usage Examples:
  - Extract rnd archive: `superfreq milo2dir test.milo_ps2 ext_test --convertTextures --preset=gh2`
  - Create rnd archive: `superfreq dir2milo ext_test test.milo_ps2 --preset=gh2`
    - Note: Any `.png` files in the "Tex" directory will automatically be converted and serialized to milo encoded textures