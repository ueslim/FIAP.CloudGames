using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

public class GamesTests
{
    [Fact]
    public async Task CreateGame_Persists()
    {
        var options = new DbContextOptionsBuilder<GamesDbContext>()
            .UseInMemoryDatabase("games-tests-create")
            .Options;
        await using var db = new GamesDbContext(options);
        var queue = new Mock<Azure.Storage.Queues.QueueClient>(MockBehavior.Loose, new object[] { "UseDevelopmentStorage=true", "games-events" });
        var search = new NoopGamesSearch(db);
        var controller = new GamesController(db, search, queue.Object);

        var dto = new CreateGameDto("Title","Desc","Dev","Pub",DateTime.UtcNow,"Genre",10,"url",new[]{"tag"});
        var result = await controller.Create(dto) as Microsoft.AspNetCore.Mvc.CreatedAtActionResult;

        result.Should().NotBeNull();
        (await db.Games.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Search_UsesQuery()
    {
        var options = new DbContextOptionsBuilder<GamesDbContext>()
            .UseInMemoryDatabase("games-tests-search")
            .Options;
        await using var db = new GamesDbContext(options);
        db.Games.Add(new Game { Id = Guid.NewGuid(), Title = "Halo", Description = "Shooter", Genre = "FPS", Price = 20 });
        db.Games.Add(new Game { Id = Guid.NewGuid(), Title = "Forza", Description = "Racing", Genre = "Racing", Price = 30 });
        await db.SaveChangesAsync();
        var queue = new Mock<Azure.Storage.Queues.QueueClient>(MockBehavior.Loose, new object[] { "UseDevelopmentStorage=true", "games-events" });
        var search = new NoopGamesSearch(db);
        var controller = new GamesController(db, search, queue.Object);

        var ok = await controller.Search("Hal") as Microsoft.AspNetCore.Mvc.OkObjectResult;
        var list = ok!.Value as System.Collections.IEnumerable;
        list.Should().NotBeNull();
    }
}
