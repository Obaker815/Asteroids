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
        private readonly float lifetime;
        private readonly float impulse;
        private readonly float radius;
        private readonly int count;

        private Stopwatch elapsedTimeSW;
        private readonly float interval;
        private readonly float duration;
        private readonly int maxTriggers;
        private int numTriggers;
        private bool isPlaying;

        private Vector2 position;
        private float sweepAngle;
        private float angle;

        private CancellationTokenSource cts;
        private Task animationTask;

        public Vector2 Position { get { return position; } set { position = value; } }
        public float SweepAngle { get { return sweepAngle; } set { sweepAngle = value; } }
        public float Angle      { get { return angle; } set { angle = value; } }

        public ParticleEffect(Type particleType,
                              Vector2 position,
                              float interval,
                              float lifetime,
                              float impulse,
                              int count = 1,
                              float radius = 0f,
                              float duration = -1f,
                              int maxTriggers = -1,
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
            this.count = count;
            this.radius = radius;
            this.duration = duration;
            this.maxTriggers = maxTriggers;
            this.angle = angle;
            this.sweepAngle = sweepAngle;
            this.angularVelocity = angularVelocity;
            this.lifetimeRange = lifetimeRange;

            elapsedTimeSW = null!;
            animationTask = null!;
            cts = null!;
            numTriggers = 0;
        }

        /// <summary>
        /// Starts the <see cref="ParticleEffect"/> animation.
        /// </summary>
        public void Start()
        {
            if (isPlaying) return;
            cts = new CancellationTokenSource();
            animationTask = Task.Run(() => Animate(cts.Token));
        }

        /// <summary>
        /// Stops the <see cref="ParticleEffect"/> animation.
        /// </summary>
        public void Stop()
        {
            if (!isPlaying) return;
            cts.Cancel();
            animationTask?.Wait();
            animationTask = null!;
        }

        /// <summary>
        /// The animation loop for the <see cref="ParticleEffect"/> class
        /// </summary>
        /// <param name="token">The <see cref="CancellationToken"/> used for the <see cref="Task"/></param>
        private async Task Animate(CancellationToken token)
        {
            float lastEmitTime = 0f;
            elapsedTimeSW = Stopwatch.StartNew();

            while (!token.IsCancellationRequested && (numTriggers < maxTriggers || maxTriggers < 0))
            {
                if (duration > 0f && elapsedTimeSW.Elapsed.TotalSeconds >= duration)
                    break;

                float elapsedSeconds = (float)elapsedTimeSW.Elapsed.TotalSeconds;
                if (elapsedSeconds - lastEmitTime >= interval)
                {
                    lastEmitTime += interval;
                    for (int i = 0; i < count; i++)
                        EmitParticle();
                    numTriggers++;
                }

                await Task.Delay(1, token);
            }

            isPlaying = false;
        }

        /// <summary>
        /// Emits a single <see cref="Particle"/> from the <see cref="ParticleEffect"/>
        /// </summary>
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
