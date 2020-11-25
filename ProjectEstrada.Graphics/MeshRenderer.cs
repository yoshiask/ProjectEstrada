using ProjectEstrada.Graphics.Helpers;

namespace ProjectEstrada.Graphics
{
    public class MeshRenderer : RendererBase
    {
        public MeshRenderer(Mesh mesh)
        {
            VertexPositions = mesh.VertexPositions;
            VertexColors = mesh.VertexColors;
            TriangleIndicies = mesh.Triangles;

            Initialize();
        }

        ~MeshRenderer()
        {
            Deconstruct();
        }
    }
}
