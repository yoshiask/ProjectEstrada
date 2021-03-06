﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace ProjectEstrada.Graphics
{
    public class SampleRenderer : RendererBase
    {
        public SampleRenderer()
        {
            VertexPositions = new List<Vector3>()
            {
                new Vector3(-1.0f, -1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f,  1.0f),
                new Vector3(-1.0f,  1.0f, -1.0f),
                new Vector3(-1.0f,  1.0f,  1.0f),
                new Vector3( 1.0f, -1.0f, -1.0f),
                new Vector3( 1.0f, -1.0f,  1.0f),
                new Vector3( 1.0f,  1.0f, -1.0f),
                new Vector3( 1.0f,  1.0f,  1.0f),
            };

            VertexColors = new List<Vector3>()
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

            TriangleIndicies = new List<Tuple<short, short, short>>
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

            Initialize();
        }

        ~SampleRenderer()
        {
            Deconstruct();
        }
    }
}
