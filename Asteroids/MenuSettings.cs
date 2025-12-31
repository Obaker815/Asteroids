namespace Asteroids
{
    internal class MenuSettings : IMenu
    {
        public List<Control> Controls { get; set; } = [];
        private Dictionary<string, int> ControlStyleOptions = new()
        {
            {"Classic", 0 },
            {"Two-Stick", 1 },
        };
        
        private Label Fullscreenlbl;
        private Label ControlStylelbl;
        private Label FPSlbl;
        private Label Debuglbl;
        private Label KeyLayoutlbl;

        private CheckBox Fullscreencb;
        private ComboBox ControlStyleddm;
        private TextBox FPStb;
        private CheckBox Debugcb;
        private Button KeyLayoutbtn;

        public MenuSettings()
        {
            MakeControls();
        }

        private void UpdateControls()
        {
            Fullscreencb!.Checked = Global.CONFIGS.Fullscreen;
            Debugcb!.Checked      = Global.CONFIGS.DebugAvailable;
            FPStb!.Text          = Global.CONFIGS.FPS.ToString();
            ControlStyleddm!.SelectedItem = ControlStyleOptions
                .FirstOrDefault(kv => kv.Value == Global.CONFIGS.ControlStyle).Key;
        }
        private void MakeControls()
        {
            int labelX = 100;
            int controlX = 350;
            Controls.Add(Fullscreenlbl = new Label()
            {
                Text = "Fullscreen:",
                Location = new Point(labelX, 50),
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
            });
            Controls.Add(Fullscreencb = new CheckBox()
            {
                Location = new Point(controlX, 53),
                Checked = Global.CONFIGS.Fullscreen,
                BackColor = Color.Transparent,
            });

            Controls.Add(ControlStylelbl = new Label()
            {
                Text = "Control Style:",
                Location = new Point(labelX, 100),
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
            });
            Controls.Add(ControlStyleddm = new ComboBox()
            {
                Location = new Point(controlX, 95),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
            });
            ControlStyleddm.Items.AddRange(ControlStyleOptions.Keys.ToArray());
            ControlStyleddm.SelectedItem = ControlStyleOptions
                .FirstOrDefault(kv => kv.Value == Global.CONFIGS.ControlStyle).Key;

            Controls.Add(FPSlbl = new Label()
            {
                Text = "FPS:",
                Location = new Point(labelX, 150),
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
            });
            Controls.Add(FPStb = new TextBox()
            {
                Location = new Point(controlX, 148),
                Text = Global.CONFIGS.FPS.ToString(),
                BackColor = Color.White,
            });

            Controls.Add(Debuglbl = new Label()
            {
                Text = "Debug Mode:",
                Location = new Point(labelX, 200),
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
            });
            Controls.Add(Debugcb = new CheckBox()
            {
                Location = new Point(controlX, 203),
                Checked = Global.CONFIGS.DebugAvailable,
                BackColor = Color.Transparent,
            });

            Controls.Add(KeyLayoutlbl = new Label()
            {
                Text = "Keyboard Layout:",
                Location = new Point(labelX, 250),
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
            });
            Controls.Add(KeyLayoutbtn = new Button()
            {
                Text = "Edit Keybinds",
                Location = new Point(controlX, 245),
                AutoSize = true,
                BackColor = Color.White,
            });

            Fullscreencb!.CheckedChanged += (s, e) => { Global.CONFIGS.Fullscreen     = Fullscreencb.Checked; UpdateControls(); };
            Debugcb!.CheckedChanged      += (s, e) => { Global.CONFIGS.DebugAvailable = Debugcb.Checked;      UpdateControls(); };
            KeyLayoutbtn!.Click          += (s, e) => { Global.CURRENT_STATE          = GameState.KeybindsMenu; };
            ControlStyleddm!.SelectedIndexChanged += (s, e) => 
            { 
                if (ControlStyleddm.SelectedItem != null && 
                    ControlStyleOptions.TryGetValue(ControlStyleddm.SelectedItem.ToString()!, out int style))
                {
                    Global.CONFIGS.ControlStyle = style;
                }
                UpdateControls();
            };
            FPStb!.TextChanged += (s, e) => 
            { 
                if (int.TryParse(FPStb.Text, out int fps))
                {
                    Global.CONFIGS.FPS = fps;
                }
            };
        }

        public void Draw(Graphics g)
        {
            if (Global.DEBUG)
            {
                foreach (Control control in Controls)
                {
                    Rectangle rect = control.Bounds;
                    g.DrawRectangle(Pens.Red, rect);
                }
            }
        }

        public void Update()
        {
            GameForm.ActiveGameform?.InvokeAction(() => { if (GameForm.ActiveGameform?.Focused == true) UpdateControls(); });
        }
    }
}
