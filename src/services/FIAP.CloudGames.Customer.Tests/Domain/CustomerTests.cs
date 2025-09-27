using System;
using FIAP.CloudGames.Customer.API.Models;
using FluentAssertions;
using Xunit;

namespace FIAP.CloudGames.Customer.Tests.Domain
{
    public class CustomerTests
    {
        private const string ValidCpf = "52998224725";

        [Fact]
        public void Constructor_Should_Set_Properties_And_Defaults()
        {
            var id = Guid.NewGuid();
            var c = new FIAP.CloudGames.Customer.API.Models.Customer(id, "John Doe", "john@doe.com", ValidCpf);

            c.Id.Should().Be(id);
            c.Name.Should().Be("John Doe");
            c.Email.Should().NotBeNull();
            c.Cpf.Number.Should().Be(ValidCpf);
            c.Excluded.Should().BeFalse();
            c.Address.Should().BeNull();
        }

        [Fact]
        public void ChangeEmail_Should_Update_Email_Value_Object()
        {
            var c = new FIAP.CloudGames.Customer.API.Models.Customer(Guid.NewGuid(), "John", "old@mail.com", ValidCpf);
            var before = c.Email;

            c.ChangeEmail("new@mail.com");

            c.Email.Should().NotBeNull();
            c.Email.Should().NotBeSameAs(before);
        }

        [Fact]
        public void AssignAddress_Should_Set_Address()
        {
            var custId = Guid.NewGuid();
            var c = new FIAP.CloudGames.Customer.API.Models.Customer(custId, "John", "john@mail.com", ValidCpf);

            var a = new Address("Main St", "123", "Apt 1", "Downtown", "12345-678", "City", "ST", custId);
            c.AssignAddress(a);

            c.Address.Should().NotBeNull();
            c.Address.Street.Should().Be("Main St");
            c.Address.Number.Should().Be("123");
            c.Address.CustomerId.Should().Be(custId);
        }
    }
}
