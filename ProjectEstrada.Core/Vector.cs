using System;

namespace ProjectEstrada.Core
{
	public class Vector : IEquatable<Vector>
	{
		double[] vector;

		/// <summary>
		/// Gets the dimension of the vector.
		/// </summary>
		public int Dimension
		{
			get {
				return vector.Length;
			}
		}

		/// <summary>
		/// Creates a vector given a point or a list of values.
		/// </summary>
		/// <param name="vals"></param>
		public Vector(params double[] vals)
		{
			vector = vals;
		}

		/// <summary>
		/// Creates an empty vector with the given dimension.
		/// </summary>
		/// <param name="dim"></param>
		public Vector(int dim)
		{
			vector = new double[dim];
		}

		//

		/// <summary>
		/// Sets the dimension of the vector.
		/// </summary>
		/// <param name="dim"></param>
		public void SetDimension(int dim)
		{
			for (int i = Dimension; i < dim; i++)
			{
				SetComponent(i, 0);
			}
		}

		/// <summary>
		/// Gets the value of a single component of the vector.
		/// </summary>
		/// <param name="index">The location of the coordinate. 0 is X, 1 is Y, 2 is Z, and so on</param>
		public double GetComponent(int index)
		{
			return vector[index];
		}

		/// <summary>
		/// Sets the value of a single component of the vector.
		/// </summary>
		/// <param name="index">The location of the coordinate. 0 is X, 1 is Y, 2 is Z, and so on</param>
		/// <param name="v">The new value of the component</param>
		public void SetComponent(int index, double v)
		{
			vector[index] = v;
		}

		/// <summary>
		/// Gets the length, or magnitude, of the vector.
		/// </summary>
		public double GetMagnitude()
		{
			double sum = 0;
			foreach (double v in vector)
			{
				sum += Math.Pow(v, 2);
			}
			return Math.Sqrt(sum);
		}

		/// <summary>
		/// Gets the unit vector in the same directon of the vector.
		/// </summary>
		/// <returns></returns>
		public Vector GetUnitVector()
		{
			return this / GetMagnitude();
		}

		/// <summary>
		/// Gets the angle of the vector with any axis.
		/// </summary>
		/// <param name="dir">the direction, or axis, where 0 is X, 1 is Y, 2 is Z, and so on</param>
		public double GetAngleWithAxis(int dir)
		{
			Vector axis = GetAxisVector(dir, Dimension);
			return Math.Acos(this * axis / GetMagnitude());
		}

		/// <summary>
		/// Gets the angle of the vector with the x-axis.
		/// </summary>
		public double GetAngleWithX()
		{
			return GetAngleWithAxis(0);
		}

		/// <summary>
		/// Gets the angle of the vector with the y-axis.
		/// </summary>
		public double GetAngleWithY()
		{
			return GetAngleWithAxis(1);
		}

		/// <summary>
		/// Gets the angle of the vector with the z-axis.
		/// </summary>
		public double GetAngleWithZ()
		{
			return GetAngleWithAxis(2);
		}

		//

		public override string ToString()
		{
			return "<" + String.Join(", ", vector) + ">";
		}
		public double[] ToArray()
		{
			return vector;
		}
		/// <summary>
		/// Takes in two vectors and makes them the same length by padding the shorter vector with zeros until both vectors are the same dimension.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		public static void MakeSameDimension(Vector a, Vector b)
		{
			if(a.Dimension >= b.Dimension)
			{
				b.SetDimension(a.Dimension);
			}
			if(b.Dimension < a.Dimension)
			{
				a.SetDimension(b.Dimension);
			}
		}
		/// <summary>
		/// Creates a unit vector that points purely in the direction of an orthogonal axis.
		/// </summary>
		/// <param name="dir">the direction, or axis, where 0 is X, 1 is Y, 2 is Z, and so on</param>
		/// <param name="dim">the desired dimension or length of the outputed vector</param>
		/// <returns></returns>
		public static Vector GetAxisVector(int dir, int dim)
		{
			Vector axis = new Vector(dim);
			for (int i = 0; i < dim; i++)
			{
				if (i == dir)
				{
					axis.SetComponent(i, 1);
				}
				else
				{
					axis.SetComponent(i, 0);
				}
			}
			return axis;
		}

		//

