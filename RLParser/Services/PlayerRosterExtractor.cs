using Newtonsoft.Json.Linq;
using RLParser.Models;
using System;
using System.Collections.Generic;

namespace RLParser.Services
{
    public class PlayerRosterExtractor
    {
        public List<PlayerData> Extract(JObject jsonObject)
        {
            var players = new List<PlayerData>();
            var playerStats = jsonObject.SelectToken("Properties.PlayerStats");

            if (playerStats is null)
            {
                return players;
            }

            foreach (var player in playerStats)
            {
                players.Add(new PlayerData
                {
                    Name = player["Name"]?.ToObject<string>() ?? "N/A",
                    Score = player["Score"]?.ToObject<int>() ?? 0,
                    Goals = player["Goals"]?.ToObject<int>() ?? 0,
                    Team = player["Team"]?.ToObject<int>() ?? 0,
                    TotalBoostGrabs = 0
                });
            }

            foreach (var player in players)
            {
                Console.WriteLine($"[PLAYER EXTRACTION] Extracted Player: {player.Name} | Score: {player.Score} | Goals: {player.Goals} | Team: {player.Team}");
            }

            return players;
        }
    }
}