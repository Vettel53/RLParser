using ReactiveUI;
using RLParser.Models;
using RLParser.Services;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RLParser.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage;

    public MainWindowViewModel()
    {
        _currentPage = new HomeViewModel();
    }

    [RelayCommand]
    private void NavigateToHome()
    {
        CurrentPage = new HomeViewModel();
    }
    [RelayCommand]
    private void NavigateReplays() {
        CurrentPage = new ReplaysViewModel();
    }

    //private string _analysisResult = "No file parsed yet.";

    //// The UI will "Bind" to this property
    //public string AnalysisResult
    //{
    //    get => _analysisResult;
    //    set => this.RaiseAndSetIfChanged(ref _analysisResult, value);
    //}

    //public void OnAnalyzeClicked()
    //{
    //    // 1. Call your logic
    //    ReplayParser parser = new ReplayParser();
    //    string fileName = "test.replay";
    //    List<PlayerData> players = parser.ParseFile(fileName);
    //    // 2. Update the property (this triggers the UI update)
    //    AnalysisResult = "Parsing Rocket League Replay... Done! Found" + players[0].Score;
    //}
}