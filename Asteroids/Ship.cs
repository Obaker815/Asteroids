using System.Numerics;
using SharpDX.XInput;

namespace Asteroids
{
    internal class Ship : Wrapable
    {
        public static List<Ship> ships = [];
        private Vector2 lookDir = new(1, 0);
        private Vector2 moveDir = new(1, 0);
        private bool accelerating = false;

        // 0 = Classic, 1 = TwoStick
        int controlStyle;
        float deadzone = 0.25f;

        private const float ACCELERATION = 200f;
        private const float MAX_VELOCITY = 300f;

        public Ship(Vector2 startPosition) : base(startPosition)
        {
            ships.Add(this);
        }
        
        /// <summary>
        /// Override Draw method from Wrapable
        /// </summary>
        /// <param name="g">Graphics object to be drawn to</param>
        /// <param name="position">Position to be drawn</param>
        public override void Draw(Graphics g, Vector2 position)
        {
            // Rotation angle and offset
            float rotation = MathF.Atan2(lookDir.Y, lookDir.X);
            float angleOffset = float.Pi / 12;
            float ratio = 0.8f;

            // Ship Length
            float length = 20;
            position += Vector2.Transform(new(length * ratio, 0), Matrix3x2.CreateRotation(rotation));

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
            Vector2 centerBase = (leftArmEnd + rightArmEnd) / 2;

            Action<Color, int> DrawShip = (Color c, int thickness) =>
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
            };

            // Draw the ship in white with thickness 1
            DrawShip(Color.White, 1);
        }

        public void Update(Dictionary<string, Keybind> Keys, Controller controller, float dt)
        {
            Action<Vector2, float, float> Classic = (moveDir, throttle, brake) =>
            {
                Vector2.Normalize(moveDir);

                Vector2 velocity = Vector2.Zero;
                velocity += ACCELERATION * moveDir * throttle * dt;
                velocity -= Vector2.Normalize(base.velocity) * ACCELERATION * brake * dt;

                base.velocity += velocity;
                this.moveDir = moveDir;
                this.lookDir = this.moveDir;

                if (base.velocity.LengthSquared() > MAX_VELOCITY * MAX_VELOCITY)
                    base.velocity = Vector2.Normalize(base.velocity) * MAX_VELOCITY;
            };
            Action<Vector2, Vector2> TwoStick = (moveDir, lookDir) =>
            {
                base.velocity += ACCELERATION * moveDir * dt;

                if (base.velocity.LengthSquared() > MAX_VELOCITY * MAX_VELOCITY)
                    base.velocity = Vector2.Normalize(base.velocity) * MAX_VELOCITY;

                this.lookDir = lookDir;
                this.moveDir = Vector2.Normalize(velocity);
                if (this.moveDir == Vector2.Zero) this.moveDir = Vector2.Normalize(this.moveDir);
            };


            if (controller.IsConnected)
            {
                Gamepad gamepad = controller.GetState().Gamepad;
                if (controlStyle == 0)
                {


                } else if (controlStyle == 1)
                {

                }
            }
            else
            {
                if (controlStyle == 0)
                {

                }
                else if (controlStyle == 1)
                {

                }
            }

        }

        private (Vector2, Vector2) GetStickPositions(Gamepad gamepad, float deadzone)
        {
            float leftX = gamepad.LeftThumbX / 32768f;
            float leftY = gamepad.LeftThumbY / 32768f;
            float rightX = gamepad.RightThumbX / 32768f;
            float rightY = gamepad.RightThumbY / 32768f;

            if (float.Abs(leftX) < deadzone) leftX = 0;
            if (float.Abs(leftY) < deadzone) leftY = 0;
            if (float.Abs(rightX) < deadzone) rightX = 0;
            if (float.Abs(rightY) < deadzone) rightY = 0;

            return (new(leftX, leftY), new(rightX, rightY));
        }
    }
}
