namespace Asteroids
{
    internal class MenuKeybinds : IMenu
    {
        public List<Control> Controls { get; set; } = [];
        
        Keys LastPressedKey { get; set; }

        public MenuKeybinds()
        {

        }

        public void Draw(Graphics g)
        {

        }

        public void Update()
        {

        }
        
        private async Task<Keys> GetKeypress()
        {
            LastPressedKey = Keys.None;

            int MaxRetries = 50;

            for (int i = 0; i < MaxRetries; i++) 
                if (LastPressedKey == Keys.None)
                    await Task.Delay(100);

            return LastPressedKey;
        }
    }
}
