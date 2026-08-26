using System;
using System.Collections.Generic;
using ChromaCube.Cameras;
using ChromaCube.Core;
using ChromaCube.Data;
using ChromaCube.Movement;
using ChromaCube.Rendering;
using UnityEngine;

namespace ChromaCube.Level
{
    public class LevelManager : MonoBehaviour
    {
        public event Action<LevelData, int, int> OnLevelLoaded;
        public event Action<int, int> OnCaptureChanged;
        public event Action<TileData> OnTileCaptured;
        public event Action<LevelData, int, int> OnLevelCompleted;
        public event Action<int> OnMoveCountChanged;

        /// <summary>
        /// Distance from tile surface to cube center.
        /// Cube half-height (0.46) + tile half-thickness (0.06) = 0.52.
        /// The old value 0.62 left a visible 0.10-unit gap where the cube floated above tiles.
        /// </summary>
        private const float CubeSurfaceOffset = 0.52f;

        [SerializeField] private BoardRenderer boardRenderer;
        [SerializeField] private CubeRenderer cubeRenderer;
        [SerializeField] private ClassicMovementController movementController;
        [SerializeField] private CameraRigController cameraRig;
        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        private readonly CaptureSystem captureSystem = new CaptureSystem();
        private readonly WinConditionSystem winConditionSystem = new WinConditionSystem();
        private readonly System.Collections.Generic.List<System.Action> undoStack = new System.Collections.Generic.List<System.Action>();
        private LevelData currentLevel;
        private CubeOrientation orientation;
        private Vector2Int cubeGridPosition;
        private WorldCubeFace cubeWorldFace;
        private bool completed;
        private int moveCount;

        public bool AcceptsInput => currentLevel != null && !completed && movementController != null && !movementController.IsMoving;
        public IReadOnlyList<LevelData> Levels => levels;
        public int CurrentLevelIndex { get; private set; } = -1;
        public int MoveCount => moveCount;

        public void Initialize(BoardRenderer board, CubeRenderer cube, ClassicMovementController movement, CameraRigController rig, List<LevelData> availableLevels)
        {
            boardRenderer = board;
            cubeRenderer = cube;
            movementController = movement;
            cameraRig = rig;
            levels = availableLevels;
        }

        public void LoadLevel(int listIndex)
        {
            if (listIndex < 0 || listIndex >= levels.Count)
            {
                return;
            }

            CurrentLevelIndex = listIndex;
            currentLevel = Instantiate(levels[listIndex]);
            currentLevel.tiles = CloneTiles(levels[listIndex].tiles);
            if (currentLevel.mechanicsMode == MechanicsMode.WorldCube)
            {
                PopulateWorldCubeTiles(currentLevel);
            }

            orientation = CubeOrientation.Identity();
            cubeGridPosition = currentLevel.start;
            cubeWorldFace = WorldCubeFace.Top;
            completed = false;
            undoStack.Clear();
            moveCount = 0;
            OnMoveCountChanged?.Invoke(moveCount);

            boardRenderer.Render(currentLevel);
            cubeRenderer.Build(currentLevel);
            if (currentLevel.mechanicsMode == MechanicsMode.WorldCube)
            {
                var frame = BoardRenderer.GetWorldCubeFrame(cubeWorldFace);
                cubeRenderer.transform.position = boardRenderer.WorldCubeTileCenter(cubeWorldFace, cubeGridPosition, currentLevel) + frame.normal * CubeSurfaceOffset;
                cubeRenderer.transform.rotation = boardRenderer.WorldCubeSurfaceRotation(cubeWorldFace);
                movementController.Configure(CanMove, GetTargetPosition, GetRotationAxis, CommitMove);
            }
            else
            {
                cubeRenderer.transform.position = boardRenderer.GridToWorld(cubeGridPosition, currentLevel) + Vector3.up * CubeSurfaceOffset;
                cubeRenderer.transform.rotation = Quaternion.identity;
                movementController.Configure(CanMove, GetTargetPosition, CommitMove);
            }

            cameraRig.Configure(cubeRenderer.transform, currentLevel);

            ResolveCapture();
            UpdateBottomFacePreview();
            OnLevelLoaded?.Invoke(currentLevel, captureSystem.CountCapturedRequired(currentLevel.tiles), CountRequired());
        }

