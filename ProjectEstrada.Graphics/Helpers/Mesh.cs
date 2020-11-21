using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace ProjectEstrada.Graphics.Helpers
{
    public class Mesh
    {
        public List<Vector3> VertexPositions { get; set; } = new List<Vector3>();
        public List<Vector3> VertexColors { get; set; } = new List<Vector3>();
        public List<Vector3> VertexNormals { get; set; } = new List<Vector3>();
        public List<Tuple<int, int, int>> Triangles { get; set; } = new List<Tuple<int, int, int>>();

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
                                mesh.Triangles.Add(new Tuple<int, int, int>(
                                    int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3])
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
    }
}
