using FIAP.CloudGames.Identity.API.Data;
using FIAP.CloudGames.Identity.API.Extensions;
using FIAP.CloudGames.Identity.API.Models;
using FIAP.CloudGames.Identity.API.Services;
using FIAP.CloudGames.WebAPI.Core.Identity;
using FIAP.CloudGames.WebAPI.Core.User;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NetDevPack.Security.Jwt.Core.Interfaces;
using System.Security.Claims;
using Xunit;

namespace FIAP.CloudGames.Identity.Tests.Services
{
    public class AuthenticationServiceTests
    {
        private static ApplicationDbContext BuildInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private static Mock<UserManager<IdentityUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<SignInManager<IdentityUser>> MockSignInManager(UserManager<IdentityUser> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            return new Mock<SignInManager<IdentityUser>>(userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
        }

        // AppSettings do seu projeto tem apenas AuthenticationJwksUrl
        private static IOptions<AppSettings> AppSettings() =>
            Options.Create(new AppSettings { AuthenticationJwksUrl = "http://jwks.local/.well-known/jwks.json" });

        private static IOptions<AppTokenSettings> TokenSettings(int refreshHours = 4) =>
            Options.Create(new AppTokenSettings { RefreshTokenExpiration = refreshHours });

        private static IAspNetUser BuildAspNetUserWithHttp(string scheme = "https", string host = "localhost")
        {
            var http = new DefaultHttpContext { Request = { Scheme = scheme, Host = new HostString(host) } };
            var aspMock = new Mock<IAspNetUser>();
            aspMock.Setup(a => a.GetHttpContext()).Returns(http);
            return aspMock.Object;
        }

        private static IJwtService BuildJwksService()
        {
            var key = new SymmetricSecurityKey(new byte[32]);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwks = new Mock<IJwtService>();
            jwks.Setup(j => j.GetCurrentSigningCredentials()).ReturnsAsync(creds);
            return jwks.Object;
        }

        private static (AuthenticationService SUT, ApplicationDbContext Ctx) BuildService(
            string dbName,
            IdentityUser user,
            IEnumerable<Claim> userClaims,
            IEnumerable<string> roles,
            int refreshHours = 4)
        {
            var ctx = BuildInMemoryContext(dbName);

            var umMock = MockUserManager();
            umMock.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            umMock.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync(userClaims is null ? new List<Claim>() : new List<Claim>(userClaims));
            umMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(roles is null ? new List<string>() : new List<string>(roles));

            var smMock = MockSignInManager(umMock.Object);

            var sut = new AuthenticationService(
                smMock.Object,
                umMock.Object,
                AppSettings(),
                TokenSettings(refreshHours),
                ctx,
                BuildJwksService(),
                BuildAspNetUserWithHttp());

            return (sut, ctx);
        }

        [Fact]
        public async Task GerarJwt_Should_Return_UserResponse_With_Tokens_And_Persist_RefreshToken()
        {
            var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = "user@mail.com", UserName = "user@mail.com" };
            var claims = new List<Claim> { new Claim("custom", "123") };
            var roles = new List<string> { "admin", "player" };
            var (sut, ctx) = BuildService($"db-{Guid.NewGuid()}", user, claims, roles, refreshHours: 6);

            ctx.RefreshTokens.Add(new RefreshToken { Username = user.Email, ExpirationDate = DateTime.UtcNow.AddHours(1) });
            await ctx.SaveChangesAsync();

            var response = await sut.GerarJwt(user.Email);

            response.Should().NotBeNull();
            response.AccessToken.Should().NotBeNullOrWhiteSpace();
            response.ExpiresIn.Should().BeGreaterThan(0);
            response.UserToken.Should().NotBeNull();
            response.UserToken.Id.Should().Be(user.Id);
            response.UserToken.Email.Should().Be(user.Email);
            response.UserToken.Claims.Should().Contain(c => c.Type == "custom" && c.Value == "123");
            response.UserToken.Claims.Should().Contain(c => c.Type == "role" && (c.Value == "admin" || c.Value == "player"));

            var tokens = await ctx.RefreshTokens.Where(r => r.Username == user.Email).ToListAsync();
            tokens.Should().HaveCount(1);
            tokens[0].Token.Should().Be(response.RefreshToken);
            tokens[0].ExpirationDate.Should().BeAfter(DateTime.UtcNow.AddHours(5));
        }

        [Fact]
        public async Task ObterRefreshToken_Should_Return_Token_When_Not_Expired()
        {
            var db = $"db-{Guid.NewGuid()}";
            var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = "u@mail.com" };
            var (sut, ctx) = BuildService(db, user, null, null);

            var rt = new RefreshToken { Username = user.Email, ExpirationDate = DateTime.UtcNow.AddHours(2) };
            ctx.RefreshTokens.Add(rt);
            await ctx.SaveChangesAsync();

            var found = await sut.ObterRefreshToken(rt.Token);

            found.Should().NotBeNull();
            found!.Token.Should().Be(rt.Token);
        }

        [Fact]
        public async Task ObterRefreshToken_Should_Return_Null_When_Not_Found()
        {
            var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = "u@mail.com" };
            var (sut, _) = BuildService($"db-{Guid.NewGuid()}", user, null, null);

            var found = await sut.ObterRefreshToken(Guid.NewGuid());

            found.Should().BeNull();
        }

        [Fact]
        public async Task ObterRefreshToken_Should_Return_Null_When_Expired()
        {
            var db = $"db-{Guid.NewGuid()}";
            var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = "u@mail.com" };
            var (sut, ctx) = BuildService(db, user, null, null);

            var expired = new RefreshToken { Username = user.Email, ExpirationDate = DateTime.UtcNow.AddHours(-1) };
            ctx.RefreshTokens.Add(expired);
            await ctx.SaveChangesAsync();

            var found = await sut.ObterRefreshToken(expired.Token);

            found.Should().BeNull();
        }
    }
}