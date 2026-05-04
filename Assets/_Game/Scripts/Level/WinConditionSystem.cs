using System.Collections.Generic;
using ChromaCube.Data;

namespace ChromaCube.Level
{
    public class WinConditionSystem
    {
        public bool IsComplete(IEnumerable<TileData> tiles)
        {
            foreach (var tile in tiles)
            {
                if (tile.active && tile.required && !tile.captured)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
