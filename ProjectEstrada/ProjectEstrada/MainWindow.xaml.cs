using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
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
        private ISwapChainPanelNative* m_swapChainNative;
        private IDXGISwapChain1* m_swapChain;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        unsafe void InitSwapChain()
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
                    IDXGISwapChain1* swapChain;
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
                    // ordering than the API default. It is required for compatibility with
                    // Direct2D.
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
                    ID3D11Device* m_d3dDevice;
                    ID3D11DeviceContext* m_d3dContext;
                    D3D11CreateDevice(
                        (IDXGIAdapter*)IntPtr.Zero.ToPointer(), // Specify nullptr to use the default adapter.
                        D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE,
                        IntPtr.Zero,
                        (uint)creationFlags,
                        InteropUtilities.AsPointer(featureLevels.AsSpan()),
                        (uint)featureLevels.Length,
                        D3D11_SDK_VERSION, // UWP apps must set this to D3D11_SDK_VERSION.
                        &m_d3dDevice, // Returns the Direct3D device created.
                        InteropUtilities.AsPointer(ref pFeatureLevel),
                        &m_d3dContext // Returns the device immediate context.
                    );

                    // QI for DXGI device
                    IDXGIDevice* dxgiDevice;
                    ((ComPtr<ID3D11Device>)m_d3dDevice).As((ComPtr<IDXGIDevice>*)&dxgiDevice);

                    // Get the DXGI adapter.
                    IDXGIAdapter* dxgiAdapter;
                    dxgiDevice->GetAdapter(&dxgiAdapter);

                    // Get the DXGI factory.
                    IDXGIFactory2* dxgiFactory;
                    var dxgiFactoryGuid = typeof(IDXGIFactory2).GUID;
                    dxgiAdapter->GetParent(InteropUtilities.AsPointer(ref dxgiFactoryGuid), (void**)&dxgiFactory);

                    // Create a swap chain by calling CreateSwapChainForComposition.
                    dxgiFactory->CreateSwapChainForComposition(
                        (IUnknown*)m_d3dDevice,
                        &swapChainDesc,
                        null,        // Allow on any display. 
                        &swapChain
                    );

                    m_swapChainNative->SetSwapChain((IDXGISwapChain*)swapChain);

                    int hr = swapChain->Present(1, 0);
                    Marshal.ThrowExceptionForHR(hr);

                    m_swapChain = swapChain;
                }
                finally
                {
                    m_swapChainNative->Release();
                }
            }

            SwapChain.SizeChanged += SwapChain_SizeChanged;
        }

        private void SwapChain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int hr = m_swapChain->ResizeBuffers(
                0,  // Don't change buffer count
                (uint)e.NewSize.Width,
                (uint)e.NewSize.Height,
                DXGI_FORMAT.DXGI_FORMAT_UNKNOWN,    // Don't change format
                0   // No flags
            );
            Marshal.ThrowExceptionForHR(hr);

            var rand = new Random();
            var randColor = new DXGI_RGBA((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
            System.Diagnostics.Debug.WriteLine($"rgb({randColor.r}, {randColor.g}, {randColor.b})");
            hr = m_swapChain->SetBackgroundColor(InteropUtilities.AsPointer(ref randColor));
            Marshal.ThrowExceptionForHR(hr);

            hr = m_swapChain->Present(1, 0);
            Marshal.ThrowExceptionForHR(hr);
        }

        private void SwapChain_Loaded(object sender, RoutedEventArgs e)
        {
            InitSwapChain();
            HideCoreWindow();
        }

        static unsafe void HideCoreWindow()
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
            {
                ShowWindow(HWND, SW_HIDE);
            }
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            m_swapChain->Release();
            m_swapChainNative->Release();
        }
    }
}
