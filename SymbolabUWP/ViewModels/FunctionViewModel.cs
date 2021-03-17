using Microsoft.Toolkit.Mvvm.ComponentModel;
using ProjectEstrada.Core.Functions;
using ProjectEstrada.Core.Helpers;
using Windows.UI;

namespace SymbolabUWP.ViewModels
{
    public class FunctionViewModel : ObservableRecipient
    {
        public FunctionViewModel() { }

        private GenericFunction function;
        public GenericFunction Function
        {
            get => function;
            set => SetProperty(ref function, value);
        }

        private string inputText;
        public string InputText
        {
            get => inputText;
            set
            {
                SetProperty(ref inputText, value);

                const string errorText = @"\color{red}\text{Error: Failed to parse function}";
                try
                {
                    Function = ParseLaTeX.ParseFunction(value);
                    if (Function == null)
                        OutputLaTeX = @"\color{red}\text{Error: Failed to parse function}";
                    else
                        OutputLaTeX = Function.Latexise();
                }
                catch
                {
                    OutputLaTeX = errorText;
                }
            }
        }

        private string outputLaTeX;
        public string OutputLaTeX
        {
            get => outputLaTeX;
            set => SetProperty(ref outputLaTeX, value);
        }

        private Color color;
        public Color Color
        {
            get => color;
            set => SetProperty(ref color, value);
        }

        private double fontSize = 40;
        public double FontSize
        {
            get => fontSize;
            set => SetProperty(ref fontSize, value);
        }

        public FunctionType RequestedType
        {
            get => Function.RequestedType;
            set => SetProperty(Function.RequestedType, value, Function, (u, n) => u.RequestedType = n);
        }
    }
}
