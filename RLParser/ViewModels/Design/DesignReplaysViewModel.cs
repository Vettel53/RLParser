using RLParser.Models;
using RLParser.Services.Parsing;
using RLParser.ViewModels;
using System.Collections.Generic;

namespace RLParser.ViewModels.Design
{
    public sealed class DesignReplaysViewModel : ReplaysViewModel
    {
        public DesignReplaysViewModel()
        {
            ReplayParseContext firstContext = new(new List<PlayerData>
            {
                new() { Name = "Blue Player", Team = 0, Score = 540, Goals = 2, TotalBoostGrabs = 18, BigBoostGrabs = 6, SmallBoostGrabs = 12, BoostSteals = 1 },
                new() { Name = "Orange Player", Team = 1, Score = 620, Goals = 3, TotalBoostGrabs = 21, BigBoostGrabs = 8, SmallBoostGrabs = 13, BoostSteals = 2 }
            })
            {
                ReplayName = "Sample Replay 1",
                Map = "Mannfield (Night)",
                MatchType = "Ranked Standard",
                Team0Score = 2,
                Team1Score = 3,
                TeamSize = 3,
                CurrentGameState = "PostMatch"
            };
            RecentReplays.Add(new ReplayCardItem
            {
                Context = firstContext,
                Map = "Mannfield (Night)",
                MatchType = "Ranked Standard",
                ReplayName = "Sample Replay 1",
                PlayerCount = 2,
                Team0Score = "Blue: 2",
                Team1Score = "Orange: 3",
                TeamSize = "3",
                ThumbnailUri = "avares://RLParser/Assets/deathmetal.jpg"
            });

            RecentReplays.Add(new ReplayCardItem
            {
                Context = new ReplayParseContext(new List<PlayerData>
                {
                    new() { Name = "Second Sample", Team = 0, Score = 410, Goals = 1, TotalBoostGrabs = 15, BigBoostGrabs = 5, SmallBoostGrabs = 10 }
                })
                {
                    ReplayName = "Sample Replay 2",
                    Map = "DFH Stadium",
                    MatchType = "Ranked Doubles",
                    Team0Score = 1,
                    Team1Score = 4,
                    TeamSize = 2,
                    CurrentGameState = "PostMatch"
                },
                Map = "DFH Stadium",
                MatchType = "Ranked Doubles",
                ReplayName = "Sample Replay 2",
                PlayerCount = 1,
                Team0Score = "Blue: 1",
                Team1Score = "Orange: 4",
                TeamSize = "2",
                ThumbnailUri = "avares://RLParser/Assets/deathmetal.jpg"
            });

        }
    }
}
