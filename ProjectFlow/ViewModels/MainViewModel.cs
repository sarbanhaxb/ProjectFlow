using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectFlow.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string pageTitle = "Главная панель";

        [RelayCommand]
        private void Navigate(string page)
        {
            PageTitle = page;
        }
    }
}
