using SharpDX.XInput;
using System.Drawing.Drawing2D;

namespace Asteroids
{
    public partial class GameForm : Form
    {
        public GameForm()
        {
            InitializeComponent();
        }

        private void GameForm_Shown(object sender, EventArgs e)
        {
            Wrapable.SetBounds(this.ClientRectangle.Size);
            new Ship(new(this.ClientRectangle.Width / 2, this.ClientRectangle.Height / 2));
        }

        private Dictionary<string, Keybind> KeyBindings = ConstructKeybindings();
        private Controller Gamepad = new(UserIndex.One);
        // key down and key up event
        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            foreach (string Key in KeyBindings.Keys)
            {
                // check if the key pressed matches any keybinds
                if (e.KeyCode == KeyBindings[Key].Key)
                {
                    // edit the Keybind in the dictionary
                    Keybind kb = KeyBindings[Key];
                    if (!kb.IsPressed)
                    {
                        kb.FirstPress = true;
                    }
                    kb.IsPressed = true;
                    KeyBindings[Key] = kb;
                }
            }
        }
        private void GameForm_KeyUp(object sender, KeyEventArgs e)
        {
            foreach (string Key in KeyBindings.Keys)
            {
                // check if the key pressed matches any keybinds
                if (e.KeyCode == KeyBindings[Key].Key)
                {
                    // edit the Keybind in the dictionary
                    Keybind kb = KeyBindings[Key];
                    kb.IsPressed = false;
                    KeyBindings[Key] = kb;
                }
            }
        }

        /// <summary>
        /// Constructor for the keybindings dictionary
        /// </summary>
        /// <returns>A dictionary with a string key and struct Keybind value</returns>
        private static Dictionary<string, Keybind> ConstructKeybindings()
        {
            return new()
            {
                { "Up", new Keybind(Keys.W) },
                { "Down", new Keybind(Keys.S) },
                { "Left", new Keybind(Keys.A) },
                { "Right", new Keybind(Keys.D) },
                { "UpAlt", new Keybind(Keys.Up) },
                { "DownAlt", new Keybind(Keys.Down) },
                { "LeftAlt", new Keybind(Keys.Left) },
                { "RightAlt", new Keybind(Keys.Right) },
                { "Shoot", new Keybind(Keys.Space) },
            };
        }

        /// <summary>
        /// Duplicate a keybinding dictionary
        /// </summary>
        /// <param name="old">The dictionary to be duplicated</param>
        /// <returns>A dictionary with a string key and struct Keybind value</returns>
        private static Dictionary<string, Keybind> DuplicateKeybindings(Dictionary<string, Keybind> old)
        {
            Dictionary<string, Keybind> keys = ConstructKeybindings();
            foreach (var key in old.Keys)
            {
                keys[key] = old[key];
            }
            return keys;
        }

        private void GameForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.Clear(Color.Black);
            foreach (Wrapable w in Wrapable.Wrapables)
            {
                w.Draw(g);
            }
        }
    }
}
