using CommunityToolkit.Mvvm.ComponentModel;

namespace ProjectFlow.ViewModels;

public partial class PlaceholderViewModel : ObservableObject
{
    public string Message { get; }

    public PlaceholderViewModel(string message)
    {
        Message = message;
    }
}