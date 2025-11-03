using System.Diagnostics;
using System.Numerics;

namespace Asteroids
{
    internal class Asteroid : Wrapable
    {
        public static List<Asteroid> AsteroidEntities = [];

        private const int MAX_ASTEROIDS = 26;

        private readonly int size;
        private Vector2[] points = [];

        private readonly static Dictionary<int, (float radius, float speed)> SizePropertyDict = new()
        {
            { 1, (10, 80) },
            { 2, (20, 60) },
            { 3, (30, 50) },
        };

        /// <summary>
        /// Creates a new <see cref="Asteroid"/> at a <see cref="Random"/> position on the boundry of the <see cref="Rectangle"/>
        /// </summary>
        /// <param name="screen">The <see cref="Rectangle"/> for the <see cref="Asteroid"/> to spawn on</param>
        /// <returns>The created <see cref="Asteroid"/></returns>
        public static Asteroid? NewAsteroid(Rectangle screen, int size)
        {
            if (!SizePropertyDict.ContainsKey(size) || AsteroidEntities.Count >= MAX_ASTEROIDS)
                return null;

            Random rnd = new();

            float yPos;
            float xPos;

            if (rnd.Next(0, 2) == 0)
            {
                yPos = (float)rnd.NextDouble() * screen.Height + screen.Y;
                xPos = 0;
            }
            else
            {
                yPos = 0; 
                xPos = (float)rnd.NextDouble() * screen.Width + screen.X;
            }

            return new Asteroid(new(xPos, yPos), size, rnd);
        }

        /// <summary>
        /// Creates three smaller asteroids
        /// </summary>
        private void SpawnChildren()
        {
            Random rnd = new();

            for (int i = 0; i < 3; i++)
            {
                if (!SizePropertyDict.ContainsKey(size) || AsteroidEntities.Count >= MAX_ASTEROIDS)
                    break;

                _ = new Asteroid(position, size - 1, rnd);
            }
        }

        /// <summary>
        /// The constructor for <see cref="Asteroid"/>
        /// </summary>
        /// <param name="startPosition">The <see cref="Vector2"/> position for the <see cref="Asteroid"/> to spawn</param>
        /// <param name="rnd">The <see cref="Random"/> used for velocity angle</param>
        public Asteroid(Vector2 startPosition, int size, Random rnd) : base(startPosition)
        {
            float velocityAngle = 2 * (float)rnd.NextDouble() * float.Pi;
            base.velocity = Vector2.Transform(new(1, 0), Matrix3x2.CreateRotation(velocityAngle)) * SizePropertyDict[size].speed;
            base.radius = SizePropertyDict[size].radius;

            this.size = size;

            AsteroidEntities.Add(this);
        }

        public void Update()
        {
            Entity? collided = CollisionCheck(this, typeof(Bullet));

            if (collided is not null && collided is Bullet)
            {
                Bullet? collidedBullet = collided as Bullet;
                if (collidedBullet?.parent is Ship)
                {
                    toRemove.Add(this);
                    toRemove.Add(collided);
                }
            }
        }

        public override void Draw(Graphics g, Vector2 Position)
        {
            g.FillEllipse(Brushes.Blue, new RectangleF(Position.X - radius, Position.Y - radius, 2 * radius, 2 * radius));
        }

        public override void Remove()
        {
            AsteroidEntities.Remove(this);

            if (size > 1)
                SpawnChildren();

            base.Remove();
        }
    }
}
