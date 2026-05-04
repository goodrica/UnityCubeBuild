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
        public event Action<LevelData> OnLevelCompleted;

        [SerializeField] private BoardRenderer boardRenderer;
        [SerializeField] private CubeRenderer cubeRenderer;
        [SerializeField] private ClassicMovementController movementController;
        [SerializeField] private CameraRigController cameraRig;
        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        private readonly CaptureSystem captureSystem = new CaptureSystem();
        private readonly WinConditionSystem winConditionSystem = new WinConditionSystem();
        private LevelData currentLevel;
        private CubeOrientation orientation;
        private Vector2Int cubeGridPosition;
        private bool completed;

        public bool AcceptsInput => currentLevel != null && !completed && movementController != null && !movementController.IsMoving;
        public IReadOnlyList<LevelData> Levels => levels;
        public int CurrentLevelIndex { get; private set; } = -1;

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
            orientation = CubeOrientation.Identity();
            cubeGridPosition = currentLevel.start;
            completed = false;

            boardRenderer.Render(currentLevel);
            cubeRenderer.Build(currentLevel);
            cubeRenderer.transform.position = boardRenderer.GridToWorld(cubeGridPosition, currentLevel) + Vector3.up * 0.62f;
            cubeRenderer.transform.rotation = Quaternion.identity;
            movementController.Configure(CanMove, GetTargetPosition, CommitMove);
            cameraRig.Configure(cubeRenderer.transform, currentLevel);

            ResolveCapture();
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

            var cubeScreenPosition = viewCamera.WorldToScreenPoint(boardRenderer.GridToWorld(cubeGridPosition, currentLevel));
            var bestDirection = Direction.North;
            var bestDot = float.NegativeInfinity;

            foreach (var direction in new[] { Direction.North, Direction.South, Direction.East, Direction.West })
            {
                var targetGridPosition = cubeGridPosition + DirectionToGridOffset(direction);
                var targetScreenPosition = viewCamera.WorldToScreenPoint(boardRenderer.GridToWorld(targetGridPosition, currentLevel));
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
            var target = cubeGridPosition + DirectionToGridOffset(direction);
            return GetTileAt(target) != null;
        }

        private Vector3 GetTargetPosition(Direction direction)
        {
            var target = cubeGridPosition + DirectionToGridOffset(direction);
            return boardRenderer.GridToWorld(target, currentLevel) + Vector3.up * 0.62f;
        }

        private void CommitMove(Direction direction)
        {
            cubeGridPosition += DirectionToGridOffset(direction);
            orientation.Roll(direction);
            ResolveCapture();
        }

        private void ResolveCapture()
        {
            var tile = GetTileAt(cubeGridPosition);
            var captured = captureSystem.TryCapture(currentLevel, orientation, tile);
            if (captured)
            {
                boardRenderer.RefreshCapturedState();
                OnTileCaptured?.Invoke(tile);
            }

            OnCaptureChanged?.Invoke(captureSystem.CountCapturedRequired(currentLevel.tiles), CountRequired());

            if (winConditionSystem.IsComplete(currentLevel.tiles))
            {
                completed = true;
                OnLevelCompleted?.Invoke(currentLevel);
            }
        }

        private TileData GetTileAt(Vector2Int gridPosition)
        {
            foreach (var tile in currentLevel.tiles)
            {
                if (tile.active && tile.gridPos == gridPosition)
                {
                    return tile;
                }
            }

            return null;
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
    }
}
