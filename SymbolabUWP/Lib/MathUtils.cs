using AngouriMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolabUWP.Lib
{
    public static class MathUtils
    {
        public static IEnumerable<double> FindVerticalAsymptotes(string funcString, string variName)
        {
            return FindVerticalAsymptotes(funcString, new VariableEntity(variName));
        }
        public static IEnumerable<double> FindVerticalAsymptotes(string funcString, VariableEntity vari)
        {
            Entity equation = $"1 / ({funcString})";
            EntitySet set = equation.SolveEquation(vari);
            return set.Select(s => s.GetValue().Re);
        }
    }
}
