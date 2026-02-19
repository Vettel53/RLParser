using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RocketRP; // The core namespace
using RocketRP.Serializers;
using RLParser.Models;

namespace RLParser.Services
{
    public class ReplayParser
    {
        public List<PlayerData> ParseFile(string inputFilePath)
        {
            var results = new List<PlayerData>();
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(baseDir, inputFilePath);
            System.Diagnostics.Debug.WriteLine("Looking for file at: " + filePath);

            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine("Error: File not found at " + filePath);
                return null;
            }

            System.Diagnostics.Debug.WriteLine("Parsing Replay...");

            try
            {
                // ARGUMENTS:
                // 1. path: Path to the file
                // 2. parseNetstream (bool): 
                //    - true: Parses specific player movements/events (slower, needed for heatmaps/stats).
                //    - false: Only parses header info like players, score, map (faster).
                // 3. enforceCRC (bool): Checks if the file is corrupted.
                var replay = Replay.Deserialize(filePath, parseNetstream: true, true);
                var serializer = new ReplayJsonSerializer();
                string jsonOutput = serializer.Serialize(replay, prettyPrint: true);
                string outputPath = "replay_dump.json";
                File.WriteAllText(outputPath, jsonOutput);

                System.Diagnostics.Debug.WriteLine("Replay parsed successfully!");
                System.Diagnostics.Debug.WriteLine($"Engine Version: {replay.EngineVersion}");
                System.Diagnostics.Debug.WriteLine($"Licensee Version: {replay.LicenseeVersion}");
                System.Diagnostics.Debug.WriteLine($"Done! Analysis saved to {outputPath}");

                JObject jsonObject = JObject.Parse(jsonOutput);
                var playerStats = jsonObject.SelectToken("Properties.PlayerStats");
                if (playerStats != null)
                {
                    foreach (var player in playerStats)
                    {
                        results.Add(new PlayerData
                            {
                                Name = player.SelectToken("Name")?.ToString(),
                                Score = player["Score"]?.ToObject<int>() ?? 0,
                                Goals = player["Goals"]?.ToObject<int>() ?? 0,
                                Team = player["Team"]?.ToObject<int>() ?? 0
                            }
                        );
                        var name = player.SelectToken("Name")?.ToString();
                        var platform = player.SelectToken("Platform")?.ToString();

                        // These are direct children of the player object
                        var score = player["Score"];
                        var goals = player["Goals"];
                        var team = player["Team"];

                        System.Diagnostics.Debug.WriteLine($"Player: {name} | Score: {score} | Goals: {goals} | Team: {team}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("PlayerStats not found in JSON output.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An error occurred: {ex.Message}");
            }

            return results;
        }
    }
}
