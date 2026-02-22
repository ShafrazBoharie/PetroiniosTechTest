namespace PetroIneos.PowerPositions.Services;

public interface IPowerReportGeneratorService
{
    void ExportToCsv(Dictionary<int, double> aggregatedData, string exportDirectory, DateTime extractTime);
}

public class PowerReportGeneratorService(ILogger<IPowerReportGeneratorService> logger) : IPowerReportGeneratorService
{
    public void ExportToCsv(Dictionary<int, double> aggregatedData, string exportDirectory, DateTime extractTime)
    {
        var fileName = $"PowerPosition_{extractTime:yyyyMMdd}_{extractTime:HHmm}.csv";
        var filePath = Path.Combine(exportDirectory, fileName);

        // idempotent — create if not exist
        Directory.CreateDirectory(exportDirectory);

        try
        {
            logger.LogInformation("Writing to {FileName}", fileName);
            using var writer = new StreamWriter(filePath, append: false, System.Text.Encoding.UTF8);

            writer.WriteLine("Local Time, Volume");

            foreach (var pair in aggregatedData.OrderBy(x => x.Key))
            {
                var localTime = GetLocalTimeForPeriod(pair.Key);
                writer.WriteLine($"{localTime},{pair.Value}");
            }
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Error writing to {FileName}", fileName);
            throw new InvalidOperationException($"Failed to write report to '{filePath}'.", ex);
        }
    }

    private string GetLocalTimeForPeriod(int period)
    {
        var hour = (22 + period) % 24;
        return $"{hour:D2}:00";
    }
}

