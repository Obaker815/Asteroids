using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids
{
    internal static class Global
    {
        public static bool DEBUG = false;
        public const float DEBUG_DIRECTION_LINE_LENGTH = 3f;
        public static int FPS = 165;
        public static int CONTROL_STYLE = 1; // 0 = Classic, 1 = TwoStick
        
        public static Vector2 Normalize(Vector2 v)
        {
            if (v == Vector2.Zero) return Vector2.Zero;
            if (float.IsNaN(v.X) || float.IsNaN(v.Y)) return Vector2.Zero;
            return Vector2.Normalize(v);
        }
    }
}
