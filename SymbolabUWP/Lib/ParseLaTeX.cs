using AngouriMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SymbolabUWP.Lib
{
    public static class ParseLaTeX
    {
        public static Entity ParseExpression(string latexIn)
        {
            return MathS.FromString(ConvertToAngouriMathString(latexIn));
        }

        public static string ConvertToAngouriMathString(string latexIn)
        {
            string output = "";
            var matches = Regex.Matches(latexIn, @"\\?(\{.*\}|[^\\\s])*");
            foreach (Match m in matches)
            {
                string lPart = m.Value;
                if (lPart.Length == 0)
                    continue;

                if (lPart.StartsWith(@"\cdot"))
                {
                    lPart = lPart.Replace(@"\cdot", "*");
                }
                else if (lPart.StartsWith(@"\sin") || lPart.StartsWith(@"\cos") || lPart.StartsWith(@"\tan") ||
                    lPart.StartsWith(@"\arcsin") || lPart.StartsWith(@"\arccos") || lPart.StartsWith(@"\arctan") ||
                    lPart.StartsWith(@"\csc") || lPart.StartsWith(@"\sec") || lPart.StartsWith(@"\cot"))
                {
                    lPart = lPart.Remove(0, 1); // Just remove the slash
                }
                else if (lPart.StartsWith(@"\left"))
                {
                    lPart = lPart.Remove(0, @"\left".Length);
                }
                else if (lPart.StartsWith(@"\right"))
                {
                    lPart = lPart.Remove(0, @"\right".Length);
                }
                else if (lPart.StartsWith(@"\over"))
                {
                    lPart = lPart.Replace(@"\over", "/");
                }
                else if (lPart.StartsWith(@"\frac"))
                {
                    var parameters = ParseParameters(lPart);

                    // Handle derivatives
                    if (parameters.Count >= 3 && parameters[0].StartsWith("d") && parameters[1].StartsWith("d"))
                    {
                        // Get the variable to derive with respect to
                        string varName = Regex.Match(parameters[1], @"d(\S)$").Value.Substring(1);
                        var varWRT = new VariableEntity(varName);

                        // Derive the function
                        var derFunc = MathS.FromString(parameters[2]).Derive(varWRT);
                        lPart = derFunc.Simplify().ToString();
                    }
                    else
                    {
                        lPart = $"({parameters[0]})/({parameters[1]})";
                    }
                }
                else if (lPart.StartsWith(@"\sqrt"))
                {
                    var parameters = ParseParameters(lPart);
                    lPart = $"sqrt({parameters[0]})";
                }
                else if (lPart.StartsWith(@"\int"))
                {
                    // User must put paratheses around the expression to integrate
                    var parameters = ParseParameters(lPart);
                    if (parameters.Count == 3)
                    {
                        // Get the expression to integrate and prepare for AngouriMath
                        string math = ConvertToAngouriMathString(parameters[2]);

                        // Get the variable to integrate with respect to
                        var varName = Regex.Match(parameters[2], @"d(\S)$").Value.Substring(1);
                        var varWRT = new VariableEntity(varName);

                        // TODO: Support expressions for start and end
                        // Get start and end of interval
                        double start = Double.Parse(parameters[0]);
                        double end = Double.Parse(parameters[1]);

                        var intFunc = MathS.FromString(
                            math.Substring(0, math.Length - varName.Length - 1)
                        ).DefiniteIntegral(
                            varWRT,
                            new AngouriMath.Core.Number(start),
                            new AngouriMath.Core.Number(end)
                        );
                        lPart = intFunc.ToString();
                    }
                    else
                    {
                        // Get the expression to integrate and prepare for AngouriMath
                        string math = ConvertToAngouriMathString(
                            lPart.Remove(0, @"\int".Length).Remove(m.Value.Length - 2)
                        );
                        var expr = MathS.FromString(math);
                        throw new NotImplementedException("AngouriMath does not support indefinite integrals yet");
                    }
                }
                output += lPart;
            }
            return output;
        }

        /// <summary>
        /// Parses a LaTeX command and returns a list of AngouriMath-ready expressions
        /// </summary>
        public static List<string> ParseParameters(string latexIn)
        {
            var matches = Regex.Matches(latexIn, @"\{(.*?)\}");
            List<string> parameters = new List<string>(matches.Count);
            foreach (Match m in matches)
            {
                // Remove curly brackets
                string latex = m.Value.Remove(0, 1).Remove(m.Value.Length - 2);
                parameters.Add(ConvertToAngouriMathString(latex));
            }
            return parameters;
        }
    }
}
