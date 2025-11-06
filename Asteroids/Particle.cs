using System.Configuration;
using System.Numerics;

namespace Asteroids
{
    internal class Particle
    {
        public static List<Particle> Particles = [];
        public static List<Particle> ToRemove = [];

        private readonly float angularVelocity;
        private readonly Vector2 velocity;

        internal readonly (Color color, float t)[] gradient;
        internal readonly float lifetime;
        internal Vector2 position;
        internal float rotation;
        internal float age;

        /// <summary>
        /// The constructor for the <see cref="Particle"/> class
        /// </summary>
        /// <param name="Position">The <see cref="Vector2"/> position for the <see cref="Particle"/> to start</param>
        /// <param name="Velocity">The <see cref="Vector2"/> velocity for the <see cref="Particle"/> to have</param>
        /// <param name="AngularVelocity">The <see cref="float"/> angular velocity for the <see cref="Particle"/> to have</param>
        /// <param name="Lifetime">The <see cref="float"/> lifetime of the <see cref="Particle"/></param>
        protected Particle(Vector2 position, Vector2 velocity, float angularVelocity, float lifetime, float rotation, (Color color, float t)[] gradient)
        {
            this.angularVelocity = angularVelocity;
            this.position = position;
            this.velocity = velocity;
            this.lifetime = lifetime;
            this.gradient = gradient;
            this.rotation = rotation;

            age = 0;

            Particles.Add(this);
        }

        /// <summary>
        /// The constructor for the <see cref="Particle"/> class
        /// </summary>
        /// <param name="Position">The <see cref="Vector2"/> position for the <see cref="Particle"/> to start</param>
        /// <param name="Velocity">The <see cref="Vector2"/> velocity for the <see cref="Particle"/> to have</param>
        /// <param name="AngularVelocity">The <see cref="(float Min, float Max)"/> angular velocity for the <see cref="Particle"/> to have</param>
        /// <param name="Lifetime">The <see cref="float"/> lifetime of the <see cref="Particle"/></param>
        protected Particle(Vector2 position, Vector2 velocity, (float Min, float Max) angularVelocity, float lifetime, float rotation, (Color color, float t)[] gradient)
        {
            this.angularVelocity = (float)new Random().NextDouble() * (angularVelocity.Max - angularVelocity.Min) + angularVelocity.Min;
            this.position = position;
            this.velocity = velocity;
            this.lifetime = lifetime;
            this.gradient = gradient;
            this.rotation = rotation;

            age = 0;

            Particles.Add(this);
        }

        /// <summary>
        /// The update method for the <see cref="Particle"/> class
        /// </summary>
        /// <param name="dt">Deltatime in seconds</param>
        public void Update(float dt)
        {
            rotation += angularVelocity * dt;
            position += velocity * dt;
            age += dt;

            if (age >= lifetime && lifetime > 0)
            {
                Remove();
            }
        }

        /// <summary>
        /// The draw method for the <see cref="Particle"/> class
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object to draw the <see cref="Particle"/> to</param>
        /// <exception cref="NotImplementedException">Thrown when the method is not implemented in the derived class</exception>
        public virtual void Draw(Graphics g)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Draws all the particles
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object to draw the <see cref="Particle"/>(s) to</param>
        public static void DrawAll(Graphics g)
        {
            Particle[] particles = [.. Particle.Particles];
            foreach (Particle p in particles)
            {
                p?.Draw(g);
            }
        }
        /// <summary>
        /// Get the <see cref="Color"/> of the <see cref="gradient"/> at the <paramref name="t"/> value
        /// </summary>
        /// <param name="t">Time: [0, 1]</param>
        /// <returns>The <see cref="Color"/> at the <paramref name="t"/> value in the <see cref="gradient"/> </returns>
        public Color GetColor(float t)
        {
            if (gradient == null || gradient.Length == 0)
                return Color.White;

            // Ensure gradient covers full [0,1] range with transparency
            List<(Color color, float t)> list = [.. gradient];
            if (list[0].t > 0f)
                list.Insert(0, (Color.FromArgb(0, list[0].color), 0f));
            if (list[^1].t < 1f)
                list.Add((Color.FromArgb(0, list[^1].color), 1f));

            t = Math.Clamp(t, 0f, 1f);

            // Find segment around t
            (Color c1, float t1) = list[0];
            (Color c2, float t2) = list[^1];
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (t >= list[i].t && t <= list[i + 1].t)
                {
                    c1 = list[i].color;
                    t1 = list[i].t;
                    c2 = list[i + 1].color;
                    t2 = list[i + 1].t;
                    break;
                }
            }

            // Linear interpolate color channels
            float lerpT = (t - t1) / Math.Max(t2 - t1, 0.0001f);
            byte r = (byte)Global.Lerp(c1.R, c2.R, lerpT);
            byte g = (byte)Global.Lerp(c1.G, c2.G, lerpT);
            byte b = (byte)Global.Lerp(c1.B, c2.B, lerpT);
            byte a = (byte)Global.Lerp(c1.A, c2.A, lerpT);

            return Color.FromArgb(a, r, g, b);
        }


        /// <summary>
        /// Removes all <see cref="Particle"/>s in the <see cref="ToRemove"/> list from the <see cref="Particles"/> list
        /// </summary>
        public static void RemoveAll()
        {
            foreach (Particle p in ToRemove)
            {
                try
                {
                    Particles.Remove(p);
                }
                catch
                {
                    continue;
                }
            }
            ToRemove.Clear();
        }
        public virtual void Remove()
        {
            ToRemove.Add(this);
        }
    }
}
