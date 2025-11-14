using System.Numerics;

namespace Asteroids
{
    internal static class Global
    {
        public static bool DEBUG = false;
        public const float DEBUG_DIRECTION_LINE_LENGTH = 3f;
        public const bool PLAYER_COLLISION = true;

        public static int FPS = 60;
        public static int CONTROL_STYLE = 0;

        /// <summary>
        /// A safe normalization function for <see cref="Vector2"/> values
        /// </summary>
        /// <param name="v">The input vector</param>
        /// <returns>
        /// A normalized <see cref="Vector2"/> pointing in the same direction as <paramref name="v"/>,  
        /// or <see cref="Vector2.Zero"/> if the input is zero or contains <see cref="float.NaN"/> components.
        /// </returns>
        public static Vector2 Normalize(Vector2 v)
        {
            if (v == Vector2.Zero) return Vector2.Zero;
            if (float.IsNaN(v.X) || float.IsNaN(v.Y)) return Vector2.Zero;
            return Vector2.Normalize(v);
        }

        /// <summary>
        /// A two dimensional linear interpolation function <see cref="Vector2"/> values
        /// </summary>
        /// <param name="a">The start vector</param>
        /// <param name="b">The end vector</param>
        /// <param name="t">The interpolation factor</param>
        /// <returns>The interpolated <see cref="Vector2"/> value between <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return new(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
        }

        /// <summary>
        /// A linear interpolation function of <see cref="float"/> values
        /// </summary>
        /// <param name="a">The start value</param>
        /// <param name="b">The end value</param>
        /// <param name="t">The interpolation factor</param>
        /// <returns>The interpolated <see cref="float"/> value between <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}
