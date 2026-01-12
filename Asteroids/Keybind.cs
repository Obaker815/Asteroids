namespace Asteroids
{
    public struct Keybind(Keys key)
    {
        public Keys Key { get; set; } = key;
        public bool IsPressed { get; set; } = false;
        public bool FirstPress { get; set; } = false;
    }
}
