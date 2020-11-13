using AngouriMath;
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
    }
}
