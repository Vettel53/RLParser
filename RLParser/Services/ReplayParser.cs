using Avalonia.Platform.Storage;
using Newtonsoft.Json.Linq;
using RLParser.Models;
using RLParser.Services.Parsing;
using System;
using System.Collections.Generic;

namespace RLParser.Services
{
    public class ReplayParser
    {
        private readonly ReplayFileReader _fileReader;
        private readonly PlayerRosterExtractor _playerRosterExtractor;

        private readonly GameStateUpdateHandler _gameStateHandler = new();
        private readonly CarUpdateHandler _carUpdateHandler = new();
        private readonly PriUpdateHandler _priUpdateHandler = new();
        private readonly BoostUpdateHandler _boostUpdateHandler = new();

        public ReplayParser() : this(new ReplayFileReader(), new PlayerRosterExtractor())
        {
        }

        public ReplayParser(ReplayFileReader fileReader, PlayerRosterExtractor playerRosterExtractor)
        {
            _fileReader = fileReader;
            _playerRosterExtractor = playerRosterExtractor;
        }

        public JObject ParseFileToJson(IStorageFile file)
        {
            return _fileReader.ParseFileToJson(file);
        }

        public List<PlayerData> ParseReplayJson(JObject jsonObject, out ReplayParseContext? context)
        {
            var extractedPlayers = _playerRosterExtractor.Extract(jsonObject);
            if (extractedPlayers.Count == 0)
            {
                Console.WriteLine("No player data found in JSON.");
                context = null;
                return null;
            }

            context = new ReplayParseContext(extractedPlayers);
            ExtractMatchProperties(jsonObject, context);
            ProcessActorUpdates(jsonObject, context);

            foreach (var kvp in context.ActiveCarToPlayerName)
            {
                Console.WriteLine($"PRI: {kvp.Key}, Name: {kvp.Value}");
            }

            extractedPlayers.ForEach(Console.WriteLine);
            return extractedPlayers;
        }

        private void ExtractMatchProperties(JObject jsonObject, ReplayParseContext context)
        {
            var matchProperties = jsonObject.SelectToken("Properties") ?? null;
            if (matchProperties is null) {
                Console.WriteLine("No match properties found in JSON.");
                return;
            }

            string replayName = matchProperties["ReplayName"]?.ToObject<string>() ?? "N/A";
            string mapName = matchProperties["MapName"]?["Value"]?.ToObject<string>() ?? "N/A";
            string matchType = matchProperties["MatchType"]?["Value"]?.ToObject<string>() ?? "N/A";
            int team0Score = matchProperties["Team0Score"]?.ToObject<int>() ?? 0;
            int team1Score = matchProperties["Team1Score"]?.ToObject<int>() ?? 0;
            int teamSize = matchProperties["TeamSize"]?.ToObject<int>() ?? 0;

            context.ReplayName = replayName;
            context.Map = mapName;
            context.MatchType = matchType;
            context.Team0Score = team0Score;
            context.Team1Score = team1Score;
            context.TeamSize = teamSize;
            Console.WriteLine($"Extracted Match Properties - ReplayName: {replayName}, Map: {mapName}, MatchType: {matchType}, Team0Score: {team0Score}, Team1Score: {team1Score}, TeamSize: {teamSize}");
        }

        private void ProcessActorUpdates(JObject jsonObject, ReplayParseContext context)
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
                    continue;
                }

                foreach (var update in actorUpdates)
                {
                    DispatchActorUpdate(update, context);
                }
            }
        }

        private void DispatchActorUpdate(JToken update, ReplayParseContext context)
        {
            string objectName = (string)update["ObjectName"];

            switch (objectName)
            {
                case "TAGame.GameEvent_Soccar_TA":
                    _gameStateHandler.Handle(update, context);
                    break;

                case "TAGame.Car_TA":
                    _carUpdateHandler.Handle(update, context);
                    break;

                case "TAGame.PRI_TA":
                    _priUpdateHandler.Handle(update, context);
                    break;

                case "TAGame.VehiclePickup_Boost_TA":
                    _boostUpdateHandler.Handle(update, context);
                    break;
            }
        }
    }
}
