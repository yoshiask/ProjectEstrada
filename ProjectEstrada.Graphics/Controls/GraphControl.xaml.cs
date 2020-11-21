using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectEstrada.Graphics.Controls
{
    public sealed partial class GraphControl : UserControl
    {
        public ObservableCollection<string> Functions { get; } = new ObservableCollection<string>();

        Func<Vector3, Vector3> Function = (Vector3 v) => new Vector3(v.X, v.Y, v.Z * 0.5f);

        public GraphControl()
        {
            this.InitializeComponent();

            var VertexPositions = new List<Vector3>()
            {
                new Vector3(-1.0f, -1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f,  1.0f),
                new Vector3(-1.0f,  1.0f, -1.0f),
                new Vector3(-1.0f,  1.0f,  1.0f),
                new Vector3( 1.0f, -1.0f, -1.0f),
                new Vector3( 1.0f, -1.0f,  1.0f),
                new Vector3( 1.0f,  1.0f, -1.0f),
                new Vector3( 1.0f,  1.0f,  1.0f),
            }.Select(v => Function(v)).ToList();

            var VertexColors = new List<Vector3>()
            {
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 1.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 0.0f),
                new Vector3(1.0f, 1.0f, 1.0f),
            };

            var TriangleIndicies = new List<Tuple<short, short, short>>
            {
                new Tuple<short, short, short>(0, 1, 2), // -x
                new Tuple<short, short, short>(1, 3, 2),

                new Tuple<short, short, short>(4, 6, 5), // +x
                new Tuple<short, short, short>(5, 6, 7),

                new Tuple<short, short, short>(0, 5, 1), // -y
                new Tuple<short, short, short>(0, 4, 5),

                new Tuple<short, short, short>(2, 7, 6), // +y
                new Tuple<short, short, short>(2, 3, 7),

                new Tuple<short, short, short>(0, 6, 4), // -z
                new Tuple<short, short, short>(0, 2, 6),

                new Tuple<short, short, short>(1, 7, 3), // +z
                new Tuple<short, short, short>(1, 5, 7),
            };

            Content = new GLUWPControl(() => new RendererBase(VertexPositions, VertexColors, TriangleIndicies));

            Loaded += GraphControl_Loaded;
        }

        private void GraphControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
