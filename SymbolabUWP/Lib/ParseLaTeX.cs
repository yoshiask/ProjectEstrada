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
            var matches = Regex.Matches(latexIn.Replace(@"\ ", " "), @"\\?(\{.*\}|[^\\\s])*");
            foreach (Match m in matches)
            {
                string lPart = m.Value;
                if (lPart.Length == 0)
                    continue;

                if (Char.IsDigit(lPart[0]) && Char.IsLetter(lPart.Last()) && lPart.All(c => Char.IsLetterOrDigit(c)))
                {
                    int startOfVar = 1;
                    while (Char.IsDigit(lPart[startOfVar]))
                    {
                        if (startOfVar + 1 < lPart.Length)
                            startOfVar++;
                    }
                    lPart = lPart.Insert(startOfVar, "*");
                }
                else if (Char.IsLetter(lPart[0]) && Char.IsDigit(lPart.Last()) && lPart.All(c => Char.IsLetterOrDigit(c)))
                {
                    int startOfNum = 1;
                    while (Char.IsLetter(lPart[startOfNum]))
                    {
                        if (startOfNum + 1 < lPart.Length)
                            startOfNum++;
                    }
                    lPart = lPart.Insert(startOfNum, "*");
                }

                else if (!lPart.StartsWith("\\"))
                {
                    // Do nothing, but skip the branches that check for LaTeX commands
                }
                else if (lPart.StartsWith(@"\,") || lPart.StartsWith(@"\:") || lPart.StartsWith(@"\;") ||
                    lPart.StartsWith(@"\,") || lPart.StartsWith(@"\ "))
                {
                    // Replace the LaTeX command with a space
                    lPart = " " + lPart.Remove(0, 2);
                }
                else if (lPart.StartsWith(@"\quad"))
                {
                    lPart = "  " + lPart.Remove(0, @"\quad".Length);
                }
                else if (lPart.StartsWith(@"\qquad"))
                {
                    lPart = "   " + lPart.Remove(0, @"\qquad".Length);
                }
                else if (lPart.StartsWith(@"\cdot"))
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
                        var varWRT = MathS.Var(varName);

                        // Derive the function with respect to varWRT
                        var derFunc = MathS.FromString(parameters[2], true).Differentiate(varWRT);
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
                        string expression = parameters[2];

                        // Get the variable to integrate with respect to
                        string varName = Regex.Match(expression, @"d(\S)$").Value;
                        if (!String.IsNullOrEmpty(varName))
                        {
                            expression = expression.Replace(varName, String.Empty);
                            varName = varName.Substring(1);
                        }
                        else
                            varName = "x";
                        var varWRT = MathS.Var(varName);

                        // TODO: Support expressions for start and end
                        // Get start and end of interval
                        double start = Double.Parse(parameters[0]);
                        double end = Double.Parse(parameters[1]);

                        var intFunc = MathS.FromString(expression).Integrate(varWRT);
                        lPart = (intFunc.Substitute(varWRT, end) - intFunc.Substitute(varWRT, start)).Simplify().ToString();
                    }
                    else if (parameters.Count == 1)
                    {
                        string expression = parameters[0];

                        // Get the variable to integrate with respect to
                        string varName = Regex.Match(expression, @"d(\S)$").Value;
                        if (!String.IsNullOrEmpty(varName))
                        {
                            expression = expression.Replace(varName, String.Empty);
                            varName = varName.Substring(1);
                        }
                        else
                            varName = "x";
                        var varWRT = MathS.Var(varName);

                        lPart = MathS.FromString(expression).Integrate(varWRT).Simplify().ToString();
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
