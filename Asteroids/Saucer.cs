using System.Drawing;
using System.Numerics;
using Timer = System.Windows.Forms.Timer;

namespace Asteroids
{
    internal class Saucer : Wrapable
    {
        public static List<Saucer> Saucers = new();
        private Vector2 shootDir;
        private float speed;
        private int shootMS;

        public Saucer(bool isSmall, Vector2 startPosition) : base(startPosition)
        {
            Saucers.Add(this);
            shootDir = new Vector2(1,0);

            if (isSmall)
            {
                speed = 125;
                radius = 7;
                shootMS = 1000;
            }
            else
            {
                speed = 75;
                radius = 10;
                shootMS = 1500;
            }

            Timer ShootTimer = new Timer();
            ShootTimer.Interval = shootMS;
            ShootTimer.Tick += (sender, e) =>
            {
                new Bullet(position, Vector2.Zero, shootDir, 100, this);
            };
            ShootTimer.Start();
        }

        public void Updates(float dt)
        {
            Vector2 closest = new Vector2(100000, 100000);
            foreach (Ship s in Ship.Ships)
            {
                Vector2 pos = s.GetClosest(base.position);
                Vector2 contender = pos - base.position;
                float distance = contender.LengthSquared();
                if (distance < closest.LengthSquared())
                {
                    closest = contender;
                }
            }
            shootDir = Global.Normalize(closest);

            velocity = new Vector2(speed, shootDir.Y * speed / 2);

        }

        public override void Draw(Graphics g, Vector2 position)
        {
            // Make an action to draw a line
            Action<Pen, Vector2, Vector2> DrawLine = (Pen p, Vector2 v1, Vector2 v2) =>
            {
                g.DrawLine(p, v1.X, v1.Y, v2.X, v2.Y);
            };

            float radius = this.radius / 1.2f;
            position.Y += this.radius * 0.6f;

            // Make an action to draw the saucer
            Action<Color, int> DrawSaucer = (Color c, int thickness) =>
            {
                Pen pen = new(c, thickness);

                // Draw the top of the saucer
                g.DrawArc(pen, position.X - radius, position.Y - 2*radius, 2*radius, 2*radius, 180, 180);

                // Draw the top level of the saucer
                DrawLine(pen, new Vector2(position.X - radius, position.Y - radius), new Vector2(position.X + radius, position.Y - radius));

                float layerTwoWidth = 2f;
                float layerTwoHeight = 0.3f;
                float layerThreeHeight = -0.4f;

                // Draw the connecting parts of the top - second level of the saucer
                DrawLine(pen, new Vector2(position.X - radius, position.Y - radius), new Vector2(position.X - radius * layerTwoWidth, position.Y - radius * layerTwoHeight));
                DrawLine(pen, new Vector2(position.X + radius * layerTwoWidth, position.Y - radius * layerTwoHeight), new Vector2(position.X + radius, position.Y - radius));

                // Draw the second level of the saucer
                DrawLine(pen, new Vector2(position.X + radius * layerTwoWidth, position.Y - radius * layerTwoHeight), new Vector2(position.X - radius * layerTwoWidth, position.Y - radius * layerTwoHeight));


                // Draw the connecting parts of the second - third level of the saucer
                DrawLine(pen, new Vector2(position.X - radius, position.Y - radius * layerThreeHeight), new Vector2(position.X - radius * layerTwoWidth, position.Y - radius * layerTwoHeight));
                DrawLine(pen, new Vector2(position.X + radius * layerTwoWidth, position.Y - radius * layerTwoHeight), new Vector2(position.X + radius, position.Y - radius * layerThreeHeight));

                // Draw the third level of the saucer
                DrawLine(pen, new Vector2(position.X - radius, position.Y - radius * layerThreeHeight), new Vector2(position.X + radius, position.Y - radius * layerThreeHeight));
            };

            // Draw the saucer in white with thickness 1
            DrawSaucer(Color.White, 1);

            if (Global.DEBUG)
            {
                Vector2 tmpPosition = base.position + new Vector2(Bounds.X, Bounds.Y);

                g.DrawLine(Pens.Red, tmpPosition.X, tmpPosition.Y, 
                    tmpPosition.X + shootDir.X * radius * Global.DEBUG_DIRECTION_LINE_LENGTH, 
                    tmpPosition.Y + shootDir.Y * radius * Global.DEBUG_DIRECTION_LINE_LENGTH);
            }
        }
        public override void Remove()
        {
            base.Remove();
            Saucers.Remove(this);
        }
    }
}
