using GLUWP;
using System;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectEstrada.Graphics.Controls
{
    internal sealed partial class GLUWPControl : UserControl
    {
        private OpenGLES mOpenGLES;

        private EGLSurface mRenderSurface; // This surface is associated with a swapChainPanel on the page
        private object mRenderSurfaceCriticalSection = new object();
        private IAsyncAction mRenderLoopWorker;

        private Func<RendererBase> RendererInit;

        public GLUWPControl(Func<RendererBase> rendererInit) : this(new OpenGLES(), rendererInit)
        {
        }

        public GLUWPControl() : this(() => new RendererBase())
        {
        }

        internal GLUWPControl(OpenGLES openGLES, Func<RendererBase> rendererInit)
        {
            mOpenGLES = openGLES;
            mRenderSurface = EGL.NO_SURFACE;
            RendererInit = rendererInit;
            InitializeComponent();

            CoreWindow window = Window.Current.CoreWindow;

            window.VisibilityChanged += new TypedEventHandler<CoreWindow, VisibilityChangedEventArgs>((win, args) => OnVisibilityChanged(win, args));

            Loaded += (sender, args) => OnLoaded(sender, args);
        }

        ~GLUWPControl()
        {
            StopRenderLoop();
            DestroyRenderSurface();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // The SwapChainPanel has been created and arranged in the page layout, so EGL can be initialized.
            CreateRenderSurface();
            StartRenderLoop();
        }

        private void OnVisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            if (args.Visible && mRenderSurface != EGL.NO_SURFACE)
            {
                StartRenderLoop();
            }
            else
            {
                StopRenderLoop();
            }
        }

        private void CreateRenderSurface()
        {
            if (mOpenGLES != null && mRenderSurface == EGL.NO_SURFACE)
            {
                // The app can configure the the SwapChainPanel which may boost performance.
                // By default, this template uses the default configuration.
                mRenderSurface = mOpenGLES.CreateSurface(swapChainPanel, null, null);

                // You can configure the SwapChainPanel to render at a lower resolution and be scaled up to
                // the swapchain panel size. This scaling is often free on mobile hardware.
                //
                // One way to configure the SwapChainPanel is to specify precisely which resolution it should render at.
                // Size customRenderSurfaceSize = Size(800, 600);
                // mRenderSurface = mOpenGLES->CreateSurface(swapChainPanel, &customRenderSurfaceSize, nullptr);
                //
                // Another way is to tell the SwapChainPanel to render at a certain scale factor compared to its size.
                // e.g. if the SwapChainPanel is 1920x1280 then setting a factor of 0.5f will make the app render at 960x640
                // float customResolutionScale = 0.5f;
                // mRenderSurface = mOpenGLES->CreateSurface(swapChainPanel, nullptr, &customResolutionScale);
                // 
            }
        }

        private void DestroyRenderSurface()
        {
            if (mOpenGLES != null)
            {
                mOpenGLES.DestroySurface(mRenderSurface);
            }

            mRenderSurface = EGL.NO_SURFACE;
        }

        void RecoverFromLostDevice()
        {
            // Stop the render loop, reset OpenGLES, recreate the render surface
            // and start the render loop again to recover from a lost device.

            StopRenderLoop();

            {
                lock (mRenderSurfaceCriticalSection)
                {
                    DestroyRenderSurface();
                    mOpenGLES.Reset();
                    CreateRenderSurface();
                }
            }

            StartRenderLoop();
        }

        void StartRenderLoop()
        {
            // If the render loop is already running then do not start another thread.
            if (mRenderLoopWorker != null && mRenderLoopWorker.Status == AsyncStatus.Started)
            {
                return;
            }

            //var throttler = new Helpers.EventThrottler<TextChangedEventArgs>(Constants.TypingTimeout, handler => SearchField.TextChanged += new TextChangedEventHandler(handler));
            //throttler.Invoked += (s, args) =>
            //{
            //    // s is SearchField
            //    // args is TextChangedEventArgs
            //};

            // Create a task for rendering that will be run on a background thread.
            var workItemHandler =
                new Windows.System.Threading.WorkItemHandler(action =>
                {
                    lock (mRenderSurfaceCriticalSection)
                    {
                        mOpenGLES.MakeCurrent(mRenderSurface);

                        //var renderer = new RendererBase();
                        var renderer = RendererInit();

                        while (action.Status == AsyncStatus.Started)
                        {
                            int panelWidth = 0;
                            int panelHeight = 0;
                            mOpenGLES.GetSurfaceDimensions(mRenderSurface, ref panelWidth, ref panelHeight);

                            // Logic to update the scene could go here
                            renderer.UpdateWindowSize(panelWidth, panelHeight);
                            renderer.Draw();

                            // The call to eglSwapBuffers might not be successful (i.e. due to Device Lost)
                            // If the call fails, then we must reinitialize EGL and the GL resources.
                            if (mOpenGLES.SwapBuffers(mRenderSurface) != EGL.TRUE)
                            {
                                // XAML objects like the SwapChainPanel must only be manipulated on the UI thread.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                swapChainPanel.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                                    new DispatchedHandler(() =>
                                    {
                                        RecoverFromLostDevice();
                                    }));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                                return;
                            }
                        }
                    }
                });

            // Run task on a dedicated high priority background thread.
            mRenderLoopWorker = Windows.System.Threading.ThreadPool.RunAsync(workItemHandler,
                Windows.System.Threading.WorkItemPriority.High,
                Windows.System.Threading.WorkItemOptions.TimeSliced);
        }

        void StopRenderLoop()
        {
            if (mRenderLoopWorker != null)
            {
                mRenderLoopWorker.Cancel();
                mRenderLoopWorker = null;
            }
        }
    }
}
