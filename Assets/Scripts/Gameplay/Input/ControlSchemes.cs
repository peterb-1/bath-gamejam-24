namespace Gameplay.Input
{
    /// <summary>
    /// We don't define separate control schemes in the input actions asset for keyboard and mouse.
    /// But, we do detect mouse movement, and consider that a separate state.
    /// This allows us to have the cursor hidden by default, and only show it when the user moves the mouse.
    /// </summary>
    
    public enum ControlScheme
    {
        Keyboard,
        Gamepad,
        Mouse
    }
    
    public static class ControlSchemeIdentifiers
    {
        public const string KEYBOARD_MOUSE = "Keyboard&Mouse";
        public const string GAMEPAD = "Gamepad";
    }
}
