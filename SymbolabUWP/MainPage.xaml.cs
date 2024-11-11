using ProjectEstrada.Core.ViewModels;
using ProjectEstrada.Graphics.Core;
using System;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SPIRV;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SymbolabUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel => (MainViewModel)DataContext;

        private TextBox lastFocusedTextBox = null;

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = ((App)Application.Current).Services.GetService(typeof(MainViewModel));

            Window.Current.SetTitleBar(TitlebarGrid);
        }

        private void Text_Click(object sender, RoutedEventArgs e)
        {
            if (lastFocusedTextBox == null)
                return;

            string text = lastFocusedTextBox.SelectedText;
            int start = lastFocusedTextBox.SelectionStart;
            int length = lastFocusedTextBox.SelectionLength;
            lastFocusedTextBox.Text = lastFocusedTextBox.Text.Remove(start, length);
            lastFocusedTextBox.Text = lastFocusedTextBox.Text.Insert(start, @"\text{" + text + "}");
            lastFocusedTextBox.SelectionStart = start + 6;
        }

        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            if (lastFocusedTextBox == null)
                return;

            string text = lastFocusedTextBox.SelectedText;
            int start = lastFocusedTextBox.SelectionStart;
            int length = lastFocusedTextBox.SelectionLength;
            lastFocusedTextBox.Text = lastFocusedTextBox.Text.Remove(start, length);
            lastFocusedTextBox.Text = lastFocusedTextBox.Text.Insert(start, @"\bf{" + text + "}");
            lastFocusedTextBox.SelectionStart = start + 4;
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            if (lastFocusedTextBox == null)
                return;

            string text = lastFocusedTextBox.SelectedText;
            int start = lastFocusedTextBox.SelectionStart;
            int length = lastFocusedTextBox.SelectionLength;
            lastFocusedTextBox.Text = lastFocusedTextBox.Text.Remove(start, length);
            lastFocusedTextBox.Text = lastFocusedTextBox.Text.Insert(start, @"\it{" + text + "}");
            lastFocusedTextBox.SelectionStart = start + 4;
        }

        private void Underline_Click(object sender, RoutedEventArgs e)
        {
            if (lastFocusedTextBox == null)
                return;

            string text = lastFocusedTextBox.SelectedText;
            int start = lastFocusedTextBox.SelectionStart;
            int length = lastFocusedTextBox.SelectionLength;
            lastFocusedTextBox.Text = lastFocusedTextBox.Text.Remove(start, length);
            lastFocusedTextBox.Text = lastFocusedTextBox.Text.Insert(start, @"\underline{" + text + "}");
            lastFocusedTextBox.SelectionStart = start + 11;
        }

        private void List_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void OpenGraph_Click(object sender, RoutedEventArgs e)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            string formulaText = lastFocusedTextBox.Text;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame frame = new Frame();
                frame.Navigate(typeof(Views.GraphView), formulaText);
                Window.Current.Content = frame;
                // You have to activate the window in order to show it later.
                Window.Current.Activate();

                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }

        private void TextInput_GotFocus(object sender, RoutedEventArgs e)
        {
            lastFocusedTextBox = sender as TextBox;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Functions.Add(new FunctionViewModel());
        }

        private static GraphicsDevice _graphicsDevice;
        private static CommandList _commandList;
        private static DeviceBuffer _vertexBuffer;
        private static DeviceBuffer _indexBuffer;
        private static Shader[] _shaders;
        private static Pipeline _pipeline;

        private const string VertexCode = @"
#version 450
layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;
layout(location = 0) out vec4 fsin_Color;
void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}";

        private const string FragmentCode = @"
#version 450
layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;
void main()
{
    fsout_Color = fsin_Color;
}";

        private void RenderMain()
        {
            
            var source = GraphicsPanel.CreateCoreIndependentInputSource(CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse);
            //.ProcessEvents(CoreProcessEventsOption.ProcessUntilQuit);

            //source.
            source.DispatcherQueue.TryEnqueue(() =>
            {
                CreateResources();
                while (!_graphicsDevice.MainSwapchain.IsDisposed)
                    Draw();
                DisposeResources();
            });
        }

        private static void CreateResources()
        {
            ResourceFactory factory = _graphicsDevice.ResourceFactory;

            VertexPositionColor[] quadVertices =
            {
                new VertexPositionColor(new Vector2(-.75f, .75f), RgbaFloat.Red),
                new VertexPositionColor(new Vector2(.75f, .75f), RgbaFloat.Green),
                new VertexPositionColor(new Vector2(-.75f, -.75f), RgbaFloat.Blue),
                new VertexPositionColor(new Vector2(.75f, -.75f), RgbaFloat.Yellow)
            };
            BufferDescription vbDescription = new BufferDescription(
                4 * VertexPositionColor.SizeInBytes,
                BufferUsage.VertexBuffer);
            _vertexBuffer = factory.CreateBuffer(vbDescription);
            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, quadVertices);

            ushort[] quadIndices = { 0, 1, 2, 3 };
            BufferDescription ibDescription = new BufferDescription(
                4 * sizeof(ushort),
                BufferUsage.IndexBuffer);
            _indexBuffer = factory.CreateBuffer(ibDescription);
            _graphicsDevice.UpdateBuffer(_indexBuffer, 0, quadIndices);

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

            ShaderDescription vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(VertexCode),
                "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(FragmentCode),
                "main");

            _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            // Create pipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: _shaders);
            pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;

            _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            _commandList = factory.CreateCommandList();
        }

        private static void Draw()
        {
            // Begin() must be called before commands can be issued.
            _commandList.Begin();

            // We want to render directly to the output window.
            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);

            // Set all relevant state to draw our quad.
            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _commandList.SetPipeline(_pipeline);
            // Issue a Draw command for a single instance with 4 indices.
            _commandList.DrawIndexed(
                indexCount: 4,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);

            // End() must be called before commands can be submitted for execution.
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);

            // Once commands have been submitted, the rendered image can be presented to the application window.
            _graphicsDevice.SwapBuffers();
        }

        private static void DisposeResources()
        {
            _pipeline.Dispose();
            foreach (Shader shader in _shaders)
            {
                shader.Dispose();
            }
            _commandList.Dispose();
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _graphicsDevice.Dispose();
        }

        private async void GraphicsPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var ss = SwapchainSource.CreateUwp(GraphicsPanel, DisplayInformation.GetForCurrentView().LogicalDpi);
            var scd = new SwapchainDescription(
                ss,
                (uint)GraphicsPanel.ActualWidth, (uint)GraphicsPanel.ActualHeight,
                PixelFormat.R32_Float,
                false);

            var options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R32_Float,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true);
            var backend = GraphicsBackend.Direct3D11;

            var d3dOptions = new D3D11DeviceOptions()
            {
                DeviceCreationFlags = (uint)Vortice.Direct3D11.DeviceCreationFlags.Debug
            };

            if (backend == GraphicsBackend.Direct3D11)
            {
                _graphicsDevice = GraphicsDevice.CreateD3D11(options, d3dOptions, scd);
            }
            else
            {
                throw new NotImplementedException();
            }

            await Task.Run(RenderMain);
        }
    }
}
