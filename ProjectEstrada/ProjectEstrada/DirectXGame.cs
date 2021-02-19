using System;
using System.Numerics;
using TerraFX.Interop;
using TerraFX.Utilities;
using static TerraFX.Interop.Windows;

namespace ProjectEstrada
{
    unsafe class DirectXGame : DirectXApp
    {
        private ComPtr<ID3D11Buffer> vertexBuffer;

        public DirectXGame(HINSTANCE hInstance) : base(hInstance) { }

        ~DirectXGame() { }

		public bool init()
		{
			// initialize the core DirectX application
			bool applicationInitialization = base.init();
			if (!applicationInitialization)
				return false;

			// initialize game graphics
			applicationInitialization = initGraphics();
			if (!applicationInitialization)
				return applicationInitialization;

			return true;
		}

        public bool initGraphics()
		{
            // create the triangle
            Vector3[] triangleVertices = { new Vector3(0.0f, 0.1f, 0.3f), new Vector3(0.11f, -0.1f, 0.3f), new Vector3(-0.11f, -0.1f, 0.3f) };

            // set up buffer description
            D3D11_BUFFER_DESC bd;
            bd.ByteWidth = (uint)(sizeof(Vector3) * triangleVertices.Length);
            bd.Usage = D3D11_USAGE.D3D11_USAGE_DEFAULT;
            bd.BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_VERTEX_BUFFER;
            bd.CPUAccessFlags = 0;
            bd.MiscFlags = 0;
            bd.StructureByteStride = 0;

            // define subresource data
            D3D11_SUBRESOURCE_DATA srd = new D3D11_SUBRESOURCE_DATA
            {
                pSysMem = triangleVertices.AsSpan().GetPointer()
            };

            // create the vertex buffer
            if (FAILED(d3d->dev->CreateBuffer(&bd, &srd, vertexBuffer.GetAddressOf())))
                throw new Exception("Critical Error: Unable to create vertex buffer!");

            return true;
        }

        public int run()
        {
            // run the core DirectX application
            return base.run();
        }

        public void shutdown(object obj) { }

        public int update(double dt)
        {
            // return success
            return 0;
        }

        public int render(double farSeer)
        {
            // clear the back buffer and the depth/stencil buffer
            d3d->clearBuffers();

            // render

            // print FPS information
            if (!d2d->printFPS().wasSuccessful())
                throw new Exception("Failed to print FPS information!");

            // set the vertex buffer
            uint stride = (uint)sizeof(Vector3);
            uint offset = 0;
            d3d->devCon->IASetVertexBuffers(0, 1, vertexBuffer.GetAddressOf(), &stride, &offset);

            // set primitive topology
            d3d->devCon->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY.D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

            // draw 3 vertices, starting from vertex 0
            d3d->devCon->Draw(3, 0);

            // present the scene
            if (!d3d->present())
                throw new Exception("Failed to present the scene!");

            // return success
            return 0;
        }
    }
}
