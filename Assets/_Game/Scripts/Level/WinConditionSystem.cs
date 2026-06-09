using System.Collections.Generic;
using ChromaCube.Data;

namespace ChromaCube.Level
{
    public class WinConditionSystem
    {
        public bool IsComplete(IEnumerable<TileData> tiles)
        {
            // Guard: null tile list cannot be complete
            if (tiles == null) return false;

            // At least one active required tile must exist, and all of them must be captured.
            // Without this guard an empty or all-inactive tile set would incorrectly return true
            // and trigger an instant win on a broken/unloaded level.
            bool anyRequired = false;
            foreach (var tile in tiles)
            {
                if (tile.active && tile.required)
                {
                    anyRequired = true;
                    if (!tile.captured) return false;
                }
            }

            return anyRequired;
        }
    }
}
