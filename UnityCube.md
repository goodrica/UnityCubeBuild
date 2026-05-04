# UnityCube: Unity Rebuild Starter (Chroma Cube)

## Goal
Rebuild this puzzle game in **Unity** from scratch with the same core identity:
- Player controls a **1x1x1 cube** with unique colors on each face.
- Player rolls across tile boards and captures required tiles when the **bottom face color** matches tile color.
- Many classic levels (flat board) plus one special **World Cube** level where gravity is local to cube faces.

Use this file as your single source of truth when bootstrapping the new Unity project.

---

## High-Level Design

### Core Loop
1. Load level data.
2. Accept player move input.
3. Validate move.
4. Animate cube roll (90-degree pivot).
5. Update cube orientation state.
6. Resolve tile capture.
7. Check win condition (all required tiles captured).
8. Advance to next level.

### Two Mechanics Modes
- `classic`: flat board (2D grid on XZ plane).
- `world-cube`: 4x4x4 cube shell traversal with dynamic down vector toward world cube center.

Only one level should use `world-cube` at first.

---

## Unity Project Setup

## Engine / Packages
- Unity 2022 LTS or Unity 6.
- URP recommended.
- Input System package (optional but preferred).
- TextMeshPro.

## Folder Layout
Use this structure:

```text
Assets/
  _Game/
    Art/
    Audio/
    Materials/
    Prefabs/
      Cube/
      Tiles/
      UI/
    Scenes/
      Bootstrap.unity
      Game.unity
    Scripts/
      Core/
      Data/
      Level/
      Movement/
      Rendering/
      UI/
      Audio/
    ScriptableObjects/
      Levels/
```

---

## Data Model (Unity)

Create these core data structures:

- `FaceKey` enum: Top, Bottom, North, South, East, West
- `Direction` enum: North, South, East, West
- `ControlMode` enum: CameraRelative, BoardRelative
- `MechanicsMode` enum: Classic, WorldCube

- `CubeOrientation`:
  - `FaceKey top, bottom, north, south, east, west`
  - `Roll(Direction dir)` updates orientation.

- `TileData`:
  - `string id`
  - `Vector2Int gridPos`
  - `string colorId`
  - `bool required`
  - `bool captured`
  - `bool active`

- `LevelData` (ScriptableObject):
  - `string levelId`
  - `int index`
  - `string title`
  - `string subtitle`
  - `string hint`
  - `int width, height`
  - `Vector2Int start`
  - `MechanicsMode mechanicsMode`
  - `List<TileData> tiles`
  - `FaceColorMap` (face->color id)
  - optional music/background refs

---

## Scene Architecture

## Game Scene Root
- `GameRoot` (empty GO)
  - `GameManager`
  - `LevelManager`
  - `InputRouter`
  - `CameraRig`
  - `BoardRoot`
  - `PlayerCube`
  - `UIRoot`

## Player Cube Setup
- Parent GO: `PlayerCubeRoot`
- Child GO: `VisualCube` with 6-material cube mesh
- Optional edge outline child

---

## Movement Rules

## Classic Mode
- Movement changes grid pos by 1 tile.
- If target tile is inactive/outside bounds: block move.
- Animate roll with pivot around edge.
- Update `CubeOrientation`.

## WorldCube Mode (4x4x4 shell)
- Treat world as six connected 4x4 faces (cube shell traversal).
- Keep local frame:
  - `faceNormal`
  - `faceRight`
  - `faceForward`
  - `tileU`, `tileV`
- Dynamic down vector every frame:
  - `down = (worldCubeCenter - playerPosition).normalized`
- Edge transition:
  - Detect `tileU/tileV` leaving `[0..3]`
  - Rotate frame 90 degrees to adjacent face
  - Wrap to next face tile coordinates
  - Snap to tile center on new face

---

## Capture Logic

On move complete:
1. Read bottom face key from current orientation.
2. Map face key to color id.
3. If landed tile color == bottom color and tile is required/active, set captured true.
4. Win when all required active tiles are captured.

---

## Camera Behavior

## Classic
- Slight angled overview camera.
- Optional drag yaw around board.

