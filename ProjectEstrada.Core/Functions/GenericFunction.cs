using AngouriMath;
using AngouriMath.Core;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using VectorNum = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace ProjectEstrada.Core.Functions
{
    public class GenericFunction : ILatexiseable
    {
        public FunctionType Type
        {
            get
            {
                // TODO: This really should be put with the code that parses functions,
                // since a function with no inputs isn't a function
                Guard.IsGreaterThanOrEqualTo(Inputs.Count, 1, nameof(Inputs));

                if (Inputs.Count == 1)
                {
                    if (FunctionBody.Dimension == 1)
                        return FunctionType.Simple;
                    else
                        return FunctionType.Parametric;
                }
                else
                {
                    if (FunctionBody.Dimension == 1)
                        return FunctionType.Scalar;
                    else
                        return FunctionType.VectorValued;
                }
            }
        }

        public FunctionType RequestedType { get; set; }

        public string Name { get; set; }

        public List<Entity.Variable> Inputs { get; set; }

        public Vector FunctionBody { get; set; }

        public bool Equals(GenericFunction other)
        {
            // TODO: Should the variable names be counted as part of equality?
            return other.FunctionBody.Simplify() == this.FunctionBody.Simplify();
        }

        public VectorNum Evaluate(params double[] inputVals)
        {
            return Substitute(inputVals.Select(d => (Entity)d).ToArray()).FunctionBody.EvalNumerical();
        }
        public double EvaluateAsScalar(params double[] inputVals)
        {
            return Evaluate(inputVals)[(int)Constants.Axis.X];
        }
        public Vector2 EvaluateAsVector2(params double[] inputVals)
        {
            var vector = Evaluate(inputVals);
            return new Vector2((float)vector[(int)Constants.Axis.X], (float)vector[(int)Constants.Axis.Y]);
        }
        public Vector3 EvaluateAsVector3(params double[] inputVals)
        {
            var vector = Evaluate(inputVals);
            return new Vector3((float)vector[(int)Constants.Axis.X], (float)vector[(int)Constants.Axis.Y],
                               (float)vector[(int)Constants.Axis.Z]);
        }
        public Vector4 EvaluateAsVector4(params double[] inputVals)
        {
            var vector = Evaluate(inputVals);
            return new Vector4((float)vector[(int)Constants.Axis.X], (float)vector[(int)Constants.Axis.Y],
                               (float)vector[(int)Constants.Axis.Z], (float)vector[(int)Constants.Axis.W]);
        }

        public VectorNum Evaluate(IDictionary<Entity.Variable, double> inputs) => Evaluate(MapInputs(inputs));
        public double EvaluateAsScalar(IDictionary<Entity.Variable, double> inputs) => EvaluateAsScalar(MapInputs(inputs));
        public Vector2 EvaluateAsVector2(IDictionary<Entity.Variable, double> inputs) => EvaluateAsVector2(MapInputs(inputs));
        public Vector3 EvaluateAsVector3(IDictionary<Entity.Variable, double> inputs) => EvaluateAsVector3(MapInputs(inputs));
        public Vector4 EvaluateAsVector4(IDictionary<Entity.Variable, double> inputs) => EvaluateAsVector4(MapInputs(inputs));

        public GenericFunction Substitute(params Entity[] inputs)
        {
            var newFunc = this.Copy();
            for (int i = 0; i < Inputs.Count; i++)
            {
                newFunc.FunctionBody = newFunc.FunctionBody.Substitute(Inputs[i], inputs[i]);
            }
            return newFunc;
        }

        public GenericFunction Substitute(IDictionary<Entity.Variable, Entity> inputs) => Substitute(MapInputs(inputs));

        private T[] MapInputs<T>(IDictionary<Entity.Variable, T> inputs)
        {
            var values = new T[inputs.Count];
            foreach (var valuePair in inputs)
            {
                int i = Inputs.IndexOf(valuePair.Key);
                values[i] = valuePair.Value;
            }
            return values;
        }

        public string Latexise()
        {
            string varList = "(" + String.Join(",", Inputs) + ")";

            switch (RequestedType)
            {
                case FunctionType.Simple:
                case FunctionType.Scalar:
                    return $"{Name}{varList} = {FunctionBody[0].Latexise()}";

                case FunctionType.Parametric:
                case FunctionType.VectorValued:
                    // TODO: Change vec to overrightarrow
                    return $"\\vec{{{Name}}}{varList} = {FunctionBody.Latexise()}";

                case FunctionType.Transformation:
                    if (Type == FunctionType.Simple)
                    {
                        return $"\\widehat{{{Name}}}:{Inputs[0]} \\rightarrow {FunctionBody[0].Latexise()}";
                    }
                    else
                    {
                        return $"\\widehat{{{Name}}}:{varList} \\rightarrow {FunctionBody.Latexise("(", ")")}";
                    }

                default:
                    throw new NotImplementedException(Type.ToString());
            }
        }
    }
}
