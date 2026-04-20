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
    }
}