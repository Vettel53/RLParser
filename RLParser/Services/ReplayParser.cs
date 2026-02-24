using Avalonia.Platform.Storage;
using Newtonsoft.Json.Linq;
using RLParser.Models;
using RocketRP; // The core namespace
using RocketRP.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;

namespace RLParser.Services
{
    public class ReplayParser
    {
        Dictionary<int, int> activeCarToPlayerMap = new Dictionary<int, int>();

        public JObject ParseFileToJson(IStorageFile file)
        {
            string jsonOutput = null;
            try
            {
                var replay = Replay.Deserialize(file.TryGetLocalPath(), parseNetstream: true, true);
                var serializer = new ReplayJsonSerializer();
                jsonOutput = serializer.Serialize(replay, prettyPrint: true);
                string outputPath = "replay_dump.json";
                //File.WriteAllText(outputPath, jsonOutput);

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
                // var name = player["Name"]?.ToObject<string>() ?? "N/A";
            }

            if (extractedPlayers.Count == 0)
            {
                Console.WriteLine("No player data found in JSON.");
                return null;
            }

            foreach (var player in extractedPlayers)
            {
                Console.WriteLine($"Extracted Player: {player.Name} | Score: {player.Score} | Goals: {player.Goals} | Team: {player.Team}");
            }

            MatchPlayerReplicationInfo(jsonObject);

            return extractedPlayers;
        }

        public void MatchPlayerReplicationInfo(JObject jsonObject)
        {
            JArray frames = (JArray)jsonObject["Frames"];
            if (frames is null)
            {
                Console.WriteLine("No frames found in parsed JSON.");
                return;
            }

            foreach (var frame in frames)
            {
                JArray actorUpdates = (JArray)frame["ActorUpdates"];
                if (actorUpdates is null)
                {
                    continue; // No actor updates in this frame, skip to the next one
                }

                foreach (var update in actorUpdates)
                {
                    string objectName = (string)update["ObjectName"];

                    switch (objectName)
                    {
                        case "TAGame.Car_TA":
                            HandleCarUpdate(update);
                            break;
                    }

                }
            }
        }

        private void HandleCarUpdate(JToken update)
        {
            int carChannelId = (int)update["ChannelId"]; // The physical car object (players are assigned objects)

            var priToken = update["ActorData"]?["PlayerReplicationInfo"];
            if (priToken != null)
            {
                HandlePlayerReplicationInfo(priToken, carChannelId);
            }

            var rbState = update["ActorData"]?["ReplicatedRBState"];
            if (rbState != null)
            {
                HandleReplicatedRigidBodyState(rbState, carChannelId);
            }
        }

        private void HandlePlayerReplicationInfo(JToken priToken, int carChannelId)
        {
            int targetIndex = (int)priToken["TargetIndex"]; // The actual player

            if (targetIndex != -1) // When cars are destroyed, targetIndex is set to -1, so ignore those
            {
                activeCarToPlayerMap[carChannelId] = targetIndex; // add or update mapping (Channel 126 now belongs to Player 36)
                Console.WriteLine($"\nFound Car_TA (Channel {carChannelId}) linked to Player PRI (TargetIndex {targetIndex})");
            }
        }

        private void HandleReplicatedRigidBodyState(JToken rbState, int carChannelId)
        {
            var linearVel = rbState["LinearVelocity"];
            if (linearVel != null)
            {
                double velX = (double)linearVel["X"];
                double velY = (double)linearVel["Y"];
                double velZ = (double)linearVel["Z"];
                // Calculate the 3D magnitude (pythagorean theorem)
                double speedMagnitude = Math.Sqrt((velX * velX) + (velY * velY) + (velZ * velZ));
                // Look up who is driving this car from the class level dictionary
                if (activeCarToPlayerMap.TryGetValue(carChannelId, out int drivingPlayerTargetIndex))
                {
                    double speedMph = (double)(speedMagnitude / 44.704) / 100; // Convert Unreal units (cm/s) to MPH, divide by 100 for (idk)
                    Console.WriteLine($"Player {drivingPlayerTargetIndex} is moving at speed: {speedMph} MPH");
                    // You can add this speedMagnitude to a running total for the player to average later!
                }
            }

        }
    }
}
