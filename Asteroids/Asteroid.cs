using System.Diagnostics;
using System.Numerics;

namespace Asteroids
{
    internal class Asteroid : Wrapable
    {
        public static List<Asteroid> AsteroidEntities = [];

        private const float MAX_POINT_OFFSET = 1.3f;
        private const float MIN_POINT_OFFSET = 0.7f;
        private const int MAX_ASTEROIDS = 26;
        private const int NUM_POINTS = 10;

        private readonly float angularVelocity;
        private readonly Vector2[] points;
        private readonly int size;

        private readonly static Dictionary<int, (float radius, float speed, float rotateSpeed)> SizePropertyDict = new()
        {
            { 1, (13, 80, 4) },
            { 2, (20, 60, 2) },
            { 3, (30, 50, 1) },
        };

        public int Size { get { return size; } }

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

            if (rnd.Next(0, 2) == 1)
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

            this.angularVelocity = (((float)rnd.NextDouble() * 2) - 1f) * SizePropertyDict[size].rotateSpeed;
            this.size = size;
            this.points = GenShape();

            AsteroidEntities.Add(this);
        }

        /// <summary>
        /// The update procedure for the <see cref="Asteroid"/> class
        /// </summary>
        /// <param name="dt">Deltatime in seconds</param>
        public new void Update(float dt)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = Vector2.Transform(points[i], Matrix3x2.CreateRotation(angularVelocity * dt));
            }

            Entity? collided = CollisionCheck(this, typeof(Bullet));

            if (collided is not null && collided is Bullet)
            {
                Bullet? collidedBullet = collided as Bullet;
                if (collidedBullet?.parent is Ship)
                {
                    if (collidedBullet is not null)
                    {
                        float angle = MathF.Atan2(collidedBullet.Velocity.Y, collidedBullet.Velocity.X);

                        ParticleEffect p = new ParticleEffect(typeof(ParticleDot),
                                                    position: this.position,
                                                    interval: 0.01f,
                                                    lifetime: 0.5f,
                                                    lifetimeRange: (-0.4f, 0.5f),
                                                    maxTriggers: 2,
                                                    impulse: 100,
                                                    count: 10 * size,
                                                    radius: radius,
                                                    angle: angle - float.Pi / 3,
                                                    sweepAngle: float.Pi / 3 * 2);
                        p.Start();
                    }
                    toRemove.Add(this);
                    toRemove.Add(collided);
                }
            }
        }

        /// <summary>
        /// Generates a random <see cref="Asteroid"/> shape using the constants <see cref="MAX_POINT_OFFSET"/>, <see cref="MIN_POINT_OFFSET"/>, and <see cref="NUM_POINTS"/>
        /// </summary>
        /// <returns><see cref="Vector2[]"/> of vectors relative to position</returns>
        public static Vector2[] GenShape()
        {
            List<Vector2> shape = [];

            Random rnd = new();
            float angleIncrement = 2 * float.Pi / NUM_POINTS;

            Vector2 vector = new(1, 0);
            vector = Vector2.Transform(vector, Matrix3x2.CreateRotation(angleIncrement * (float)rnd.NextDouble()));

            for (int i = 0; i < NUM_POINTS; i++)
            {
                Vector2 current = vector;
                vector = Vector2.Transform(vector, Matrix3x2.CreateRotation(angleIncrement));

                current *= (MAX_POINT_OFFSET - MIN_POINT_OFFSET) * (float)rnd.NextDouble() + MIN_POINT_OFFSET;
                shape.Add(current);
            }

            return [.. shape];
        }

        public override void Draw(Graphics g, Vector2 Position)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 start = Position + points[i] * radius;
                Vector2 end = Position + points[(i + 1) % points.Length] * radius;

                g.DrawLine(Pens.White, start.X, start.Y, end.X, end.Y);
            }
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
