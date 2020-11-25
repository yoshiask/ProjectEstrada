using VectorNum = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace System.Numerics
{
    public static class VectorExtensions
    {
        public static VectorNum AsMathNetVector(this Vector2 v)
        {
            return VectorNum.Build.DenseOfArray(new double[]
            {
                v.X, v.Y
            });
        }

        public static VectorNum AsMathNetVector(this Vector3 v)
        {
            return VectorNum.Build.DenseOfArray(new double[]
            {
                v.X, v.Y, v.Z
            });
        }

        public static VectorNum AsMathNetVector(this Vector4 v)
        {
            return VectorNum.Build.DenseOfArray(new double[]
            {
                v.X, v.Y, v.Z, v.W
            });
        }
    }
}
