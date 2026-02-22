namespace PetroIneos.PowerPositions.Config;

public record PowerPositionsSettings
{
    public string OutputPath { get; init; } = string.Empty;
    public int IntervalInMinutes { get; init; }
}