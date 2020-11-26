using AngouriMath;
using CSharpMath.Atom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SymbolabUWP.Lib
{
    public static class MathUtils
    {
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
