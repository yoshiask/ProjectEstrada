using AngouriMath;
using System;
using System.Linq;

namespace ProjectEstrada.Core.Functions
{
    public class ScalarFunction : Function<Entity, double>, IEquatable<ScalarFunction>
    {
        public override FunctionType Type => Inputs.Count == 1 ? FunctionType.Simple : FunctionType.Scalar;

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

        #region Operations

        public Vector Parameterize()
		{
            Vector vec = new Vector(Inputs.Count +1);
            for (int i = 0; i < Inputs.Count - 1; i++)
			{
                vec[i] = Inputs.ElementAt(i);
			}
            vec[Inputs.Count - 1] = FunctionBody;
            return vec;
		}

        public Vector Normals()
		{
            Vector vectorFunc = Parameterize();
            Vector normalFunc = new Vector(vectorFunc.Size);
            for (int i = 0; i < vectorFunc.Size - 1; i++)
			{
                normalFunc[i] = -vectorFunc[i].Differentiate(Inputs.ElementAt(i));
			}
            normalFunc[Inputs.Count - 1] = 1;
            return normalFunc;
		}

		#endregion
	}
}
