using UnityEngine;

namespace ChromaCube.Core
{
    /// <summary>
    /// Central toggle for diagnostic logging.
    /// Set DebugCapture to true to see capture check details in the Console.
    /// </summary>
    public static class Settings
    {
        [Tooltip("Enable verbose capture-system logging in the Console.")]
        public const bool DebugCapture = false;

        [Tooltip("Enable verbose movement logging in the Console.")]
        public const bool DebugMovement = false;
    }
}
