using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ProjectEstrada.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ProjectEstrada.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        public MainViewModel ViewModel => (MainViewModel)DataContext;

        private TextBox lastFocusedTextBox = null;

        public Shell()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).Services.GetService(typeof(MainViewModel));

            Window.Current.SetTitleBar(TitlebarGrid);

            GraphContainer.Child = new Graphics.Controls.GraphControl()
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
            Window newView = new();
            string formulaText = lastFocusedTextBox.Text;
            Frame frame = new();
            frame.Navigate(typeof(Views.GraphView), formulaText);
            newView.Content = frame;
            newView.Activate();
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
