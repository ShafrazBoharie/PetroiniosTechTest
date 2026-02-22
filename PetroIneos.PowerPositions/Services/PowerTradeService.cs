using Services;

namespace PetroIneos.PowerPositions.Services;

public interface IPowerTradeService
{
    Task<IEnumerable<PowerTrade>> GetAggregatedTrades(DateTime date);
}

public class PowerTradeService(IPowerService powerService, ILogger<IPowerTradeService> logger) : IPowerTradeService
{
    public async Task<IEnumerable<PowerTrade>> GetAggregatedTrades(DateTime date)
    {
        if (date == default)
        {
            logger.LogError("Date must be a valid date.");
            throw new ArgumentException("Date must be a valid date.", nameof(date));
        }

        try
        {
            logger.LogInformation("Retrieving trades for {Date}", date);
            return await powerService.GetTradesAsync(date);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve trades for {date:yyyy-MM-dd}.", ex);
        }
    }
}