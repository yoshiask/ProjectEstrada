﻿using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using TerraFX.Interop;
using TerraFX.Utilities;
using static TerraFX.Interop.Windows;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ProjectEstrada
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public unsafe sealed partial class MainWindow : Window
    {
        ComPtr<ISwapChainPanelNative> m_swapChainNative;
        ComPtr<IDXGISwapChain1> m_swapChain;
        ComPtr<IDXGIDevice> m_dxgiDevice;
        ComPtr<ID3D11Device> m_d3dDevice;
        ComPtr<ID3D11DeviceContext> m_d3dContext;
        ComPtr<ID3D11RenderTargetView> m_renderTargetView;
        ComPtr<ID3D11DepthStencilView> m_depthStencilView;
        ComPtr<ID3D11Buffer> vertexBuffer;

        DXGI_FORMAT colorFormat = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        void InitSwapChain()
        {
            var panelUnknown = Marshal.GetIUnknownForObject(SwapChain);
            IntPtr swapChainPanelNativePtr = IntPtr.Zero;
            try
            {
                var guid = Guid.Parse("63aad0b8-7c24-40ff-85a8-640d944cc325");
                Marshal.ThrowExceptionForHR(Marshal.QueryInterface(panelUnknown, ref guid, out swapChainPanelNativePtr));
            }
            finally
            {
                Marshal.Release(panelUnknown);
            }

            if (swapChainPanelNativePtr != IntPtr.Zero)
            {
                m_swapChainNative = (ISwapChainPanelNative*)swapChainPanelNativePtr;

                try
                {
                    #region Init SwapChain
                    DXGI_SWAP_CHAIN_DESC1 swapChainDesc = new DXGI_SWAP_CHAIN_DESC1
                    {
                        Width = (uint)SwapChain.ActualWidth,
                        Height = (uint)SwapChain.ActualHeight,
                        Format = colorFormat,        // This is the most common swapchain format.
                        Stereo = 0,
                        BufferUsage = (uint)(1L << (1 + 4)),
                        BufferCount = 2,
                        Scaling = DXGI_SCALING.DXGI_SCALING_STRETCH,
                        SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL,
                        Flags = 0
                    };
                    swapChainDesc.SampleDesc.Count = 1;                          // Don't use multi-sampling.
                    swapChainDesc.SampleDesc.Quality = 0;

                    // Get D3DDevice
                    // This flag adds support for surfaces with a different color channel 
                    // ordering than the API default. It is required for compatibility with Direct2D.
                    D3D11_CREATE_DEVICE_FLAG creationFlags = D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT;

                    #if DEBUG
                    // If the project is in a debug build, enable debugging via SDK Layers.
                    creationFlags |= D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_DEBUG;
                    #endif

                    // This example only uses feature level 9.1.
                    D3D_FEATURE_LEVEL[] featureLevels =
                    {
                        D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_9_1
                    };
                    var pFeatureLevel = D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_1;

                    // Create the Direct3D 11 API device object and a corresponding context.
                    D3D11CreateDevice(
                        (IDXGIAdapter*)IntPtr.Zero.ToPointer(), // Specify nullptr to use the default adapter.
                        D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE,
                        IntPtr.Zero,
                        (uint)creationFlags,
                        InteropUtilities.AsPointer(featureLevels.AsSpan()),
                        (uint)featureLevels.Length,
                        D3D11_SDK_VERSION, // UWP apps must set this to D3D11_SDK_VERSION.
                        m_d3dDevice.GetAddressOf(), // Returns the Direct3D device created.
                        InteropUtilities.AsPointer(ref pFeatureLevel),
                        m_d3dContext.GetAddressOf() // Returns the device immediate context.
                    );

                    // QI for DXGI device
                    m_d3dDevice.As(ref m_dxgiDevice);

                    // Get the DXGI adapter.
                    IDXGIAdapter* dxgiAdapter;
                    m_dxgiDevice.Get()->GetAdapter(&dxgiAdapter);

                    // Get the DXGI factory.
                    IDXGIFactory2* dxgiFactory;
                    var dxgiFactoryGuid = typeof(IDXGIFactory2).GUID;
                    dxgiAdapter->GetParent(InteropUtilities.AsPointer(ref dxgiFactoryGuid), (void**)&dxgiFactory);

                    // Create a swap chain by calling CreateSwapChainForComposition.
                    dxgiFactory->CreateSwapChainForComposition(
                        (IUnknown*)(ID3D11Device*)m_d3dDevice,
                        &swapChainDesc,
                        null,        // Allow on any display. 
                        m_swapChain.GetAddressOf()
                    );

                    m_swapChainNative.Get()->SetSwapChain((IDXGISwapChain*)m_swapChain.Get());
                    #endregion

                    InitPipeline();
                    InitGraphics();

                    int hr = m_swapChain.Get()->Present(1, 0);
                    Marshal.ThrowExceptionForHR(hr);

                    SwapChain.SizeChanged += SwapChain_SizeChanged;
                }
                finally
                {
                    m_swapChainNative.Get()->Release();
                }
            }
        }

        private void SwapChain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            uint width, height;
            if (sender is Tuple<double, double> size)
            {
                width = (uint)size.Item1;
                height = (uint)size.Item2;
            }
            else
            {
                width = (uint)e.NewSize.Width;
                height = (uint)e.NewSize.Height;
            }
            if (width <= 0 || height <= 0)
                return;

            InitPipeline();

            m_d3dContext.Get()->ClearState();
            m_renderTargetView.Dispose();
            m_depthStencilView.Dispose();
            m_renderTargetView = new ComPtr<ID3D11RenderTargetView>();
            m_depthStencilView = new ComPtr<ID3D11DepthStencilView>();

            // resize the swap chain
            int hr = m_swapChain.Get()->ResizeBuffers(
                0,  // Don't change buffer count
                width,
                height,
                DXGI_FORMAT.DXGI_FORMAT_UNKNOWN,    // Don't change format
                0   // No flags
            );
            Marshal.ThrowExceptionForHR(hr);

            // (re)-create the render target view
            ComPtr<ID3D11Texture2D> backBuffer = new ComPtr<ID3D11Texture2D>();
            var d3d11Text2D = typeof(ID3D11Texture2D).GUID;
            D3D11_RENDER_TARGET_VIEW_DESC desc = new D3D11_RENDER_TARGET_VIEW_DESC
            {
                ViewDimension = D3D11_RTV_DIMENSION.D3D11_RTV_DIMENSION_BUFFER
            };
            m_renderTargetView = new ComPtr<ID3D11RenderTargetView>();

            if (FAILED(m_swapChain.Get()->GetBuffer(0, InteropUtilities.AsPointer(ref d3d11Text2D), (void**)backBuffer.GetAddressOf())))
                throw new Exception("Direct3D was unable to acquire the back buffer!");
            if (FAILED(m_d3dDevice.Get()->CreateRenderTargetView(
                (ID3D11Resource*)backBuffer.Get(),
                null,
                m_renderTargetView.GetAddressOf())))
                throw new Exception("Direct3D was unable to create the render target view!");

            // create the depth and stencil buffer
            D3D11_TEXTURE2D_DESC dsd;
            ComPtr<ID3D11Texture2D> dsBuffer = new ComPtr<ID3D11Texture2D>();
            backBuffer.Get()->GetDesc(&dsd);
            dsd.Format = DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT;
            dsd.Usage = D3D11_USAGE.D3D11_USAGE_DEFAULT;
            dsd.BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_DEPTH_STENCIL;
            if (FAILED(m_d3dDevice.Get()->CreateTexture2D(&dsd, null, dsBuffer.GetAddressOf())))
                throw new Exception("Direct3D was unable to create a 2D-texture!");
            if (FAILED(m_d3dDevice.Get()->CreateDepthStencilView((ID3D11Resource*)dsBuffer.Get(), null, m_depthStencilView.GetAddressOf())))
                throw new Exception("Direct3D was unable to create the depth and stencil buffer!");

            // activate the depth and stencil buffer
            m_d3dContext.Get()->OMSetRenderTargets(1, m_renderTargetView.GetAddressOf(), m_depthStencilView.Get());

            // set the viewport to the entire backbuffer
            D3D11_VIEWPORT vp;
            vp.TopLeftX = 0;
            vp.TopLeftY = 0;
            vp.Width = dsd.Width;
            vp.Height = dsd.Height;
            vp.MinDepth = 0.0f;
            vp.MaxDepth = 1.0f;
            m_d3dContext.Get()->RSSetViewports(1, &vp);

            Present();
            //m_renderTargetView.Get()->Release();
            backBuffer.Get()->Release();
        }

        private void SwapChain_Loaded(object sender, RoutedEventArgs e)
        {
            InitSwapChain();
            HideCoreWindow();

            var swapChainPanel = sender as Microsoft.UI.Xaml.Controls.SwapChainPanel;
            SwapChain_SizeChanged(new Tuple<double, double>(swapChainPanel.ActualWidth, swapChainPanel.ActualHeight), null);
            Render();
        }

        bool TryCompileVertexShader(string shaderName, out ComPtr<ID3DBlob> shaderBlob, out ComPtr<ID3D11VertexShader> vertexShader)
        {
            try
            {
                uint flags = D3DCOMPILE_ENABLE_STRICTNESS;
#if DEBUG
                flags |= D3DCOMPILE_DEBUG;
#endif
                // Prefer higher CS shader profile when possible as CS 5.0 provides better performance on 11-class hardware.
                var profile = (m_d3dDevice.Get()->GetFeatureLevel() >= D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0) ? "vs_5_0" : "vs_4_0_level_9_1";
                shaderBlob = new ComPtr<ID3DBlob>();
                ID3DBlob* errorBlob;
                HRESULT shaderCompileHr = D3DCompileFromFile(
                    GetShaderFilePath(shaderName).Select(c => (ushort)c).ToArray().AsSpan().AsPointer(),
                    null,
                    D3D_COMPILE_STANDARD_FILE_INCLUDE,
                    Encoding.ASCII.GetBytes(shaderName).Select(b => (sbyte)b).ToArray().AsSpan().AsPointer(),
                    profile.Select(c => (sbyte)c).ToArray().AsSpan().AsPointer(),
                    flags,
                    0,
                    shaderBlob.GetAddressOf(),
                    &errorBlob
                );
                if (shaderCompileHr.FAILED)
                {
                    var errorStr = Marshal.PtrToStringAnsi(new IntPtr(errorBlob->GetBufferPointer()));
                    errorBlob->Release();
                    throw new Exception(errorStr);
                }

                vertexShader = new ComPtr<ID3D11VertexShader>();
                Marshal.ThrowExceptionForHR(
                    m_d3dDevice.Get()->CreateVertexShader(
                        shaderBlob.Get()->GetBufferPointer(),
                        shaderBlob.Get()->GetBufferSize(),
                        null,
                        vertexShader.GetAddressOf()
                    )
                );

                return true;
            }
            catch
            {
                vertexShader = null;
                shaderBlob = null;
                return false;
            }
        }

        bool TryCompilePixelShader(string shaderName, out ComPtr<ID3DBlob> shaderBlob, out ComPtr<ID3D11PixelShader> pixelShader)
        {
            try
            {
                shaderBlob = new ComPtr<ID3DBlob>();
                ID3DBlob* errorBlob;
                uint flags = D3DCOMPILE_ENABLE_STRICTNESS;
#if DEBUG
                flags |= D3DCOMPILE_DEBUG;
#endif
                // Prefer higher CS shader profile when possible as CS 5.0 provides better performance on 11-class hardware.
                var profile = (m_d3dDevice.Get()->GetFeatureLevel() >= D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0) ? "ps_5_0" : "ps_4_0_level_9_1";
                HRESULT shaderCompileHr = D3DCompileFromFile(
                    GetShaderFilePath(shaderName).Select(c => (ushort)c).ToArray().AsSpan().AsPointer(),
                    null,
                    D3D_COMPILE_STANDARD_FILE_INCLUDE,
                    Encoding.ASCII.GetBytes(shaderName).Select(b => (sbyte)b).ToArray().AsSpan().AsPointer(),
                    profile.Select(c => (sbyte)c).ToArray().AsSpan().AsPointer(),
                    flags,
                    0,
                    shaderBlob.GetAddressOf(),
                    &errorBlob
                );
                if (shaderCompileHr.FAILED)
                {
                    var errorStr = Marshal.PtrToStringAnsi(new IntPtr(errorBlob->GetBufferPointer()));
                    errorBlob->Release();
                    throw new Exception(errorStr);
                }

                pixelShader = new ComPtr<ID3D11PixelShader>();
                Marshal.ThrowExceptionForHR(
                    m_d3dDevice.Get()->CreatePixelShader(
                        shaderBlob.Get()->GetBufferPointer(),
                        shaderBlob.Get()->GetBufferSize(),
                        null,
                        pixelShader.GetAddressOf()
                    )
                );

                return true;
            }
            catch
            {
                shaderBlob = null;
                pixelShader = null;
                return false;
            }
        }

        void InitPipeline()
        {
            TryCompileVertexShader("SimpleVertexShader", out var vertexShaderBlob, out var vertexShader);
            TryCompilePixelShader("SimplePixelShader", out var pixelShaderBlob, out var pixelShader);

            // Set the shader objects as the active shaders
            m_d3dContext.Get()->VSSetShader(vertexShader, null, 0);
            m_d3dContext.Get()->PSSetShader(pixelShader, null, 0);

            // Create an input layout that matches the layout defined in the vertex shader code.
            D3D11_INPUT_ELEMENT_DESC[] basicVertexLayoutDesc =
            {
                new D3D11_INPUT_ELEMENT_DESC
                {
                    SemanticName = "POSITION".GetAsciiSpan().AsPointer(),
                    SemanticIndex = 0,
                    Format = DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT,
                    InputSlot = 0,
                    AlignedByteOffset = 0,
                    InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA,
                    InstanceDataStepRate = 0
                },
            };

            var inputLayout = new ComPtr<ID3D11InputLayout>();
            Marshal.ThrowExceptionForHR(
                m_d3dDevice.Get()->CreateInputLayout(
                    basicVertexLayoutDesc.AsSpan().AsPointer(),
                    (uint)basicVertexLayoutDesc.Length,
                    vertexShaderBlob.Get()->GetBufferPointer(),
                    vertexShaderBlob.Get()->GetBufferSize(),
                    inputLayout.GetAddressOf()
                )
            );

            // set active input layout
            m_d3dContext.Get()->IASetInputLayout(inputLayout.Get());
        }

        void InitGraphics()
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
            if (FAILED(m_d3dDevice.Get()->CreateBuffer(&bd, &srd, vertexBuffer.GetAddressOf())))
                throw new Exception("Critical Error: Unable to create vertex buffer!");
        }

        void ClearBuffers()
        {
            var devCon = m_d3dContext.Get();
            devCon->ClearState();

            // clear the back buffer and depth / stencil buffer
            float[] color = new[] { 0.0f, 0.0f, 0.0f, 0.0f };
            devCon->ClearRenderTargetView(m_renderTargetView.Get(), color.AsSpan().AsPointer());
            devCon->ClearDepthStencilView(
                m_depthStencilView.Get(), (uint)(D3D11_CLEAR_FLAG.D3D11_CLEAR_DEPTH | D3D11_CLEAR_FLAG.D3D11_CLEAR_STENCIL), 1.0f, 0);
        }

        void Render()
        {
            // clear the back buffer and the depth/stencil buffer
            ClearBuffers();

            // render

            // set the vertex buffer
            uint stride = (uint)sizeof(Vector3);
            uint offset = 0;
            m_d3dContext.Get()->IASetVertexBuffers(0, 1, vertexBuffer.GetAddressOf(), &stride, &offset);

            // set primitive topology
            m_d3dContext.Get()->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY.D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

            // draw 3 vertices, starting from vertex 0
            m_d3dContext.Get()->Draw(3, 0);

            // present the scene
            if (Present() != 0)
                throw new Exception("Failed to present the scene!");

        }

        int Present()
        {
            HRESULT hr = m_swapChain.Get()->Present(0, DXGI_PRESENT_DO_NOT_WAIT);
            if (FAILED(hr) && hr != DXGI_ERROR_WAS_STILL_DRAWING)
            {
                System.Diagnostics.Debug.WriteLine("The presentation of the scene failed!");
                throw new Exception("Direct3D failed to present the scene!");
            }

            // return success
            return 0;
        }

        static void HideCoreWindow()
        {
            IntPtr HWND = IntPtr.Zero;
            try
            {
                var unknown = Marshal.GetIUnknownForObject(Windows.UI.Core.CoreWindow.GetForCurrentThread());
                IntPtr coreInteropPtr = IntPtr.Zero;
                try
                {
                    var guid = typeof(ICoreWindowInterop).GUID;
                    Marshal.ThrowExceptionForHR(Marshal.QueryInterface(unknown, ref guid, out coreInteropPtr));
                }
                finally
                {
                    Marshal.Release(unknown);
                }

                if (coreInteropPtr != IntPtr.Zero)
                {
                    var coreInterop = (ICoreWindowInterop*)coreInteropPtr;
                    Marshal.ThrowExceptionForHR(coreInterop->get_WindowHandle(&HWND));
                    coreInterop->Release();
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to get core window handle");
            }

            if (HWND != IntPtr.Zero)
                _ = ShowWindow(HWND, SW_HIDE);
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            m_swapChain.Dispose();
            m_swapChainNative.Dispose();
        }
    }
}
