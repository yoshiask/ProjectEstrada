using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Text.RegularExpressions;
using AngouriMath;

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
            set {
                _formula = value;
                Bindings.Update();
            }
        }
        private string FormulaLaTeX {
            get {
                return _formula;
            }
            set {
                _formula = value;
                Bindings.Update();
            }
        }

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
                FormulaText = Lib.ParseLaTeX.ConvertToAngouriMathString(FormulaLaTeX);
            }
            base.OnNavigatedTo(e);
        }

        private float SinFunc(float x)
        {
            return (float)(
                25 * Math.Sin(x/15) * Math.Cos(x/3)
            );
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
            args.DrawingSession.DrawLine(
                new Vector2(0, yOffset),
                new Vector2(width, yOffset),
                Colors.Gray, 5
            );
            // Draw y-axis
            args.DrawingSession.DrawLine(
                new Vector2(xOffset, 0),
                new Vector2(xOffset, height),
                Colors.Gray, 5
            );

            // Parse the supplied function
            Entity.Variable vari;
            Entity func;
            Entity funcD1; // First derivative of func
            Entity funcD2; // Second derivative of func
            Match match = Regex.Match(FormulaText,
                @"(?<fname>\S+)\((?<variable>\S+)\)\s*=+\s*(?<formula>[\S\s]+)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                string fname = match.Groups["fname"].Value;
                string variable = match.Groups["variable"].Value;
                string formula = match.Groups["formula"].Value;

                vari = MathS.Var(variable);
                func = MathS.FromString(formula);
                funcD1 = func.Differentiate(vari).Simplify();
                funcD2 = funcD1.Differentiate(vari).Simplify();
            }
            else
            {
                vari = MathS.Var("x");
                func = MathS.FromString(FormulaText);
                funcD1 = func.Differentiate(vari).Simplify();
                funcD2 = funcD1.Differentiate(vari).Simplify();
            }

            var offset = new Vector2(xOffset, yOffset);
            var pathBuilder = new CanvasPathBuilder(args.DrawingSession);
            pathBuilder.BeginFigure(-xOffset, SinFunc(-xOffset) + yOffset);
            for (float x = -xOffset; x < xOffset; x++)
            {
                try
                {
                    pathBuilder.AddLine(
                        Evaluate(x, func, vari) + offset
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
            args.DrawingSession.DrawGeometry(
                CanvasGeometry.CreatePath(pathBuilder),
                Colors.White,
                5
            );

            args.DrawingSession.Flush();
        }
    }
}
