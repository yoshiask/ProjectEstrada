using GLUWP.ES20;
using GLUWP.ES20Enums;
using ProjectEstrada.Graphics.Helpers;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ProjectEstrada.Graphics
{
    public class RendererBase
    {
        internal float[] vertexPositions;
        internal IList<Vector3> vertexPositionVectors = new List<Vector3>();
        public IList<Vector3> VertexPositions
        {
            get => vertexPositionVectors;
            set
            {
                vertexPositionVectors = value;
                vertexPositions = new float[value.Count * 3];
                for (int i = 0; i < value.Count; i++)
                {
                    var position = value[i];
                    vertexPositions[3 * i] = position.X;
                    vertexPositions[3 * i + 1] = position.Y;
                    vertexPositions[3 * i + 2] = position.Z;
                }
            }
        }

        internal float[] vertexColors;
        internal IList<Vector3> vertexColorVectors = new List<Vector3>();
        public IList<Vector3> VertexColors
        {
            get => vertexColorVectors;
            set
            {
                vertexColorVectors = value;
                vertexColors = new float[value.Count * 3];
                for (int i = 0; i < value.Count; i++)
                {
                    var color = value[i];
                    vertexColors[3 * i] = color.X;
                    vertexColors[3 * i + 1] = color.Y;
                    vertexColors[3 * i + 2] = color.Z;
                }
            }
        }

        internal short[] indices;
        internal IList<Tuple<short, short, short>> triangleIndiciesVectors = new List<Tuple<short, short, short>>();
        public IList<Tuple<short, short, short>> TriangleIndicies
        {
            get => triangleIndiciesVectors;
            set
            {
                triangleIndiciesVectors = value;
                indices = new short[value.Count * 3];
                for (int i = 0; i < value.Count; i++)
                {
                    var triangle = value[i];
                    indices[3 * i] = triangle.Item1;
                    indices[3 * i + 1] = triangle.Item2;
                    indices[3 * i + 2] = triangle.Item3;
                }
            }
        }

        internal int mProgram;
        internal int mWindowWidth;
        internal int mWindowHeight;

        internal int mPositionAttribLocation;
        internal int mColorAttribLocation;

        internal int mModelUniformLocation;
        internal int mViewUniformLocation;
        internal int mProjUniformLocation;

        internal int mVertexPositionBuffer;
        internal int mVertexColorBuffer;
        internal int mIndexBuffer;

        internal int mDrawCount;

        internal int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);

            string[] sourceArray = new string[] { source };
            GL.ShaderSource(shader, sourceArray);

            GL.CompileShader(shader);

            int compileResult = GL.GetShaderiv(shader, ShaderParameter.CompileStatus);


            if (compileResult == 0)
            {
                int infoLogLength = GL.GetShaderiv(shader, ShaderParameter.InfoLogLength);

                string infoLog;
                GL.GetShaderInfoLog(shader, infoLogLength, out int length, out infoLog);

                var errorMessage = $"Shader compilation failed: {infoLog}";

                throw new ApplicationException(errorMessage);
            }

            return shader;
        }

        internal int CompileProgram(string vsSource, string fsSource)
        {
            int program = GL.CreateProgram();

            if (program == 0)
            {
                throw new ApplicationException("Program creation failed");
            }

            int vs = CompileShader(ShaderType.VertexShader, vsSource);
            int fs = CompileShader(ShaderType.FragmentShader, fsSource);

            if (vs == 0 || fs == 0)
            {
                GL.DeleteShader(fs);
                GL.DeleteShader(vs);
                GL.DeleteProgram(program);
                return 0;
            }

            GL.AttachShader(program, vs);
            GL.DeleteShader(vs);

            GL.AttachShader(program, fs);
            GL.DeleteShader(fs);

            GL.LinkProgram(program);

            int linkStatus = GL.GetProgramiv(program, GetProgramParameterName.LinkStatus);

            if (linkStatus == 0)
            {
                var infoLogLength = GL.GetProgramiv(program, GetProgramParameterName.InfoLogLength);

                string infoLog;
                GL.GetProgramInfoLog(program, infoLogLength, out int length, out infoLog);

                var errorMessage = $"Program link failed: {infoLog}";

                throw new ApplicationException(errorMessage);
            }


            return program;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        /// <remarks>
        /// Only use this to implement a fully custom constructor that generates geometry data
        /// </remarks>
        internal RendererBase()
        {
            
        }
        public RendererBase(IList<Vector3> vPos, IList<Vector3> vCol, IList<Tuple<short, short, short>> idxs)
        {
            VertexPositions = vPos;
            VertexColors = vCol;
            TriangleIndicies = idxs;

            Initialize();
        }

        ~RendererBase()
        {
            Deconstruct();
        }

        internal void Initialize()
        {
            mWindowWidth = 0;
            mWindowHeight = 0;
            mDrawCount = 0;

            // Vertex Shader source
            string vs = @"
                uniform mat4 uModelMatrix;
                uniform mat4 uViewMatrix;
                uniform mat4 uProjMatrix;
                attribute vec4 aPosition;
                attribute vec4 aColor;
                varying vec4 vColor;
                void main()
                {
                    gl_Position = uProjMatrix * uViewMatrix * uModelMatrix * aPosition;
                    vColor = aColor;
                }
            ";

            // Fragment Shader source
            string fs = @"
                precision mediump float;
                varying vec4 vColor;
                void main()
                {
                    gl_FragColor = vColor;
                }
            ";

            // Set up the shader and its uniform/attribute locations.
            mProgram = CompileProgram(vs, fs);
            mPositionAttribLocation = GL.GetAttribLocation(mProgram, "aPosition");
            mColorAttribLocation = GL.GetAttribLocation(mProgram, "aColor");
            mModelUniformLocation = GL.GetUniformLocation(mProgram, "uModelMatrix");
            mViewUniformLocation = GL.GetUniformLocation(mProgram, "uViewMatrix");
            mProjUniformLocation = GL.GetUniformLocation(mProgram, "uProjMatrix");

            // Then set up the cube geometry.

            mVertexPositionBuffer = GL.GenBuffers(1)[0];
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexPositionBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexPositions, BufferUsageHint.StaticDraw);

            // Vertex colors

            mVertexColorBuffer = GL.GenBuffers(1)[0];
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexColorBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexColors, BufferUsageHint.StaticDraw);

            // Face indices

            mIndexBuffer = GL.GenBuffers(1)[0];
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mIndexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices, BufferUsageHint.StaticDraw);
        }

        internal void Deconstruct()
        {
            if (mProgram != 0)
            {
                GL.DeleteProgram(mProgram);
                mProgram = 0;
            }

            if (mVertexPositionBuffer != 0)
            {
                GL.DeleteBuffers(1, new int[] { mVertexPositionBuffer });
                mVertexPositionBuffer = 0;
            }

            if (mVertexColorBuffer != 0)
            {
                GL.DeleteBuffers(1, new int[] { mVertexColorBuffer });
                mVertexColorBuffer = 0;
            }

            if (mIndexBuffer != 0)
            {
                GL.DeleteBuffers(1, new int[] { mIndexBuffer });
                mIndexBuffer = 0;
            }
        }

        public void Draw()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (mProgram == 0)
                return;

            GL.UseProgram(mProgram);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexPositionBuffer);
            GL.EnableVertexAttribArray(mPositionAttribLocation);
            GL.VertexAttribPointer(mPositionAttribLocation, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexColorBuffer);
            GL.EnableVertexAttribArray(mColorAttribLocation);
            GL.VertexAttribPointer(mColorAttribLocation, 3, VertexAttribPointerType.Float, false, 0, 0);

            //var modelMatrix = MathHelper.Flatten(MathHelper.SimpleModelMatrix((float)mDrawCount / 1000.0f).ToArray());
            var modelMatrix = MathHelper.SimpleModelMatrix((float)mDrawCount / 2000.0f);
            GL.UniformMatrix4(mModelUniformLocation, 1, false, modelMatrix.m);

            //var viewMatrix = MathHelper.Flatten(MathHelper.SimpleViewMatrix().ToArray());
            var viewMatrix = MathHelper.SimpleViewMatrix((float)Math.PI / 6, 5);
            GL.UniformMatrix4(mViewUniformLocation, 1, false, viewMatrix.m);

            //var projectionMatrix = MathHelper.Flatten(MathHelper.SimpleProjectionMatrix((float)mWindowWidth / (float)mWindowHeight).ToArray());
            var projectionMatrix = MathHelper.SimpleProjectionMatrix((float)mWindowWidth / (float)mWindowHeight);
            GL.UniformMatrix4(mProjUniformLocation, 1, false, projectionMatrix.m);

            // Draw 36 indices: six faces, two triangles per face, 3 indices per triangle
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mIndexBuffer);
            GL.DrawElements(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedShort, 0);

            mDrawCount += 1;
        }

        public void UpdateWindowSize(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            mWindowWidth = width;
            mWindowHeight = height;
        }
    }
}
