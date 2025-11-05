using SharpDX.XInput;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace Asteroids
{
    public partial class GameForm : Form
    {
        private readonly Rectangle preferredRect = new(0, 0, 800, 480);
        private readonly Size preferredSize = new(800, 480);
        private readonly float ratio = 480 / 800f;
        private readonly Size borderSize;

        private static float freezeTime = 0f;
        private static float dtModifier = 1f;

        public GameForm()
        {
            InitializeComponent();
            borderSize = new Size(this.Size.Width - this.ClientSize.Width, this.Size.Height - this.ClientSize.Height);
        }

        private void GameForm_Shown(object sender, EventArgs e)
        {
            Wrapable.SetBounds(preferredSize);

            _ = new Ship(new(preferredSize.Width / 2, preferredSize.Height / 2));
            _ = new Saucer(false, new(preferredSize.Width / 2, preferredSize.Height / 2));

            for (int i = 0; i < 5; i++)
                _ = Asteroid.NewAsteroid(this.preferredRect, 3);

            Task.Run(GameMainLoop);
        }

        private void InvokeAction(Action action)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(action);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }
            else
            {
                action();
            }
        }

        public static void AddFreezeTime(float time, float modifier)
        {
            freezeTime = time;
            dtModifier = modifier;
        }

        bool running = false;
        private void GameMainLoop()
        {
            if (running) return;
            running = true;

            Stopwatch deltatimeSW = Stopwatch.StartNew();
            Stopwatch elapsedtimeSW = Stopwatch.StartNew();

            int lastTitleUpdate = 0;
            while (running)
            {
                float dt = (float)deltatimeSW.Elapsed.TotalSeconds;
                deltatimeSW.Restart();

                if (freezeTime > 0f)
                {
                    freezeTime -= dt;
                    freezeTime = float.Max(freezeTime, 0);
                    dt *= dtModifier;
                }

                // Update keybinds
                Dictionary<string, Keybind> currentKeyBindings = DuplicateKeybindings(KeyBindings);
                foreach (string Key in KeyBindings.Keys)
                {
                    Keybind kb = KeyBindings[Key];
                    if (kb.FirstPress)
                    {
                        kb.FirstPress = false;
                    }
                    KeyBindings[Key] = kb;
                }

                // Update Ships
                Ship[] ships = [.. Ship.Ships];
                foreach (Ship ship in ships)
                {
                    ship?.Update(currentKeyBindings, controller, dt);
                }

                // Update Saucers
                Saucer[] saucers = [.. Saucer.Saucers];
                foreach (Saucer saucer in saucers)
                {
                    saucer?.Update();
                }

                // Update Asteroids
                Asteroid[] asteroids = [.. Asteroid.AsteroidEntities];
                foreach (Asteroid asteroid in asteroids)
                {
                    asteroid?.Update(dt);
                }

                // Update Bullets
                Bullet[] bullets = [.. Bullet.Bullets];
                foreach (Bullet bullet in bullets)
                {
                    bullet?.Update();
                }

                // Update all entities
                Entity[] entities = [.. Entity.Entities];
                foreach (Entity entity in entities)
                {
                    entity?.Update(dt);
                }

                // Wrap all wrapables
                Wrapable[] wrapables = [.. Wrapable.Wrapables];
                foreach (Wrapable wrapable in wrapables)
                {
                    wrapable?.WrapPosition();
                }

                // Redraw the screen
                InvokeAction(this.Invalidate);

                // Remove destroyed entities
                foreach (Entity e in Entity.toRemove)
                {
                    e.Remove();
                }
                Entity.toRemove = [];

                float frameTime;
                if (Global.FPS != 0)
                    frameTime = 1000f / Global.FPS;
                else
                    frameTime = 0;

                if ((int)elapsedtimeSW.Elapsed.TotalSeconds > lastTitleUpdate)
                {
                    lastTitleUpdate = (int)elapsedtimeSW.Elapsed.TotalSeconds;
                    if (Global.DEBUG)
                        InvokeAction(() => { this.Text = ($"Frame Time: {dt * 1000:0.00} ms - FrameRate: {1f / dt:0.00}"); });
                    else
                        InvokeAction(() => { this.Text = "Asteroids"; });
                }

                while (deltatimeSW.Elapsed.TotalMilliseconds < frameTime) { }
            }

            deltatimeSW.Stop();
            elapsedtimeSW.Stop();
        }

        private readonly Dictionary<string, Keybind> KeyBindings = ConstructKeybindings();
        private readonly Controller controller = new(UserIndex.One);

        private const Keys FULLSCREEN_KEY = Keys.F11;
        private const Keys CLOSE_KEY = Keys.Escape;

        private const string DEBUG_STRING = "UUDDLRLR";
        private string lastKeysPressed = "";
        // key down and key up event
        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == CLOSE_KEY)
                this.Close();

            if (e.KeyCode == FULLSCREEN_KEY)
            {
                if (this.FormBorderStyle == FormBorderStyle.None)
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Normal;

                    this.Width = preferredSize.Width + borderSize.Width;

                    GameForm_ResizeEnd(sender, e);
                }
                else
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                }
            }
            foreach (string Key in KeyBindings.Keys)
            {
                // check if the key pressed matches any keybinds
                if (e.KeyCode == KeyBindings[Key].Key)
                {
                    // edit the Keybind in the dictionary
                    Keybind kb = KeyBindings[Key];
                    kb.FirstPress = true;
                    kb.IsPressed = true;
                    KeyBindings[Key] = kb;

                    // Debug code for cheat code detection
                    lastKeysPressed += Key.First();
                }
            }

            if (lastKeysPressed.Length > 8) lastKeysPressed = lastKeysPressed.Substring(1, 8);
            if (DEBUG_STRING == lastKeysPressed)
            {
                Global.DEBUG = !Global.DEBUG;
                Wrapable.SetBounds(preferredSize);
                lastKeysPressed = "";
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
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float scaleX = (float)this.ClientSize.Width / preferredSize.Width;
            float scaleY = (float)this.ClientSize.Height / preferredSize.Height;

            g.ScaleTransform(scaleX, scaleY);
            if (Global.DEBUG)
            {
                g.ScaleTransform(1/2f, 1/2f);
                g.TranslateTransform(preferredSize.Width / 2, preferredSize.Height / 2);
            }

            g.Clear(Color.Black);

            Wrapable[] wrapables = [.. Wrapable.Wrapables];
            foreach (Wrapable wrapable in wrapables)
            {
                wrapable?.Draw(g);
            }

            g.ScaleTransform(1 / scaleX, 1 / scaleY);
        }

        private void GameForm_ResizeEnd(object sender, EventArgs e)
        {
            this.Size = new Size(this.ClientSize.Width + borderSize.Width, (int)((this.ClientSize.Width + borderSize.Height) * ratio));
        }
    }
}
