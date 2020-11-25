using System;
using System.Collections.Generic;
using System.Linq;
using VectorNum = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace ProjectEstrada.Core
{
    public class Constants
    {
        public static readonly VectorNum XAxis = VectorNum.Build.DenseOfArray(new double[] { 1, 0, 0 });
        public static readonly VectorNum YAxis = VectorNum.Build.DenseOfArray(new double[] { 0, 1, 0 });
        public static readonly VectorNum ZAxis = VectorNum.Build.DenseOfArray(new double[] { 0, 0, 1 });
        public static readonly VectorNum WAxis = VectorNum.Build.DenseOfArray(new double[] { 0, 0, 0, 1 });

        public enum Axis
        {
            X, Y, Z, W
        }

        public static readonly Dictionary<Axis, VectorNum> AxisUnitVectors = new Dictionary<Axis, VectorNum>()
        {
            { Axis.X, XAxis },
            { Axis.Y, YAxis },
            { Axis.Z, ZAxis },
            { Axis.W, WAxis },
        };
    }
}
