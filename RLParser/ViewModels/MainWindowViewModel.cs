using System;
using ReactiveUI;
using RLParser.Models;
using RLParser.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RLParser.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase _currentPage;
    private readonly ReplaysViewModel _replaysViewModel = new();
    private readonly HomeViewModel _homeViewModel = new();
    

    private readonly FilesService _filesService;
    public MainWindowViewModel(FilesService filesService)
    {
        _currentPage = _homeViewModel;
        _filesService = filesService;
    }
    
    public async Task UploadReplayCommand()
    {
        var file = await _filesService.OpenFileAsync();
            
        if (file is null)
        {
            Console.WriteLine("Failed to open uploaded file.");
            return;
        }
        
        Console.WriteLine($"File '{file.Name}' opened successfully.");

        await _replaysViewModel.ParseUploadedReplay(file);
    }

    [RelayCommand]
    private void NavigateToHome()
    {
        CurrentPage = _homeViewModel;
    }
    [RelayCommand]
    private void NavigateReplays() {
        CurrentPage = _replaysViewModel;
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