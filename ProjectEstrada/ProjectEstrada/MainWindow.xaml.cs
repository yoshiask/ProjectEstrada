using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
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
                        Format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,        // This is the most common swapchain format.
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

                    int hr = m_swapChain.Get()->Present(1, 0);
                    Marshal.ThrowExceptionForHR(hr);
                    #endregion

                    // Compile vertex shader
                    #region Vertex shader
                    const string vertexShaderName = "SimpleVertexShader";
                    uint flags = D3DCOMPILE_ENABLE_STRICTNESS;
#if DEBUG
                    flags |= D3DCOMPILE_DEBUG;
#endif
                    // Prefer higher CS shader profile when possible as CS 5.0 provides better performance on 11-class hardware.
                    var profile = (m_d3dDevice.Get()->GetFeatureLevel() >= D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0) ? "vs_5_0" : "vs_4_0_level_9_1";
                    ID3DBlob* shaderBlob;
                    ID3DBlob* errorBlob;
                    HRESULT shaderCompileHr = D3DCompileFromFile(
                        GetShaderFilePath(vertexShaderName).Select(c => (ushort)c).ToArray().AsSpan().AsPointer(),
                        null,
                        D3D_COMPILE_STANDARD_FILE_INCLUDE,
                        Encoding.ASCII.GetBytes(vertexShaderName).Select(b => (sbyte)b).ToArray().AsSpan().AsPointer(),
                        profile.Select(c => (sbyte)c).ToArray().AsSpan().AsPointer(),
                        flags,
                        0,
                        &shaderBlob,
                        &errorBlob
                    );
                    if (shaderCompileHr.FAILED)
                    {
                        var errorStr = Marshal.PtrToStringAnsi(new IntPtr(errorBlob->GetBufferPointer()));
                        errorBlob->Release();
                        return;
                    }

                    ID3D11VertexShader* vertexShader;
                    Marshal.ThrowExceptionForHR(
                        m_d3dDevice.Get()->CreateVertexShader(
                            shaderBlob->GetBufferPointer(),
                            shaderBlob->GetBufferSize(),
                            null,
                            &vertexShader
                        )
                    );

                    // Create an input layout that matches the layout defined in the vertex shader code.
                    // For this lesson, this is simply a DirectX::XMFLOAT2 vector defining the vertex position.
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

                    ID3D11InputLayout* inputLayout;
                    Marshal.ThrowExceptionForHR(
                        m_d3dDevice.Get()->CreateInputLayout(
                            basicVertexLayoutDesc.AsSpan().AsPointer(),
                            (uint)basicVertexLayoutDesc.Length,
                            shaderBlob->GetBufferPointer(),
                            shaderBlob->GetBufferSize(),
                            &inputLayout
                        )
                    ); 
                    #endregion

                    // Compile pixel shader
                    #region Pixel shader
                    const string pixelShaderName = "SimplePixelShader";
                    flags = D3DCOMPILE_ENABLE_STRICTNESS;
#if DEBUG
                    flags |= D3DCOMPILE_DEBUG;
#endif
                    // Prefer higher CS shader profile when possible as CS 5.0 provides better performance on 11-class hardware.
                    profile = (m_d3dDevice.Get()->GetFeatureLevel() >= D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0) ? "ps_5_0" : "ps_4_0_level_9_1";
                    shaderCompileHr = D3DCompileFromFile(
                        GetShaderFilePath(pixelShaderName).Select(c => (ushort)c).ToArray().AsSpan().AsPointer(),
                        null,
                        D3D_COMPILE_STANDARD_FILE_INCLUDE,
                        Encoding.ASCII.GetBytes(pixelShaderName).Select(b => (sbyte)b).ToArray().AsSpan().AsPointer(),
                        profile.Select(c => (sbyte)c).ToArray().AsSpan().AsPointer(),
                        flags,
                        0,
                        &shaderBlob,
                        &errorBlob
                    );
                    if (shaderCompileHr.FAILED)
                    {
                        var errorStr = Marshal.PtrToStringAnsi(new IntPtr(errorBlob->GetBufferPointer()));
                        errorBlob->Release();
                        return;
                    }

                    ID3D11PixelShader* pixelShader;
                    Marshal.ThrowExceptionForHR(
                        m_d3dDevice.Get()->CreatePixelShader(
                            shaderBlob->GetBufferPointer(),
                            shaderBlob->GetBufferSize(),
                            null,
                            &pixelShader
                        )
                    );

                    // Create an input layout that matches the layout defined in the vertex shader code.
                    D3D11_INPUT_ELEMENT_DESC[] basicPixelLayoutDesc =
                    {
                          new D3D11_INPUT_ELEMENT_DESC
                          {
                              SemanticName = "SV_POSITION".GetAsciiSpan().AsPointer(),
                              SemanticIndex = 0,
                              Format = DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT,
                              InputSlot = 0,
                              AlignedByteOffset = 0,
                              InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA,
                              InstanceDataStepRate = 0
                          },
                    };

                    Marshal.ThrowExceptionForHR(
                        m_d3dDevice.Get()->CreateInputLayout(
                            basicPixelLayoutDesc.AsSpan().AsPointer(),
                            (uint)basicPixelLayoutDesc.Length,
                            shaderBlob->GetBufferPointer(),
                            shaderBlob->GetBufferSize(),
                            &inputLayout
                        )
                    );
                    #endregion

                    shaderBlob->Release();

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
            int hr = m_swapChain.Get()->ResizeBuffers(
                0,  // Don't change buffer count
                (uint)e.NewSize.Width,
                (uint)e.NewSize.Height,
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

            //var rand = new Random();
            //var randColor = new DXGI_RGBA((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
            //System.Diagnostics.Debug.WriteLine($"rgb({randColor.r}, {randColor.g}, {randColor.b})");
            //hr = m_swapChain->SetBackgroundColor(InteropUtilities.AsPointer(ref randColor));
            //Marshal.ThrowExceptionForHR(hr);

            hr = m_swapChain.Get()->Present(1, 0);
            Marshal.ThrowExceptionForHR(hr);

            m_renderTargetView.Get()->Release();
            backBuffer.Get()->Release();
        }

        private void SwapChain_Loaded(object sender, RoutedEventArgs e)
        {
            InitSwapChain();
            HideCoreWindow();
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
