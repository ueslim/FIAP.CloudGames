using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class FunctionsTests
{
    [Fact]
    public async Task GamesIndexUpdater_HandlesGameCreated()
    {
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
        var func = new GamesIndexUpdater(loggerFactory.Object);
        var msg = JsonSerializer.Serialize(new { Type = "GameCreated", Data = new { Id = Guid.NewGuid(), Title = "T" } });
        Func<Task> act = async () => await func.Run(msg);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PaymentsConfirmationProcessor_HandlesGamePurchased()
    {
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
        var func = new PaymentsConfirmationProcessor(loggerFactory.Object);
        Environment.SetEnvironmentVariable("PaymentsDb", "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CloudGames.Payments;Integrated Security=True;Trust Server Certificate=True");
        var msg = JsonSerializer.Serialize(new { Type = "GamePurchased", PaymentId = Guid.NewGuid(), UserId = Guid.NewGuid(), GameId = Guid.NewGuid(), Amount = 1.0m });
        Func<Task> act = async () => await func.Run(msg);
        await act.Should().NotThrowAsync();
    }
}
