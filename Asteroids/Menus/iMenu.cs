namespace Asteroids.Menus
{
    internal interface IMenu
    {
        public List<Control> Controls { get; set; }
        public void Draw(Graphics g);
        public void Update();
    }
}
