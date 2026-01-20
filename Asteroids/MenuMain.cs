using System.Numerics;

namespace Asteroids
{
    internal class MenuMain : IMenu
    {
        public List<Control> Controls { get; set; } = [];

        private static readonly int fontSize = 15;
        public static Scoreboard Scoreboard = new();

        private static Rectangle scoreboardRectangele = new()
        {
            Height = GameForm.preferredRect.Height / 2,
            Width = GameForm.preferredRect.Width,

            Y = GameForm.preferredRect.Height / 5,
            X = 0,
        };

        public static Dictionary<string, Keybind> MenuKeys = new()
        {
            {"Start", new(Keys.Space) },
            {"Settings", new(Keys.F10) }
        };

        public MenuMain()
        {
            Controls.Add(new Label()
            {
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font(GameForm.PublicFonts!.Families[0], 20),
                Location = new Point(10, 10),
                Text = $"Exit : Esc \nOptions : {MenuKeys["Settings"].Key.ToString()}".ToUpper()
            });
        }

        public void Draw(Graphics g)
        {
            // Format for the header
            using StringFormat headerFormat = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };

            // Height and font of each entry on the scoreboard
            float height = scoreboardRectangele.Height / (Scoreboard.Entries!.Length + 1);
            Font font = new(GameForm.PublicFonts!.Families[0], fontSize);

            string[] lines = Scoreboard.Entries.Select(e => 
                                      (e.Name + "     ")[0.. 5].ToUpper() + 
                                      " ---------------- " + 
                                      e.Score.ToString("D8"))
                                      .ToArray();

            // Maximum number of characters in any line
            int maxChars;
            if (lines.Length != 0)
                maxChars = Math.Max(1, lines.Max(s => s.Length));
            else
                maxChars = 29;
            var measureFormat = StringFormat.GenericTypographic;

            // Measurements per character
            var charSizeCache = new Dictionary<char, SizeF>();
            SizeF MeasureChar(char c)
            {
                if (!charSizeCache.TryGetValue(c, out var size))
                {
                    size = g.MeasureString(c.ToString(), font, PointF.Empty, measureFormat);
                    charSizeCache[c] = size;
                }
                return size;
            }

            // Get max glyph width
            float maxGlyphWidth = 0f;
            foreach (var line in lines)
                foreach (char ch in line)
                    maxGlyphWidth = Math.Max(maxGlyphWidth, MeasureChar(ch).Width);

            // Base case
            if (maxGlyphWidth <= 0f) maxGlyphWidth = g.MeasureString("W", font, PointF.Empty, measureFormat).Width;

            // Padding
            const float perSlotPadding = 0f;
            float slotWidth = maxGlyphWidth + perSlotPadding;

            // Total width of the monospaced block and left start to center it
            float totalWidth = slotWidth * maxChars;
            float blockLeft = scoreboardRectangele.Left + (scoreboardRectangele.Width - totalWidth) / 2f;

            // font height for vertical centering in each row
            float fontHeight = g.MeasureString("Hg", font, PointF.Empty, measureFormat).Height;

            // Draw header (row 0) centered normally
            float headerY = scoreboardRectangele.Top + height * 0;
            g.DrawString("HIGH SCORES", font, Brushes.White,
                         new RectangleF(scoreboardRectangele.Left, headerY, scoreboardRectangele.Width, fontHeight * 1.3f),
                         headerFormat);

            // Draw monospaced rows for each scoreboard entry
            for (int i = 0; i < lines.Length; i++)
            {
                string entry = lines[i];
                float yRowTop = scoreboardRectangele.Top + height * (i + 1); // +1 because row 0 was header

                // vertical center of characters inside the row
                float yChar = yRowTop + (height - fontHeight) / 2f;

                // draw each character into its slot
                for (int c = 0; c < maxChars; c++)
                {
                    char ch = c < entry.Length ? entry[c] : ' '; // empty slots get a space

                    float slotLeft = blockLeft + c * slotWidth;

                    // measure (cached) and center the char inside the slot
                    SizeF charSize = MeasureChar(ch);
                    float xChar = slotLeft + (slotWidth - charSize.Width) / 2f;

                    // Draw the single character (using GenericTypographic for consistent metrics)
                    g.DrawString(ch.ToString(), font, Brushes.White, new PointF(xChar, yChar), measureFormat);
                }
            }

            // Draw start text
            float StartTextY = scoreboardRectangele.Bottom + height * 1;
            g.DrawString($"-- {MenuKeys["Start"].Key.ToString()} to start game --".ToUpper(), font, Brushes.White,
                         new RectangleF(scoreboardRectangele.Left, StartTextY, scoreboardRectangele.Width, fontHeight * 1.3f),
                         headerFormat);
        }

        public void Update()
        {
            if (MenuKeys["Start"].FirstPress)    Global.CURRENT_STATE = GameState.Playing;
            if (MenuKeys["Settings"].FirstPress) Global.CURRENT_STATE = GameState.SettingsMenu;

            foreach (string Key in MenuKeys.Keys)
            {
                Keybind kb = MenuKeys[Key];
                if (kb.FirstPress)
                {
                    kb.FirstPress = false;
                }
                MenuKeys[Key] = kb;
            }
        }
    }
}
