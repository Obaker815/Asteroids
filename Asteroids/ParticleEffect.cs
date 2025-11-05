using System.CodeDom;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;

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

            _ = (Particle)Activator.CreateInstance(particleType, emitPosition, velocity, angularVel, lifetime, gradient)!;
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

            float radius = MathF.Max(this.radius, 0.1f);
            float armLength = radius + 50;

            Vector2 arm1Offset = Vector2.Transform(new(0, -radius), Matrix3x2.CreateRotation(angle)) + position;
            Vector2 arm1End = Vector2.Transform(new(armLength, 0), Matrix3x2.CreateRotation(angle)) + arm1Offset;

            Vector2 arm2Offset = Vector2.Transform(new(0, radius), Matrix3x2.CreateRotation(angle + sweepAngle)) + position;
            Vector2 arm2End = Vector2.Transform(new(armLength, 0), Matrix3x2.CreateRotation(angle + sweepAngle)) + arm2Offset;

            g.DrawEllipse(Pens.Red, position.X - radius, position.Y - radius, 2 * radius, 2 * radius);
            g.DrawLine(Pens.Red, arm1Offset.X, arm1Offset.Y, arm1End.X, arm1End.Y);
            g.DrawLine(Pens.Red, arm2Offset.X, arm2Offset.Y, arm2End.X, arm2End.Y);

            float dx1 = arm1End.X - arm1Offset.X;
            float dx2 = arm2End.X - arm2Offset.X;

            if (MathF.Abs(dx1) < 1e-5f || MathF.Abs(dx2) < 1e-5f)
            {
                g.DrawLine(Pens.Red, arm1End.X, arm1End.Y, arm2End.X, arm2End.Y);
                return;
            }

            float m1 = (arm1End.Y - arm1Offset.Y) / dx1;
            float m2 = (arm2End.Y - arm2Offset.Y) / dx2;

            if (MathF.Abs(m1 - m2) < 1e-5f)
            {
                g.DrawLine(Pens.Red, arm1End.X, arm1End.Y, arm2End.X, arm2End.Y);
                return;
            }

            float c1 = arm1Offset.Y - m1 * arm1Offset.X;
            float c2 = arm2Offset.Y - m2 * arm2Offset.X;

            float xA = (c2 - c1) / (m1 - m2);
            float yA = m1 * xA + c1;
            Vector2 intersect = new(xA, yA);

            if (float.IsNaN(xA) || float.IsNaN(yA) || float.IsInfinity(xA) || float.IsInfinity(yA))
            {
                g.DrawLine(Pens.Red, arm1End.X, arm1End.Y, arm2End.X, arm2End.Y);
                return;
            }

            float R = Vector2.Distance(intersect, arm1End);
            if (R < 1f || R > 10000f)
            {
                g.DrawLine(Pens.Red, arm1End.X, arm1End.Y, arm2End.X, arm2End.Y);
                return;
            }

            float startAngle = MathF.Atan2(arm1End.Y - intersect.Y, arm1End.X - intersect.X) * 180f / MathF.PI;
            float endAngle = MathF.Atan2(arm2End.Y - intersect.Y, arm2End.X - intersect.X) * 180f / MathF.PI;
            float sweep = endAngle - startAngle;
            if (sweep < 0) sweep += 360f;

            g.DrawArc(Pens.Red,
                new RectangleF(intersect.X - R, intersect.Y - R, R * 2, R * 2),
                startAngle, sweep);
        }

        public void Remove()
        {
            ParticleEffects.Remove(this);
        }
    }
}
