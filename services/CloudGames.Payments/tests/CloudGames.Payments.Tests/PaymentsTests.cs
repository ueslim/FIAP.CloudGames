using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

public class PaymentsTests
{
    [Fact]
    public async Task Initiate_EnqueuesEvent()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase("payments-tests-init")
            .Options;
        await using var db = new PaymentsDbContext(options);
        var queue = new Mock<Azure.Storage.Queues.QueueClient>(MockBehavior.Loose, new object[] { "UseDevelopmentStorage=true", "payments-events" });
        var svc = new PaymentService(db, queue.Object);
        var userId = Guid.NewGuid();
        var dto = new InitiatePaymentDto(Guid.NewGuid(), 19.9m);

        var resp = await svc.InitiateAsync(userId, dto);

        resp.Status.Should().Be("Pending");
        (await db.PaymentEvents.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Status_ReturnsCurrentState()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase("payments-tests-status")
            .Options;
        await using var db = new PaymentsDbContext(options);
        var p = new Payment { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), GameId = Guid.NewGuid(), Amount = 5m, Status = PaymentStatus.Pending };
        db.Payments.Add(p);
        await db.SaveChangesAsync();
        var svc = new PaymentService(db, new Mock<Azure.Storage.Queues.QueueClient>(MockBehavior.Loose, new object[] { "UseDevelopmentStorage=true", "payments-events" }).Object);

        var resp = await svc.GetStatusAsync(p.Id);

        resp.PaymentId.Should().Be(p.Id);
        resp.Status.Should().Be("Pending");
    }
}
