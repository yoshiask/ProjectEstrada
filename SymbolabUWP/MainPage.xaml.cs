using SymbolabUWP.ViewModels;
using System;
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
        public MainViewModel ViewModel => (MainViewModel)DataContext;

        private TextBox lastFocusedTextBox = null;

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = ((App)Application.Current).Services.GetService(typeof(MainViewModel));

            Window.Current.SetTitleBar(TitlebarGrid);

            GraphContainer.Child = new ProjectEstrada.Graphics.Controls.GraphControl()
            {
                
            };
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
    }
}
