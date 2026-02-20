using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RLParser.Models;
using RLParser.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Logging;
using Avalonia.Platform.Storage;
using Newtonsoft.Json.Linq;

namespace RLParser.ViewModels
{
    public partial class ReplaysViewModel : ViewModelBase
    {
        private readonly ReplayParser _replayParser = new();
        [ObservableProperty] private string _selectedFileName;

        [ObservableProperty] private List<PlayerData> _playerList;

        [RelayCommand]
        public async Task ParseUploadedReplay(IStorageFile file)
        {
            await using var stream = await file.OpenReadAsync();
            SelectedFileName = $"Selected: {file.Name}";
            Console.WriteLine($"File '{file.Name}': Attempting parsing...");
            
            JObject replayJson = _replayParser.ParseFileToJson(file);
            Console.WriteLine("Got past this");
            if (replayJson is null)
            {
                Console.WriteLine("ERROR: replayJson is null.");
                return; // error log something, notification in Avalonia UI maybe?
            }

            List<PlayerData> extractedPlayers = _replayParser.ParseReplayJson(replayJson);
            Console.WriteLine($"ReplaysViewModel : " + extractedPlayers);
            //// 2. Update the property (this triggers the UI update)
            //AnalysisResult = "Parsing Rocket League Replay... Done! Found" + players[0].Score;
        }

    }
}