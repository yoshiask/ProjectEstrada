using AngouriMath;
using System;
using System.Linq;

namespace ProjectEstrada.Core.Functions
{
    public class ScalarFunction : Function<Entity, double>, IEquatable<ScalarFunction>
    {
        public override FunctionType Type => Inputs.Count == 1 ? FunctionType.Basic : FunctionType.Scalar;

        public bool Equals(ScalarFunction other)
        {
            return other.FunctionBody.Simplify() == this.FunctionBody.Simplify();
        }

        public override double Evaluate(params double[] inputVals)
        {
            return Substitute(inputVals.Select(d => (Entity)d).ToArray()).FunctionBody.EvalNumerical().ToNumerics().Real;
        }

        public override Function<Entity, double> Substitute(params Entity[] inputs)
        {
            var newFunc = this.Copy();
            for (int i = 0; i < Inputs.Count; i++)
            {
                newFunc.FunctionBody = newFunc.FunctionBody.Substitute(Inputs[i], inputs[i]);
            }
            return newFunc;
        }
    }
}
