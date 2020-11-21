using AngouriMath;
using System;
using VectorNum = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace ProjectEstrada.Core.Functions
{
    // TODO: Look into switching to Entity.Tensor

    public class ParametricFunction : Function<Vector, VectorNum>, IEquatable<ParametricFunction>
    {
        public override FunctionType Type => FunctionType.Parametric;

        public bool Equals(ParametricFunction other)
        {
            return other.FunctionBody.Simplify() == this.FunctionBody.Simplify();
        }

        public override VectorNum Evaluate(params double[] inputVals)
        {
            return FunctionBody.Substitute(Inputs[0], inputVals[0]).EvalNumerical();
        }

        public override Function<Vector, VectorNum> Substitute(params Entity[] inputs)
        {
            var newFunc = this.Copy();
            newFunc.FunctionBody = newFunc.FunctionBody.Substitute(Inputs[0], inputs[0]);
            return newFunc;
        }
    }
}
