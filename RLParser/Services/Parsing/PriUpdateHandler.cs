using Newtonsoft.Json.Linq;
using System;

namespace RLParser.Services.Parsing
{
    public class PriUpdateHandler
    {
        public void Handle(JToken update, ReplayParseContext context)
        {
            // For PRI_TA, this ChannelId is the PRI channel/id.
            int priChannelId = update["ChannelId"]?.Value<int>() ?? -1;
            if (priChannelId == -1)
            {
                return;
            }

            var actorToken = update["ActorData"];
            if (actorToken is null)
            {
                return;
            }

            var playerName = actorToken["PlayerName"]?.ToString();
            if (string.IsNullOrWhiteSpace(playerName))
            {
                return;
            }

            // Ensure this PRI is actually linked to at least one active car mapping.
            if (!context.ActiveCarToPlayerMap.ContainsValue(priChannelId))
            {
                Console.WriteLine($"[FAILURE MATCH-NAME-PRI] PRI {priChannelId} not found in car->PRI map.");
                return;
            }

            // Name map is keyed by PRI index.
            context.ActiveCarToPlayerName[priChannelId] = playerName;
            Console.WriteLine($"[MATCH NAME - PRI]: PRI {priChannelId} is player ({playerName})");
        }
    }
}