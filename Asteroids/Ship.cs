using System.Diagnostics;
using System.Numerics;
using SharpDX.XInput;

namespace Asteroids
{
    internal class Ship : Wrapable
    {
        // Particle effects for when the ship is destroyed
        private readonly ParticleEffect[] destroyEffects =
            [
                new(
                    particleType: typeof(ParticleLine),
                    position: new(0, 0),
                    args: [20f, 1],
                    interval: 0.01f,
                    lifetime: 2,
                    impulse: 50,
                    count: 3,
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
                    interval: 0.02f,
                    lifetime: 1f,
                    impulse: 50,
                    count: 100,
                    maxTriggers: 5,
                    impulseRange: (-150, 250),
                    lifetimeRange: (-0.5f, 0.5f),
                    gradient: [
                        (Color.White, 0.0f),
                        (Color.White, 0.5f),
                        ])
                ];

        // Constants
        internal const float ANGULAR_ACCELERATION = float.Pi * 2f;
        internal const float ACCELERATION = 400f;
        internal const float MAX_VELOCITY = 400f;
        internal const float DEADZONE = 0.25f;
        internal const int BULLET_TIME = 2000;
        internal const int MAX_BULLETS = 4;

        // Private ship properties
        internal Vector2 respawnLocation;
        internal Vector2 lookDir = new(0, -1);
        internal Vector2 moveDir = new(0, -1);
        internal bool accelerating = false;
        internal bool respawning = false;
        internal float respawnTime = 0f;
        internal float iFrames = 0f;
        internal int numBullets = 0;

        // Public ship properties
        public bool Respawning => respawning;
        public int NumBullets { get { return numBullets; } set { numBullets = int.Clamp(value, 0, 4); } }
        public int lives = 5;

        public static List<Ship> Ships = [];

        /// <summary>
        /// Constructor for the <see cref="Ship"/> class
        /// </summary>
        /// <param name="startPosition">The start, and respawn <see cref="Vector2"/> position of the <see cref="Ship"/></param>
        public Ship(Vector2 startPosition) : base(startPosition)
        {
            respawnLocation = startPosition;
            Ships.Add(this);
            base.radius = 10f;
            iFrames = 1f;
        }

        /// <summary>
        /// Override Draw method from <see cref="Wrapable.Draw(Graphics, Vector2)"/>
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> to draw to</param>
        /// <param name="position">The <see cref="Vector2"/> position to be drawn</param>
        public override void Draw(Graphics g, Vector2 Position, Color color)
        {
            Draw(g, Position, lookDir, color,
                base.radius, accelerating, respawning, iFrames);
        }

        /// <summary>
        /// The Draw method for <see cref="Ship"/>
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> to draw to</param>
        /// <param name="position">The <see cref="Vector2"/> position to be drawn</param>
        /// <param name="lookDir">The forward direction of the ship as <see cref="Vector2"/></param>
        /// <param name="radius">The radius of the <see cref="Ship"/></param>
        /// <param name="accelerating">If the ship should have a flame</param>
        /// <param name="respawning">If the ship is respawning</param>
        /// <peram name="iframes">The current invincibility frames of the ship</peram>
        public static void Draw(Graphics g, Vector2 position, Vector2 lookDir, Color color, float radius, bool accelerating, bool respawning = false, float iframes = 0f)
        {
            if (respawning) return;
            if (float.Floor(iframes * 10) % 2 == 1) return;

            Vector2 initialPos = position;
            lookDir *= -1;

            // Rotation angle and offset
            float rotation = MathF.Atan2(lookDir.Y, lookDir.X);
            float angleOffset = float.Pi / 12;
            float ratio = 0.8f;

            // Ship Length
            float length = radius * 3f;
            position -= Vector2.Transform(new(radius * 1.6f, 0), Matrix3x2.CreateRotation(rotation));

            // Calculate arm vector
            Vector2 leftArm = Vector2.Transform(new(length, 0), Matrix3x2.CreateRotation(rotation + angleOffset));
            Vector2 rightArm = Vector2.Transform(new(length, 0), Matrix3x2.CreateRotation(rotation - angleOffset));

            // Make arm end positions
            Vector2 leftArmEnd = position + leftArm;
            Vector2 rightArmEnd = position + rightArm;

            // Calculate center bar ends
            Vector2 barLeft = position + leftArm * ratio;
            Vector2 barRight = position + rightArm * ratio;

            // Calculate the tip of the "flame"
            Vector2 centerBase = (leftArm + rightArm) / 2f * 1.2f + position;

            void DrawShip(Color c, int thickness)
            {
                Pen pen = new(c, thickness);

                // Draw ship
                g.DrawLine(pen, position.X, position.Y, leftArmEnd.X, leftArmEnd.Y);
                g.DrawLine(pen, position.X, position.Y, rightArmEnd.X, rightArmEnd.Y);
                g.DrawLine(pen, barLeft.X, barLeft.Y, barRight.X, barRight.Y);

                if (accelerating)
                {
                    // Draw flame if accelerating
                    g.DrawLine(pen, barLeft.X, barLeft.Y, centerBase.X, centerBase.Y);
                    g.DrawLine(pen, barRight.X, barRight.Y, centerBase.X, centerBase.Y);
                }
            }

            // Draw the ship in white with thickness 1
            DrawShip(Color.White, 1);

            if (Global.DEBUG)
            {
                g.DrawLine(Pens.Red, 
                    initialPos.X, 
                    initialPos.Y, 
                    initialPos.X + lookDir.X * radius * Global.DEBUG_DIRECTION_LINE_LENGTH, 
                    initialPos.Y + lookDir.Y * radius * Global.DEBUG_DIRECTION_LINE_LENGTH);
            }
        }

