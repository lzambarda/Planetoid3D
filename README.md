# Planetoid 3D

This is the original code from Planetoid 3D minus some authentication code I had to remove to make this work without the authentication server.

For the older Planetoid games published on YoYo Games, see https://github.com/lzambarda/planetoid-games

## Development

The game uses [MonoGame](https://github.com/mono/MonoGame) as a replacement for XNA.
Content is built with MGCB (MonoGame Content Builder) as part of `dotnet build`.

### Prerequisites

- .NET SDK 8 or newer, with a working `dotnet` on `PATH` (a broken `dotnet` symlink earlier on `PATH` makes shader compilation fail).
- [git-lfs](https://git-lfs.com/): the `.wav` files are stored in LFS.
- On macOS/Linux, Wine is required to compile the `.fx` shaders
  (see [MonoGame's macOS setup guide](https://docs.monogame.net/articles/getting_started/1_setting_up_your_os_for_development_macos.html)),
  with `MGFXC_WINE_PATH` pointing at the Wine prefix (e.g. `~/.winemonogame`).
  - Homebrew's Wine casks are disabled; use the [Gcenx builds](https://github.com/Gcenx/macOS_Wine_builds) (needs Rosetta on Apple Silicon).
  - The prefix must be created with MonoGame's `net9_mgfxc_wine_setup.sh`: MonoGame 3.8.6 runs `c:\fxccs.dll` inside it.
    A prefix from an older MonoGame setup fails with `Internal CLR error` in `Console.set_OutputEncoding`.

### Building

```sh
scripts/run.sh             # builds content (MGCB) and the game, then runs it
scripts/run.sh --release   # same, in Release configuration
```

The game loads some data files relative to the working directory, so it must be run from its build output directory (which `scripts/run.sh` does).

The MonoGame package versions in `Planetoid3D.csproj` must match the tool versions in `.config/dotnet-tools.json`.

## Notable issues

### Occasional start effect artifacts

Sometimes the stars are showing artifacts as the API for drawing primitives changed.

### Audio crackles

MonoGame is yet to release a functioning processor for original XNA soundbanks.
This is still using the old compilation I used for the official release.

### Code is ugly

Indeed :-P
