namespace RLParser.Models;

public sealed class ReplayCardItem
{
    public required string Arena { get; init; }
    public required string Playlist { get; init; }
    public required string ResultText { get; init; }
    public required string ScoreText { get; init; }
    public string ThumbnailUri { get; init; } = "avares://RLParser/Assets/deathmetal.jpg";
}