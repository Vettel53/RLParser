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

        public List<PlayerData> ParseReplayJson(JObject jsonObject)
        {
            var extractedPlayers = _playerRosterExtractor.Extract(jsonObject);
            if (extractedPlayers.Count == 0)
            {
                Console.WriteLine("No player data found in JSON.");
                return null;
            }

            var context = new ReplayParseContext(extractedPlayers);
            ProcessActorUpdates(jsonObject, context);

            foreach (var kvp in context.ActiveCarToPlayerName)
            {
                Console.WriteLine($"PRI: {kvp.Key}, Name: {kvp.Value}");
            }

            extractedPlayers.ForEach(Console.WriteLine);
            return extractedPlayers;
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
