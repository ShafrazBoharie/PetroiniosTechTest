namespace PetroIneos.PowerPositions.Services;

public interface IPowerTradeAggregator
{
    Task<Dictionary<int, double>> GetAggregatedTrades(DateTime date);
}

public class PowerTradeAggregator(IPowerTradeService powerTradeService, ILogger<IPowerTradeAggregator> logger) :IPowerTradeAggregator
{
    public async Task<Dictionary<int, double>> GetAggregatedTrades(DateTime date)
    {
        if (date == default)
        {
            logger.LogError("Date cannot be empty");
            throw new ArgumentException("Invalid date.Date must not be default.", nameof(date));
        }

        try
        {
            var trades = await powerTradeService.GetAggregatedTrades(date);

            return trades
                .SelectMany(t => t.Periods)
                .GroupBy(p => p.Period)
                .ToDictionary(g => g.Key, g =>Math.Round(g.Sum(p => p.Volume),4));

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting trades");
            throw new InvalidOperationException($"Failed to retrieve trades for {date:yyyy-MM-dd}.", ex);
        }
    }

}



