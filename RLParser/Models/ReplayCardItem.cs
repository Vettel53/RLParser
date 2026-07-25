using RLParser.Services.Parsing;

namespace RLParser.Models;

public sealed class ReplayCardItem
{
    public required ReplayParseContext Context { get; init; }
    public required string Map { get; init; } = "Unknown";
    public required string MatchType { get; init; } = "Unknown";
    public required string ReplayName { get; init; } = "Unknown";
    public required int PlayerCount { get; init; }
    public required string Team0Score { get; init; } = "0";
    public required string Team1Score { get; init; } = "0";
    public required string TeamSize { get; init; } = "0";
    public string ThumbnailUri { get; init; } = "avares://RLParser/Assets/deathmetal.jpg";
}
