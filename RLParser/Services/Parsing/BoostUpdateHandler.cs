using Newtonsoft.Json.Linq;
using RLParser.Models;
using RLParser.Services.ReferenceData;
using System;
using System.Collections.Generic;

namespace RLParser.Services.Parsing
{
    public class BoostUpdateHandler
    {
        private enum BigPadLocation
        {
            Unknown = -1,
            BlueGoal = 0, // team 0 is blue
            OrangeGoal = 1, // team 1 is orange
            Midfield = 2,
        }

        public void Handle(JToken update, ReplayParseContext context)
        {
            if (!TryGetNewPickupEvent(update, context, out int boostPadChannelId, out int instigatorActorId))
            {
                return;
            }

            if (!TryResolvePlayer(context, instigatorActorId, out int playerPriIndex, out string? playerName))
            {
                return;
            }

            bool isBigPad = CheckBoostSize(update);
            string padTypeString = isBigPad ? "Big (100)" : "Small (12)";

            BigPadLocation location = isBigPad
                ? CheckBigBoostLocation(update)
                : BigPadLocation.Unknown;

            // TODO: team check can use this once PRI -> Team mapping is available.
            bool boostSteal = CheckIfBoostSteal(playerName, context, location);
            if (boostSteal)
            {
                Console.WriteLine($"[BOOST STEAL] {playerName} stole a boost from the {(location == BigPadLocation.OrangeGoal ? "Orange" : "Blue")} Goal big pad!");
            } else {
                Console.WriteLine($"[BOOST GRAB] {playerName} grabbed a {padTypeString} boost from channel {boostPadChannelId}.");
            }

            context.IncrementBoostGrab(playerName, isBigPad);
        }

        private bool CheckIfBoostSteal(string? playerName, ReplayParseContext context, BigPadLocation location)
        {
            int team = -1; 
            foreach (var player in context.ExtractedPlayers)
            {
                if (player.Name == playerName)
                {
                    team = player.Team;
                }
            }

            if (team == -1) return false;
            
            return (team == 0 && location == BigPadLocation.OrangeGoal) // blue team stole orange boost
                || (team == 1 && location == BigPadLocation.BlueGoal); // orange team stole blue boost
        }

        private bool TryGetNewPickupEvent(
            JToken update,
            ReplayParseContext context,
            out int boostPadChannelId,
            out int instigatorActorId)
        {
            boostPadChannelId = update["ChannelId"]?.Value<int>() ?? -1;
            instigatorActorId = -1;

            if (boostPadChannelId == -1)
            {
                return false;
            }

            var pickupData = update.SelectToken("ActorData.NewReplicatedPickupData");
            if (pickupData == null)
            {
                return false;
            }

            int currentPickupValue = pickupData["PickedUp"]?.Value<int>() ?? -1;
            if (currentPickupValue == -1)
            {
                return false;
            }

            instigatorActorId = pickupData.SelectToken("Instigator.TargetIndex")?.Value<int>() ?? -1;

            bool isNewGrabEvent = IsNewGrabEvent(
                context,
                boostPadChannelId,
                currentPickupValue,
                instigatorActorId);

            context.LastBoostPickupValues[boostPadChannelId] = currentPickupValue;

            return isNewGrabEvent && instigatorActorId != -1;
        }

        private static bool IsNewGrabEvent(
            ReplayParseContext context,
            int boostPadChannelId,
            int currentPickupValue,
            int instigatorActorId)
        {
            if (!context.LastBoostPickupValues.TryGetValue(boostPadChannelId, out int previousValue))
            {
                return instigatorActorId != -1;
            }

            return currentPickupValue != previousValue;
        }

        private static bool TryResolvePlayer(
            ReplayParseContext context,
            int instigatorActorId,
            out int playerPriIndex,
            out string? playerName)
        {
            playerName = null;

            if (!context.ActiveCarToPlayerMap.TryGetValue(instigatorActorId, out playerPriIndex))
            {
                return false;
            }

            context.ActiveCarToPlayerName.TryGetValue(playerPriIndex, out playerName);
            return true;
        }

        private BigPadLocation CheckBigBoostLocation(JToken update)
        {
            if (!TryGetBoostPad(update, out BoostPad? pad))
            {
                return BigPadLocation.Unknown;
            }

            if (pad.BoostAmount < 15)
            {
                return BigPadLocation.Unknown;
            }

            double y = pad.Location.Y;

            if (Math.Abs(y) < 0.001)
            {
                return BigPadLocation.Midfield;
            }

            if (y < 0) { return BigPadLocation.BlueGoal; }
            if (y > 0) { return BigPadLocation.OrangeGoal; }

            return BigPadLocation.Unknown;
        }

        private bool CheckBoostSize(JToken update)
        {
            return TryGetBoostPad(update, out BoostPad? pad) && pad.BoostAmount >= 15;
        }

        private static bool TryGetBoostPad(JToken update, out BoostPad? pad)
        {
            pad = null;

            string boostPadTypeName = update["TypeName"]?.Value<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(boostPadTypeName))
            {
                return false;
            }

            if (!BoostPadCatlog._boostPadCatalog.TryGetValue(boostPadTypeName, out BoostPad foundPad))
            {
                return false;
            }

            pad = foundPad;
            return true;
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