using System.Collections.Generic;
using ChromaCube.Core;
using UnityEngine;

namespace ChromaCube.Data
{
    [CreateAssetMenu(menuName = "Chroma Cube/Level Data", fileName = "LevelData")]
    public class LevelData : ScriptableObject
    {
        public string levelId;
        public int index;
        public string title;
        public string subtitle;
        [TextArea] public string hint;
        public int width;
        public int height;
        public Vector2Int start;
        public MechanicsMode mechanicsMode = MechanicsMode.Classic;
        public Texture2D backgroundTexture;
        public List<TileData> tiles = new List<TileData>();
        public List<FaceColorEntry> faceColorMap = new List<FaceColorEntry>();

        [Tooltip("Expected number of moves for a perfect/3-star run")]
        public int parMoveCount = 8;

        [Tooltip("Moves at or below this = 3 stars (0 = use parMoveCount)")]
        public int star3Threshold = 0;

        [Tooltip("Moves at or below this = 2 stars (0 = parMoveCount + ~30%)")]
        public int star2Threshold = 0;

        [Tooltip("Moves at or below this = 1 star (0 = parMoveCount + ~80%)")]
        public int star1Threshold = 0;

        public string GetColorForFace(FaceKey face)
        {
            for (var i = 0; i < faceColorMap.Count; i++)
            {
                if (faceColorMap[i].face == face)
                {
                    return faceColorMap[i].colorId;
                }
            }

            return "stone";
        }
    }
}
