﻿using CSharpMath.Rendering.BackEnd;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SymbolabUWP.Controls
{
    public sealed partial class LatexCanvas : UserControl
    {
        public LatexCanvas()
        {
            this.InitializeComponent();
        }
        public LatexCanvas(string latexString)
        {
            InitializeComponent();
            LaTeXString = latexString;

            // Register callbacks to redraw the canvas when
            // the visual properties are changed
            RegisterPropertyChangedCallback(BackgroundProperty,
                new DependencyPropertyChangedCallback((obj, pr) => Canvas.Invalidate()));
            RegisterPropertyChangedCallback(ForegroundProperty,
                new DependencyPropertyChangedCallback((obj, pr) => Canvas.Invalidate()));
            RegisterPropertyChangedCallback(FontSizeProperty,
                new DependencyPropertyChangedCallback((obj, pr) => Canvas.Invalidate()));
        }

        private void Canvas_PaintSurface(object sender, SkiaSharp.Views.UWP.SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear();

            // Handle the text color
            byte r = 0, g = 0, b = 0, a = 255;
            if (Foreground is SolidColorBrush)
            {
                var fore = (Foreground as SolidColorBrush).Color;
                r = fore.R;
                g = fore.G;
                b = fore.B;
                a = fore.A;
            }
            SkiaSharp.SKColor textColor = new SkiaSharp.SKColor(r, g, b, a);

            // Handle the text color
            r = 255; g = 255; b = 255; a = 255;
            if (Background is SolidColorBrush)
            {
                var back = (Background as SolidColorBrush).Color;
                r = back.R;
                g = back.G;
                b = back.B;
                a = back.A;
            }
            SkiaSharp.SKColor backColor = new SkiaSharp.SKColor(r, g, b, a);
            e.Surface.Canvas.DrawColor(backColor);

            var painter = new CSharpMath.SkiaSharp.MathPainter
            {
                LaTeX = LaTeXString,
                FontSize = (float)FontSize,
                TextColor = textColor,
            };
            painter.Draw(e.Surface.Canvas);
        }

        public string LaTeXString {
            get => (string)GetValue(LaTeXStringProperty);
            set {
                SetValue(LaTeXStringProperty, value);
                Canvas.Invalidate();
            }
        }
        public static readonly DependencyProperty LaTeXStringProperty =
            DependencyProperty.Register(nameof(LaTeXString), typeof(string), typeof(LatexCanvas), null);
    }
}
