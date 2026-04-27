using RLParser.Models;
using RLParser.ViewModels;

namespace RLParser.ViewModels.Design
{
    public sealed class DesignReplaysViewModel : ReplaysViewModel
    {
        public DesignReplaysViewModel()
        {
            RecentReplays.Add(new ReplayCardItem
            {
                Map = "Mannfield (Night)",
                MatchType = "Ranked Standard",
                ReplayName = "Sample Replay 1",
                PlayerCount = "PARSE SUCCESS",
                Team0Score = "Blue: 2",
                Team1Score = "Orange: 3",
                TeamSize = "3",
                ThumbnailUri = "avares://RLParser/Assets/deathmetal.jpg"
            });

            RecentReplays.Add(new ReplayCardItem
            {
                Map = "DFH Stadium",
                MatchType = "Ranked Doubles",
                ReplayName = "Sample Replay 2",
                PlayerCount = "PARSE SUCCESS",
                Team0Score = "Blue: 1",
                Team1Score = "Orange: 4",
                TeamSize = "2",
                ThumbnailUri = "avares://RLParser/Assets/deathmetal.jpg"
            });
        }
    }
}