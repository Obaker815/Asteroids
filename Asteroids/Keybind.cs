namespace Asteroids
{
    internal struct Keybind(Keys key)
    {
        public Keys Key { get; } = key;
        public bool IsPressed { get; set; } = false;
        public bool FirstPress { get; set; } = false;
    }
}
