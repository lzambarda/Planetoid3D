# Planetoid 3D

This is the original code from Planetoid 3D minus some authentication code I had to remove to make this work without the authentication server.

## Development

The game uses [MonoGame](https://github.com/mono/MonoGame) as a replacement for XNA.

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