using SharpDX.XInput;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Asteroids
{
    public partial class GameForm : Form
    {
        public static readonly PrivateFontCollection? PublicFonts = new();
        private readonly Controller controller = new(UserIndex.One);
        private readonly Dictionary<string, Keybind> OptionBindings = new()
        {
            { "Exit",               new Keybind(Keys.Escape )},
            { "DebugMode",          new Keybind(Keys.F1     )},
            { "DebugShowParticles", new Keybind(Keys.F2     )},
            { "Fullscreen",         new Keybind(Keys.F11    )},
            { "ShowFramerate",      new Keybind(Keys.F12    )},
        };
        public Keymap Keymap;

        private string frameRate = "";
        private string frameTime = "";

        public static readonly Rectangle preferredRect = new(0, 0, 800, 480);
        private readonly Size preferredSize = new(800, 450);
        private readonly Size borderSize;
        private readonly ParticleEffect starsEffect;

        private const float ROUND_START_TIME = 4;
        private float currentRoundEndTime;
        public  bool roundStarting = false;

        private static float freezeTime = 0f;
        private static float dtModifier = 1f;
        private static GameForm? activeGameform;

        public GameForm()
        {
            InitializeComponent();
            elapsedtimeSW = null!;
            Keymap = null!;

            // Get the size of the border
            borderSize = new Size(
                this.Size.Width - this.ClientSize.Width,
                this.Size.Height - this.ClientSize.Height);

            // Load font
            PublicFonts!.AddFontFile(Global.DATA_PATH + Global.FONT_PATH_BASE + @"OrchestraOfStrings/OrchestraOfStrings-yoLd.ttf");
            FontFamily fontFamily = PublicFonts.Families[0];
            Font = new Font(fontFamily, 20f);

            // Stars background
            starsEffect = new(
                typeof(ParticleDot),
                position: new(preferredSize.Width / 2, preferredSize.Height / 2),
                args: [],
                interval: 0.2f,
                lifetime: 10f,
                impulse: 0f,
                count: 5,
                radius: float.Sqrt(
                    preferredSize.Width  * preferredSize.Width / 4 + 
                    preferredSize.Height * preferredSize.Height / 4),
                lifetimeRange: (-5, 5),
                impulseRange: (0, 1),
                gradient: [
                    (Color.White, 0.5f),
                    ]);
        }

        private void GameForm_GotFocus(object sender, EventArgs e)
        {
            ActiveGameform = this;
        }

        private void GameForm_Shown(object sender, EventArgs e)
        {
            this.Focus();

            Wrapable.SetBounds(preferredSize);

            if (Global.CONFIGS.Fullscreen)
                Fullscreen(true);

            Global.STATE_MENU[GameState.MainMenu]     = new MenuMain();
            Global.STATE_MENU[GameState.SettingsMenu] = new MenuSettings();
            Global.STATE_MENU[GameState.KeybindsMenu] = new MenuKeybinds();
            Global.STATE_MENU[GameState.Playing]      = new MenuNone();
            Global.STATE_MENU[GameState.None]         = new MenuNone();

            Task.Run(GameMainLoop);
            Focus();
        }

        public void InvokeAction(Action action)
        {
            if (this.InvokeRequired)
            {
                try
                { this.Invoke(action); }

                catch (ObjectDisposedException)
                { return; }

                return;
            }
            action();
        }

        public static void AddFreezeTime(float time, float modifier)
        {
            freezeTime = time;
            dtModifier = modifier;
        }

        private void Fullscreen(bool fullscreen)
        {
            if (!fullscreen && this.FormBorderStyle != FormBorderStyle.Sizable)
            {
                this.InvokeAction(() =>
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Normal;

                    this.Size = new Size(preferredSize.Width + borderSize.Width, preferredSize.Height + borderSize.Height);

                    Global.CONFIGS.Fullscreen = false;
                });
            }
            else if (fullscreen && this.FormBorderStyle != FormBorderStyle.None)
            {
                this.InvokeAction(() =>
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                });
                Global.CONFIGS.Fullscreen = true;
            }
        }

        private Task ProcessOptionChanges()
        {
            if (Global.SUPPRESS_OPTION_CHANGED_EVENT)
                return Task.CompletedTask;

            if (Global.CONFIGS.DebugAvailable)
            {
                if (OptionBindings["DebugMode"].FirstPress)
                    Global.DEBUG = !Global.DEBUG;

                if (OptionBindings["DebugShowParticles"].FirstPress)
                    Global.DEBUG_PARTICLE_DRAW = !Global.DEBUG_PARTICLE_DRAW;
            }

            if (OptionBindings["Exit"].FirstPress)
                Global.CURRENT_STATE = Global.STATE_RETURN[Global.CURRENT_STATE];

            if (OptionBindings["Fullscreen"].FirstPress)
                Global.CONFIGS.Fullscreen = !Global.CONFIGS.Fullscreen;

            if (OptionBindings["ShowFramerate"].FirstPress)
                Global.FPSDISPLAY = !Global.FPSDISPLAY;
            
            if (Global.CURRENT_STATE == GameState.None)
                Application.Exit();

            Fullscreen(Global.CONFIGS.Fullscreen);

            return Task.CompletedTask;
        }

        Stopwatch elapsedtimeSW;
        bool running = false;
        private async void GameMainLoop()
        {
            if (running) return;
            running = true;

            Stopwatch deltatimeSW = Stopwatch.StartNew();
            elapsedtimeSW = Stopwatch.StartNew();

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
                
                await ProcessOptionChanges();

                // Load and unload menus if the state has changed
                if (Global.CURRENT_STATE != Global.PREVIOUS_STATE)
                {
                    this.InvokeAction(() =>
                    {
                        foreach (Control control in Global.STATE_MENU[Global.PREVIOUS_STATE].Controls) this.Controls.Remove(control);
                        foreach (Control control in Global.STATE_MENU[Global.CURRENT_STATE].Controls)  this.Controls.Add(control);
                    });
                    
                    Global.PREVIOUS_STATE = Global.CURRENT_STATE;
                }

                if (Global.CURRENT_STATE != GameState.Playing)
                {
                    if (starsEffect.IsPlaying) starsEffect.Stop();
                    Global.STATE_MENU[Global.CURRENT_STATE].Update();
                }
                else
                {
                    if (!starsEffect.IsPlaying) starsEffect.Start();

                    if (Ship.Ships[0].lives == 0 && !Ship.Ships[0].Respawning)
                    {
                        Global.CURRENT_STATE = GameState.MainMenu;
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
                            await LevelManager.Instance.NewRound(preferredRect);
                            roundStarting = false;
                        }
                    }
                    LevelManager.Instance.SaucerUpdate(dt);

                    // Update Ships
                    Ship[] ships = [.. Ship.Ships];
                    foreach (Ship ship in ships)
                    {
                        ship?.Update(Keymap.keybinds, controller, dt);
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

                    // Remove entities
                    Entity.RemoveAll();

                    // Remove particles
                    Particle.RemoveAll();
                }

                // Update keybinds
                foreach (string Key in OptionBindings.Keys)
                {
                    Keybind kb = OptionBindings[Key];
                    if (kb.FirstPress)
                    {
                        kb.FirstPress = false;
                    }
                    OptionBindings[Key] = kb;
                }

                foreach (string Key in Keymap.keybinds.Keys)
                {
                    Keybind kb = Keymap.keybinds[Key];
                    if (kb.FirstPress)
                    {
                        kb.FirstPress = false;
                    }
                    Keymap.keybinds[Key] = kb;
                }

                // Redraw the screen
                InvokeAction(this.Invalidate);

                float frameTime;
                if (Global.CONFIGS.FPS != 0)
                    frameTime = 1000f / Global.CONFIGS.FPS;
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
            void CheckDict(Dictionary<string, Keybind> dict)
            {
                foreach (string Key in dict.Keys)
                    // check if the key pressed matches any keybinds
                    if (e.KeyCode == dict[Key].Key)
                    {
                        // edit the Keybind in the dictionary
                        Keybind kb = dict[Key];
                        kb.FirstPress = true;
                        kb.IsPressed = true;
                        dict[Key] = kb;
                    }
            }

            CheckDict(Keymap.keybinds);
            CheckDict(OptionBindings);
            if (Global.CURRENT_STATE == GameState.MainMenu) CheckDict(MenuMain.MenuKeys);
            
            if (Global.CURRENT_STATE == GameState.KeybindsMenu)
                ((MenuKeybinds)Global.STATE_MENU[GameState.KeybindsMenu]).LastPressedKey = e.KeyCode;
        }

        private void GameForm_KeyUp(object sender, KeyEventArgs e)
        {
            foreach (string Key in Keymap.keybinds.Keys)
            {
                // check if the key pressed matches any keybinds
                if (e.KeyCode == Keymap.keybinds[Key].Key)
                {
                    // edit the Keybind in the dictionary
                    Keybind kb = Keymap.keybinds[Key];
                    kb.IsPressed = false;
                    Keymap.keybinds[Key] = kb;
                }
            }
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

            // Transform the Graphics object to fit the window size
            Matrix originalTransform = g.Transform.Clone();
            g.TranslateTransform(xdiff / 2, ydiff / 2);
            g.ScaleTransform(scale, scale);

            if (Global.CURRENT_STATE == GameState.Playing)
            {
                if (Global.DEBUG)
                {
                    g.ScaleTransform(0.5f, 0.5f);
                    g.TranslateTransform(preferredSize.Width / 2, preferredSize.Height / 2);
                    if (Global.DEBUG_PARTICLE_DRAW)
                        ParticleEffect.DebugDrawAll(g);
                }

                // Draw all particles
                Particle.DrawAll(g);

                // Draw all wrapable objects
                Wrapable[] wrapables = [.. Wrapable.Wrapables];
                foreach (Wrapable wrapable in wrapables)
                    wrapable?.Draw(g);
                Asteroid.FinalDraw(g);

                // Draw UI elements on top of everything
                string score = LevelManager.Instance.Score.ToString("D8");
                g.DrawString(score, Font, Brushes.White, 10, 10);

                // Draw the current life count
                float livesYPos = 60;
                float livesXPos = 25;
                float livesSpacing = 24;
                for (int i = 0; i < Ship.Ships[0].lives; i++)
                {
                    float posX = livesXPos + i * livesSpacing;
                    float posY = livesYPos;

                    Ship.Draw(g,
                        new(posX, posY),
                        new(0, -1),
                        10,
                        (i == Ship.Ships[0].lives - 1) && !Ship.Ships[0].Respawning);
                }
            }
            else Global.STATE_MENU[Global.CURRENT_STATE].Draw(g);    

            // if the fps display is enabled, draw it
            if (Global.FPSDISPLAY)
            {
                g.DrawString($"Framerate: {frameRate}fps", Font, Brushes.White, 10, preferredSize.Height - 60);
                g.DrawString($"Frametime: {frameTime}ms",  Font, Brushes.White, 10, preferredSize.Height - 30);
            }

            // reset the transform of the graphics object
            g.Transform = originalTransform;

            // Draw the bars on the top & bottom || left & right to hide how wrapping works when the graphics object doesn't scale exactly
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

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            JSONManager.WriteJson(Global.DATA_PATH + Global.CONFIG_PATH, Global.CONFIGS);
            JSONManager.WriteJson(Global.DATA_PATH + Global.SCOREBOARD_PATH, MenuMain.Scoreboard);

            if (Global.CONFIGS.LastUsedKeymap != "default_keybinds.json") 
                JSONManager.WriteJson(Global.DATA_PATH + Global.KEYBIND_PATH_BASE + Global.CONFIGS.LastUsedKeymap, Keymap);
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            string ScoreboardPath = Global.DATA_PATH + Global.SCOREBOARD_PATH;
            string ConfigPath = Global.DATA_PATH + Global.CONFIG_PATH;

            File.Delete(ScoreboardPath);
            if (!File.Exists(ConfigPath))
            {
                File.Create(ConfigPath).Close();

                ConfigsJSON tempcfg = new()
                {
                    Fullscreen = false,
                    ControlStyle = 0,
                    FPS = 60,
                    DebugAvailable = false
                };

                JSONManager.WriteJson(ConfigPath, tempcfg);
            }
            if (!File.Exists(ScoreboardPath))
            {
                File.Create(ScoreboardPath).Close();

                Scoreboard tempScoreboard = new()
                {
                    Entries = Enumerable.Range(0, 10)
                        .Select(_ => new ScoreboardEntry
                        {
                            Name = "_____",
                            Score = 0,
                        })
                        .ToArray()
                };

                JSONManager.WriteJson(ScoreboardPath, tempScoreboard);
            }
            string defaultKeybindPath = Global.DATA_PATH + Global.KEYBIND_PATH_BASE + Global.DEFAULT_KEYBIND_FILE;
            if (!File.Exists(defaultKeybindPath))
            {
                File.Create(defaultKeybindPath).Close();
                JSONManager.WriteJson(defaultKeybindPath, new Keymap());
            }

            Global.CONFIGS      = JSONManager.ReadJson<ConfigsJSON> (ConfigPath);
            MenuMain.Scoreboard = JSONManager.ReadJson<Scoreboard>  (ScoreboardPath);
            MenuMain.Scoreboard.SortEntries();

            string keybindFile = (Global.CONFIGS.LastUsedKeymap == null || Global.CONFIGS.LastUsedKeymap == "")
                ? Global.DEFAULT_KEYBIND_FILE
                : Global.CONFIGS.LastUsedKeymap;

            Global.CONFIGS.LastUsedKeymap = keybindFile;
            Keymap = JSONManager.ReadJson<Keymap>(Global.DATA_PATH + Global.KEYBIND_PATH_BASE + Global.CONFIGS.LastUsedKeymap);
        }
    }
}
