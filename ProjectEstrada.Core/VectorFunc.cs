using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEstrada.Core
{
	public class VectorFunc
	{
		Func<double, NumericalVector> vectorFunc;

		public int Dimension { get; internal set; }

		public VectorFunc(params Func<double, double>[] functions)
		{
			vectorFunc = (double t) =>
			{
				NumericalVector r = new NumericalVector(functions.Length);
				for (int i = 0; i < r.Dimension; i++)
				{
					r.SetComponent(i, functions[i](t));
				}
				return r;
			};

			Dimension = functions.Length;
		}

		public VectorFunc(Func<double, NumericalVector> function, int dim)
		{
			vectorFunc = function;
			Dimension = dim;
		}

		public NumericalVector Evaluate(double t)
		{
			return vectorFunc(t);
		}
	}
}
