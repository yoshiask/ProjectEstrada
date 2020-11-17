using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Text.RegularExpressions;
using AngouriMath;
using System.Collections.Generic;
using System.Linq;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SymbolabUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GraphView : Page
    {
        string _formula;
        private string FormulaText {
            get {
                return _formula;
            }
            set
            {
                _formula = value;
                Match match = Regex.Match(value, @"(?<fname>\S+)\((?<variable>\S+)\)\s*=+\s*(?<formula>[\S\s]+)", RegexOptions.Singleline);
                if (match.Success)
                {
                    string fname = match.Groups["fname"].Value;
                    string variable = match.Groups["variable"].Value;
                    string formula = match.Groups["formula"].Value;

                    Variable = MathS.Var(variable);
                    Function = MathS.FromString(formula).Simplify();
                }
                else
                {
                    Variable = MathS.Var("x");
                    Function = MathS.FromString(value).Simplify();
                }
                FormulaLaTeX = Function.Latexise();

                //Bindings.Update();
            }
        }

        string _formulaLaTeX;
        private string FormulaLaTeX {
            get => _formulaLaTeX;
            set {
                _formulaLaTeX = value;
                //Bindings.Update();
            }
        }

        private Entity _function;
        private Entity _functionFirst;
        private Entity Function
        {
            get => _function;
            set
            {
                _function = value;
                FunctionFirst = value.Differentiate(Variable).Simplify();

                // Get a list of intervals that are continuous
                // TODO: Do holes need to be handled as well?
                VerticalAsymptotes = Lib.MathUtils.FindVerticalAsymptotes(value, Variable).OrderBy(d => d).ToList();
                VerticalAsymptotes.Insert(0, double.MinValue);
                VerticalAsymptotes.Add(double.MaxValue);
                Intervals = Lib.MathUtils.AdjacentPairs(VerticalAsymptotes).ToArray();
            }
        }
        private Entity FunctionFirst
        {
            get => _functionFirst;
            set
            {
                _functionFirst = value;
                FunctionSecond = value.Differentiate(Variable).Simplify();
            }
        }
        private Entity FunctionSecond { get; set; }

        private Entity.Variable Variable { get; set; }

        private IList<double> VerticalAsymptotes;

        private IList<Tuple<double, double>> Intervals = new List<Tuple<double, double>>();

        public GraphView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Parameter as string))
            {
                //EquationBox.Text = (string)e.Parameter;
                FormulaLaTeX = (string)e.Parameter;
                FormulaText = Lib.ParseLaTeX.ConvertToMathString(FormulaLaTeX);
            }
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private float AnimOffset(float x)
        {
            return x + (DateTime.Now.Millisecond / 5);
        }

        private Vector2 Evaluate(Func<float, float> func, float x)
        {
            return new Vector2(x, func(x));
        }
        private Vector2 Evaluate(float x, Entity func, Entity.Variable varEn)
        {
            return new Vector2(x, -(float)func.Substitute(varEn, x).EvalNumerical().ToNumerics().Real);
        }

        private void GraphCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            float height = (float)sender.Size.Height;
            float width = (float)sender.Size.Width;
            float yOffset = height / 2;
            float xOffset = width / 2;

            // Draw x-axis
            //args.DrawingSession.DrawLine(
            //    new Vector2(0, yOffset),
            //    new Vector2(width, yOffset),
            //    Colors.Gray, 5
            //);
            //// Draw y-axis
            //args.DrawingSession.DrawLine(
            //    new Vector2(xOffset, 0),
            //    new Vector2(xOffset, height),
            //    Colors.Gray, 5
            //);

            var offset = new Vector2(xOffset, yOffset);
            foreach (Tuple<double, double> interval in Intervals)
            {
                float x_min = (float)Math.Max(interval.Item1 + 1, -xOffset);
                float x_max = (float)Math.Min(interval.Item2, xOffset);

                var pathBuilder = new CanvasPathBuilder(args.DrawingSession);
                pathBuilder.BeginFigure(Evaluate(x_min, Function, Variable));
                for (float x = x_min; x < x_max; x++)
                {
                    try
                    {
                        pathBuilder.AddLine(
                            Evaluate(x, Function, Variable) + offset
                        );
                    }
                    catch (Exception ex)
                    {
                        pathBuilder.AddLine(offset);
                    }
                    //pathBuilder.AddLine(new Vector2(
                    //    x + xOffset,
                    //    SinFunc(AnimOffset(x)) + yOffset
                    //));
                }
                pathBuilder.EndFigure(CanvasFigureLoop.Open);
                //args.DrawingSession.DrawGeometry(
                //    CanvasGeometry.CreatePath(pathBuilder),
                //    Colors.White,
                //    5
                //);
            }

            args.DrawingSession.Flush();
        }
    }
}
