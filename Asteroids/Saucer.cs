using System.Numerics;

namespace Asteroids
{
    internal class Saucer : Wrapable
    {
        public static List<Saucer> Saucers = [];

        private readonly float ShootInterval;
        private readonly float speed;

        private Vector2 shootDir;
        private float shootTime;

        /// <summary>
        /// The constructor for the <see cref="Saucer"/> class
        /// </summary>
        /// <param name="isSmall">Is the saucer small?</param>
        /// <param name="startPosition">The position for the saucer to spawn</param>
        public Saucer(bool isSmall, Vector2 startPosition) : base(startPosition)
        {
            Saucers.Add(this);
            shootDir = new Vector2(1,0);

            if (isSmall)
            {
                speed = 125;
                radius = 8f;
                ShootInterval = 1;
            }
            else
            {
                speed = 75;
                radius = 12;
                ShootInterval = 1.5f;
            }
        }

        /// <summary>
        /// The update method for the <see cref="Saucer"/> class
        /// </summary>
        public new void Update(float dt)
        {
            shootTime += dt;

            Vector2 closest = new(100000, 100000);
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

            if (shootTime >= ShootInterval)
            {
                shootTime -= ShootInterval;
                _ = new Bullet(position, Vector2.Zero, shootDir, 200, 2000, this);
            };

            velocity = new Vector2(speed, shootDir.Y * speed / 2);

            Entity? collided = CollisionCheck(this, typeof(Bullet));
            if (collided is not null)
            {
                Bullet? b = collided as Bullet;
                if (b?.parent is Ship)
                {
                    b.collided = true;
                    toRemove.Add(collided);
                    toRemove.Add(this);
                }
            }
        }

        /// <summary>
        /// The overridden draw method for the <see cref="Saucer"/> class
        /// </summary>
        /// <param name="g"><see cref="Graphics"/> object to be drawn to</param>
        /// <param name="position">The <see cref="Vector2"/> position to draw a saucer</param>
        public override void Draw(Graphics g, Vector2 position)
        {
            // Make an action to draw a line
            void DrawLine(Pen p, Vector2 v1, Vector2 v2)
            {
                g.DrawLine(p, v1.X, v1.Y, v2.X, v2.Y);
            }

            float radius = this.radius;
            position.Y += this.radius * 0.6f;

            // Make an action to draw the saucer
            void DrawSaucer(Color c, int thickness)
            {
                Pen pen = new(c, thickness);

                // Draw the top of the saucer
                g.DrawArc(pen, position.X - radius, position.Y - 2 * radius, 2 * radius, 2 * radius, 180, 180);

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
            }

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
