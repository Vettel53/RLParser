using Avalonia.Logging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using RLParser.Models;
using RLParser.Services;
using RLParser.Services.Parsing;
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
        [ObservableProperty] private ReplayCardItem? _selectedReplay;
        [ObservableProperty] private bool _isShowingReplayDetails;

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

            List<PlayerData> extractedPlayers = _replayParser.ParseReplayJson(replayJson, out ReplayParseContext? context) ?? [];
            PlayerList = extractedPlayers;

            if (context is null)
            {
                Console.WriteLine("ERROR: replay parse context is null.");
                return;
            }

            ReplayCardItem replayCard = new()
            {
                Context = context,
                Map = context.Map, // placeholder we aren;t extracting arena yet
                MatchType = context.MatchType, // placeholder we aren;t extracting match type yet
                ReplayName = context.ReplayName, // placeholder we aren;t extracting replay name yet
                PlayerCount = extractedPlayers.Count,
                Team0Score = $"Blue: {context.Team0Score}",
                Team1Score = $"Orange: {context.Team1Score}",
                TeamSize = context.TeamSize > 0 ? context.TeamSize.ToString() : "0"
            };

            RecentReplays.Insert(0, replayCard);

            await Task.CompletedTask;
        }

        partial void OnSelectedReplayChanged(ReplayCardItem? value)
        {
            if (value is not null)
            {
                IsShowingReplayDetails = true;
            }
        }

        [RelayCommand]
        private void CloseReplayDetails()
        {
            IsShowingReplayDetails = false;
            SelectedReplay = null;
        }
    }
}
