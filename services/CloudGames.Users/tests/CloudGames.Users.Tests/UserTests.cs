using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

public class UserTests
{
    [Fact]
    public async Task Register_PersistsUser()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase("users-tests-register")
            .Options;
        await using var db = new UsersDbContext(options);
        var token = new Mock<ITokenService>();
        var queue = new Mock<Azure.Storage.Queues.QueueClient>(MockBehavior.Loose, new object[] { "UseDevelopmentStorage=true", "users-events" });
        var service = new UserService(db, token.Object, queue.Object);

        var dto = new CreateUserDto("Alice","alice@mail.com","P@ssw0rd!");
        var created = await service.RegisterAsync(dto);

        created.Email.Should().Be("alice@mail.com");
        (await db.Users.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Login_ReturnsJwt()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase("users-tests-login")
            .Options;
        await using var db = new UsersDbContext(options);
        var user = new User { Id = Guid.NewGuid(), Name = "Test", Email = "t@t.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!"), IsActive = true };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = new Mock<ITokenService>();
        token.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("dummy.jwt.token");
        var queue = new Mock<Azure.Storage.Queues.QueueClient>(MockBehavior.Loose, new object[] { "UseDevelopmentStorage=true", "users-events" });

        var service = new UserService(db, token.Object, queue.Object);
        var resp = await service.LoginAsync(new LoginDto("t@t.com","P@ssw0rd!"));

        resp.Token.Should().Be("dummy.jwt.token");
    }
}
