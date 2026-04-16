using Avalonia.Platform.Storage;
using Newtonsoft.Json.Linq;
using RLParser.Models;
using RocketRP; // The core namespace
using RocketRP.Actors.Engine;
using RocketRP.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RLParser.Services
{
    public class ReplayParser
    {
        List<PlayerData> extractedPlayers = new List<PlayerData>();
        Dictionary<int, int> activeCarToPlayerMap = new Dictionary<int, int>();
        Dictionary<int, string> activeCarToPlayerName = new Dictionary<int, string>();
        Dictionary<int, int> lastBoostPickupValues = new Dictionary<int, int>();
        Dictionary<int, bool> isBigBoostPadMap = new Dictionary<int, bool>();
        private string currentGameState = "PreMatch"; // Track active vs dead time

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
                    Team = player["Team"]?.ToObject<int>() ?? 0,
                    TotalBoostGrabs = 0
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
                Console.WriteLine($"[PLAYER EXTRACTION] Extracted Player: {player.Name} | Score: {player.Score} | Goals: {player.Goals} | Team: {player.Team}");
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
                        case "TAGame.GameEvent_Soccar_TA":
                            HandleGameState(update);
                            break;
                        case "TAGame.Car_TA":
                            HandleCarUpdate(update);
                            break;
                        case "TAGame.PRI_TA":
                            HandlePriUpdate(update);
                            break;
                        case "TAGame.VehiclePickup_Boost_TA":
                            HandleBoostUpdate(update);
                            break;
                        case "TAGame.CarComponent_Boost_TA":
                            //HandleBoostSpawn(update);
                            break;
                    }
                }
            }

            foreach (var kvp in activeCarToPlayerName)
            {
                Console.WriteLine($"ChannelId: {kvp.Key}, Name: {kvp.Value}");
            }

            extractedPlayers.ForEach(Console.WriteLine);
        }

        private void HandleBoostSpawn(JToken update)
        {
            throw new NotImplementedException();
        }

        private void HandleGameState(JToken update)
        {
            var stateName = update.SelectToken("ActorData.ReplicatedStateName.Value")?.ToString();
            if (stateName != null)
            {
                if (currentGameState != stateName) 
                {
                    currentGameState = stateName;
                    Console.WriteLine($"\n--- [MATCH STATE] Changed to: {currentGameState} ---");
                }
            }
        }

        private void HandleBoostUpdate(JToken update)
        {
            int boostPadChannelId = update["ChannelId"]?.Value<int>() ?? -1;
            if (boostPadChannelId == -1) return;

            // 1. INTERCEPT ACTOR SPAWN: 
            // Check if this frame contains the initial spawn coordinates for the pad
            var initialPosition = update.SelectToken("Vector") ?? update.SelectToken("InitialPosition");
            if (initialPosition != null && !isBigBoostPadMap.ContainsKey(boostPadChannelId))
            {
                double x = initialPosition["X"]?.Value<double>() ?? 0;
                double y = initialPosition["Y"]?.Value<double>() ?? 0;

                // Big Boosts are located at specific symmetric coordinates on standard maps.
                // Mid boosts: X ≈ ±3584, Y ≈ 0
                // Corner boosts: X ≈ ±3072, Y ≈ ±4096
                double absX = Math.Abs(x);
                double absY = Math.Abs(y);
                Console.WriteLine(x + " " + y);

                bool isBig = false;
                
                // Using a tolerance range for standard maps
                if (absX > 3400 && absY < 500) isBig = true; // Mids
                else if (absX > 2900 && absY > 3900) isBig = true; // Corners

                isBigBoostPadMap[boostPadChannelId] = isBig;
            }

            // 2. PROCESS PICKUP EVENT (Your existing logic)
            var pickupData = update.SelectToken("ActorData.NewReplicatedPickupData");
            if (pickupData == null) return;

            int currentPickupValue = pickupData["PickedUp"]?.Value<int>() ?? -1;
            int instigatorActorId = pickupData.SelectToken("Instigator.TargetIndex")?.Value<int>() ?? -1;

            if (currentPickupValue == -1) return;

            bool isNewGrabEvent = false;

            if (!lastBoostPickupValues.TryGetValue(boostPadChannelId, out int previousValue))
            {
                if (instigatorActorId != -1) isNewGrabEvent = true;
            }
            else 
            {
                if (currentPickupValue != previousValue) isNewGrabEvent = true;
            }

            lastBoostPickupValues[boostPadChannelId] = currentPickupValue;

            if (isNewGrabEvent && instigatorActorId != -1)
            {
                if (activeCarToPlayerMap.TryGetValue(instigatorActorId, out int playerPRIIndex)) 
                {
                    activeCarToPlayerName.TryGetValue(playerPRIIndex, out string playerName);

                    //if (currentGameState != "Active") return;

                    // Check if it's a big pad or small pad from our cached map
                    bool isBigPad = isBigBoostPadMap.GetValueOrDefault(boostPadChannelId, false);
                    string padTypeString = isBigPad ? "Big (100)" : "Small (12)";

                    Console.WriteLine($"[BOOST EVENT] {playerName ?? "Unknown"} grabbed {padTypeString} Boost Pad {boostPadChannelId}");
                    IncrementBoostGrab(playerName, isBigPad);
                }
            }
        }

        // 3. UPDATE INCREMENT METHOD
        private void IncrementBoostGrab(string? playerName, bool isBig)
        {
            foreach (var player in extractedPlayers)
            {
                if (player.Name.Equals(playerName))
                {
                    player.TotalBoostGrabs++;
                    if (isBig)
                    {
                        player.BigBoostGrabs++;
                    }
                    else
                    {
                        player.SmallBoostGrabs++;
                    }
                }
            }
        }

        private void HandlePriUpdate(JToken update)
        {
            int carChannelId = (int)update["ChannelId"]; // The physical car object (players are assigned objects)

            var actorToken = update["ActorData"];
            if (actorToken != null)
            {
                MatchNameToChannelId(actorToken, carChannelId);
            }
        }

        private void MatchNameToChannelId(JToken actorToken, int carChannelId)
        {
            if (activeCarToPlayerMap.ContainsValue(carChannelId))
            {
                var playerName = actorToken["PlayerName"];
                if (playerName != null)
                {
                    if (activeCarToPlayerName.ContainsKey(carChannelId)) return; // no dupe channel ids, throws exceptions

                    activeCarToPlayerName.Add(carChannelId, playerName.ToString());
                    Console.WriteLine("[MATCH NAME - CHANNELID]: " + carChannelId + " is player (" + playerName + ")");
                }
            } else
            {
                Console.WriteLine("[FAILURE MATCH-NAME-CHANNELID] Value " + carChannelId + " not found.");
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
                activeCarToPlayerName.TryGetValue(targetIndex, out string name);
                Console.WriteLine($"[CAR MATCH] Found Car_TA (Channel {carChannelId}) linked to Player PRI (TargetIndex {targetIndex}) (Name {name ?? "Unknown"}");
            }
        }

        private void HandleReplicatedRigidBodyState(JToken rbState, int carChannelId)
        {
            var linearVel = rbState?["LinearVelocity"];
            if (linearVel is JObject) // Check if linearVel actually has xyz child properties, linearVel's internal value can be "null" and bypass a null check
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
                    //Console.WriteLine($"Player {drivingPlayerTargetIndex} is moving at speed: {speedMph} MPH");
                    // You can add this speedMagnitude to a running total for the player to average later!
                }
            }

        }
    }
}
