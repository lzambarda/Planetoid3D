# Planetoid 3D

This is the original code from Planetoid 3D minus some authentication code I had to remove to make this work without the authentication server.

## Development

The game uses [MonoGame](https://github.com/mono/MonoGame) as a replacement for XNA.
Content is built with MGCB (MonoGame Content Builder) as part of `dotnet build`.

### Prerequisites

- .NET SDK 8 or newer, with a working `dotnet` on `PATH` (a broken `dotnet` symlink earlier on `PATH` makes shader compilation fail).
- [git-lfs](https://git-lfs.com/): the `.wav` files are stored in LFS.
- On macOS/Linux, Wine is required to compile the `.fx` shaders
  (see [MonoGame's macOS setup guide](https://docs.monogame.net/articles/getting_started/1_setting_up_your_os_for_development_macos.html)),
  with `MGFXC_WINE_PATH` pointing at the Wine prefix (e.g. `~/.winemonogame`).

### Building

```sh
cd Planetoid3D
dotnet build   # restores the mgcb tool, builds Content/Content.mgcb, then the game
dotnet run
```

The MonoGame package versions in `Planetoid3D.csproj` must match the tool versions in `.config/dotnet-tools.json`.

## Notable issues

### Missing Particles, Atmospheres and some other effects

At the time of writing the mgcb pipeline doesn't correcty compile some `.fx` files.
It seems to be caused by `wine` throwing a page fault exception.

### Occasional start effect artifacts

Sometimes the stars are showing artifacts as the API for drawing primitives changed.

### Audio crackles

MonoGame is yet to release a functioning processor for original XNA soundbanks.
This is still using the old compilation I used for the official release.

### Code is ugly

Indeed :-P