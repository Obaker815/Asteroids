namespace Asteroids
{
    internal struct Keybind
    {
        public Keys Key { get; }
        public bool IsPressed { get; set; }
        public bool FirstPress { get; set; }

        // take a key and make the keybind
        public Keybind(Keys key)
        {
            Key = key;
            IsPressed = false;
            FirstPress = false;
        }
    }
}
