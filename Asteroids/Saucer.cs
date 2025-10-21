using System.Numerics;

namespace Asteroids
{
    internal class Saucer : Wrapable
    {
        public static List<Saucer> Saucers = new();
        public Saucer(Vector2 startPosition) : base(startPosition)
        {
            Saucers.Add(this);
        }

        public override void Draw(Graphics g, Vector2 position)
        {
            // Define the saucer gridMap
            float scale = 3f;
            Vector2[,] Map = new Vector2[4, 14];
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                for (int j = 0; j < Map.GetLength(1); j++)
                {
                    Map[i, j] = new Vector2(j * scale, i * scale);
                    Map[i, j] -= new Vector2(Map.GetLength(1) * scale / 2f, Map.GetLength(0) * scale / 2f);
                    Map[i, j] += position;
                }
            }
            
            // Make an action to draw a line
            Action<Pen, Vector2, Vector2> DrawLine = (Pen p, Vector2 v1, Vector2 v2) =>
            {
                g.DrawLine(p, v1.X, v1.Y, v2.X, v2.Y);
            };

            // Make an action to draw the saucer
            Action<Color, int> DrawSaucer = (Color c, int thickness) =>
            {
                Pen pen = new(c, thickness);

                // Draw layer 0
                DrawLine(pen, Map[0, 6], Map[0, 8]);

                // Draw layer 0 to layer 1
                DrawLine(pen, Map[0, 6], Map[1, 6]);
                DrawLine(pen, Map[0, 8], Map[1, 8]);

                // Draw layer 1
                DrawLine(pen, Map[1, 5], Map[1, 9]);

                // Draw layer 1 to layer 2
                DrawLine(pen, Map[1, 5], Map[2, 4]);
                DrawLine(pen, Map[1, 9], Map[2, 10]);

                // Draw layer 2
                DrawLine(pen, Map[2, 3], Map[2, 11]);

                // Draw layer 2 to layer 3
                DrawLine(pen, Map[2, 4], Map[3, 5]);
                DrawLine(pen, Map[2, 10], Map[3, 9]);

                // Draw layer 3
                DrawLine(pen, Map[3, 5], Map[3, 9]);
            };

            // Draw the saucer in white with thickness 1
            DrawSaucer(Color.White, 1);
        }
    }
}
