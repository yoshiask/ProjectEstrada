using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using VectorNum = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace ProjectEstrada.Core
{
    public class Constants
    {
        public static readonly VectorNum Zero  = VectorNum.Build.DenseOfArray(new double[] { 0, 0, 0 });
        public static readonly VectorNum XAxis = VectorNum.Build.DenseOfArray(new double[] { 1, 0, 0 });
        public static readonly VectorNum YAxis = VectorNum.Build.DenseOfArray(new double[] { 0, 1, 0 });
        public static readonly VectorNum ZAxis = VectorNum.Build.DenseOfArray(new double[] { 0, 0, 1 });
        public static readonly VectorNum WAxis = VectorNum.Build.DenseOfArray(new double[] { 0, 0, 0, 1 });

        public static readonly Vector ZeroVec2 = new Vector(new Entity[] { 0, 0 });
        public static readonly Vector ZeroVec3 = new Vector(new Entity[] { 0, 0, 0 });
        public static readonly Vector ZeroVec4 = new Vector(new Entity[] { 0, 0, 0, 0 });
        public static readonly Vector IHat = new Vector(new Entity[] { 1, 0, 0 });
        public static readonly Vector JHat = new Vector(new Entity[] { 0, 1, 0 });
        public static readonly Vector KHat = new Vector(new Entity[] { 0, 0, 1 });
        public static readonly Vector LHat = new Vector(new Entity[] { 0, 0, 0, 1 });

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

        public static Vector GetAxisVector(int axis)
		{
			return axis switch
			{
				0 => IHat,
				1 => JHat,
				2 => KHat,
				3 => LHat,
				_ => ZeroVec3,
			};
		}
        public static Vector GetAxisVector(Axis axis)
		{
            return GetAxisVector((int)axis);
		}
    }
}
