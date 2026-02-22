using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PetroIneos.PowerPositions.Services;
using Services;

namespace PetroIneos.PowerPositions.Tests.Services;

public class PowerTradeAggregatorTests
{
    private PowerTradeAggregator sut;
    private readonly Mock<ILogger<IPowerTradeAggregator>> _logger;
    private readonly Mock<IPowerTradeService> _tradeService;


    public PowerTradeAggregatorTests()
    {
        _logger = new Mock<ILogger<IPowerTradeAggregator>>();
        _tradeService = new Mock<IPowerTradeService>();
        sut= new PowerTradeAggregator(_tradeService.Object, _logger.Object);

    }

    [Fact]
    public async Task GetAggregatedTrades_Should_ThrowExceptions_For_DefaultDateTime()
    {
        var defaultDateTime = default(DateTime);

        var act = async() => await sut.GetAggregatedTrades(defaultDateTime);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid date.Date must not be default.*");

        _tradeService.Verify(service => service.GetAggregatedTrades(defaultDateTime), Times.Never);
    }

    [Fact]
    public async Task GetAggregateTrades_Should_Call_TradeService()
    {
        var dateTime = DateTime.Now;
        var expectedOutPutFromTradeService = ExpectedOutputFromTradeService(dateTime);
        var expectedOutput = ExpectedOutput();

        _tradeService.Setup(service => service.GetAggregatedTrades(dateTime))
            .ReturnsAsync(expectedOutPutFromTradeService);

        var result = await sut.GetAggregatedTrades(dateTime);

        _tradeService.Verify(service => service.GetAggregatedTrades(dateTime), Times.Once);

        result.Should().BeEquivalentTo(expectedOutput);

    }

    [Fact]
    public async Task GetAggregateTrades_ShouldThrowExceptions_For_PowerTradeService_ThrowsAnException()
    {
        var defaultDateTime = DateTime.Now;
        _tradeService.Setup(service => service.GetAggregatedTrades(defaultDateTime)).Throws<Exception>();

        var act = async() => await sut.GetAggregatedTrades(defaultDateTime);

        await act.Should().ThrowAsync<Exception>();
    }

    private Dictionary<int, double> ExpectedOutput()
    {
        var result = new Dictionary<int, double>();

        foreach (var i in Enumerable.Range(1, 24))
        {
            result[i] = i * 100;
        }

        return result;

    }

    private List<PowerTrade> ExpectedOutputFromTradeService(DateTime date)
    {
        var trade = PowerTrade.Create(date, 24);

        for (int i = 0; i < trade.Periods.Length; i++)
        {
            trade.Periods[i].Volume = (i + 1) * 100;
        }

        return new List<PowerTrade> { trade };
    }
}