        public void TryMove(Direction direction)
        {
            if (!AcceptsInput)
            {
                return;
            }

            movementController.TryMove(direction);
        }

        public void TryMoveFromScreenIntent(Vector2 screenIntent, UnityEngine.Camera viewCamera)
        {
            if (!AcceptsInput)
            {
                return;
            }

            if (viewCamera == null)
            {
                TryMove(ScreenIntentFallback(screenIntent));
                return;
            }

            var cubeWorldPosition = currentLevel.mechanicsMode == MechanicsMode.WorldCube
                ? GetCurrentWorldCubePosition()
                : boardRenderer.GridToWorld(cubeGridPosition, currentLevel);
            var cubeScreenPosition = viewCamera.WorldToScreenPoint(cubeWorldPosition);
            var bestDirection = Direction.North;
            var bestDot = float.NegativeInfinity;

            foreach (var direction in new[] { Direction.North, Direction.South, Direction.East, Direction.West })
            {
                var targetWorldPosition = currentLevel.mechanicsMode == MechanicsMode.WorldCube
                    ? GetWorldCubeTargetCenter(direction)
                    : boardRenderer.GridToWorld(cubeGridPosition + DirectionToGridOffset(direction), currentLevel);
                var targetScreenPosition = viewCamera.WorldToScreenPoint(targetWorldPosition);
                var screenDelta = new Vector2(targetScreenPosition.x - cubeScreenPosition.x, targetScreenPosition.y - cubeScreenPosition.y);
                if (screenDelta.sqrMagnitude < 0.001f)
                {
                    continue;
                }

                var dot = Vector2.Dot(screenDelta.normalized, screenIntent.normalized);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestDirection = direction;
                }
            }

            TryMove(bestDirection);
        }

        public void RestartCurrentLevel()
        {
            if (CurrentLevelIndex >= 0)
            {
                LoadLevel(CurrentLevelIndex);
            }
        }

        public void UndoLastMove()
        {
            if (completed)
            {
                return;
            }

            if (undoStack.Count == 0)
            {
                return;
            }

            var snapshot = undoStack[^1];
            undoStack.RemoveAt(undoStack.Count - 1);
            RestoreSnapshot(snapshot);
        }

        public void LoadNextLevel()
        {
            var next = CurrentLevelIndex + 1;
            if (next >= levels.Count)
            {
                next = 0;
            }

            LoadLevel(next);
        }

        private bool CanMove(Direction direction)
        {
            if (currentLevel.mechanicsMode == MechanicsMode.WorldCube)
            {
                var worldTarget = GetWorldCubeTarget(direction);
                return GetTileAt(worldTarget.face, worldTarget.gridPosition) != null;
            }

            var target = cubeGridPosition + DirectionToGridOffset(direction);
            return GetTileAt(target) != null;
        }

        private Vector3 GetTargetPosition(Direction direction)
        {
            if (currentLevel.mechanicsMode == MechanicsMode.WorldCube)
            {
                var worldTarget = GetWorldCubeTarget(direction);
                return boardRenderer.WorldCubeTileCenter(worldTarget.face, worldTarget.gridPosition, currentLevel) + worldTarget.frame.normal * CubeSurfaceOffset;
            }

            var target = cubeGridPosition + DirectionToGridOffset(direction);
            return boardRenderer.GridToWorld(target, currentLevel) + Vector3.up * CubeSurfaceOffset;
        }

        private Vector3 GetRotationAxis(Direction direction)
        {
            var frame = BoardRenderer.GetWorldCubeFrame(cubeWorldFace);
            switch (direction)
            {
                case Direction.North:
                    return frame.right;
                case Direction.South:
                    return -frame.right;
                case Direction.East:
                    return -frame.forward;
                case Direction.West:
                    return frame.forward;
                default:
                    return frame.right;
            }
        }

