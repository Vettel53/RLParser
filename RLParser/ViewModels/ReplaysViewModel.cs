using Avalonia.Logging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using RLParser.Models;
using RLParser.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace RLParser.ViewModels
{
    public partial class ReplaysViewModel : ViewModelBase
    {
        private readonly ReplayParser _replayParser = new();

        [ObservableProperty] private string _selectedFileName = string.Empty;
        [ObservableProperty] private List<PlayerData> _playerList = [];

        public ObservableCollection<ReplayCardItem> RecentReplays { get; } = [];

        [RelayCommand]
        public async Task ParseUploadedReplay(IStorageFile file)
        {
            SelectedFileName = $"Selected: {file.Name}";
            Console.WriteLine($"File '{file.Name}': Attempting parsing...");

            JObject replayJson = _replayParser.ParseFileToJson(file);
            if (replayJson is null)
            {
                Console.WriteLine("ERROR: replayJson is null.");
                return;
            }

            List<PlayerData> extractedPlayers = _replayParser.ParseReplayJson(replayJson) ?? [];
            PlayerList = extractedPlayers;

            RecentReplays.Insert(0, new ReplayCardItem
            {
                Arena = Path.GetFileNameWithoutExtension(file.Name),
                Playlist = "N/A",
                ResultText = extractedPlayers.Count > 0 ? "PARSE SUCCESS" : "NO PLAYERS FOUND",
                ScoreText = $"Players: {extractedPlayers.Count}"
            });

            await Task.CompletedTask;
        }
    }
}