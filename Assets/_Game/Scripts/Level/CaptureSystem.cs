using System.Collections.Generic;
using ChromaCube.Core;
using ChromaCube.Data;
using UnityEngine;

namespace ChromaCube.Level
{
    public class CaptureSystem
    {
        public bool TryCapture(
            LevelData level,
            CubeOrientation orientation,
            TileData tile,
            FaceKey resolvedBottomFace,
            Vector2Int gridPos,
            WorldCubeFace worldFace = WorldCubeFace.Top)
        {
            if (tile == null || !tile.active || !tile.required || tile.captured)
            {
                if (tile != null && tile.required && tile.captured && Settings.DebugCapture)
                {
                    Debug.Log($"[CaptureSystem] Tile {tile.id} at {gridPos} face {worldFace} already captured — skip.");
                }
                return false;
            }

            var bottomColor = level.GetColorForFace(resolvedBottomFace);

            if (Settings.DebugCapture)
            {
                Debug.Log($"[CaptureSystem] Check tile={tile.id} color={tile.colorId} " +
                          $"face={resolvedBottomFace} bottomColor={bottomColor} " +
                          $"grid={gridPos} face={worldFace} match={tile.colorId == bottomColor}");
            }

            if (tile.colorId != bottomColor)
            {
                return false;
            }

            tile.captured = true;
            return true;
        }

        public int CountCapturedRequired(IEnumerable<TileData> tiles)
        {
            var count = 0;
            foreach (var tile in tiles)
            {
                if (tile.active && tile.required && tile.captured)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
