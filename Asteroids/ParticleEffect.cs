using System.Diagnostics;
using System.Numerics;

namespace Asteroids
{
    internal class ParticleEffect
    {
        private static readonly ThreadLocal<Random> random = new(() => new Random());

        private (float Min, float Max) lifetimeRange;
        private (float Min, float Max) angularVelocity;
        private readonly Type particleType;
        private readonly Vector2 position;
        private readonly float lifetime;
        private readonly float impulse;
        private readonly float radius;
        private readonly int count;

        private Stopwatch elapsedTimeSW;
        private readonly float interval;
        private readonly float duration;
        private bool isPlaying;

        private readonly float sweepAngle;
        private readonly float angle;

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

        private CancellationTokenSource cts;
        public void Start()
        {
            if (isPlaying) return;
            cts = new CancellationTokenSource();
            animationTask = Task.Run(() => Animate(cts.Token));
        }

        public void Stop()
        {
            if (!isPlaying) return;
            cts.Cancel();
            animationTask?.Wait();
            animationTask = null;
        }


        private async Task Animate(CancellationToken token)
        {
            if (isPlaying) return;
            isPlaying = true;

            float lastEmitTime = 0f;
            elapsedTimeSW = Stopwatch.StartNew();

            while (!token.IsCancellationRequested)
            {
                if (duration > 0f && elapsedTimeSW.Elapsed.TotalSeconds >= duration)
                    break;

                float elapsedSeconds = (float)elapsedTimeSW.Elapsed.TotalSeconds;
                if (elapsedSeconds - lastEmitTime >= interval)
                {
                    lastEmitTime += interval;
                    for (int i = 0; i < count; i++)
                        EmitParticle();
                }

                await Task.Delay(1, token);
            }

            isPlaying = false;
        }


        private void EmitParticle()
        {
            var rnd = random.Value!;

            float angleOffset = (float)rnd.NextDouble() * sweepAngle;
            float emitAngle = angle + angleOffset;

            Vector2 velocity = Vector2.Transform(
                new(impulse, 0), 
                Matrix3x2.CreateRotation(emitAngle)
                );

            Vector2 emitPosition = position + Vector2.Transform(
                new((float)rnd.NextDouble() * radius, 0), 
                Matrix3x2.CreateRotation((float)rnd.NextDouble() * 2*float.Pi)
                );

            float angularVel = (float)rnd.NextDouble() * (angularVelocity.Max - angularVelocity.Min) + angularVelocity.Min;
            float lifetime = this.lifetime + (float)rnd.NextDouble() * (lifetimeRange.Max - lifetimeRange.Min) + lifetimeRange.Min;

            _ = (Particle)Activator.CreateInstance(particleType, emitPosition, velocity, angularVel, lifetime)!;
        }
    }
}