        private void CommitMove(Direction direction)
        {
            undoStack.Add(CreateSnapshot());

            if (currentLevel.mechanicsMode == MechanicsMode.WorldCube)
            {
                var target = GetWorldCubeTarget(direction);
                cubeWorldFace = target.face;
                cubeGridPosition = target.gridPosition;
            }
            else
            {
                cubeGridPosition += DirectionToGridOffset(direction);
            }

            moveCount++;
            OnMoveCountChanged?.Invoke(moveCount);

            orientation.Roll(direction);
            UpdateBottomFacePreview();
            ResolveCapture();
        }

        private void ResolveCapture()
        {
            var tile = GetTileAt(cubeGridPosition);
            var captured = captureSystem.TryCapture(currentLevel, orientation, tile, ResolveBottomFace(), cubeGridPosition, cubeWorldFace);
            if (captured)
            {
                boardRenderer.RefreshCapturedState(tile);
                OnTileCaptured?.Invoke(tile);
            }

            OnCaptureChanged?.Invoke(captureSystem.CountCapturedRequired(currentLevel.tiles), CountRequired());

            if (winConditionSystem.IsComplete(currentLevel.tiles))
            {
                completed = true;

                var stars = SaveSystem.EvaluateStars(
                    moveCount,
                    currentLevel.parMoveCount,
                    currentLevel.star1Threshold,
                    currentLevel.star2Threshold,
                    currentLevel.star3Threshold);

                SaveSystem.SetBestMoves(currentLevel.levelId, moveCount);
                SaveSystem.SetBestStars(currentLevel.levelId, stars);

                OnLevelCompleted?.Invoke(currentLevel, stars, moveCount);
            }
        }

        private void RestoreSnapshot(LevelSnapshot snapshot)
        {
            cubeGridPosition = snapshot.position;
            cubeWorldFace = snapshot.face;
            orientation = new CubeOrientation(snapshot.orientationFaceStates);
            moveCount = snapshot.moveCount;
            OnMoveCountChanged?.Invoke(moveCount);

            foreach (var capturedId in snapshot.capturedTileIds)
            {
                foreach (var tile in currentLevel.tiles)
                {
                    if (tile.id == capturedId)
                    {
                        tile.captured = false;
                        break;
                    }
                }
            }

            boardRenderer.RefreshCapturedState();
            UpdateCubeTransform();
            UpdateBottomFacePreview();
        }

        private void UpdateCubeTransform()
        {
            if (currentLevel.mechanicsMode == MechanicsMode.WorldCube)
            {
                var frame = BoardRenderer.GetWorldCubeFrame(cubeWorldFace);
                cubeRenderer.transform.position = boardRenderer.WorldCubeTileCenter(cubeWorldFace, cubeGridPosition, currentLevel) + frame.normal * CubeSurfaceOffset;
                cubeRenderer.transform.rotation = boardRenderer.WorldCubeSurfaceRotation(cubeWorldFace);
            }
            else
            {
                cubeRenderer.transform.position = boardRenderer.GridToWorld(cubeGridPosition, currentLevel) + Vector3.up * CubeSurfaceOffset;
                cubeRenderer.transform.rotation = Quaternion.identity;
            }
        }

        private readonly struct LevelSnapshot
        {
            public readonly Vector2Int position;
            public readonly WorldCubeFace face;
            public readonly string[] orientationFaceStates;
            public readonly int moveCount;
            public readonly string[] capturedTileIds;

            public LevelSnapshot(Vector2Int position, WorldCubeFace face, string[] orientationFaceStates, int moveCount, string[] capturedTileIds)
            {
                this.position = position;
                this.face = face;
                this.orientationFaceStates = orientationFaceStates;
                this.moveCount = moveCount;
                this.capturedTileIds = capturedTileIds;
            }
        }

