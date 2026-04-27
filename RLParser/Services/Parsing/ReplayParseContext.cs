using RLParser.Models;
using System;
using System.Collections.Generic;

namespace RLParser.Services.Parsing
{
    public class ReplayParseContext
    {
        public ReplayParseContext(List<PlayerData> extractedPlayers)
        {
            ExtractedPlayers = extractedPlayers ?? throw new ArgumentNullException(nameof(extractedPlayers));
        }

        public List<PlayerData> ExtractedPlayers { get; }

        // carChannelId -> playerPriIndex
        public Dictionary<int, int> ActiveCarToPlayerMap { get; } = new();

        // playerPriIndex -> playerName
        public Dictionary<int, string> ActiveCarToPlayerName { get; } = new();

        public Dictionary<int, int> LastBoostPickupValues { get; } = new();
        public Dictionary<int, bool> IsBigBoostPadMap { get; } = new();

        public string CurrentGameState { get; set; } = "PreMatch";
        public string Map { get; set; } = "Unknown";
        public string MatchType { get; set; } = "Unknown";
        public string ReplayName { get; set; } = "Unknown";
        public int Team0Score { get; set; } = 0;
        public int Team1Score { get; set; } = 0;
        public int TeamSize { get; set; } = 0;

        public void IncrementBoostGrab(string playerName, bool isBig)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                return;
            }

            foreach (var player in ExtractedPlayers)
            {
                if (!player.Name.Equals(playerName, StringComparison.Ordinal))
                {
                    continue;
                }

                player.TotalBoostGrabs++;
                if (isBig) player.BigBoostGrabs++;
                else player.SmallBoostGrabs++;
                return;
            }
        }

        internal void IncrementBoostSteal(string? playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                Console.WriteLine($"[WARNING] Attempted to increment boost steal for invalid player name: '{playerName}'");
                return;
            }

            foreach (var player in ExtractedPlayers)
            {
                if (!player.Name.Equals(playerName, StringComparison.Ordinal))
                {
                    continue;
                }

                player.BoostSteals++;
                return;
            }
        }
    }
}