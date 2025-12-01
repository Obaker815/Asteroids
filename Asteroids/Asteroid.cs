using System.Numerics;

namespace Asteroids
{
    internal class Asteroid : Wrapable
    {
        private static readonly List<PointF[]> paths = [];
        private static readonly Random random = new();
        public static List<Asteroid> AsteroidEntities = [];
        private static readonly ParticleEffect[] destroyEffects =
            [
                new(
                    particleType: typeof(ParticleLine),
                    position: new(0, 0),
                    args: [20f, 1],
                    interval: 0.01f,
                    lifetime: 2,
                    impulse: 50,
                    count: NUM_POINTS + 1,
                    maxTriggers: 1,
                    angularVelocity: (-6, 6),
                    lifetimeRange: (-0.3f, 0.5f),
                    gradient: [
                        (Color.White, 0f),
                        (Color.White, 0.5f),
                        ]),
                new(
                    particleType: typeof(ParticleDot),
                    position: new(0, 0),
                    args: [],
                    interval: 0.05f,
                    lifetime: 1,
                    impulse: 100,
                    count: 20,
                    maxTriggers: 1,
                    angularVelocity: (0, 0),
                    impulseRange: (-50, 50),
                    lifetimeRange: (-0.3f, 0.5f),
                    gradient: [
                        (Color.White, 0f),
                        (Color.White, 0.5f),
                        ])
                ];

        private const float MAX_POINT_OFFSET = 1.3f;
        private const float MIN_POINT_OFFSET = 0.7f;
        private const int MAX_ASTEROIDS = 26;
        private const int NUM_POINTS = 10;

        private readonly float angularVelocity;
        private readonly int size;
        private Vector2[] points;

        private readonly static Dictionary<int, (float radius, float speed, float rotateSpeed, int score)> SizePropertyDict = new()
        {
            { 1, (13, 80, 4, 100) },
            { 2, (20, 60, 2, 50) },
            { 3, (30, 50, 1, 20) },
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

            float yPos;
            float xPos;

            if (random.Next(0, 2) == 1)
            {
                yPos = (float)random.NextDouble() * screen.Height + screen.Y;
                xPos = 0;
            }
            else
            {
                yPos = 0; 
                xPos = (float)random.NextDouble() * screen.Width + screen.X;
            }

            return new Asteroid(new(xPos, yPos), size, random);
        }

        /// <summary>
        /// Creates three smaller asteroids
        /// </summary>
        private void SpawnChildren()
        {
            for (int i = 0; i < 3; i++)
            {
                if (!SizePropertyDict.ContainsKey(size) || AsteroidEntities.Count >= MAX_ASTEROIDS)
                    break;

                _ = new Asteroid(position, size - 1, random);
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

            this.points = GenShape().Result;
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
                if (collidedBullet?.Parent is Ship)
                {
                    LevelManager.Instance.AddScore(SizePropertyDict[size].score);
                    collidedBullet.Collided = true;
                    toRemove.Add(this);
                }
                toRemove.Add(collided);
            }
        }            

        /// <summary>
        /// Generates a random <see cref="Asteroid"/> shape using the constants <see cref="MAX_POINT_OFFSET"/>, <see cref="MIN_POINT_OFFSET"/>, and <see cref="NUM_POINTS"/>
        /// </summary>
        /// <returns><see cref="Vector2[]"/> of vectors relative to position</returns>
        public static Task<Vector2[]> GenShape()
        {
            List<Vector2> shape = [];

            float angleIncrement = 2 * float.Pi / NUM_POINTS;

            Vector2 vector = new(1, 0);
            vector = Vector2.Transform(vector, Matrix3x2.CreateRotation(angleIncrement * (float)random.NextDouble()));

            for (int i = 0; i < NUM_POINTS; i++)
            {
                Vector2 current = vector;
                vector = Vector2.Transform(vector, Matrix3x2.CreateRotation(angleIncrement));

                current *= (MAX_POINT_OFFSET - MIN_POINT_OFFSET) * (float)random.NextDouble() + MIN_POINT_OFFSET;
                shape.Add(current);
            }

            return Task.FromResult(shape.ToArray());
        }

        public override void Draw(Graphics g, Vector2 Position)
        {
            async void draw()
            {
                points ??= await GenShape();
                List<PointF> path = [];

                for (int i = 0; i <= points.Length; i++)
                {
                    Vector2 point = Position + points[i % points.Length] * radius;
                    path.Add(new(point.X, point.Y));
                }

                paths.Add([.. path]);
            }
            draw();
        }

        public static void FinalDraw(Graphics g)
        {
            foreach (PointF[] path in paths)
                g.FillPolygon(Brushes.Black, path);
            
            foreach (PointF[] path in paths)
                g.DrawPolygon(Pens.White, path);

            paths.Clear();
        }

        public override void Remove()
        {
            AsteroidEntities.Remove(this);

            ParticleEffect[] pe = [.. destroyEffects];
            foreach (ParticleEffect e in destroyEffects)
            {
                e.Position = this.position;
                if (e.Args.Length == 2)
                    e.Args = [(float)(this.size * 8), (int)1];
                e.Radius = this.radius;
                GameForm.ActiveGameform?.InvokeAction(e.Start);
            }

            if (size > 1)
                SpawnChildren();

            base.Remove();
        }
    }
}
