using System;
using System.Runtime.CompilerServices;

namespace ProjectEstrada.Graphics.Core
{
    public static class MathEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
#if !(NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
            if (min > max)
                throw new ArgumentException($"The minimum ({min}) cannot be greater than the maximum ({max})");

            if (value < min)
                return min;
            else if (value > max)
                return max;

            return value;
#else
            return Math.Clamp(value, min, max);
#endif
        }

        public static float DegreesToRadians(float degrees)
        {
            return (float)Math.PI / 180f * degrees;
        }
    }
}
