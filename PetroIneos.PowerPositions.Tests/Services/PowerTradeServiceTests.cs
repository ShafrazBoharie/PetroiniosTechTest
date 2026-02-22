using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PetroIneos.PowerPositions.Services;
using Services;

namespace PetroIneos.PowerPositions.Tests.Services;

public class PowerTradeServiceTests
{
    private readonly Mock<IPowerService> _powerService;
    private readonly Mock<ILogger<PowerTradeService>> _logger;
    private readonly PowerTradeService _sut;

    public PowerTradeServiceTests()
    {
        _powerService = new Mock<IPowerService>();
        _logger = new Mock<ILogger<PowerTradeService>>();
        _sut = new PowerTradeService(_powerService.Object, _logger.Object);
    }

    [Fact]
    public async Task GetAggregateTradeService_ShouldCall_GetTradesAsync()
    {
        var dateTimeNow = DateTime.Now;
        _powerService.Setup(p => p.GetTradesAsync(dateTimeNow))
            .ReturnsAsync(new List<PowerTrade>());

        await _sut.GetAggregatedTrades(dateTimeNow);
        _powerService.Verify(p => p.GetTradesAsync(dateTimeNow), Times.Once);

    }

    [Fact]
    public async Task GetAggregateCall_with_defaultDateTime_Should_Throw_ArgumentException()
    {
        var defaultDateTime = default(DateTime);

        var act = async () => await _sut.GetAggregatedTrades(defaultDateTime);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Date must be a valid date.*");
        _powerService.Verify(p => p.GetTradesAsync(defaultDateTime), Times.Never);

    }

}