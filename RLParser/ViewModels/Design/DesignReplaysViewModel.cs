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
                Arena = "Mannfield (Night)",
                Playlist = "Ranked Standard",
                ResultText = "PARSE SUCCESS",
                ScoreText = "Players: 6",
                ThumbnailUri = "avares://RLParser/Assets/deathmetal.jpg"
            });

            RecentReplays.Add(new ReplayCardItem
            {
                Arena = "DFH Stadium",
                Playlist = "Ranked Doubles",
                ResultText = "PARSE SUCCESS",
                ScoreText = "Players: 4",
                ThumbnailUri = "avares://RLParser/Assets/deathmetal.jpg"
            });
        }
    }
}