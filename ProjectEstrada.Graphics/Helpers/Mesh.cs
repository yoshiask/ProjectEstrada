using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Windows.Foundation;

namespace ProjectEstrada.Graphics.Helpers
{
    public class Mesh
    {
        public List<Vector3> VertexPositions { get; set; } = new List<Vector3>();
        public List<Vector3> VertexColors { get; set; } = new List<Vector3>();
        public List<Vector3> VertexNormals { get; set; } = new List<Vector3>();
        public List<Tuple<short, short, short>> Triangles { get; set; } = new List<Tuple<short, short, short>>();

        public static Mesh FromObj(Stream stream)
        {
            var mesh = new Mesh();
            var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] parts = line.Split(' ');
                switch (parts[0])
                {
                    // Geometric vertices
                    case "v":
                        mesh.VertexPositions.Add(new Vector3(
                            float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])
                        ));
                        break;

                    // Vertex normals
                    case "vn":
                        mesh.VertexNormals.Add(new Vector3(
                            float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])
                        ));
                        break;

                    // Polygonal face element
                    case "f":
                        // For each vertex referenced...
                        for (int i = 1; i <= 3; i++)
                        {
                            string[] vIdxStr = parts[i].Split('/');
                            if (vIdxStr.Length == 1)
                            {
                                mesh.Triangles.Add(new Tuple<short, short, short>(
                                    short.Parse(parts[1]), short.Parse(parts[2]), short.Parse(parts[3])
                                ));
                            }
                            else if (vIdxStr.Length == 3)
                            {

                            }
                        }
                        break;
                }
            }
            return mesh;
        }

        public static Mesh CreateSquarePlane((float width, float height) size, int subdivisions)
        {
            var mesh = new Mesh();
            int sideResolution = (int)Math.Pow(2, subdivisions);
            int verticesPerSide = sideResolution + 1;

            if (sideResolution == 1)
            {
                // This mesh has only two triangles. It's almost certainly faster
                // to just hard code it than run through the entire algorithm
                mesh.VertexPositions = new List<Vector3>()
                {
                    new Vector3(-size.width, 0f,  size.height),
                    new Vector3( size.width, 0f,  size.height),
                    new Vector3(-size.width, 0f, -size.height),
                    new Vector3( size.width, 0f, -size.height)
                };
                mesh.Triangles = new List<Tuple<short, short, short>>()
                {
                    new Tuple<short, short, short>(0, 1, 2),
                    new Tuple<short, short, short>(1, 2, 3)
                };
            }
            else
            {
                // Generate the list of vertices
                // This loop generates a grid whose outer vertices form a square with radius 1,
                // and then scales it up by the requested width and height.
                mesh.VertexPositions = new List<Vector3>(verticesPerSide * verticesPerSide);
                float delta = 2f / sideResolution;
                int radius = sideResolution / 2;
                for (int y = radius; y >= -radius; y--)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        mesh.VertexPositions.Add(new Vector3(
                            x * delta * size.width, 0, y * delta * size.height
                        ));
                    }
                }

                // Create the list of triangles
                mesh.Triangles = new List<Tuple<short, short, short>>(sideResolution * sideResolution * 2);
                // Loop through the top left corner of each quad, which contains two triangles.
                // Ignore the last row and last column of vertices, since they aren't the top
                // left corner of triangles.
                int numTopLeftCorners = (sideResolution - 1) * (sideResolution - 1);
                for (int i = 0; i < numTopLeftCorners; i++)
                {
                    // Top triangle
                    mesh.Triangles.Add(new Tuple<short, short, short>(
                        (short)i, (short)(i + 1), (short)(i + verticesPerSide)
                    ));

                    // Bottom triangle
                    mesh.Triangles.Add(new Tuple<short, short, short>(
                        (short)(i + 1), (short)(i + verticesPerSide), (short)(i + verticesPerSide + 1)
                    ));
                }
            }

            return mesh;
        }

        public static Mesh CreateSquarePlane(Vector3 vectorA, Vector3 vectorB, Vector3 vectorC, Rect size)
        {
            throw new NotImplementedException();
        }

        public void Extrude(Vector3 direction, float amount)
        {
            throw new NotImplementedException();
        }

        public void ExtrudeAlongNormals(float amount)
        {
            throw new NotImplementedException();
        }
    }
}
