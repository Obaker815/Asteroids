using System.Numerics;
using SharpDX.XInput;

namespace Asteroids
{
    internal class Ship : Wrapable
    {
        // Other shit idk
        private Vector2 respawnLocation;
        private Vector2 lookDir = new(1, 0);
        private Vector2 moveDir = new(-1, 0);
        private bool accelerating = false;
        private bool respawning = false;
        private float respawnTime = 0f;
        private float iFrames = 0f;

        public static List<Ship> Ships = [];
        public int numBullets = 0;

        // Constants
        private const float ACCELERATION = 400f;
        private const float ANGULAR_ACCELERATION = float.Pi * 2f;
        private const float MAX_VELOCITY = 400f;
        private const float DEADZONE = 0.25f;
        private const int MAX_BULLETS = 4;

        /// <summary>
        /// Constructor for the <see cref="Ship"/> class
        /// </summary>
        /// <param name="startPosition">The start, and respawn <see cref="Vector2"> position of the <see cref="Ship"></param>
        public Ship(Vector2 startPosition) : base(startPosition)
        {
            respawnLocation = startPosition;
            Ships.Add(this);
            base.radius = 10f;
            iFrames = 1f;
        }
        
        /// <summary>
        /// Override Draw method from <see cref="Wrapable"/>
        /// </summary>
        /// <param name="g"><see cref="Graphics"> to be drawn to</param>
        /// <param name="position"><see cref="Vector2"> position to be drawn</param>
        public override void Draw(Graphics g, Vector2 position)
        {
            Vector2 lookDir = -this.lookDir;

            // Rotation angle and offset
            float rotation = MathF.Atan2(lookDir.Y, lookDir.X);
            float angleOffset = float.Pi / 12;
            float ratio = 0.8f;

            // Ship Length
            float length = base.radius * 3f;
            position -= Vector2.Transform(new(base.radius * 1.6f, 0), Matrix3x2.CreateRotation(rotation));

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
                Vector2 tmpPosition = base.position + new Vector2(Bounds.X, Bounds.Y);

                g.DrawLine(Pens.Red, 
                    tmpPosition.X, 
                    tmpPosition.Y, 
                    tmpPosition.X + this.lookDir.X * radius * Global.DEBUG_DIRECTION_LINE_LENGTH, 
                    tmpPosition.Y + this.lookDir.Y * radius * Global.DEBUG_DIRECTION_LINE_LENGTH);

                g.DrawLine(Pens.Blue,
                    tmpPosition.X,
                    tmpPosition.Y,
                    tmpPosition.X + this.moveDir.X * radius * Global.DEBUG_DIRECTION_LINE_LENGTH,
                    tmpPosition.Y + this.moveDir.Y * radius * Global.DEBUG_DIRECTION_LINE_LENGTH);
            }
        }

        /// <summary>
        /// Handles how the <see cref="Ship"/> respawns
        /// </summary>
        private void Respawn()
        {
            base.velocity = Vector2.Zero;
            respawnTime = 1.5f;
            respawning = true;
        }

        /// <summary>
        /// Handles all the updating logic of the <see cref="Ship"/> class
        /// </summary>
        /// <param name="Keys">This frame's keybinds <see cref="Dictionary{string, Keybind}"></param>
        /// <param name="controller">The controller</param>
        /// <param name="dt">Deltatime in seconds</param>
        public void Update(Dictionary<string, Keybind> Keys, Controller controller, float dt)
        {
            if (respawning)
            {
                float respawnSpeed = 5f;
                float lerpFactor = 1f - (float)Math.Exp(-respawnSpeed * dt);
                position = Global.Lerp(position, respawnLocation, lerpFactor);


                GameForm.AddFreezeTime(time: 0.5f, modifier: 0.5f);
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
            void Classic(Vector2 moveDir, float throttle, float brake)
            {
                accelerating = throttle > 0f;

                if (moveDir == Vector2.Zero)
                    moveDir = this.lookDir;

                moveDir = Global.Normalize(moveDir);

                Vector2 velocity = Vector2.Zero;
                velocity += ACCELERATION * moveDir * throttle * dt;
                velocity -= Global.Normalize(base.velocity) * ACCELERATION * brake * dt;

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
                this.moveDir = Global.Normalize(velocity);
                if (lookDir == Vector2.Zero) this.lookDir = moveDir;
            }

            if (controller.IsConnected)
            {
                Gamepad gamepad = controller.GetState().Gamepad;
                (Vector2 Lstick, Vector2 Rstick) = GetStickPositions(gamepad, DEADZONE);

                // 0 = Classic, 1 = TwoStick

                if (Global.CONTROL_STYLE == 0)
                {
                    float throttle = gamepad.RightTrigger / 255;
                    float brake = gamepad.LeftTrigger / 255;

                    Classic(Lstick, throttle, brake);

                } else if (Global.CONTROL_STYLE == 1)
                {
                    TwoStick(Lstick, Rstick);
                }
            }
            else
            {
                if (Global.CONTROL_STYLE == 0)
                {
                    Vector2 moveDir = this.moveDir;

                    float angularVelocity = 0f;
                    if (Keys["Left"].IsPressed) angularVelocity -= ANGULAR_ACCELERATION * dt;
                    if (Keys["Right"].IsPressed) angularVelocity += ANGULAR_ACCELERATION * dt;

                    moveDir = Vector2.Transform(moveDir, Matrix3x2.CreateRotation(angularVelocity));

                    float throttle = Keys["Up"].IsPressed ? 1f : 0f;
                    float brake = Keys["Down"].IsPressed ? 1f : 0f;

                    Classic(moveDir, throttle, brake);
                }
                else if (Global.CONTROL_STYLE == 1)
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

                    _ = new Bullet(position + (lookDir * radius * 1.2f), StartVelocity, lookDir, 200, 1000, this);
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

            if (iFrames > 0)
            {
                iFrames -= dt;
                iFrames = float.Max(0, iFrames);
            } else
            {
                Entity? collided = CollisionCheck(this);
                if (collided is not null)
                {
                    if (collided is Bullet)
                    {
                        Bullet? b = collided as Bullet;
                        if (b?.parent != this)
                        {
                            collisionHandle(collided);
                        }
                    }
                    else
                    {
                        collisionHandle(collided);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the stick positions as <see cref="Vector2"> from the <paramref name="gamepad"/> whilst also applying <paramref name="deadzone"/>
        /// </summary>
        /// <param name="gamepad">The <see cref="Gamepad"> to take stick postions from</param>
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
