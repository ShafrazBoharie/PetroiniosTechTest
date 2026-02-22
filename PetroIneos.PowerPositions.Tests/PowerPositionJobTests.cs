using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PetroIneos.PowerPositions.Config;
using PetroIneos.PowerPositions.Services;
using Quartz;
using Xunit;

namespace PetroIneos.PowerPositions.Tests;

public class PowerPositionsJobTests
{
    private readonly Mock<IPowerTradeAggregator> _powerTradeAggregator;
    private readonly Mock<IPowerReportGeneratorService> _powerReportGeneratorService;
    private readonly Mock<IOptions<PowerPositionsSettings>> _settings;
    private readonly Mock<ILogger<PowerPositionsJob>> _logger;
    private readonly Mock<IJobExecutionContext> _jobExecutionContext;
    private readonly PowerPositionsJob _sut;
    private readonly PowerPositionsSettings _powerPositionsSettings;

    public PowerPositionsJobTests()
    {
        _powerTradeAggregator = new Mock<IPowerTradeAggregator>();
        _powerReportGeneratorService = new Mock<IPowerReportGeneratorService>();
        _logger = new Mock<ILogger<PowerPositionsJob>>();
        _jobExecutionContext = new Mock<IJobExecutionContext>();

        _powerPositionsSettings = new PowerPositionsSettings { OutputPath = "/test/output" };
        _settings = new Mock<IOptions<PowerPositionsSettings>>();
        _settings.Setup(s => s.Value).Returns(_powerPositionsSettings);

        _sut = new PowerPositionsJob(
            _powerTradeAggregator.Object,
            _powerReportGeneratorService.Object,
            _settings.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Execute_Should_Call_GetAggregatedTrades_With_Todays_Date()
    {
        // Arrange
        var aggregatedData = new Dictionary<int, double> { { 1, 100 } };
        _powerTradeAggregator
            .Setup(s => s.GetAggregatedTrades(It.IsAny<DateTime>()))
            .ReturnsAsync(aggregatedData);

        // Act
        await _sut.Execute(_jobExecutionContext.Object);

        // Assert
        _powerTradeAggregator.Verify(
            s => s.GetAggregatedTrades(It.Is<DateTime>(d => d.Date == DateTime.UtcNow.Date)),
            Times.Once);
    }

    [Fact]
    public async Task Execute_Should_Call_ExportToCsv_With_Correct_OutputPath()
    {
        // Arrange
        var aggregatedData = new Dictionary<int, double> { { 1, 100 } };
        _powerTradeAggregator
            .Setup(s => s.GetAggregatedTrades(It.IsAny<DateTime>()))
            .ReturnsAsync(aggregatedData);

        // Act
        await _sut.Execute(_jobExecutionContext.Object);

        // Assert
        _powerReportGeneratorService.Verify(
            s => s.ExportToCsv(
                aggregatedData,
                _powerPositionsSettings.OutputPath,
                It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_Should_Call_ExportToCsv_With_Aggregated_Data()
    {
        // Arrange
        var aggregatedData = new Dictionary<int, double> { { 1, 100 }, { 2, 200 } };
        _powerTradeAggregator
            .Setup(s => s.GetAggregatedTrades(It.IsAny<DateTime>()))
            .ReturnsAsync(aggregatedData);

        Dictionary<int, double>? capturedData = null;
        _powerReportGeneratorService
            .Setup(s => s.ExportToCsv(It.IsAny<Dictionary<int, double>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Callback<Dictionary<int, double>, string, DateTime>((data, _, _) => capturedData = data);

        // Act
        await _sut.Execute(_jobExecutionContext.Object);

        // Assert
        capturedData.Should().NotBeNull();
        capturedData.Should().BeEquivalentTo(aggregatedData);
    }

    [Fact]
    public async Task Execute_Should_Throw_JobExecutionException_When_GetAggregatedTrades_Throws()
    {
        // Arrange
        _powerTradeAggregator
            .Setup(s => s.GetAggregatedTrades(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Trade service failure"));

        // Act
        var act = async () => await _sut.Execute(_jobExecutionContext.Object);

        // Assert
        await act.Should().ThrowAsync<JobExecutionException>()
            .WithInnerException<JobExecutionException, Exception>()
            .WithMessage("Trade service failure");
    }

    [Fact]
    public async Task Execute_Should_Throw_JobExecutionException_When_ExportToCsv_Throws()
    {
        // Arrange
        var aggregatedData = new Dictionary<int, double> { { 1, 100 } };
        _powerTradeAggregator
            .Setup(s => s.GetAggregatedTrades(It.IsAny<DateTime>()))
            .ReturnsAsync(aggregatedData);

        _powerReportGeneratorService
            .Setup(s => s.ExportToCsv(It.IsAny<Dictionary<int, double>>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Throws(new InvalidOperationException("Export failure"));

        // Act
        var act = async () => await _sut.Execute(_jobExecutionContext.Object);

        // Assert
        await act.Should().ThrowAsync<JobExecutionException>()
            .WithInnerException<JobExecutionException, InvalidOperationException>()
            .WithMessage("Export failure");
    }

    [Fact]
    public async Task Execute_Should_Not_Call_ExportToCsv_When_GetAggregatedTrades_Throws()
    {
        // Arrange
        _powerTradeAggregator
            .Setup(s => s.GetAggregatedTrades(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Trade service failure"));

        // Act
        try { await _sut.Execute(_jobExecutionContext.Object); } catch { }

        // Assert
        _powerReportGeneratorService.Verify(
            s => s.ExportToCsv(It.IsAny<Dictionary<int, double>>(), It.IsAny<string>(), It.IsAny<DateTime>()),
            Times.Never);
    }
}