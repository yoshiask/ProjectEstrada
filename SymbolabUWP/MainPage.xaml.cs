using System;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
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
        public MainPage()
        {
            this.InitializeComponent();

            Window.Current.SetTitleBar(TitlebarGrid);

            Lib.MathUtils.FindVerticalAsymptotes("1/x", "x");
        }

        private void Text_Click(object sender, RoutedEventArgs e)
        {
            string text = TextInput.SelectedText;
            int start = TextInput.SelectionStart;
            int length = TextInput.SelectionLength;
            TextInput.Text = TextInput.Text.Remove(start, length);
            TextInput.Text = TextInput.Text.Insert(start, @"\text{" + text + "}");
            TextInput.SelectionStart = start + 6;
        }

        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            string text = TextInput.SelectedText;
            int start = TextInput.SelectionStart;
            int length = TextInput.SelectionLength;
            TextInput.Text = TextInput.Text.Remove(start, length);
            TextInput.Text = TextInput.Text.Insert(start, @"\bf{" + text + "}");
            TextInput.SelectionStart = start + 4;
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            string text = TextInput.SelectedText;
            int start = TextInput.SelectionStart;
            int length = TextInput.SelectionLength;
            TextInput.Text = TextInput.Text.Remove(start, length);
            TextInput.Text = TextInput.Text.Insert(start, @"\it{" + text + "}");
            TextInput.SelectionStart = start + 4;
        }

        private void Underline_Click(object sender, RoutedEventArgs e)
        {
            string text = TextInput.SelectedText;
            int start = TextInput.SelectionStart;
            int length = TextInput.SelectionLength;
            TextInput.Text = TextInput.Text.Remove(start, length);
            TextInput.Text = TextInput.Text.Insert(start, @"\underline{" + text + "}");
            TextInput.SelectionStart = start + 11;
        }

        private void List_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void OpenGraph_Click(object sender, RoutedEventArgs e)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            string formulaText = TextInput.Text;
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
    }
}
