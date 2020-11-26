using AngouriMath;
using AngouriMath.Core;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProjectEstrada.Core
{
    public class Vector : IEquatable<Vector>, IList<Entity>, ILatexiseable
    {
		Entity[] vector;

		/// <summary>
		/// Gets the dimension of the vector.
		/// </summary>
		public int Dimension => vector.Length;

        public int Count => vector.Length;

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

		public Vector(string str)
        {
			// Make sure this is a string that can actually be parsed as a vector
			Guard.IsTrue(str.StartsWith("<") && str.EndsWith(">"), nameof(str));
			Guard.IsGreaterThan(str.Length, 2, nameof(str));

			// Strip the angle braces
			str = str.Remove(0, 1);
			str = str.Remove(str.Length - 1, 1);

			vector = str.Split(',').Select(s => MathS.FromString(s)).ToArray();
        }

		//

		/// <summary>
		/// Sets the dimension of the vector.
		/// </summary>
		/// <param name="dim"></param>
		public void SetDimension(int dim)
		{
			var oldVector = vector;
			int oldDim = Dimension;
			vector = new Entity[dim];
			Array.Copy(oldVector, vector, oldDim);
			//for (int i = 0; i < oldDim; i++)
            //{
			//	SetComponent(i, oldVector[i]);
            //}
			//for (int i = oldDim; i < dim; i++)
			//{
			//	SetComponent(i, 0);
			//}
		}

		/// <summary>
		/// Gets the value of a single component of the vector.
		/// </summary>
		/// <param name="index">The location of the coordinate. 0 is X, 1 is Y, 2 is Z, and so on</param>
		public Entity GetComponent(int index)
		{
			return vector[index];
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
			Vector axis = GetAxisVector(dir, Dimension);
			return MathS.Arccos(this * axis / GetMagnitude());
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

		public string Latexise()
		{
			return Latexise("<", ">");
		}
		public string Latexise(string left, string right)
        {
			return "\\left" + left + " " + String.Join(", ", vector.Select(c => c.Latexise())) + " \\right" + right;
		}

		public Entity[] ToArray()
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
			if (a.Dimension >= b.Dimension)
				b.SetDimension(a.Dimension);
			if (b.Dimension < a.Dimension)
				a.SetDimension(b.Dimension);
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
					axis.SetComponent(i, 1);
				else
					axis.SetComponent(i, 0);
			}
			return axis;
		}

		//

		public Vector Simplify()
        {
			var simple = new Entity[Dimension];
			for (int i = 0; i < Dimension; i++)
				simple[i] = vector[i].Simplify();
			return new Vector(simple);
        }

		public Vector Substitute(Entity x, Entity value)
        {
			var simple = new Entity[Dimension];
			for (int i = 0; i < Dimension; i++)
				simple[i] = vector[i].Substitute(x, value);
			return new Vector(simple);
		}

		// TODO: If AngouriMath creates an Entity.Vector, this should be removed
		public MathNet.Numerics.LinearAlgebra.Vector<double> EvalNumerical()
        {
			var numerical = new double[Dimension];
			for (int i = 0; i < Dimension; i++)
				numerical[i] = vector[i].EvalNumerical().ToNumerics().Real;
			return MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(numerical);
		}

		#region Operator Overloads
		public static Vector operator -(Vector a)
		{
			Vector negative = new Vector(a.Dimension);
			for (int i = 0; i < a.Dimension; i++)
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
			for (int i = 0; i < Dimension; i++)
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
			for (int i = 0; i < Dimension; i++)
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
			for (int i = 0; i < Dimension; i++)
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
			for (int i = 0; i < Dimension; i++)
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
		public Vector Scalar(Entity b)
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

		#region List
		public int IndexOf(Entity item)
		{
			for (int i = 0; i < vector.Length; i++)
            {
				if (vector[i] == item)
					return i;
            }
			return -1;
		}

		public void Insert(int index, Entity item)
		{
			SetDimension(Dimension + 1);
			var span = vector.AsSpan();
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		public void Add(Entity item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			var span = vector.AsSpan();
			span.Fill(0);
			vector = span.ToArray();
		}

		public bool Contains(Entity item)
		{
			foreach (Entity e in vector)
            {
				if (e == item)
					return true;
            }
			return false;
		}

		public void CopyTo(Entity[] array, int arrayIndex)
		{
			vector.CopyTo(array, arrayIndex);
		}

		public bool Remove(Entity item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<Entity> GetEnumerator()
		{
			return (IEnumerator<Entity>)vector.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return vector.GetEnumerator();
		}
        #endregion
    }
}
