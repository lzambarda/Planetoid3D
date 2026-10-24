#!/usr/bin/env bash
# Builds the content (via MGCB) and the game, then runs it.
#
# Usage: scripts/run.sh [--release] [-- <game args>]

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$REPO_ROOT/Planetoid3D/Planetoid3D.csproj"

CONFIGURATION="Debug"
GAME_ARGS=()
while [[ $# -gt 0 ]]; do
    case "$1" in
        --release) CONFIGURATION="Release"; shift ;;
        --) shift; GAME_ARGS=("$@"); break ;;
        -h|--help) sed -n '2,4p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
        *) echo "Unknown argument: $1" >&2; exit 1 ;;
    esac
done

if ! command -v dotnet > /dev/null 2>&1; then
    echo "dotnet not found on PATH" >&2
    exit 1
fi

# Shader compilation on macOS/Linux goes through Wine.
if [[ "$(uname -s)" != MINGW* && "$(uname -s)" != MSYS* ]]; then
    export MGFXC_WINE_PATH="${MGFXC_WINE_PATH:-$HOME/.winemonogame}"
    if ! command -v wine > /dev/null 2>&1 && ! command -v wine64 > /dev/null 2>&1; then
        echo "Wine not found on PATH: shaders cannot be compiled (see README.md)" >&2
        exit 1
    fi
    if [[ ! -f "$MGFXC_WINE_PATH/drive_c/fxccs.dll" ]]; then
        echo "Wine prefix $MGFXC_WINE_PATH is not set up for MonoGame (see README.md)" >&2
        exit 1
    fi
fi

dotnet build "$PROJECT" -c "$CONFIGURATION"

# The game loads some files (e.g. Content/Pages/pages.xml) relative to the
# working directory, so run it from the build output directory.
TARGET_PATH="$(dotnet msbuild "$PROJECT" -p:Configuration="$CONFIGURATION" -getProperty:TargetPath)"
cd "$(dirname "$TARGET_PATH")"
exec dotnet "$TARGET_PATH" ${GAME_ARGS[@]+"${GAME_ARGS[@]}"}
