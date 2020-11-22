using AngouriMath;
using ProjectEstrada.Core.Functions;
using ProjectEstrada.Graphics.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
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

        static ScalarFunction scalar = new ScalarFunction()
        {
            FunctionBody = "x*y^2",
            Inputs = new List<Entity.Variable>()
            {
                "x", "y"
            }
        };

        public GraphControl()
        {
            this.InitializeComponent();

            var plane = Mesh.CreateSquarePlane((1f, 1f), 5);

            plane.VertexPositions = plane.VertexPositions.Select(v =>
            {
                v.Y = (float)scalar.Evaluate(v.X, v.Z);
                return v;
            }).ToList();

            // (v.Y + 1) / 2 + 0.25f, 0f, 2 / (v.Y + 1) - 0.25f
            plane.VertexColors = plane.VertexPositions.Select(v => new Vector3(
                MathHelper.MapRange(v.Y, -1, 1, 0, 1), 0f,  1 / MathHelper.MapRange(v.Y, -1, 1, 0, 1) + 0.35f
            )).ToList();

            Content = new GLUWPControl(() => new MeshRenderer(plane));

            Loaded += GraphControl_Loaded;
        }

        private void GraphControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
