using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RLParser.Models;
using RLParser.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RLParser.ViewModels
{
    public partial class ReplaysViewModel : ViewModelBase
    {
        private readonly Services.FilesService _filesService;
        [ObservableProperty]
        private string _selectedFileName;

        public ReplaysViewModel()
        {
        }

        public ReplaysViewModel(Services.FilesService filesService)
        {
            _filesService = filesService;
        }

        [RelayCommand]
        public async Task OpenReplayCommand()
        {
            var file = await _filesService.OpenFileAsync();

            if (file != null)
            {
                await using var stream = await file.OpenReadAsync();
                System.Diagnostics.Debug.WriteLine($"File '{file.Name}' opened successfully.");
                SelectedFileName = $"Selected: {file.Name}";
                //ReplayParser parser = new ReplayParser();
                //string fileName = "test.replay";
                //List<PlayerData> players = parser.ParseFile(fileName);
                //// 2. Update the property (this triggers the UI update)
                //AnalysisResult = "Parsing Rocket League Replay... Done! Found" + players[0].Score;
            }
        }

    }
}