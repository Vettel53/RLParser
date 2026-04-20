using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace RLParser.Services.Parsing
{
    public class BoostUpdateHandler
    {
        public void Handle(JToken update, ReplayParseContext context)
        {
            int boostPadChannelId = update["ChannelId"]?.Value<int>() ?? -1;
            if (boostPadChannelId == -1)
            {
                return;
            }

            CachePadSizeIfNeeded(update, boostPadChannelId, context);

            var pickupData = update.SelectToken("ActorData.NewReplicatedPickupData");
            if (pickupData == null)
            {
                return;
            }

            int currentPickupValue = pickupData["PickedUp"]?.Value<int>() ?? -1;
            int instigatorActorId = pickupData.SelectToken("Instigator.TargetIndex")?.Value<int>() ?? -1;

            if (currentPickupValue == -1)
            {
                return;
            }

            bool isNewGrabEvent = false;

            if (!context.LastBoostPickupValues.TryGetValue(boostPadChannelId, out int previousValue))
            {
                if (instigatorActorId != -1) isNewGrabEvent = true;
            }
            else if (currentPickupValue != previousValue)
            {
                isNewGrabEvent = true;
            }

            context.LastBoostPickupValues[boostPadChannelId] = currentPickupValue;

            if (!isNewGrabEvent || instigatorActorId == -1)
            {
                return;
            }

            if (!context.ActiveCarToPlayerMap.TryGetValue(instigatorActorId, out int playerPriIndex))
            {
                return;
            }

            context.ActiveCarToPlayerName.TryGetValue(playerPriIndex, out string playerName);

            bool isBigPad = context.IsBigBoostPadMap.GetValueOrDefault(boostPadChannelId, false);
            string padTypeString = isBigPad ? "Big (100)" : "Small (12)";

            Console.WriteLine($"[BOOST EVENT] {playerName ?? "Unknown"} grabbed {padTypeString} Boost Pad {boostPadChannelId}");
            context.IncrementBoostGrab(playerName, isBigPad);
        }

        private static void CachePadSizeIfNeeded(JToken update, int boostPadChannelId, ReplayParseContext context)
        {
            if (context.IsBigBoostPadMap.ContainsKey(boostPadChannelId))
            {
                return;
            }

            var initialPosition = update.SelectToken("Vector") ?? update.SelectToken("InitialPosition");
            if (initialPosition == null)
            {
                return;
            }

            double x = initialPosition["X"]?.Value<double>() ?? 0;
            double y = initialPosition["Y"]?.Value<double>() ?? 0;

            double absX = Math.Abs(x);
            double absY = Math.Abs(y);

            bool isBig = false;
            if (absX > 3400 && absY < 500) isBig = true;
            else if (absX > 2900 && absY > 3900) isBig = true;

            context.IsBigBoostPadMap[boostPadChannelId] = isBig;
        }
    }
}