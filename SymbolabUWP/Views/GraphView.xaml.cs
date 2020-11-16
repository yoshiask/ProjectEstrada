using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Text.RegularExpressions;
using AngouriMath;
using System.Collections.Generic;
using System.Linq;
using SharpGen.Runtime;
using Vortice.DXGI;
using Vortice.Direct3D12;
using Vortice.Direct3D11;
using Windows.UI.Core;
using Vortice.Mathematics;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using System.Diagnostics;

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

                Bindings.Update();
            }
        }

        string _formulaLaTeX;
        private string FormulaLaTeX {
            get => _formulaLaTeX;
            set {
                _formulaLaTeX = value;
                Bindings.Update();
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

            //DrawToSwapChain();
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

            var d3dPanel = new ProjectEstrada_Graphics.D3DPanel();
            d3dPanel.StartRenderLoop();

            MainGrid.Children.Add(d3dPanel);
            Grid.SetRow(d3dPanel, 1);
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

        private struct PCVertex
        {
            public Point3 Position;
            public Point3 Color;
        }

        private void DrawToSwapChain()
        {
            var nativePanel = ComObject.QueryInterface<ISwapChainPanelNative>((IInspectable)SwapChainPanel);
            var desc = new SwapChainDescription1()
            {
                Width = 796,
                Height = 420,
                Format = Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Vortice.DXGI.Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipSequential,
                Flags = SwapChainFlags.None
            };

            var deviceFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
            deviceFlags |= DeviceCreationFlags.Debug;
#endif
            D3D11.D3D11CreateDevice(
                null,
                Vortice.Direct3D.DriverType.Hardware,
                deviceFlags,
                new Vortice.Direct3D.FeatureLevel[]
                {
                    Vortice.Direct3D.FeatureLevel.Level_11_0
                },
                out ID3D11Device device,
                out ID3D11DeviceContext context
            );

            // QI for DXGI device
            var dxgiDevice = device.QueryInterface<IDXGIDevice>();

#if DEBUG
            // Enable debug for the device
            //var debug = ComObject.As<IDXGIDebug>(device);
            //var info = ComObject.As<IDXGIInfoQueue>(debug);

#endif

            // Get the DXGI adapter
            dxgiDevice.GetAdapter(out var dxgiAdapter);

            // Get the DXGI factory
            var dxgiFactory = dxgiAdapter.GetParent<IDXGIFactory2>();

            // Create a swap chain by calling CreateSwapChainForComposition
            var swapChain = dxgiFactory.CreateSwapChainForComposition(dxgiDevice, desc);

            nativePanel.SetSwapChain(swapChain);

            context.IASetPrimitiveTopology(Vortice.Direct3D.PrimitiveTopology.PointList);

            var elementDesc = new Vortice.Direct3D11.InputElementDescription[]
            {
                new Vortice.Direct3D11.InputElementDescription()
                {
                    SemanticName = "POSITION",
                    Format = Format.R32G32B32_Float,
                    Classification = Vortice.Direct3D11.InputClassification.PerVertexData
                }
            };

            var mesh = new PCVertex[]
            {
                new PCVertex()
                {
                    Position = new Point3(-1, -1, -1),
                    Color = new Point3(0, 0, 0)
                },
                new PCVertex()
                {
                    Position = new Point3(-1, -1, 1),
                    Color = new Point3(0, 0, 255)
                },
                new PCVertex()
                {
                    Position = new Point3(-1, 1, -1),
                    Color = new Point3(0, 255, 0)
                },
            };

            //var shader = await LoadShader();

            try
            {
                var inputLayout = device.CreateInputLayout(elementDesc, new byte[] { });
            }
            catch (SharpGenException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            swapChain.Present(1, PresentFlags.Test);
        }

        private async Task<byte[]> LoadShader()
        {
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await InstallationFolder.GetFileAsync(@"Views\plastic.bin");
            return await File.ReadAllBytesAsync(file.Path);
        }
    }
}
