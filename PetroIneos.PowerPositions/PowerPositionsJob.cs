using Microsoft.Extensions.Options;
using PetroIneos.PowerPositions.Config;
using PetroIneos.PowerPositions.Services;
using Quartz;

namespace PetroIneos.PowerPositions;

[DisallowConcurrentExecution]
public class PowerPositionsJob(
    IPowerTradeAggregator powerTradeAggregator,
    IPowerReportGeneratorService powerReportGeneratorService,
    IOptions<PowerPositionsSettings> settings,
    ILogger<PowerPositionsJob> logger) :IJob
{

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Starting scheduled power position extract");

        try
        {
            // 1. Determine local London time for the extract
            var londonZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            var extractTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, londonZone);

            var aggregatedData = await powerTradeAggregator.GetAggregatedTrades(extractTime.Date);

            powerReportGeneratorService.ExportToCsv(aggregatedData, settings.Value.OutputPath,extractTime);

            logger.LogInformation("Completed scheduled power position extract");

        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred during the power extraction job");
            // Quartz allows you to throw a JobExecutionException to trigger retries if configured
            throw new JobExecutionException(ex, refireImmediately: false);
        }

    }
}