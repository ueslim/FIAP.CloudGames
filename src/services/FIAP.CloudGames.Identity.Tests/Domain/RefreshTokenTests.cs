using System;
using FIAP.CloudGames.Identity.API.Models;
using FluentAssertions;
using Xunit;

namespace FIAP.CloudGames.Identity.Tests.Domain
{
    public class RefreshTokenTests
    {
        [Fact]
        public void Ctor_Should_Generate_Id_And_Token()
        {
            var rt = new RefreshToken();
            rt.Id.Should().NotBe(Guid.Empty);
            rt.Token.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void Properties_Should_Be_Settable()
        {
            var rt = new RefreshToken
            {
                Username = "user@mail.com",
                ExpirationDate = DateTime.UtcNow.AddHours(2)
            };

            rt.Username.Should().Be("user@mail.com");
            rt.ExpirationDate.Should().BeAfter(DateTime.UtcNow.AddMinutes(30));
        }
    }
}
