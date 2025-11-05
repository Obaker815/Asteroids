using System.Diagnostics;
using System.Numerics;

namespace Asteroids
{
    internal class ParticleEffect
    {
        private static readonly Random random = new Random();
        private readonly int count;

        private (float Min, float Max) lifetimeRange;
        private (float Min, float Max) angularVelocity;
        private Type particleType;
        private Vector2 position;
        private float lifetime;
        private float impulse;
        private float radius;

        private Stopwatch elapsedTimeSW;
        private float interval;
        private float duration;
        private bool isPlaying;

        private float sweepAngle;
        private float angle;

        private Task animationTask;

        /// <summary>
        /// Creates a new particle effect with configurable emission pattern and lifespan.
        /// </summary>
        /// <param name="Position">The center position where particles originate.</param>
        /// <param name="Impulse">The initial speed or force applied to each particle.</param>
        /// <param name="Count">Number of particles to emit.</param>
        /// <param name="Radius">Optional spawn radius around the center position.</param>
        /// <param name="Duration">Duration (in seconds) before the particles expire. Use -1 for infinite.</param>
        /// <param name="Angle">Starting angle (in radians) of the emission arc.</param>
        /// <param name="SweepAngle">Total angular range (in radians) for particle emission.</param>
        /// <param name="AngularVelocity">Optional random angular velocity range (Min, Max) in radians per second.</param>
        public ParticleEffect(
            Type particleType,
            Vector2 position,
            float interval,
            float lifetime,
            float impulse,
            int count = 1,
            float radius = 0f,
            float duration = -1f,
            float angle = 0f,
            float sweepAngle = 2 * float.Pi,
            (float Min, float Max) angularVelocity = default,
            (float Min, float Max) lifetimeRange = default)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
            if (interval <= 0)
                throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than zero.");
            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius cannot be negative.");

            this.particleType = particleType;
            this.position = position;
            this.interval = interval;
            this.lifetime = lifetime;
            this.impulse = impulse;
            this.radius = radius;
            this.duration = duration;
            this.angle = angle;
            this.sweepAngle = sweepAngle;
            this.angularVelocity = angularVelocity;
            this.lifetimeRange = lifetimeRange;
        }

        public void Start()
        {
            animationTask = Task.Run(Animate);
        }
        public void Stop()
        {
            isPlaying = false;
            animationTask.Dispose();
        }

        private void Animate()
        {
            if (isPlaying)
                return;
            isPlaying = true;

            float lastEmitTime = 0f;
            elapsedTimeSW = Stopwatch.StartNew();
            while (isPlaying)
            {
                if (duration > 0f && elapsedTimeSW.Elapsed.TotalSeconds >= duration)
                {
                    isPlaying = false;
                    return;
                }
                float elapsedSeconds = (float)elapsedTimeSW.Elapsed.TotalSeconds;

                if (elapsedSeconds - lastEmitTime >= interval)
                {
                    lastEmitTime += interval;
                    for (int i = 0; i < count; i++)
                    {
                        EmitParticle();
                    }
                }
            }
        }

        private void EmitParticle()
        {
            float angleOffset = (float)random.NextDouble() * sweepAngle;
            float emitAngle = angle + angleOffset;

            Vector2 velocity = Vector2.Transform(
                new(impulse, 0), 
                Matrix3x2.CreateRotation(emitAngle)
                );

            Vector2 emitPosition = position + Vector2.Transform(
                new((float)random.NextDouble() * radius, 0), 
                Matrix3x2.CreateRotation((float)random.NextDouble() * 2*float.Pi)
                );

            float angularVel = (float)random.NextDouble() * (angularVelocity.Max - angularVelocity.Min) + angularVelocity.Min;
            _ = (Particle)Activator.CreateInstance(particleType, emitPosition, velocity, lifetime)!;
        }
    }
}
