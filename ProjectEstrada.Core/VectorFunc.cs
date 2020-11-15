using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEstrada.Core
{
	public class VectorFunc
	{
		Func<double, Vector> vectorFunc;

		public int Dimension { get; internal set; }

		public VectorFunc(params Func<double, double>[] functions)
		{
			vectorFunc = (double t) =>
			{
				Vector r = new Vector(functions.Length);
				for (int i = 0; i < r.Dimension; i++)
				{
					r.SetComponent(i, functions[i](t));
				}
				return r;
			};

			Dimension = functions.Length;
		}

		public VectorFunc(Func<double, Vector> function, int dim)
		{
			vectorFunc = function;
			Dimension = dim;
		}

		public Vector Evaluate(double t)
		{
			return vectorFunc(t);
		}
	}
}
