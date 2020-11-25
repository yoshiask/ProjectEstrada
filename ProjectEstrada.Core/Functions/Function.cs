using AngouriMath;
using System;
using System.Collections.Generic;

namespace ProjectEstrada.Core.Functions
{
    /// <summary>
    /// A base class that represents a function that can be manipulated algebraically and evaluated.
    /// </summary>
    /// <typeparam name="TSym">The type this function uses as a symbolic input.</typeparam>
    /// <typeparam name="TEvalOut">The type this function outputs when evaluated numerically.</typeparam>
    public abstract class Function<TSym, TEvalOut> : IEquatable<Function<TSym, TEvalOut>>
        where TSym : IEquatable<TSym>
    {
        public abstract FunctionType Type { get; }

        public string Name { get; set; }

        public List<Entity.Variable> Inputs { get; set; }

        public TSym FunctionBody { get; set; }

        public bool Equals(Function<TSym, TEvalOut> other)
        {
            // TODO: Should the variable names be counted as part of equality?
            return FunctionBody.Equals(other.FunctionBody);
        }

        public abstract TEvalOut Evaluate(params double[] inputVals);

        public TEvalOut Evaluate(IDictionary<Entity.Variable, double> inputs) => Evaluate(MapInputs(inputs));

        public abstract Function<TSym, TEvalOut> Substitute(params Entity[] inputs);

        public Function<TSym, TEvalOut> Substitute(IDictionary<Entity.Variable, Entity> inputs) => Substitute(MapInputs(inputs));

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
    }

    public enum FunctionType
    {
        Simple,
        Scalar,
        Parametric,
        Transformative,
        Transformation = Transformative,
        VectorValued,
        VectorField = VectorValued
    }
}
