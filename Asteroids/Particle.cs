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
        private readonly float lifetime;

        internal Vector2 position;
        internal float rotation;
        private float age;

        /// <summary>
        /// The constructor for the <see cref="Particle"/> class
        /// </summary>
        /// <param name="Position">The <see cref="Vector2"/> position for the <see cref="Particle"/> to start</param>
        /// <param name="Velocity">The <see cref="Vector2"/> velocity for the <see cref="Particle"/> to have</param>
        /// <param name="AngularVelocity">The <see cref="float"/> angular velocity for the <see cref="Particle"/> to have</param>
        /// <param name="Lifetime">The <see cref="float"/> lifetime of the <see cref="Particle"/></param>
        protected Particle(Vector2 Position, Vector2 Velocity, float AngularVelocity, float Lifetime)
        {
            angularVelocity = AngularVelocity;
            position = Position;
            velocity = Velocity;
            lifetime = Lifetime;

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
        protected Particle(Vector2 Position, Vector2 Velocity, (float Min, float Max) AngularVelocity, float Lifetime)
        {
            angularVelocity = (float)new Random().NextDouble() * (AngularVelocity.Max - AngularVelocity.Min) + AngularVelocity.Min;
            position = Position;
            velocity = Velocity;
            lifetime = Lifetime;

            rotation = 0;
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

            if (age >= lifetime)
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
                p.Draw(g);
            }
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
