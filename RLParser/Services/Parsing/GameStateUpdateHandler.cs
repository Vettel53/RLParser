using Newtonsoft.Json.Linq;
using System;

namespace RLParser.Services.Parsing
{
    public class GameStateUpdateHandler
    {
        public void Handle(JToken update, ReplayParseContext context)
        {
            var stateName = update.SelectToken("ActorData.ReplicatedStateName.Value")?.ToString();
            if (string.IsNullOrWhiteSpace(stateName))
            {
                return;
            }

            if (!string.Equals(context.CurrentGameState, stateName, StringComparison.Ordinal))
            {
                context.CurrentGameState = stateName;
                Console.WriteLine($"\n--- [MATCH STATE] Changed to: {context.CurrentGameState} ---");
            }
        }
    }
}