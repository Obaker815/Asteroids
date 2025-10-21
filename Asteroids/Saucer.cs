using System.Drawing;
using System.Numerics;

namespace Asteroids
{
    internal class Saucer : Wrapable
    {
        public static List<Saucer> Saucers = new();
        public Saucer(Vector2 startPosition) : base(startPosition)
        {
            Saucers.Add(this);
            this.radius = 12;
        }

        public override void Draw(Graphics g, Vector2 position)
        {
            // Make an action to draw a line
            Action<Pen, Vector2, Vector2> DrawLine = (Pen p, Vector2 v1, Vector2 v2) =>
            {
                g.DrawLine(p, v1.X, v1.Y, v2.X, v2.Y);
            };

            float radius = this.radius / 1.2f;
            position.Y += this.radius * 1.5f;
            // Make an action to draw the saucer
            Action<Color, int> DrawSaucer = (Color c, int thickness) =>
            {
                Pen pen = new(c, thickness);

                // Draws a quarter-circle arc
                g.DrawArc(pen, position.X - radius, position.Y - 2*radius, 2*radius, 2*radius, 180, 180);

                DrawLine(pen, new Vector2(position.X - radius, position.Y - radius), new Vector2(position.X + radius, position.Y - radius));

                float layerTwoWidth = 2f;
                float layerTwoHeight = 0.3f;
                float layerThreeHeight = -0.4f;

                DrawLine(pen, new Vector2(position.X - radius, position.Y - radius), new Vector2(position.X - radius * layerTwoWidth, position.Y - radius * layerTwoHeight));
                DrawLine(pen, new Vector2(position.X + radius * layerTwoWidth, position.Y - radius * layerTwoHeight), new Vector2(position.X + radius, position.Y - radius));

                DrawLine(pen, new Vector2(position.X + radius * layerTwoWidth, position.Y - radius * layerTwoHeight), new Vector2(position.X - radius * layerTwoWidth, position.Y - radius * layerTwoHeight));

                DrawLine(pen, new Vector2(position.X - radius, position.Y - radius * layerThreeHeight), new Vector2(position.X - radius * layerTwoWidth, position.Y - radius * layerTwoHeight));
                DrawLine(pen, new Vector2(position.X + radius * layerTwoWidth, position.Y - radius * layerTwoHeight), new Vector2(position.X + radius, position.Y - radius * layerThreeHeight));

                DrawLine(pen, new Vector2(position.X - radius, position.Y - radius * layerThreeHeight), new Vector2(position.X + radius, position.Y - radius * layerThreeHeight));

                g.DrawEllipse(pen, this.position.X - this.radius, this.position.Y, this.radius * 2f, this.radius * 2f);
            };

            // Draw the saucer in white with thickness 1
            DrawSaucer(Color.White, 1);
        }
    }
}
