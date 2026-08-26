using UnityEngine;

namespace ChromaCube.Core
{
    /// <summary>
    /// Lightweight persistence for best move counts and star ratings per level.
    /// Uses PlayerPrefs so results survive app restarts without external files.
    /// </summary>
    public static class SaveSystem
    {
        private const string Prefix = "ChromaCube_";

        public static int GetBestMoves(string levelId)
        {
            return PlayerPrefs.GetInt(Prefix + levelId + "_moves", int.MaxValue);
        }

        public static void SetBestMoves(string levelId, int moves)
        {
            var current = GetBestMoves(levelId);
            if (moves < current)
            {
                PlayerPrefs.SetInt(Prefix + levelId + "_moves", moves);
            }
        }

        public static int GetBestStars(string levelId)
        {
            return PlayerPrefs.GetInt(Prefix + levelId + "_stars", 0);
        }

        public static void SetBestStars(string levelId, int stars)
        {
            var current = GetBestStars(levelId);
            if (stars > current)
            {
                PlayerPrefs.SetInt(Prefix + levelId + "_stars", stars);
            }
        }

        public static void ClearAll()
        {
            var keys = new System.Collections.Generic.List<string>();
            foreach (var key in PlayerPrefs.GetString(string.Empty).Split('|'))
            {
                if (key.StartsWith(Prefix))
                {
                    keys.Add(key);
                }
            }

            foreach (var key in keys)
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        public static int EvaluateStars(int moveCount, int par, int star1, int star2, int star3)
        {
            var s3 = star3 > 0 ? star3 : par;
            var s2 = star2 > 0 ? star2 : par + Mathf.CeilToInt(par * 0.3f);
            var s1 = star1 > 0 ? star1 : par + Mathf.CeilToInt(par * 0.8f);

            if (moveCount <= s3) return 3;
            if (moveCount <= s2) return 2;
            if (moveCount <= s1) return 1;
            return 0;
        }
    }
}
