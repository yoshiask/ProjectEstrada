using AngouriMath;
using ProjectEstrada.Core.Functions;
using ProjectEstrada.Core.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Windows.UI.Xaml.Controls;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectEstrada.Graphics.Controls
{
    public sealed partial class GraphControl : UserControl
    {
        public ObservableCollection<string> Functions { get; } = new ObservableCollection<string>();

        static GenericFunction scalar = new GenericFunction()
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

            var plane = Helpers.Mesh.CreateSquarePlane((1f, 1f), 5);

            plane.VertexPositions = plane.VertexPositions.Select(v =>
            {
                v.Y = (float)scalar.Evaluate(v.X, v.Z)[0];
                return v;
            }).ToList();

            // (v.Y + 1) / 2 + 0.25f, 0f, 2 / (v.Y + 1) - 0.25f
            plane.VertexColors = plane.VertexPositions.Select(v => new Vector3(
                MathHelper.MapRange(v.Y, -1, 1, 0, 1), 0f,  1 / MathHelper.MapRange(v.Y, -1, 1, 0, 1) + 0.35f
            )).ToList();

            Content = new GLUWPControl(() => new MeshRenderer(plane));
        }
    }
}
