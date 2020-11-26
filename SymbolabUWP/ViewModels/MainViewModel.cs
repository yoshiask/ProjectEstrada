using Microsoft.Toolkit.Mvvm.ComponentModel;
using ProjectEstrada.Core.Functions;
using System.Collections.ObjectModel;
using Windows.UI;

namespace SymbolabUWP.ViewModels
{
    public class MainViewModel : ObservableRecipient
    {
        public ObservableCollection<FunctionViewModel> Functions { get; set; } = new ObservableCollection<FunctionViewModel>();

    }
}