## WorldCube
- Orbit around player cube.
- Drag input controls yaw/pitch orbit.
- Camera always looks at player cube center.

---

## Input Mapping

- WASD / Arrow keys:
  - Up/W => forward intent
  - Down/S => backward intent
  - Left/A => left intent
  - Right/D => right intent
- Convert intent using control mode:
  - Camera-relative maps by camera forward/right projected to movement plane.
  - Board-relative uses canonical directions.

---

## Prompt Pack (Copy/Paste)

Use these prompts with your coding assistant in Unity.

## Prompt 1: Project Bootstrap
```text
Create Unity C# scripts for a puzzle game called Chroma Cube.
Requirements:
- 1x1x1 player cube with 6 face colors.
- Grid tile board levels loaded from ScriptableObject LevelData.
- Roll movement with 90-degree pivot and orientation tracking.
- Tile capture when bottom face color matches tile color.
- Win condition: all required tiles captured.
- Code architecture: GameManager, LevelManager, MovementController, BoardRenderer, CubeRenderer, UIController.
- Include clean, production-style C# with comments and events.
```

## Prompt 2: Cube Orientation System
```text
Implement a pure C# CubeOrientation model with FaceKey enum.
Add Roll(Direction) that updates top/bottom/north/south/east/west correctly for 90-degree rolls.
Also add methods:
- GetBottomFace()
- Clone()
- ToString()
Return unit-test-like verification examples for roll sequences.
```

## Prompt 3: Classic Movement Controller
```text
Implement ClassicMovementController for Unity:
- Input: Direction
- Validates target tile
- Executes animated 90-degree roll around cube edge pivot
- Updates cube grid position and CubeOrientation
- Emits OnMoveStarted/OnMoveCompleted events
- Supports move blocking and queue lock while animation is active
```

## Prompt 4: World Cube Controller (4x4x4 shell)
```text
Implement WorldCubeMovementController for Unity with these rules:
- Player remains attached to surface of a 4x4x4 world cube shell.
- Dynamic down vector is (worldCenter - playerPosition).normalized.
- Track faceNormal/faceRight/faceForward + local tileU/tileV.
- Edge transitions: when leaving face bounds, rotate to adjacent face by 90 degrees and remap tile coordinates.
- Always snap to exact tile center.
- Provide data needed for animation (pivot axis, pivot point, 90-degree angle).
- Expose conversion from local face coords to world position/rotation.
```

## Prompt 5: Level 5 Special Mode Only
```text
Integrate mechanics modes:
- All levels default to Classic.
- Exactly one level (Level 5) uses WorldCube mode.
- Keep shared capture logic and win checks identical between modes.
- Add factory logic in LevelManager to pick movement controller by level.mechanicsMode.
```

## Prompt 6: Camera Rig
```text
Create a Unity camera rig:
- Classic mode: stable angled framing with optional drag yaw.
- WorldCube mode: orbit camera around player cube with mouse drag yaw/pitch and smooth damping.
- Ensure controls feel consistent regardless of current cube face.
```

---

## Recommended Script List

- `GameManager.cs`
- `LevelManager.cs`
- `InputRouter.cs`
- `CubeOrientation.cs`
- `ClassicMovementController.cs`
- `WorldCubeMovementController.cs`
- `BoardRenderer.cs`
- `CubeRenderer.cs`
- `CaptureSystem.cs`
- `WinConditionSystem.cs`
- `CameraRigController.cs`
- `UIController.cs`

---

## Level 5 Initial Spec (Unity)

For the new Unity version, Level 5 should be:
- `mechanicsMode = WorldCube`
- world cube dimension: `4x4x4` shell
- player starts on one face center-ish tile
- include several required color tiles spread across multiple faces
- include passive white tiles for non-goal traversal

---

## Done Criteria

You are done when:
1. Levels 1-4 run in classic mode.
2. New Level 5 runs in world-cube mode only.
3. Cube rolls correctly over edges with 90-degree transitions.
4. Dynamic down vector updates correctly toward cube center.
5. Tile capture and win logic works in both modes.
6. Camera remains controllable and readable in both modes.

