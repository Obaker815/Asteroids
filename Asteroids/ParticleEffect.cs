using System.Diagnostics;
using System.Numerics;

namespace Asteroids
{
    internal class ParticleEffect
    {
        private static readonly List<ParticleEffect> ParticleEffects = [];
        private static readonly ThreadLocal<Random> random = new(() => new Random());

        private readonly (Color color, float t)[] gradient;
        private (float Min, float Max) angularVelocity;
        private (float Min, float Max) lifetimeRange;
        private readonly Type particleType;
        private readonly float lifetime;
        private readonly float impulse;
        private readonly int count;

        private Stopwatch elapsedTimeSW;
        private readonly float interval;
        private readonly float duration;
        private readonly int maxTriggers;
        private int numTriggers;
        private bool isPlaying;

        private readonly object[] args;
        private Vector2 position;
        private float sweepAngle;
        private float radius;
        private float angle;

        private Task animationTask;

        public Vector2 Position { get { return position; } set { position = value; } }
        public float SweepAngle { get { return sweepAngle; } set { sweepAngle = value; } }
        public float Angle      { get { return angle; } set { angle = value; } }
        public float Radius     { get { return radius; } set { radius = value; } }
        public bool IsPlaying   {  get { return isPlaying; } }

        public ParticleEffect(Type particleType,
                              Vector2 position,
                              object[] args,
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
                              (float Min, float Max) lifetimeRange = default,
                              (Color color, float t)[] gradient = null!)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
            if (interval <= 0)
                throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than zero.");
            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius cannot be negative.");

            this.particleType = particleType;
            this.position = position;
            this.args = args;
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
            numTriggers = 0;

            ParticleEffects.Add(this);
            this.gradient = gradient;
        }

        /// <summary>
        /// Starts the <see cref="ParticleEffect"/> animation.
        /// </summary>
        public void Start()
        {
            if (isPlaying) return;
            isPlaying = true;
            numTriggers = 0;
            animationTask = Task.Run(Animate);
        }

        /// <summary>
        /// Stops the <see cref="ParticleEffect"/> animation.
        /// </summary>
        public async void Stop()
        {
            if (!isPlaying) return;
            isPlaying = false;

            await animationTask;
        }

        /// <summary>
        /// The animation loop for the <see cref="ParticleEffect"/> class
        /// </summary>
        /// <param name="token">The <see cref="CancellationToken"/> used for the <see cref="Task"/></param>
        private async Task Animate()
        {
            float lastEmitTime = 0f;
            elapsedTimeSW = Stopwatch.StartNew();
            while (isPlaying && (numTriggers < maxTriggers || maxTriggers < 0))
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

                await Task.Delay(1);
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
            float rotation = (float)rnd.NextDouble() * float.Pi * 2;

            _ = Activator.CreateInstance(particleType, args, emitPosition, velocity, angularVel, lifetime, rotation, gradient)!;
        }

        /// <summary>
        /// Debug procedure to draw all the <see cref="ParticleEffect"/> funnel
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object to draw to</param>
        public static void DebugDrawAll(Graphics g)
        {
            foreach (ParticleEffect p in ParticleEffects)
            {
                p.DebugDraw(g);
            } 
        }

        /// <summary>
        /// Debug procedure to draw the <see cref="ParticleEffect"/> funnel
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> object to draw to</param>
        public void DebugDraw(Graphics g)
        {
            if (float.IsNaN(position.X) || float.IsNaN(position.Y)) return;
            if (float.IsNaN(angle) || float.IsNaN(sweepAngle)) return;

            float radius = this.Radius + 2f;
            float armLength = radius + 50;

            Vector2 arm1Offset = Vector2.Transform(new(0, -radius), Matrix3x2.CreateRotation(angle)) + position;
            Vector2 arm1End = Vector2.Transform(new(armLength, 0), Matrix3x2.CreateRotation(angle)) + arm1Offset;

            Vector2 arm2Offset = Vector2.Transform(new(0, radius), Matrix3x2.CreateRotation(angle + sweepAngle)) + position;
            Vector2 arm2End = Vector2.Transform(new(armLength, 0), Matrix3x2.CreateRotation(angle + sweepAngle)) + arm2Offset;

            Vector2 intersect = arm1Offset - Global.Normalize(arm1End - arm1Offset) * radius;
            
            g.DrawEllipse(Pens.Red, position.X - radius, position.Y - radius, 2 * radius, 2 * radius);
            g.DrawLine(Pens.Red, arm1Offset.X, arm1Offset.Y, arm1End.X, arm1End.Y);
            g.DrawLine(Pens.Red, arm2Offset.X, arm2Offset.Y, arm2End.X, arm2End.Y);
            g.DrawArc(Pens.Red, intersect.X - armLength, intersect.Y - armLength, 2 * armLength + radius, 2 * armLength + radius, angle * 180 / float.Pi, SweepAngle * 180 / float.Pi);
        }

        public void Remove()
        {
            ParticleEffects.Remove(this);
        }
    }
}
