using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace TexRender
{
    public sealed partial class MathJax : UserControl
    {
        public MathJax()
        {
            this.InitializeComponent();
        }

        private static void OnTexStringChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            MathJax renderer = element as MathJax;
            if (renderer == null)
                return;
            renderer.RenderTexString(args.NewValue as string);
        }

        public async void RenderTexString(string texString)
        {
            if (!MathJaxView.IsLoaded)
                return;
            string functionString =
                $"document.getElementById(\"input\").value = `{texString.Replace(@"\", @"\\")}`;";
            await MathJaxView.InvokeScriptAsync("eval", new string[] { functionString });
            await MathJaxView.InvokeScriptAsync("eval", new string[] { "convert();" });
        }

        private void MathJaxView_Loaded(object sender, RoutedEventArgs e)
        {
            string mathJaxImport = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Assets", "TexToSvg.html"));
            MathJaxView.NavigateToString($"<textarea id=\"input\" style=\"display: none;\"></textarea>\n<div id=\"output\"/>" + mathJaxImport);
        }

        public string TexString {
            get { return (string)GetValue(TexStringProperty); }
            set { SetValue(TexStringProperty, value); }
        }
        static readonly DependencyProperty TexStringProperty =
            DependencyProperty.RegisterAttached("TexString", typeof(string), typeof(MathJax), new PropertyMetadata(default(string), new PropertyChangedCallback(OnTexStringChanged)));
    }
}
