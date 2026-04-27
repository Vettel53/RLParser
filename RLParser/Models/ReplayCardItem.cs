namespace RLParser.Models;

public sealed class ReplayCardItem
{
    public required string Map { get; init; } = "Unknown";
    public required string MatchType { get; init; } = "Unknown";
    public required string ReplayName { get; init; } = "Unknown";
    public required string PlayerCount { get; init; } = "0";
    public required string Team0Score { get; init; } = "0";
    public required string Team1Score { get; init; } = "0";
    public required string TeamSize { get; init; } = "0";
    public string ThumbnailUri { get; init; } = "avares://RLParser/Assets/deathmetal.jpg";
}