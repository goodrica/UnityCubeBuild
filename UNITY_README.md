# Chroma Cube Unity Starter

## Unity Version

Use **Unity 2022.3 LTS**, preferably **2022.3.62f1** or newer in the 2022 LTS line.

Edition: **Unity Personal** is enough. Unity Pro is not required.

The starter uses built-in Unity primitives, the built-in `uGUI` package, and the legacy `Input` manager for WASD/arrow keys. No audio, paid assets, Blender files, or external packages are required for this first mechanics pass.

## How To Open

1. Open **Unity Hub**.
2. Choose **Add project from disk**.
3. Select this folder:
   `C:\Users\ag\Downloads\Codex Projects\CubeGame`
4. Open it with Unity 2022.3 LTS.
5. After Unity finishes compiling, run:
   `Tools > Chroma Cube > Build Starter Project`
6. Open:
   `Assets/_Game/Scenes/Game.unity`
7. Press **Play**.

## What Is Included

- Title screen
- Start button
- Level select for levels 1-4
- Four classic flat-board levels ported from the existing game data
- Runtime-created tile board
- Runtime-created six-face colored cube
- WASD / arrow-key rolling controls
- Cube orientation tracking
- Tile capture when the bottom face color matches a required tile
- Win condition when all required tiles are captured
- Restart and level-select HUD buttons

## Current Scope

This build intentionally covers only the first four classic levels. The Level 5 world-cube mode and audio are left out for the next pass.