        private LevelSnapshot CreateSnapshot()
        {
            var capturedIds = new System.Collections.Generic.List<string>();
            foreach (var tile in currentLevel.tiles)
            {
                if (tile.active && tile.captured)
                {
                    capturedIds.Add(tile.id);
                }
            }

            return new LevelSnapshot(
                cubeGridPosition,
                cubeWorldFace,
                orientation.SerializeFaceStates(),
                moveCount,
                capturedIds.ToArray());
        }

        /// <summary>
        /// Determine the bottom face using the cube's actual physical world rotation.
        /// This is the ground truth — far more reliable than the logical CubeOrientation
        /// which desyncs during WorldCube edge transitions.
        /// </summary>
        private FaceKey ResolveBottomFace()
        {
            if (cubeRenderer != null)
            {
                return CubeOrientation.GetBottomFaceFromRotation(cubeRenderer.transform.rotation);
            }

            // Fallback to logical orientation if renderer unavailable
            return orientation.GetBottomFace();
        }

        private void UpdateBottomFacePreview()
        {
            if (boardRenderer == null || currentLevel == null)
            {
                return;
            }

            var bottomFace = ResolveBottomFace();
            var bottomColor = currentLevel.GetColorForFace(bottomFace);
            if (currentLevel.mechanicsMode == MechanicsMode.WorldCube)
            {
                var frame = BoardRenderer.GetWorldCubeFrame(cubeWorldFace);
                var pos = boardRenderer.WorldCubeTileCenter(cubeWorldFace, cubeGridPosition, currentLevel);
                boardRenderer.ShowBottomFacePreview(pos, Quaternion.LookRotation(frame.forward, frame.normal), bottomColor);
            }
            else
            {
                var pos = boardRenderer.GridToWorld(cubeGridPosition, currentLevel);
                boardRenderer.ShowBottomFacePreview(pos, Quaternion.identity, bottomColor);
            }
        }

        private TileData GetTileAt(Vector2Int gridPosition)
        {
            if (currentLevel.mechanicsMode == MechanicsMode.WorldCube)
            {
                return GetTileAt(cubeWorldFace, gridPosition);
            }

            foreach (var tile in currentLevel.tiles)
            {
                if (tile.active && tile.gridPos == gridPosition)
                {
                    return tile;
                }
            }

            return null;
        }

        private TileData GetTileAt(WorldCubeFace face, Vector2Int gridPosition)
        {
            foreach (var tile in currentLevel.tiles)
            {
                if (tile.active && tile.worldFace == face && tile.gridPos == gridPosition)
                {
                    return tile;
                }
            }

            return null;
        }

        private Vector3 GetCurrentWorldCubePosition()
        {
            var frame = BoardRenderer.GetWorldCubeFrame(cubeWorldFace);
            return boardRenderer.WorldCubeTileCenter(cubeWorldFace, cubeGridPosition, currentLevel) + frame.normal * CubeSurfaceOffset;
        }

        private Vector3 GetWorldCubeTargetCenter(Direction direction)
        {
            var target = GetWorldCubeTarget(direction);
            return boardRenderer.WorldCubeTileCenter(target.face, target.gridPosition, currentLevel) + target.frame.normal * CubeSurfaceOffset;
        }

        private WorldCubeTarget GetWorldCubeTarget(Direction direction)
        {
            var gridPosition = cubeGridPosition + DirectionToGridOffset(direction);
            var frame = BoardRenderer.GetWorldCubeFrame(cubeWorldFace);
            var size = Mathf.Max(currentLevel.width, currentLevel.height);

            if (gridPosition.x >= 0 && gridPosition.x < size && gridPosition.y >= 0 && gridPosition.y < size)
            {
                return new WorldCubeTarget(cubeWorldFace, gridPosition, frame);
            }

            var nextFrame = frame;
            if (gridPosition.y < 0)
            {
                nextFrame = RotateWorldCubeFrame(frame, frame.right, 90f);
                gridPosition.y = size - 1;
            }
            else if (gridPosition.y >= size)
            {
                nextFrame = RotateWorldCubeFrame(frame, frame.right, -90f);
                gridPosition.y = 0;
            }
            else if (gridPosition.x >= size)
            {
                nextFrame = RotateWorldCubeFrame(frame, frame.forward, -90f);
                gridPosition.x = 0;
            }
            else if (gridPosition.x < 0)
            {
                nextFrame = RotateWorldCubeFrame(frame, frame.forward, 90f);
                gridPosition.x = size - 1;
            }

            var face = BoardRenderer.FaceFromNormal(nextFrame.normal);
            return new WorldCubeTarget(face, gridPosition, nextFrame);
        }

