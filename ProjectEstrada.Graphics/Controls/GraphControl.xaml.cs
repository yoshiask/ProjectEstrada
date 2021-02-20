using AngouriMath;
using ProjectEstrada.Core.Functions;
using ProjectEstrada.Core.Helpers;
using ProjectEstrada.Graphics.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MathHelper = ProjectEstrada.Core.Helpers.MathHelper;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectEstrada.Graphics.Controls
{
    public sealed partial class GraphControl : UserControl
    {
        public ObservableCollection<string> Functions { get; } = new ObservableCollection<string>();

        static GenericFunction scalar = ParseLaTeX.ParseFunction(@"f(x,y) = xy^2");

        //public GraphControl()
        //{
        //    this.InitializeComponent();

        //    var plane = Helpers.Mesh.CreateSquarePlane((1f, 1f), 5);

        //    plane.VertexPositions = plane.VertexPositions.Select(v =>
        //    {
        //        // TODO: Switch to the following:
        //        // v = scalar.Parameterize().EvaluateAsVector3(v.X, v.Z);
        //        v.Y = (float)scalar.Evaluate(v.X, v.Z)[0];
        //        return v;
        //    }).ToList();

        //    // (v.Y + 1) / 2 + 0.25f, 0f, 2 / (v.Y + 1) - 0.25f
        //    plane.VertexColors = plane.VertexPositions.Select(v => new Vector3(
        //        MathHelper.MapRange(v.Y, -1, 1, 0, 1), 0f,  1 / MathHelper.MapRange(v.Y, -1, 1, 0, 1) + 0.35f
        //    )).ToList();

        //    Content = new GLUWPControl(() => new MeshRenderer(plane));
        //}

        // An image source derived from SurfaceImageSource, used to draw DirectX content

        public DXImageSource DXDrawing { get; set; }

        public GraphControl()
        {
            this.InitializeComponent();
            DXCanvas.Loaded += ImageCanvas_Loaded;
            DXCanvas.SizeChanged += DXCanvas_SizeChanged;
        }

        private void DXCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        private void ImageCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            DXDrawing = new DXImageSource((int)DXCanvas.ActualWidth, (int)DXCanvas.ActualHeight, true);

            DXCanvas.Background = new ImageBrush()
            {
                ImageSource = DXDrawing
            };

            CompositionTarget.Rendering += AdvanceAnimation;
        }

        ~GraphControl()
        {
            CompositionTarget.Rendering -= AdvanceAnimation;
        }

        void AdvanceAnimation(object sender, object e)
        {
            // Begin updating the SurfaceImageSource
            DXDrawing.BeginDraw();

            // Clear background
            DXDrawing.Clear(Colors.CornflowerBlue);

            // Render next animation frame
            DXDrawing.RenderNextAnimationFrame();

            // Stop updating the SurfaceImageSource and draw the new frame
            DXDrawing.EndDraw();
        }
    }
}
