using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using VectorNum = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace ProjectEstrada.Core.Helpers
{
    public static class MathHelper
    {
        public static T[] Flatten<T>(T[,] twoDim)
        {
            T[] oneDim = new T[twoDim.Length];
            int stride = twoDim.GetLength(0);
            for (int i = 0; i < stride; i++)
            {
                for (int j = 0; j < twoDim.GetLength(1); j++)
                {
                    oneDim[i + j * stride] = twoDim[i, j];
                }
            }
            //Buffer.BlockCopy(twoDim, 0, oneDim, 0, twoDim.Length);
            return oneDim;
        }

        /// <summary>
        /// Returns a list of evenly-spaced numbers over a specified interval.
        /// Equivalent to np.linspace()
        /// </summary>
        public static IEnumerable<double> LinSpace(double start, double end, int partitions)
        {
            return Enumerable.Range(0, partitions + 1).Select(idx => idx != partitions
                    ? start + (end - start) / partitions * idx
                    : end);
        }

        public static float MapRange(float x, float inMin, float inMax, float outMin, float outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        public static double LinearInterpolation(double a, double b, double t)
        {
            return (1 - t) * a + t * b;
        }

        public static Vector2 BilinearInterpolation(Vector2 a, Vector2 b, double t)
        {
            return a + (float)t * (b - a);
        }

        public static Vector3 TrilinearInterpolation(Vector3 a, Vector3 b, double t)
        {
            return a + (float)t * (b - a);
        }

        public static VectorNum NDimensionalLinearInterpolation(VectorNum a, VectorNum b, double t)
        {
            return a + (float)t * (b - a);
        }
        public static Vector NDimensionalLinearInterpolation(Vector a, Vector b, double t)
        {
            return a + t * (b - a);
        }
    }
}