        /// <summary>
        /// Handles how the <see cref="Ship"/> respawns
        /// </summary>
        internal virtual void Respawn()
        {
            lives--;
            foreach (ParticleEffect p in destroyEffects)
            {
                p.Radius = base.radius;
                p.Position = this.position;
                GameForm.ActiveGameform?.InvokeAction(p.Start);
            }

            base.velocity = Vector2.Zero;
            respawnTime = 1.5f;
            respawning = true;
            
            lookDir = new(0, -1);
            moveDir = new(0, -1);
        }

        /// <summary>
        /// Handles all the updating logic of the <see cref="Ship"/> class
        /// </summary>
        /// <param name="Keys">This frame's keybinds <see cref="Dictionary{string, Keybind}"/></param>
        /// <param name="controller">The controller</param>
        /// <param name="dt">Deltatime in seconds</param>
        public virtual void Update(Dictionary<string, Keybind> Keys, Controller controller, float dt)
        {
            if (respawning)
            {
                GameForm.AddFreezeTime(time: 0.5f, modifier: 0.5f);
                position = respawnLocation;
                accelerating = false;

                respawnTime -= dt;
                if (respawnTime <= 0f)
                {
                    respawning = false;
                    respawnTime = 0f;
                    iFrames = 1f;
                }

                return;
            }

            // Classic control method
            void Classic(Vector2 moveDir, float throttle)
            {
                accelerating = throttle > 0f;

                if (moveDir == Vector2.Zero)
                    moveDir = this.lookDir;

                moveDir = Global.Normalize(moveDir);

                Vector2 velocity = Vector2.Zero;
                velocity += ACCELERATION * moveDir * throttle * dt;

                base.velocity += velocity;
                this.moveDir = moveDir;
                this.lookDir = moveDir;

                if (base.velocity.LengthSquared() > MAX_VELOCITY * MAX_VELOCITY)
                    base.velocity = Global.Normalize(base.velocity) * MAX_VELOCITY;
            }
            // Two-stick control method
            void TwoStick(Vector2 moveDir, Vector2 lookDir)
            {
                moveDir = Global.Normalize(moveDir);
                lookDir = Global.Normalize(lookDir);

                accelerating = (Vector2.Dot(moveDir, ((lookDir.LengthSquared() > 0)? lookDir : moveDir)) > 0.5f);

                base.velocity += ACCELERATION * moveDir * dt;

                if (base.velocity.LengthSquared() > MAX_VELOCITY * MAX_VELOCITY)
                    base.velocity = Global.Normalize(base.velocity) * MAX_VELOCITY;

                this.lookDir = lookDir;
                this.moveDir = (moveDir == Vector2.Zero)? Global.Normalize(velocity) : moveDir;
                if (this.lookDir == Vector2.Zero) this.lookDir = this.moveDir;
            }

            if (controller.IsConnected)
            {
                Gamepad gamepad = controller.GetState().Gamepad;
                (Vector2 Lstick, Vector2 Rstick) = GetStickPositions(gamepad, DEADZONE);

                // 0 = Classic, 1 = TwoStick

                if (Global.CONFIGS.ControlStyle == 0)
                {
                    float throttle = gamepad.RightTrigger / 255;

                    Classic(Lstick, throttle);

                } else if (Global.CONFIGS.ControlStyle == 1)
                {
                    TwoStick(Lstick, Rstick);
                }
            }
            else
            {
                if (Global.CONFIGS.ControlStyle == 0)
                {
                    Vector2 moveDir = this.moveDir;

                    float angularVelocity = 0f;
                    if (Keys["Left"].IsPressed) angularVelocity -= ANGULAR_ACCELERATION * dt;
                    if (Keys["Right"].IsPressed) angularVelocity += ANGULAR_ACCELERATION * dt;

                    moveDir = Vector2.Transform(moveDir, Matrix3x2.CreateRotation(angularVelocity));

                    float throttle = Keys["Up"].IsPressed ? 1f : 0f;

                    Classic(moveDir, throttle);
                }
                else if (Global.CONFIGS.ControlStyle == 1)
                {
                    Vector2 moveDir = Vector2.Zero;
                    Vector2 lookDir = Vector2.Zero;
                    
                    if (Keys["Up"].IsPressed) moveDir.Y -= 1;
                    if (Keys["Down"].IsPressed) moveDir.Y += 1;
                    if (Keys["Left"].IsPressed) moveDir.X -= 1;
                    if (Keys["Right"].IsPressed) moveDir.X += 1;

                    if (Keys["UpAlt"].IsPressed) lookDir.Y -= 1;
                    if (Keys["DownAlt"].IsPressed) lookDir.Y += 1;
                    if (Keys["LeftAlt"].IsPressed) lookDir.X -= 1;
                    if (Keys["RightAlt"].IsPressed) lookDir.X += 1;

                    TwoStick(moveDir, lookDir);
                }

                if (Keys["Shoot"].FirstPress && numBullets < MAX_BULLETS)
                {
                    numBullets++;
                    Vector2 lookDir = (this.lookDir != Vector2.Zero) ? this.lookDir : new(1, 0);

                    Vector2 StartVelocity = base.velocity * Vector2.Dot(base.velocity, lookDir) / base.velocity.LengthSquared();
                    if (StartVelocity.X is float.NaN || StartVelocity.Y is float.NaN)
                        StartVelocity = Vector2.Zero;

                    _ = new Bullet(position + (lookDir * radius * 1.2f), StartVelocity, lookDir, 200, BULLET_TIME, this);
                }
            }

            void collisionHandle(Entity collided)
            {
                if (collided is not Ship)
                {
                    toRemove.Add(collided);
                    Respawn();
                }
            }

            if (iFrames > 0 || !Global.PLAYER_COLLISION)
            {
                iFrames -= dt;
                iFrames = float.Max(0, iFrames);
            } else
            {
                Entity? collidedBullet = CollisionCheck(this, typeof(Bullet));
                if (collidedBullet is not null)
                {
                    Bullet? b = collidedBullet as Bullet;
                    if (b?.Parent != this)
                    {
                        collisionHandle(collidedBullet);
                    }
                }

                Entity? collidedAsteroid = CollisionCheck(this, typeof(Asteroid));
                if (collidedAsteroid is not null)
                {
                    collisionHandle(collidedAsteroid);
                }

                Entity? collidedSaucer = CollisionCheck(this, typeof(Saucer));
                if (collidedSaucer is not null)
                {
                    collisionHandle(collidedSaucer);
                }
            }
        }

        /// <summary>
        /// Gets the stick positions as <see cref="Vector2"/> from the <paramref name="gamepad"/> whilst also applying <paramref name="deadzone"/>
        /// </summary>
        /// <param name="gamepad">The <see cref="Gamepad"/> to take stick postions from</param>
        /// <param name="deadzone">The deadzone of the sticks</param>
        /// <returns>two <see cref="Vector2"> of the stick positions</returns>
        private static (Vector2, Vector2) GetStickPositions(Gamepad gamepad, float deadzone)
        {
            float leftX = gamepad.LeftThumbX / 32768f;
            float leftY = gamepad.LeftThumbY / 32768f;
            float rightX = gamepad.RightThumbX / 32768f;
            float rightY = gamepad.RightThumbY / 32768f;

            Vector2 leftStick = new(leftX, -leftY);
            Vector2 rightStick = new(rightX, -rightY);

            if (leftStick.LengthSquared() < deadzone * deadzone)
                leftStick = Vector2.Zero;
            if (rightStick.LengthSquared() < deadzone * deadzone)
                rightStick = Vector2.Zero;

            return (leftStick, rightStick);
        }

        public override void Remove()
        {
            base.Remove();
            Ships.Remove(this);
        }
    }
}
