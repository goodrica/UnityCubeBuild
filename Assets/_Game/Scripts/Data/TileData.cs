using System;
using ChromaCube.Core;
using UnityEngine;

namespace ChromaCube.Data
{
    [Serializable]
    public class TileData
    {
        public string id;
        public WorldCubeFace worldFace = WorldCubeFace.Top;
        public Vector2Int gridPos;
        public string colorId;
        public bool required;
        public bool captured;
        public bool active = true;

        public TileData CloneRuntime()
        {
            return new TileData
            {
                id = id,
                worldFace = worldFace,
                gridPos = gridPos,
                colorId = colorId,
                required = required,
                captured = false,
                active = active
            };
        }
    }
}