		#region Operator Overloads
		public static Vector operator -(Vector a)
		{
			Vector negative = new Vector(a.Dimension);
			for(int i = 0; i < a.Dimension; i++)
			{
				negative.SetComponent(i, -1 * a.GetComponent(i));
			}
			return negative;
		}
		public static Vector operator +(Vector a, Vector b)
		{
			MakeSameDimension(a, b);
			Vector add = new Vector(a.Dimension);
			for(int i = 0; i < a.Dimension; i++)
			{
				add.SetComponent(i, a.GetComponent(i) + b.GetComponent(i));
			}
			return add;
		}
		public static Vector operator +(Vector a, double b)
		{
			Vector add = new Vector(a.Dimension);
			for (int i = 0; i < a.Dimension; i++)
			{
				add.SetComponent(i, a.GetComponent(i) + b);
			}
			return add;
		}
		public static Vector operator -(Vector a, Vector b)
		{
			MakeSameDimension(a, b);
			Vector subtract = new Vector(a.Dimension);
			for (int i = 0; i < a.Dimension; i++)
			{
				subtract.SetComponent(i, a.GetComponent(i) - b.GetComponent(i));
			}
			return subtract;
		}
		public static Vector operator -(Vector a, double b)
		{
			Vector subtract = new Vector(a.Dimension);
			for (int i = 0; i < a.Dimension; i++)
			{
				subtract.SetComponent(i, a.GetComponent(i) - b);
			}
			return subtract;
		}
		public static double operator *(Vector a, Vector b)
		{
			MakeSameDimension(a, b);
			double dot = 0;
			for(int i = 0; i < a.Dimension; i++)
			{
				dot += a.GetComponent(i) * b.GetComponent(i);
			}
			return dot;
		}
		public static Vector operator *(Vector a, double b)
		{
			Vector scaled = new Vector(a.Dimension);
			for(int i = 0; i < a.Dimension; i++)
			{
				scaled.SetComponent(i, b * a.GetComponent(i));
			}
			return scaled;
		}
		public static Vector operator /(Vector a, double b)
		{
			return a * (1 / b);
		}
		#endregion

		#region Operator Methods
		/// <summary>
		/// Adds the vector 'b' to the current vector.
		/// </summary>
		/// <param name="b"></param>
		public Vector Add(Vector b)
		{
			MakeSameDimension(this, b);
			Vector add = new Vector(Dimension);
			for(int i = 0; i < Dimension; i++)
			{
				add.SetComponent(i, GetComponent(i) + b.GetComponent(i));
			}
			return add;
		}

		/// <summary>
		/// Subtracts the vector 'b' from the current vector.
		/// </summary>
		/// <param name="b"></param>
		public Vector Subtract(Vector b)
		{
			MakeSameDimension(this, b);
			Vector subtract = new Vector(Dimension);
			for (int i = 0; i < Dimension; i++)
			{
				subtract.SetComponent(i, GetComponent(i) - b.GetComponent(i));
			}
			return subtract;
		}

		/// <summary>
		/// Dots the vector b with the current vector.
		/// </summary>
		/// <param name="b"></param>
		public double Dot(Vector b, bool throwIfNotSameDimension)
		{
			if (throwIfNotSameDimension && Dimension != b.Dimension)
			{
				throw new DimensionException("Dot products require both vectors to have the same dimension");
			}
			double dot = 0;
			MakeSameDimension(this, b);
			for (int i = 0; i < Dimension; i++)
			{
				dot += GetComponent(i) * b.GetComponent(i);
			}
			return dot;
		}

		/// <summary>
		/// Crosses the vector 'b' with the current vector.
		/// </summary>
		/// <param name="b"></param>
		public Vector Cross(Vector b, bool throwIfNotTwoOrThreeDim = false)
		{
			if (throwIfNotTwoOrThreeDim && Dimension > 3 && b.Dimension > 3)
			{
				throw new DimensionException("Cross products require both vectors to have at most 3 dimensions.");
			}
			SetDimension(3);
			b.SetDimension(3);
			Vector cross = new Vector(3);
			cross.SetComponent(0, (GetComponent(1) * b.GetComponent(2)) - (GetComponent(2) * b.GetComponent(1)));
			cross.SetComponent(1, (GetComponent(2) * b.GetComponent(0)) - (GetComponent(0) * b.GetComponent(2)));
			cross.SetComponent(2, (GetComponent(0) * b.GetComponent(1)) - (GetComponent(1) * b.GetComponent(0)));
			return cross;
		}

		/// <summary>
		/// Scales up the current vector by the number 'b'. Equivalent to multiplying each component of the vector by 'b'.
		/// </summary>
		/// <param name="b"></param>
		public Vector Scalar(double b)
		{
			Vector scalar = new Vector(Dimension);
			for (int i = 0; i < Dimension; i++)
			{
				scalar.SetComponent(i, b * GetComponent(i));
			}
			return scalar;
		}

		public bool Equals(Vector b)
		{
			if (this.Dimension < b.Dimension)
			{
				for (int i = 0; i < b.Dimension; i++)
				{
					if (i < this.Dimension && this.GetComponent(i) == b.GetComponent(i))
						continue;
					else if (i >= this.Dimension && b.GetComponent(i) == 0)
						continue;
					else if (i < this.Dimension && this.GetComponent(i) != b.GetComponent(i))
						return false;
				}
			}
			else if (this.Dimension == b.Dimension)
			{
				for (int i = 0; i < this.Dimension; i++)
				{
					if (this.GetComponent(i) == b.GetComponent(i))
						continue;
					else if (this.GetComponent(i) != b.GetComponent(i))
						return false;
				}
			}
			else
			{
				for (int i = 0; i < this.Dimension; i++)
				{
					if (i < b.Dimension && this.GetComponent(i) == b.GetComponent(i))
						continue;
					else if (i >= b.Dimension && this.GetComponent(i) == 0)
						continue;
					else if (i < b.Dimension && this.GetComponent(i) != b.GetComponent(i))
						return false;
				}
			}

			return true;
		}
		#endregion
	}
}
