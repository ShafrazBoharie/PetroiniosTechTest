using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PetroIneos.PowerPositions.Services;

namespace PetroIneos.PowerPositions.Tests.Services;

public class PowerReportGeneratorServiceTests : IDisposable
{
    private PowerReportGeneratorService sut;
    private readonly string _testDirectory;

    public PowerReportGeneratorServiceTests()
    {
        var logger= new Mock<ILogger<IPowerReportGeneratorService>>();
        sut = new PowerReportGeneratorService(logger.Object);
        _testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    }

    [Fact]
    public void ExportCSV_Should_Create_CSV_CreateDirectory_If_NotExist() {
        var aggregatedData = new Dictionary<int, double> { { 1, 100 } };
        var extractTime = new DateTime(2024, 1, 15, 10, 30, 0);

        sut.ExportToCsv(aggregatedData, _testDirectory, extractTime);

        Directory.Exists(_testDirectory).Should().BeTrue();
    }

    [Fact]
    public void ExportToCsv_Should_Create_FileWith_Correct_Name()
    {
        var aggregatedData = new Dictionary<int, double> { { 1, 100 } };
        var extractTime = new DateTime(2024, 1, 15, 10, 30, 0);
        var expectedFileName = "PowerPosition_20240115_1030.csv";
        sut.ExportToCsv(aggregatedData, _testDirectory, extractTime);

        sut.ExportToCsv(aggregatedData, expectedFileName, extractTime);

        File.Exists(Path.Combine(_testDirectory, expectedFileName)).Should().BeTrue();
    }

    [Fact]
    public void ExportToCsv_Should_Write_Header_And_Data_Rows()
    {
        // Arrange
        var aggregatedData = new Dictionary<int, double> { { 1, 100 }, { 2, 200 } };
        var extractTime = new DateTime(2024, 1, 15, 10, 30, 0);

        // Act
        sut.ExportToCsv(aggregatedData, _testDirectory, extractTime);

        // Assert
        var lines = ReadCsvLines(extractTime);
        Assert.Equal("Local Time, Volume", lines[0]);
        Assert.Equal("23:00,100", lines[1]);
        Assert.Equal("00:00,200", lines[2]);
    }

    private string[] ReadCsvLines(DateTime extractTime)
    {
        var fileName = $"PowerPosition_{extractTime:yyyyMMdd}_{extractTime:HHmm}.csv";
        var filePath = Path.Combine(_testDirectory, fileName);
        return File.ReadAllLines(filePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, recursive: true);
    }

}