using Newtonsoft.Json.Linq;
using System;

namespace RLParser.Services.Parsing
{
    public class CarUpdateHandler
    {
        public void Handle(JToken update, ReplayParseContext context)
        {
            int carChannelId = update["ChannelId"]?.Value<int>() ?? -1;
            if (carChannelId == -1)
            {
                return;
            }

            var priToken = update["ActorData"]?["PlayerReplicationInfo"];
            if (priToken != null)
            {
                HandlePlayerReplicationInfo(priToken, carChannelId, context);
            }

            var rbState = update["ActorData"]?["ReplicatedRBState"];
            if (rbState != null)
            {
                HandleReplicatedRigidBodyState(rbState, carChannelId, context);
            }

            var teamPaintUpdate = update["ActorData"]?["TeamPaint"];
            if (teamPaintUpdate !=  null) {
                Console.WriteLine($"[TEAM UPDATE] Found TeamPaint update for Channel {carChannelId}");
                HandleTeamState(teamPaintUpdate, carChannelId, context);
            }
        }

        private void HandleTeamState(JToken teamPaintUpdate, int carChannelId, ReplayParseContext context)
        {
            // NOTE: Team 0 is team blue, Team 1 is team orange, and Team -1 is no team/invalid (i think)
            int teamIndex = teamPaintUpdate["Team"]?.Value<int>() ?? -1;

            if (!context.ActiveCarToPlayerMap.TryGetValue(carChannelId, out int targetIndex))
            {
                Console.WriteLine($"[TEAM MAP MISS] No player mapping for Channel {carChannelId}, Team {teamIndex}");
                return;
            }

            context.ActiveCarToPlayerName.TryGetValue(targetIndex, out string? name);

            if (teamIndex == -1)
            {
                Console.WriteLine($"[TEAM INVALID] Channel {carChannelId}, TargetIndex {targetIndex}, Name {name ?? "Unknown"}");
                return;
            }

            Console.WriteLine($"[TEAM MATCH] Found TeamPaint update (Channel {carChannelId}) linked to Player PRI (TargetIndex {targetIndex}) (Name {name ?? "Unknown"}) with Team {teamIndex}");
        }

        private static void HandlePlayerReplicationInfo(JToken priToken, int carChannelId, ReplayParseContext context)
        {
            int targetIndex = priToken["TargetIndex"]?.Value<int>() ?? -1;
            if (targetIndex == -1)
            {
                return;
            }

            context.ActiveCarToPlayerMap[carChannelId] = targetIndex;
            context.ActiveCarToPlayerName.TryGetValue(targetIndex, out string name);

            Console.WriteLine($"[CAR MATCH] Found Car_TA (Channel {carChannelId}) linked to Player PRI (TargetIndex {targetIndex}) (Name {name ?? "Unknown"})");
        }

        private static void HandleReplicatedRigidBodyState(JToken rbState, int carChannelId, ReplayParseContext context)
        {
            var linearVel = rbState["LinearVelocity"];
            if (linearVel is not JObject)
            {
                return;
            }

            double velX = linearVel["X"]?.Value<double>() ?? 0;
            double velY = linearVel["Y"]?.Value<double>() ?? 0;
            double velZ = linearVel["Z"]?.Value<double>() ?? 0;

            double speedMagnitude = Math.Sqrt((velX * velX) + (velY * velY) + (velZ * velZ));

            if (context.ActiveCarToPlayerMap.TryGetValue(carChannelId, out _))
            {
                _ = (speedMagnitude / 44.704) / 100;
            }
        }
    }
}