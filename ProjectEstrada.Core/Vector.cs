using AngouriMath;
using AngouriMath.Core;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProjectEstrada.Core
{
    public class Vector : IEquatable<Vector>
    {
		Entity[] vector;

		/// <summary>
		/// Gets the dimension of the vector.
		/// </summary>
		public Constants.Axis Dimension => (Constants.Axis)Size;

        public int Size => vector.Length;

        public bool IsReadOnly => false;

        public Entity this[int index] { get => GetComponent(index); set => SetComponent(index, value); }

        /// <summary>
        /// Creates a vector given a point or a list of values.
        /// </summary>
        /// <param name="vals"></param>
        public Vector(params Entity[] vals)
		{
			vector = vals;
		}

		/// <summary>
		/// Creates an empty vector with the given dimension.
		/// </summary>
		/// <param name="dim"></param>
		public Vector(int dim)
		{
			vector = new Entity[dim];
		}
		public Vector(Constants.Axis dim)
		{
			vector = new Entity[(int)dim];
		}

		public Vector(string str)
        {
			Guard.IsTrue(str.StartsWith("<") && str.EndsWith(">"), nameof(str));
			Guard.IsGreaterThan(str.Length, 2, nameof(str));

			vector = str.Split(',').Cast<Entity>().ToArray();
        }

		//

		/// <summary>
		/// Sets the dimension of the vector.
		/// </summary>
		/// <param name="dim"></param>
		public void SetDimension(int dim)
		{
			var oldVector = vector;
			int oldDim = Size;
			vector = new Entity[dim];
			Array.Copy(oldVector, vector, oldDim);
		}
		public void SetDimension(Constants.Axis dim)
		{
			SetDimension((int)dim);
		}

		/// <summary>
		/// Gets the value of a single component of the vector.
		/// </summary>
		/// <param name="index">The location of the coordinate. 0 is X, 1 is Y, 2 is Z, and so on</param>
		public Entity GetComponent(int index)
		{
			return vector[index];
		}
		public Entity GetComponent(Constants.Axis index)
		{
			return GetComponent((int)index);
		}

		/// <summary>
		/// Sets the value of a single component of the vector.
		/// </summary>
		/// <param name="index">The location of the coordinate. 0 is X, 1 is Y, 2 is Z, and so on</param>
		/// <param name="v">The new value of the component</param>
		public void SetComponent(int index, Entity v)
		{
			vector[index] = v;
		}
		public void SetComponent(Constants.Axis index, Entity v)
		{
			SetComponent((int)index, v);
		}

		/// <summary>
		/// Gets the length, or magnitude, of the vector.
		/// </summary>
		public Entity GetMagnitude()
		{
			Entity sum = 0;
			foreach (Entity v in vector)
			{
				sum += v.Pow(2);
			}
			return sum.Pow(1/2);
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
		public Entity GetAngleWithAxis(int dir)
		{
			Vector axis = Constants.GetAxisVector(dir);
			return MathS.Arccos(this * axis / GetMagnitude());
		}
		public Entity GetAngleWithAxis(Constants.Axis dir)
		{
			return GetAngleWithAxis((int)dir);
		}

		/// <summary>
		/// Gets the angle of the vector with the x-axis.
		/// </summary>
		public Entity GetAngleWithX()
		{
			return GetAngleWithAxis((int)Constants.Axis.X);
		}

		/// <summary>
		/// Gets the angle of the vector with the y-axis.
		/// </summary>
		public Entity GetAngleWithY()
		{
			return GetAngleWithAxis((int)Constants.Axis.Y);
		}

		/// <summary>
		/// Gets the angle of the vector with the z-axis.
		/// </summary>
		public Entity GetAngleWithZ()
		{
			return GetAngleWithAxis((int)Constants.Axis.Z);
		}

		//

		public override string ToString()
		{
			return "<" + String.Join(", ", vector.Select(c => c.ToString())) + ">";
		}

		public Entity[] ToArray()
		{
			return vector;
		}

		/// <summary>
		/// Takes in two vectors and makes them the same length by padding the shorter vector with zeros until both vectors are the same dimension.
		/// </summary>
		public static void MakeSameDimension(Vector a, Vector b)
		{
			if (a.Size >= b.Size)
				b.SetDimension(a.Size);
			if (b.Size < a.Size)
				a.SetDimension(b.Size);
		}

		//

		public Vector Simplify()
        {
			var simple = new Entity[Size];
			for (int i = 0; i < Size; i++)
				simple[i] = vector[i].Simplify();
			return new Vector(simple);
        }

		public Vector Substitute(Entity x, Entity value)
        {
			var simple = new Entity[Size];
			for (int i = 0; i < Size; i++)
				simple[i] = vector[i].Substitute(x, value);
			return new Vector(simple);
		}

		// TODO: If AngouriMath creates an Entity.Vector, this should be removed
		public MathNet.Numerics.LinearAlgebra.Vector<double> EvalNumerical()
        {
			var numerical = new double[Size];
			for (int i = 0; i < Size; i++)
				numerical[i] = vector[i].EvalNumerical().ToNumerics().Real;
			return MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(numerical);
		}

		#region Operator Overloads
		public static Vector operator -(Vector a)
		{
			Vector negative = new Vector(a.Dimension);
			for (int i = 0; i < a.Size; i++)
			{
				negative.SetComponent(i, -1 * a.GetComponent(i));
			}
			return negative;
		}
		public static Vector operator +(Vector a, Vector b)
		{
			return a.Add(b);
		}
		public static Vector operator +(Vector a, Entity b)
		{
			return a.AddScalar(b);
		}
		public static Vector operator +(Entity a, Vector b)
		{
			return b.AddScalar(a);
		}
		public static Vector operator -(Vector a, Vector b)
		{
			return a.Subtract(b);
		}
		public static Vector operator -(Vector a, Entity b)
		{
			return a.SubtractScalar(b);
		}
		public static Vector operator -(Entity a, Vector b)
		{
			return b.SubtractScalar(a);
		}
		public static Entity operator *(Vector a, Vector b)
		{
			return a.Dot(b, false);
		}
		public static Vector operator *(Vector a, Entity b)
		{
			return a.Scalar(b);
		}
		public static Vector operator *(Entity a, Vector b)
        {
			return b.Scalar(a);
        }
		public static Vector operator /(Vector a, Entity b)
		{
			return a * (1 / b);
		}

		public static implicit operator Vector(string str)
        {
			return new Vector(str);
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
			for (int i = 0; i < Size; i++)
			{
				add.SetComponent(i, GetComponent(i) + b.GetComponent(i));
			}
			return add;
		}

		/// <summary>
		/// Adds the number 'b' to the vector. Equivalent to adding 'b' to each component of the vector.
		/// </summary>
		public Vector AddScalar(Entity b)
        {
			Vector add = new Vector(Dimension);
			for (int i = 0; i < Size; i++)
			{
				add.SetComponent(i, GetComponent(i) + b);
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
			for (int i = 0; i < Size; i++)
			{
				subtract.SetComponent(i, GetComponent(i) - b.GetComponent(i));
			}
			return subtract;
		}

		/// <summary>
		/// Subtracts the number 'b' to the vector. Equivalent to subtracting 'b' from each component of the vector.
		/// </summary>
		public Vector SubtractScalar(Entity b)
		{
			Vector subtract = new Vector(Dimension);
			for (int i = 0; i < Size; i++)
			{
				subtract.SetComponent(i, GetComponent(i) - b);
			}
			return subtract;
		}

		/// <summary>
		/// Dots the vector b with the current vector.
		/// </summary>
		/// <param name="b"></param>
		public Entity Dot(Vector b, bool throwIfNotSameDimension)
		{
			if (throwIfNotSameDimension && Dimension != b.Dimension)
			{
				throw new DimensionException("Dot products require both vectors to have the same dimension");
			}
			Entity dot = 0;
			MakeSameDimension(this, b);
			for (int i = 0; i < Size; i++)
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
			if (throwIfNotTwoOrThreeDim && Size > 3 && b.Size > 3)
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
		public Vector Scalar(Entity b)
		{
			Vector scalar = new Vector(Dimension);
			for (int i = 0; i < Size; i++)
			{
				scalar.SetComponent(i, b * GetComponent(i));
			}
			return scalar;
		}

		public bool Equals(Vector b)
		{
			if (this.Dimension < b.Dimension)
			{
				for (int i = 0; i < b.Size; i++)
				{
					if (i < this.Size && this.GetComponent(i) == b.GetComponent(i))
						continue;
					else if (i >= this.Size && b.GetComponent(i) == 0)
						continue;
					else if (i < this.Size && this.GetComponent(i) != b.GetComponent(i))
						return false;
				}
			}
			else if (this.Dimension == b.Dimension)
			{
				for (int i = 0; i < this.Size; i++)
				{
					if (this.GetComponent(i) == b.GetComponent(i))
						continue;
					else if (this.GetComponent(i) != b.GetComponent(i))
						return false;
				}
			}
			else
			{
				for (int i = 0; i < this.Size; i++)
				{
					if (i < b.Size && this.GetComponent(i) == b.GetComponent(i))
						continue;
					else if (i >= b.Size && this.GetComponent(i) == 0)
						continue;
					else if (i < b.Size && this.GetComponent(i) != b.GetComponent(i))
						return false;
				}
			}

			return true;
		}
		#endregion
	}
}
