using Microsoft.Toolkit.Mvvm.ComponentModel;
using ProjectEstrada.Core.Functions;
using System.Collections.ObjectModel;

namespace ProjectEstrada.Core.ViewModels
{
    public class MainViewModel : ObservableRecipient
    {
        public ObservableCollection<FunctionViewModel> Functions { get; set; } = new ObservableCollection<FunctionViewModel>();
    }
}
