using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids
{
    internal class MenuName : IMenu
    {
        public List<Control> Controls { get; set; } = [];

        public MenuName()
        {
            TextBox NameInput;
            Button Submitbtn;

            int Xpos = 60;
            int Ypos = Xpos - 10;

            Controls.Add(new Label()
            {
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font(GameForm.PublicFonts!.Families[0], 20),
                Location = new Point(Xpos, Ypos),
                Text = "Please enter a name:"
            });
            Controls.Add(NameInput = new()
            {
                Location = new Point(Xpos, Ypos + 45),
                Width = 200,
                Font = new Font(GameForm.PublicFonts!.Families[0], 12),
                BackColor = Color.White,
            });
            Controls.Add(Submitbtn = new Button()
            {
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Gray,
                Font = new Font(GameForm.PublicFonts!.Families[0], 12),
                Location = new Point(Xpos, Ypos + 90),
                Text = "Submit",
                Padding = new Padding(5),
            });
            Submitbtn.Click += (s, e) =>
            {
                string name = NameInput.Text.Trim();
                if (string.IsNullOrEmpty(name)) name = "idiot";
                if (name.Length > 5) name = name[0..5];
                if (name.Length < 5) name = name.PadRight(5, ' ');
                name = name.ToUpper();

                Scoreboard scoreboard = MenuMain.Scoreboard;

                (bool exists, int index) = (false, 0);
                for (int i = 0; i < scoreboard.Entries!.Length; i++)
                {
                    if (scoreboard.Entries[i].Name == name)
                    {
                        exists = true;
                        index = i;
                        break;
                    }
                }

                if (exists)
                {
                    scoreboard.Entries![index].Score = (int)Math.Max(
                        scoreboard.Entries[index].Score,
                        LevelManager.Instance.Score);
                }
                else
                {
                    ScoreboardEntry newEntry = new()
                    {
                        Name = name,
                        Score = (int)LevelManager.Instance.Score,
                    };

                    scoreboard.Entries = [.. scoreboard.Entries!.Append(newEntry)];
                }

                scoreboard.SortEntries();
                Global.CURRENT_STATE = GameState.MainMenu;
            };

        } 

        public void Draw(Graphics g)
        {
        }

        public void Update()
        {
        }
    }
}
