using System;

namespace BossBot
{
    public static class MathHelper
    {
        public static double DegreesToRadians(double degrees)
        {
            return degrees / 180 * Math.PI;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        public static double NormalizeAngleDegrees(double angle)
        {
            angle = angle % 360;
            if (angle > 180)
                angle = angle - 360;
            if (angle < -180)
                angle = angle + 360;
            return angle;
        }
    }
}
