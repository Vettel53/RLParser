using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RLParser.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    public string WelcomeMessage => "Welcome to Your Replay Dashboard";

    // This collection will hold the list of matches shown on the Home screen
    public ObservableCollection<string> RecentMatches { get; } = new()
    {
        "Mannfield - Win",
        "Wasteland - Loss",
        "Champions Field - Win"
    };
}