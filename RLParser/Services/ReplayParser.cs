using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia.Platform.Storage;
using RocketRP; // The core namespace
using RocketRP.Serializers;
using RLParser.Models;

namespace RLParser.Services
{
    public class ReplayParser
    {
        public JObject ParseFileToJson(IStorageFile file)
        {
            string jsonOutput = null;
            try
            {
                var replay = Replay.Deserialize(file.TryGetLocalPath(), parseNetstream: true, true);
                var serializer = new ReplayJsonSerializer();
                jsonOutput = serializer.Serialize(replay, prettyPrint: true);
                string outputPath = "replay_dump.json";
                File.WriteAllText(outputPath, jsonOutput);

                Console.WriteLine("Replay parsed successfully!");
                Console.WriteLine($"Engine Version: {replay.EngineVersion}");
                Console.WriteLine($"Licensee Version: {replay.LicenseeVersion}");
                Console.WriteLine($"Done! Analysis saved to {outputPath}");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            // Do i need this here?
            if (jsonOutput is null)
            {
                return null;
            }

            return JObject.Parse(jsonOutput);
        }

        public List<PlayerData> ParseReplayJson(JObject jsonObject)
        {
            List<PlayerData> extractedPlayers = new List<PlayerData>();
            
            var playerStats = jsonObject.SelectToken("Properties.PlayerStats");
            if (playerStats is null)
            {
                return null;
            }
            
            foreach (var player in playerStats)
            {
                extractedPlayers.Add(new PlayerData
                    {
                        // Name = player.SelectToken("Name")?.ToString(),
                        Name = player["Name"]?.ToObject<string>() ?? "N/A",
                        Score = player["Score"]?.ToObject<int>() ?? 0,
                        Goals = player["Goals"]?.ToObject<int>() ?? 0,
                        Team = player["Team"]?.ToObject<int>() ?? 0
                    }
                );
                // Check difference between the two variable "name" methods 
                // var name = player.SelectToken("Name")?.ToString();
                var name = player["Name"]?.ToObject<string>() ?? "N/A";
                var platform = player.SelectToken("Platform")?.ToString();

                // These are direct children of the player object
                var score = player["Score"];
                var goals = player["Goals"];
                var team = player["Team"];

                Console.WriteLine($"Player: {name} | Score: {score} | Goals: {goals} | Team: {team}");
            }

            return extractedPlayers;
        }
    }
}
