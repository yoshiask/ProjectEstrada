using ProjectEstrada.Graphics.Helpers;
using System;
using System.Linq;

namespace ProjectEstrada.Graphics
{
    public class MeshRenderer : RendererBase
    {
        public MeshRenderer(Mesh mesh)
        {
            VertexPositions = mesh.VertexPositions;

            VertexColors = mesh.VertexColors;

            TriangleIndicies = mesh.Triangles.Select(t => new Tuple<short, short, short>(
                (short)t.Item1, (short)t.Item2, (short)t.Item3
            )).ToList();

            Initialize();
        }

        ~MeshRenderer()
        {
            Deconstruct();
        }
    }
}
