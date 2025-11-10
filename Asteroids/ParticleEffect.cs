using System.Diagnostics;
using System.Numerics;
using System.Reflection;

namespace Asteroids
{
    internal class ParticleEffect
    {
        private static readonly List<ParticleEffect> ParticleEffects = [];
        private static readonly ThreadLocal<Random> random = new(() => new Random());

        private readonly (Color color, float t)[] gradient;         // The color gradient the particles follow over their lifetime range
        private readonly (float Min, float Max) angularVelocity;    // The range of angular velocities that the particles can start with
        private readonly (float Min, float Max) lifetimeRange;      // The range (relative to lifetime) that the particles will live for
        private readonly (float Min, float Max) impulseRange;       // The range (relative to impulse) that the particles will be spawned with
        private readonly Type particleType;                         // The type of particle spawned
        private readonly float lifetime;                            // The default lifetime of a particle
        private readonly float impulse;                             // The starting impulse given to a particle
        private object[] args;                                      // The arguments passed to the particle constructor e.g. [float length, int width]

        private Task animationTask;
        private Stopwatch elapsedTimeSW;

        private readonly float interval;    // Time between triggers
        private readonly float duration;    // Duration of the effect
        private readonly int maxTriggers;   // Maximum number of triggers of the effect
        private readonly int count;         // Number of particles to spawn each trigger
        private int numTriggers;

        // Particle effect properties
        private Vector2 position;
        private float radius;
        private float angle;
        private float sweepAngle;
        private bool isPlaying;

        // Public properties
        public object[] Args    { get { return args; } set { args = value; } }
        public Vector2 Position { get { return position; } set { position = value; } }
        public float Radius     { get { return radius; } set { radius = value; } }
        public float Angle      { get { return angle; } set { angle = value; } }
        public float SweepAngle { get { return sweepAngle; } set { sweepAngle = value; } }
        public bool IsPlaying   { get { return isPlaying; }}

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
                              (float Min, float Max) impulseRange = default,
                              (Color color, float t)[] gradient = null!)
        {
            if (count <= 0)     throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
            if (interval < 0)   throw new ArgumentOutOfRangeException(nameof(interval), "Interval cannot be negative");
            if (radius < 0)     throw new ArgumentOutOfRangeException(nameof(radius), "Radius cannot be negative.");

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
            this.impulseRange = impulseRange;
            this.gradient = gradient;

            // Set variables to their default
            elapsedTimeSW = null!;
            animationTask = null!;
            numTriggers = 0;

            // Add this ParticleEffect to the list of ParticleEffects
            ParticleEffects.Add(this);
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
        private async Task Animate()
        {
            float lastEmitTime = 0f;
            elapsedTimeSW = Stopwatch.StartNew();
            while (isPlaying && (numTriggers < maxTriggers || maxTriggers < 0))
            {
                float elapsedSeconds = (float)elapsedTimeSW.Elapsed.TotalSeconds;
                
                // Check max duration of the particle effcect vs current duration
                if (duration > 0f && elapsedSeconds >= duration)
                    break;

                // Check if elapsed time is enough for another trigger
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

            // emitAngle should be in the range {emitAngle : angle <= emitAngle <= sweepAngle}
            float angleOffset = (float)rnd.NextDouble() * sweepAngle;
            float emitAngle = angle + angleOffset;

            // impulse should be randomised in the range (impulseRange) + impulse
            float impulse = this.impulse + ((float)rnd.NextDouble() * (impulseRange.Max - impulseRange.Min) + impulseRange.Min);
            Vector2 velocity = Vector2.Transform(
                new(impulse, 0),
                Matrix3x2.CreateRotation(emitAngle)
                );

            // emitPosition should be within the radius from position
            Vector2 emitPosition = position + Vector2.Transform(
                new((float)rnd.NextDouble() * radius, 0),
                Matrix3x2.CreateRotation((float)rnd.NextDouble() * 2 * float.Pi)
                );

            // Calculate angular velocity, lifetime and rotation of the Particle to be spawned
            float angularVel = (float)rnd.NextDouble() * (angularVelocity.Max - angularVelocity.Min) + angularVelocity.Min;
            float lifetime = this.lifetime + (float)rnd.NextDouble() * (lifetimeRange.Max - lifetimeRange.Min) + lifetimeRange.Min;
            float rotation = (float)rnd.NextDouble() * float.Pi * 2;

            // Create the arguments to be passed to the constructor
            object[] argsForCtor =
            [
                args, emitPosition, velocity, angularVel, lifetime, rotation, gradient
            ];

            // Create an instance of the particleType using Activator.CreateInstance 
            _ = Activator.CreateInstance(
                particleType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                args: argsForCtor,
                culture: null
            );
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

            g.DrawEllipse(Pens.Red, position.X - radius, position.Y - radius, 2 * radius, 2 * radius);
            g.DrawLine(Pens.Red, arm1Offset.X, arm1Offset.Y, arm1End.X, arm1End.Y);
            g.DrawLine(Pens.Red, arm2Offset.X, arm2Offset.Y, arm2End.X, arm2End.Y);
            g.DrawArc(Pens.Red, position.X - armLength, position.Y - armLength, 2 * armLength, 2 * armLength, angle * 180 / float.Pi, SweepAngle * 180 / float.Pi);
        }

        public void Remove()
        {
            ParticleEffects.Remove(this);
        }
    }
}
