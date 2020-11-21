using AngouriMath;
using System;

namespace ProjectEstrada.Core.Functions
{
    /// <summary>
    /// A class representing a scalar function that takes in exactly one value
    /// </summary>
    /// <remarks>
    /// You may notice that this class is similar to <see cref="ScalarFunction"/>.
    /// Simple functions can be displayed in more ways than a typicak scalar function
    /// (specifically, simple functions can be meaningfully displayed as transformations
    /// or functions with an output of a one-dimensional vector.
    /// </remarks>
    public class SimpleFunction : Function<Entity, double>, IEquatable<SimpleFunction>
    {
        public override FunctionType Type => FunctionType.Basic;

        public bool Equals(SimpleFunction other)
        {
            return other.FunctionBody.Simplify() == this.FunctionBody.Simplify();
        }

        public override double Evaluate(params double[] inputVals)
        {
            return FunctionBody.Substitute(Inputs[0], inputVals[0]).EvalNumerical().ToNumerics().Real;
        }

        public override Function<Entity, double> Substitute(params Entity[] inputs)
        {
            var newFunc = this.Copy();
            newFunc.FunctionBody = newFunc.FunctionBody.Substitute(Inputs[0], inputs[0]);
            return newFunc;
        }
    }
}
