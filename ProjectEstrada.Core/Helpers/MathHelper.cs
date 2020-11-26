using AngouriMath;
using CSharpMath.Atom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using VectorNum = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace ProjectEstrada.Core.Helpers
{
    public static class MathHelper
    {
        public static T[] Flatten<T>(T[,] twoDim)
        {
            T[] oneDim = new T[twoDim.Length];
            int stride = twoDim.GetLength(0);
            for (int i = 0; i < stride; i++)
            {
                for (int j = 0; j < twoDim.GetLength(1); j++)
                {
                    oneDim[i + j * stride] = twoDim[i, j];
                }
            }
            //Buffer.BlockCopy(twoDim, 0, oneDim, 0, twoDim.Length);
            return oneDim;
        }

        /// <summary>
        /// Returns a list of evenly-spaced numbers over a specified interval.
        /// Equivalent to np.linspace()
        /// </summary>
        public static IEnumerable<double> LinSpace(double start, double end, int partitions)
        {
            return Enumerable.Range(0, partitions + 1).Select(idx => idx != partitions
                    ? start + (end - start) / partitions * idx
                    : end);
        }

        public static float MapRange(float x, float inMin, float inMax, float outMin, float outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        public static double LinearInterpolation(double a, double b, double t)
        {
            return (1 - t) * a + t * b;
        }

        public static Vector2 BilinearInterpolation(Vector2 a, Vector2 b, double t)
        {
            return a + (float)t * (b - a);
        }

        public static Vector3 TrilinearInterpolation(Vector3 a, Vector3 b, double t)
        {
            return a + (float)t * (b - a);
        }

        public static VectorNum NDimensionalLinearInterpolation(VectorNum a, VectorNum b, double t)
        {
            return a + (float)t * (b - a);
        }
        public static Vector NDimensionalLinearInterpolation(Vector a, Vector b, double t)
        {
            return a + t * (b - a);
        }

        public static IEnumerable<double> FindVerticalAsymptotes(string funcString, string variName)
        {
            return FindVerticalAsymptotes(funcString, MathS.Var(variName));
        }
        public static IEnumerable<double> FindVerticalAsymptotes(string funcString, Entity.Variable vari)
        {
            Entity equation = MathS.FromString($"1 / ({funcString})").Simplify();
            Entity.Set set = equation.SolveEquation(vari);
            return set.DirectChildren.Select(s =>
            {
                var val = s.EvalNumerical().ToNumerics().Real;
                return val;
            });
        }
        public static IEnumerable<double> FindVerticalAsymptotes(Entity func, Entity.Variable vari)
        {
            Entity equation = (1 / func).Expand().Simplify();
            Entity.Set set = equation.SolveEquation(vari);
            return set.DirectChildren.SelectMany(e => e.DirectChildren.Append(e)).Where(e => e.EvaluableNumerical).Select(s =>
            {
                var val = s.EvalNumerical().ToNumerics().Real;
                return val;
            });
        }

        public static IEnumerable<Tuple<T, T>> AdjacentPairs<T>(IList<T> items)
        {
            if (items.Count == 2)
            {
                yield return new Tuple<T, T>(items[0], items[1]);
                yield break;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (i + 1 < items.Count)
                    yield return new Tuple<T, T>(items[i], items[i + 1]);
            }
        }

        public static IEnumerable<Tuple<T, T>> AdjacentPairsLoop<T>(IList<T> items)
        {
            if (items.Count == 2)
            {
                yield return new Tuple<T, T>(items[0], items[1]);
                yield break;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (i + 1 < items.Count)
                    yield return new Tuple<T, T>(items[i], items[i + 1]);
                else
                    yield return new Tuple<T, T>(items[i], items[0]);
            }
        }

        public static List<List<T>> Split<T>(this IList<T> list, T splitItem) where T : IEquatable<T>
        {
            var splitList = new List<List<T>>();

            var sublist = new List<T>();
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item.Equals(splitItem))
                {
                    splitList.Add(sublist);
                    sublist = new List<T>();
                }
                else
                {
                    sublist.Add(item);
                }
            }

            return splitList;
        }

        public static List<MathList> Split(this MathList list, MathAtom splitAtom)
        {
            var splitList = new List<MathList>();

            var sublist = new MathList();
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                bool isSplitItem = item == splitAtom;
                if (!isSplitItem)
                {
                    sublist.Add(item);
                }

                if (isSplitItem || i + 1 == list.Count)
                {
                    splitList.Add(sublist);
                    sublist = new MathList();
                }
            }

            return splitList;
        }
    }
}
