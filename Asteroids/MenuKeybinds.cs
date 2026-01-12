using System.Diagnostics;

namespace Asteroids
{
    internal class MenuKeybinds : IMenu
    {
        public List<Control> Controls { get; set; } = [];
        public Keys LastPressedKey { get; set; }

        private ComboBox Keymapddm;
        private bool dontUpdate = true;
        public MenuKeybinds()
        {
            string KeybindBasePath = Global.DATA_PATH + Global.KEYBIND_PATH_BASE;

            string[] keybindConfigs = Directory.GetFiles(KeybindBasePath);
            for (int i = 0; i < keybindConfigs.Length; i++)
            {
                keybindConfigs[i] = Global.GetFileName(keybindConfigs[i]);
                keybindConfigs[i] = keybindConfigs[i].Replace('_', ' ');
            }

            int LabelX = 100;
            int ButtonX = 250;
            int StartY = 50;
            int SpacingY = 50;
            int OffsetX = 300;
            int RepeatAfter = 5;

            int currentY = StartY;
            int repetitions = 0;

            Dictionary<string, Keybind> keymap = GameForm.ActiveGameform!.Keymap.ToDictionary();

            foreach (string keybindName in keymap.Keys)
            {
                Label KeybindLabel = new Label()
                {
                    Text = keybindName + ":",
                    AutoSize = true,
                    ForeColor = Color.White,
                    BackColor = Color.Black,
                    Location = new Point(LabelX, currentY - 20),
                };
                Button keybindButton = new Button()
                {
                    Width = 100,
                    Height = 50,
                    BackColor = Color.White,
                    Location = new Point(ButtonX, currentY - 25),
                };
                keybindButton.Click += async (s, e) =>
                {
                    keybindButton.Text = "...";
                    Keys newKey = await GetKeypress();
                    ChangeKeybind(keybindName, newKey);
                    keybindButton.Text = GameForm.ActiveGameform!.Keymap.keybinds[keybindName].Key.ToString();
                    GameForm.ActiveGameform!.Focus();
                };
                Controls.Add(KeybindLabel);
                Controls.Add(keybindButton);

                currentY += SpacingY;
                repetitions++;

                if (repetitions >= RepeatAfter)
                {
                    repetitions = 0;
                    currentY = StartY;
                    LabelX += OffsetX;
                    ButtonX += OffsetX;
                }
            }

            Label Keymaplbl = new Label()
            {
                Text = "Keymap:",
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Black,
                Location = new Point(LabelX, currentY - 20),
            };
            Keymapddm = new ComboBox()
            {
                Location = new Point(ButtonX, currentY - 20),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
            };
            Keymapddm!.SelectedIndexChanged += (s, e) => 
            { 
                if (dontUpdate) return;
                if (Keymapddm.SelectedItem != null)
                    Global.CONFIGS.LastUsedKeymap = keybindConfigs[Keymapddm.SelectedIndex].Replace(' ', '_') + ".json";
                UpdateControls();
            };

            Controls.Add(Keymaplbl);
            Controls.Add(Keymapddm);

            UpdateControls();
        } 
        private void UpdateControls()
        {
            dontUpdate = true;
            Dictionary<string, Keybind> keymap = GameForm.ActiveGameform!.Keymap.ToDictionary();
            for (int i = 1; i < Controls.Count - 2; i += 2)
            {
                Controls[i].Text = keymap.ElementAt((i - 1) / 2).Value.Key.ToString();
            }

            string KeybindBasePath = Global.DATA_PATH + Global.KEYBIND_PATH_BASE;
            string[] keybindConfigs = Directory.GetFiles(KeybindBasePath);

            for (int i = 0; i < keybindConfigs.Length; i++)
            {
                keybindConfigs[i] = Global.GetFileName(keybindConfigs[i]);
                keybindConfigs[i] = keybindConfigs[i].Replace('_', ' ');
            }

            Keymapddm.Items.Clear();
            Keymapddm.Items.AddRange([.. keybindConfigs]);
            Keymapddm.SelectedItem = Global.GetFileName(Global.CONFIGS.LastUsedKeymap).Replace('_', ' ');

            dontUpdate = false;
        }

        public void ChangeKeybind(string keybind, Keys key)
        {
            GameForm instance = GameForm.ActiveGameform!;
            Keymap keymap = instance.Keymap;
            Dictionary<string, Keybind> dict = keymap.ToDictionary();

            Keybind kb = dict[keybind];
            kb.Key = key;
            dict[keybind] = kb;
               
            keymap.FromDictionary(dict);
            instance.Keymap = keymap;
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

        }
        
        private async Task<Keys> GetKeypress()
        {
            Global.SUPPRESS_OPTION_CHANGED_EVENT = true;
            LastPressedKey = Keys.None;

            int MaxRetries = 50;

            for (int i = 0; i < MaxRetries; i++) 
                if (LastPressedKey == Keys.None)
                    await Task.Delay(100);

            Global.SUPPRESS_OPTION_CHANGED_EVENT = false;
            return (LastPressedKey == Keys.Escape) ? Keys.None: LastPressedKey;
        }
    }
}
