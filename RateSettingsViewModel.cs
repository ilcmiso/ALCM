using ALCM.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

public partial class RateSettingsViewModel : ObservableObject
{
    public ObservableCollection<RateTier> RateTiers { get; } = new();

    public RateSettingsViewModel()
    {
        RateTiers.Add(new RateTier());
    }

    [RelayCommand]
    void AddTier()
    {
        RateTiers.Add(new RateTier());
    }
}
