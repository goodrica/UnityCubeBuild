namespace ChromaCube.Core
{
    /// <summary>
    /// Central toggle for diagnostic logging.
    /// Set DebugCapture to true to see capture check details in the Console.
    /// </summary>
    public static class Settings
    {
        /// <summary>Enable verbose capture-system logging in the Console.</summary>
        public const bool DebugCapture = false;

        /// <summary>Enable verbose movement logging in the Console.</summary>
        public const bool DebugMovement = false;
    }
}
