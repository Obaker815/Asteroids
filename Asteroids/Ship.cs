using System.Numerics;
using System.Security.Cryptography.Xml;

namespace Asteroids
{
    internal class Ship : Wrapable
    {
        public static List<Ship> ships = [];
        private Vector2 lookDir = new(1, 0);
        private bool accelerating = false;
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
    }
}
