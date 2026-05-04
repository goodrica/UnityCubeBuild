using System.Collections.Generic;
using ChromaCube.Core;
using ChromaCube.Data;
using UnityEngine;

namespace ChromaCube.Level
{
    public class CaptureSystem
    {
        public bool TryCapture(LevelData level, CubeOrientation orientation, TileData tile)
        {
            if (tile == null || !tile.active || !tile.required || tile.captured)
            {
                return false;
            }

            var bottomColor = level.GetColorForFace(orientation.GetBottomFace());
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
