using SharpDX.XInput;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Numerics;

namespace Asteroids
{
    public partial class GameForm : Form
    {
        private readonly PrivateFontCollection? privateFonts;
        private readonly Dictionary<string, Keybind> KeyBindings = ConstructKeybindings();
        private readonly Controller controller = new(UserIndex.One);
        private const Keys FULLSCREEN_KEY = Keys.F11;
        private const Keys CLOSE_KEY = Keys.Escape;

        private const string DEBUG_CODE = "UUDDLRLR";
        private string lastKeysPressed = "";
        private string frameRate = "";
        private string frameTime = "";

        private readonly Rectangle preferredRect = new(0, 0, 800, 480);
        private readonly Size preferredSize = new(800, 450);
        private readonly float ratio = 450f / 800f;
        private readonly Size borderSize;

        private readonly LevelManager levelManager = new();
        private const float ROUND_START_TIME = 4;
        private float currentRoundEndTime;
        private bool roundStarting = false;

        private static float freezeTime = 0f;
        private static float dtModifier = 1f;
        private static GameForm? activeGameform;

        public GameForm()
        {
            InitializeComponent();

            borderSize = new Size(
                this.Size.Width - this.ClientSize.Width,
                this.Size.Height - this.ClientSize.Height);

            privateFonts = new PrivateFontCollection();
            privateFonts.AddFontFile(@"./font/OrchestraOfStrings-yoLd.ttf");

            FontFamily fontFamily = privateFonts.Families[0];
            Font = new Font(fontFamily, 20f);
        }


        private void GameForm_GotFocus(object sender, EventArgs e)
        {
            ActiveGameform = this;
        }

        private void GameForm_Shown(object sender, EventArgs e)
        {
            Wrapable.SetBounds(preferredSize);

            _ = new Ship(new(preferredSize.Width / 2, preferredSize.Height / 2));

            Task.Run(GameMainLoop);
            Focus();
        }

        public void InvokeAction(Action action)
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
        private async void GameMainLoop()
        {
            if (running) return;
            running = true;

            Stopwatch deltatimeSW = Stopwatch.StartNew();
            Stopwatch elapsedtimeSW = Stopwatch.StartNew();

            int lastFrameTimesUpdate = 0;
            while (running)
            {
                float dt = (float)deltatimeSW.Elapsed.TotalSeconds;
                deltatimeSW.Restart();

                if ((int)elapsedtimeSW.Elapsed.TotalSeconds > lastFrameTimesUpdate)
                {
                    this.frameRate = $"{1f / dt:0.00}";
                    this.frameTime = $"{dt * 1000:0.00}";
                    lastFrameTimesUpdate = (int)elapsedtimeSW.Elapsed.TotalSeconds;
                }

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

                if (Asteroid.AsteroidEntities.Count == 0 && !roundStarting)
                {
                    currentRoundEndTime = 0;
                    roundStarting = true;
                }
                if (roundStarting)
                {
                    currentRoundEndTime += dt;
                    if (currentRoundEndTime >= ROUND_START_TIME)
                    {
                        await levelManager.NewRound(preferredRect);
                        roundStarting = false;
                    }
                }
                levelManager.SaucerUpdate(dt);

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
                    saucer?.Update(dt);
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

                // Update all particles
                Particle[] particles = [.. Particle.Particles];
                foreach (Particle particle in particles)
                {
                    particle?.Update(dt);
                }

                // Redraw the screen
                InvokeAction(this.Invalidate);

                // Remove entities
                Entity.RemoveAll();

                // Remove particles
                Particle.RemoveAll();

                float frameTime;
                if (Global.FPS != 0)
                    frameTime = 1000f / Global.FPS;
                else
                    frameTime = 0;

                while (deltatimeSW.Elapsed.TotalMilliseconds < frameTime) { }
            }

            deltatimeSW.Stop();
            elapsedtimeSW.Stop();
        }

        public static GameForm? ActiveGameform { get => activeGameform; set => activeGameform = value; }

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

                    this.Size = new Size(preferredSize.Width + borderSize.Width, preferredSize.Height + borderSize.Height);
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
            if (DEBUG_CODE == lastKeysPressed)
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

        /// <summary>
        /// The paint event for the <see cref="GameForm"/> class
        /// </summary>
        /// <param name="sender">I have no fucking clue</param>
        /// <param name="e">The paint event arguments</param>
        private void GameForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.Clear(Color.Black);

            float scaleX = (float)ClientSize.Width / preferredSize.Width;
            float scaleY = (float)ClientSize.Height / preferredSize.Height;
            float scale = MathF.Min(scaleX, scaleY);

            float renderWidth = preferredSize.Width * scale;
            float renderHeight = preferredSize.Height * scale;

            float xdiff = ClientSize.Width - renderWidth;
            float ydiff = ClientSize.Height - renderHeight;

            Matrix originalTransform = g.Transform.Clone();

            g.TranslateTransform(xdiff / 2, ydiff / 2);
            g.ScaleTransform(scale, scale);

            if (Global.DEBUG)
            {
                g.ScaleTransform(0.5f, 0.5f);
                g.TranslateTransform(preferredSize.Width / 2, preferredSize.Height / 2);
                ParticleEffect.DebugDrawAll(g);
            }

            string score = levelManager.Score.ToString("D10");
            g.DrawString($"Framerate: {frameRate}fps", Font, Brushes.White, 10, preferredSize.Height - 60);
            g.DrawString($"Frametime: {frameTime}ms", Font, Brushes.White, 10, preferredSize.Height - 30);
            g.DrawString(score, Font, Brushes.White, 10, 10);

            Wrapable[] wrapables = [.. Wrapable.Wrapables];
            foreach (Wrapable wrapable in wrapables)
                wrapable?.Draw(g);

            Particle.DrawAll(g);

            g.Transform = originalTransform;

            Brush barBrush = new SolidBrush(Color.FromArgb(255, 30, 30, 30));

            if (xdiff > 0)
            {
                float barW = xdiff / 2;
                g.FillRectangle(barBrush, -10, -10, barW + 10, ClientSize.Height + 20);
                g.FillRectangle(barBrush, ClientSize.Width - barW, -10, barW + 10, ClientSize.Height + 10);
            }
            else if (ydiff > 0)
            {
                float barH = ydiff / 2;
                g.FillRectangle(barBrush, -10, -10, ClientSize.Width + 20, barH + 10);
                g.FillRectangle(barBrush, -10, ClientSize.Height - barH, ClientSize.Width + 20, barH + 10);
            }
        }
    }
}