        private static WorldCubeFrame RotateWorldCubeFrame(WorldCubeFrame frame, Vector3 axis, float angle)
        {
            var rotation = Quaternion.AngleAxis(angle, axis);
            return new WorldCubeFrame(rotation * frame.normal, rotation * frame.right, rotation * frame.forward);
        }

        private int CountRequired()
        {
            var count = 0;
            foreach (var tile in currentLevel.tiles)
            {
                if (tile.active && tile.required)
                {
                    count++;
                }
            }

            return count;
        }

        private static Vector2Int DirectionToGridOffset(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return new Vector2Int(0, -1);
                case Direction.South:
                    return new Vector2Int(0, 1);
                case Direction.East:
                    return new Vector2Int(1, 0);
                case Direction.West:
                    return new Vector2Int(-1, 0);
                default:
                    return Vector2Int.zero;
            }
        }

        private static Direction ScreenIntentFallback(Vector2 screenIntent)
        {
            if (Mathf.Abs(screenIntent.x) > Mathf.Abs(screenIntent.y))
            {
                return screenIntent.x >= 0f ? Direction.East : Direction.West;
            }

            return screenIntent.y >= 0f ? Direction.North : Direction.South;
        }

        private static List<TileData> CloneTiles(List<TileData> source)
        {
            var clone = new List<TileData>();
            foreach (var tile in source)
            {
                clone.Add(tile.CloneRuntime());
            }

            return clone;
        }

        private static void PopulateWorldCubeTiles(LevelData level)
        {
            var size = Mathf.Max(4, Mathf.Max(level.width, level.height));
            level.width = size;
            level.height = size;
            level.tiles = new List<TileData>();

            foreach (WorldCubeFace face in Enum.GetValues(typeof(WorldCubeFace)))
            {
                for (var row = 0; row < size; row++)
                {
                    for (var col = 0; col < size; col++)
                    {
                        level.tiles.Add(new TileData
                        {
                            id = $"world-{face}-{col}-{row}",
                            worldFace = face,
                            gridPos = new Vector2Int(col, row),
                            colorId = "stone",
                            required = false,
                            captured = false,
                            active = true
                        });
                    }
                }
            }

            SetWorldGoal(level.tiles, WorldCubeFace.Top, 2, 1, "lavender");
            SetWorldGoal(level.tiles, WorldCubeFace.North, 1, 2, "amber");
            SetWorldGoal(level.tiles, WorldCubeFace.East, 2, 1, "mint");
            SetWorldGoal(level.tiles, WorldCubeFace.South, 1, 1, "ocean");
            SetWorldGoal(level.tiles, WorldCubeFace.West, 2, 2, "coral");
            SetWorldGoal(level.tiles, WorldCubeFace.Bottom, 1, 2, "slate");
        }

        private static void SetWorldGoal(List<TileData> tiles, WorldCubeFace face, int col, int row, string colorId)
        {
            foreach (var tile in tiles)
            {
                if (tile.worldFace == face && tile.gridPos == new Vector2Int(col, row))
                {
                    tile.colorId = colorId;
                    tile.required = true;
                    return;
                }
            }
        }

        private readonly struct WorldCubeTarget
        {
            public readonly WorldCubeFace face;
            public readonly Vector2Int gridPosition;
            public readonly WorldCubeFrame frame;

            public WorldCubeTarget(WorldCubeFace face, Vector2Int gridPosition, WorldCubeFrame frame)
            {
                this.face = face;
                this.gridPosition = gridPosition;
                this.frame = frame;
            }
        }
    }
}
