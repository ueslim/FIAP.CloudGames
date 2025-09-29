using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FIAP.CloudGames.Core.Data;
using FIAP.CloudGames.Core.Messages;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using FIAP.CloudGames.Order.API.Application.Commands;
using FIAP.CloudGames.Order.API.Application.DTO;
using FIAP.CloudGames.Order.Domain.Order;
using FIAP.CloudGames.Order.Domain.Voucher;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace FIAP.CloudGames.Order.Tests.Application
{
    public class OrderCommandHandlerTests
    {
        private static AddOrderCommand ValidCommand(bool useVoucher = false, string voucher = "OFF")
        {
            return new AddOrderCommand
            {
                CustomerId = Guid.NewGuid(),
                TotalValue = 30m,
                OrderItems = new List<OrderItemDTO>{
                    new OrderItemDTO{ ProductId=Guid.NewGuid(), Name="A", Quantity=1, Value=10m, Image="i" },
                    new OrderItemDTO{ ProductId=Guid.NewGuid(), Name="B", Quantity=2, Value=10m, Image="i" }
                },
                VoucherUsed = useVoucher,
                VoucherCode = useVoucher ? voucher : null,
                Discount = useVoucher ? 10m : 0m,
                Address = new AddressDTO { Street = "s", Number = "1", City = "c", State = "st", PostalCode = "p" },
                CardName = "John Doe",
                CardNumber = "4111111111111111",
                CardExpirationDate = "12/29",
                CvvCard = "123"
            };
        }

        private static Voucher NewValidVoucherPercent10()
        {
            var v = (Voucher)Activator.CreateInstance(typeof(Voucher), nonPublic: true)!;
            typeof(Voucher).GetProperty("Id")!.SetValue(v, Guid.NewGuid());
            typeof(Voucher).GetProperty("Code")!.SetValue(v, "OFF");
            typeof(Voucher).GetProperty("Percentage")!.SetValue(v, 10m);
            typeof(Voucher).GetProperty("Quantity")!.SetValue(v, 5);
            typeof(Voucher).GetProperty("DiscountType")!.SetValue(v, VoucherDiscountType.Percent);
            typeof(Voucher).GetProperty("CreatedAt")!.SetValue(v, DateTime.UtcNow);
            typeof(Voucher).GetProperty("ExpirationDate")!.SetValue(v, DateTime.UtcNow.AddDays(1));
            typeof(Voucher).GetProperty("Active")!.SetValue(v, true);
            typeof(Voucher).GetProperty("Used")!.SetValue(v, false);
            return v;
        }

        [Fact]
        public async Task Handle_Should_Add_Order_And_Commit_On_Success()
        {
            var cmd = ValidCommand();
            var voucherRepo = new Mock<IVoucherRepository>(MockBehavior.Strict);
            voucherRepo.Setup(r => r.Dispose());

            var orderRepo = new Mock<IOrderRepository>(MockBehavior.Strict);
            orderRepo.Setup(r => r.Dispose());
            orderRepo.Setup(r => r.Add(It.IsAny<Order.Domain.Order.Order>()));
            var uow = new Mock<IUnitOfWork>(); uow.Setup(u => u.Commit()).ReturnsAsync(true);
            orderRepo.SetupGet(r => r.UnitOfWork).Returns(uow.Object);

            var bus = new Mock<IMessageBus>(MockBehavior.Strict);
            bus.Setup(b => b.RequestAsync<OrderProcessingStartedIntegrationEvent, ResponseMessage>(It.IsAny<OrderProcessingStartedIntegrationEvent>()))
               .ReturnsAsync(new ResponseMessage(new ValidationResult()));

            var sut = new OrderCommandHandler(voucherRepo.Object, orderRepo.Object, bus.Object);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.IsValid.Should().BeTrue();
            orderRepo.Verify(r => r.Add(It.IsAny<Order.Domain.Order.Order>()), Times.Once);
            uow.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_Error_When_Payment_Refused()
        {
            var cmd = ValidCommand();
            var voucherRepo = new Mock<IVoucherRepository>(MockBehavior.Strict);
            voucherRepo.Setup(r => r.Dispose());

            var orderRepo = new Mock<IOrderRepository>(MockBehavior.Strict);
            orderRepo.Setup(r => r.Dispose());

            var bus = new Mock<IMessageBus>(MockBehavior.Strict);
            var vr = new ValidationResult(new[] { new ValidationFailure("payment", "refused") });
            bus.Setup(b => b.RequestAsync<OrderProcessingStartedIntegrationEvent, ResponseMessage>(It.IsAny<OrderProcessingStartedIntegrationEvent>()))
               .ReturnsAsync(new ResponseMessage(vr));

            var sut = new OrderCommandHandler(voucherRepo.Object, orderRepo.Object, bus.Object);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "refused");
            orderRepo.Verify(r => r.Add(It.IsAny<Order.Domain.Order.Order>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_Apply_Voucher_And_Update_Repository()
        {
            var cmd = ValidCommand(useVoucher: true, voucher: "OFF");

            // itens = 30; voucher 10% => desconto 3, total final 27
            cmd.Discount = 3m;
            cmd.TotalValue = 27m;

            var voucher = NewValidVoucherPercent10();

            var voucherRepo = new Mock<IVoucherRepository>(MockBehavior.Strict);
            voucherRepo.Setup(r => r.Dispose());
            voucherRepo.Setup(r => r.GetVoucherByCode("OFF")).ReturnsAsync(voucher);
            voucherRepo.Setup(r => r.Update(voucher));

            var orderRepo = new Mock<IOrderRepository>(MockBehavior.Strict);
            orderRepo.Setup(r => r.Dispose());
            orderRepo.Setup(r => r.Add(It.IsAny<FIAP.CloudGames.Order.Domain.Order.Order>()));
            var uow = new Mock<IUnitOfWork>(); uow.Setup(u => u.Commit()).ReturnsAsync(true);
            orderRepo.SetupGet(r => r.UnitOfWork).Returns(uow.Object);

            var bus = new Mock<IMessageBus>(MockBehavior.Strict);
            bus.Setup(b => b.RequestAsync<OrderProcessingStartedIntegrationEvent, ResponseMessage>(
                        It.IsAny<OrderProcessingStartedIntegrationEvent>()))
               .ReturnsAsync(new ResponseMessage(new FluentValidation.Results.ValidationResult()));

            var sut = new OrderCommandHandler(voucherRepo.Object, orderRepo.Object, bus.Object);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.IsValid.Should().BeTrue();
            voucherRepo.Verify(r => r.Update(It.Is<Voucher>(v => v.Code == "OFF")), Times.Once);
            uow.Verify(u => u.Commit(), Times.Once);
        }



        [Fact]
        public async Task Handle_Should_Return_Error_When_Voucher_Not_Found()
        {
            var cmd = ValidCommand(useVoucher: true, voucher: "OFF");
            var voucherRepo = new Mock<IVoucherRepository>(MockBehavior.Strict);
            voucherRepo.Setup(r => r.Dispose());
            voucherRepo.Setup(r => r.GetVoucherByCode("OFF")).ReturnsAsync((Voucher)null);

            var orderRepo = new Mock<IOrderRepository>(MockBehavior.Strict);
            orderRepo.Setup(r => r.Dispose());

            var bus = new Mock<IMessageBus>(MockBehavior.Strict);

            var sut = new OrderCommandHandler(voucherRepo.Object, orderRepo.Object, bus.Object);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.IsValid.Should().BeFalse();
            result.Errors.Any(e => e.ErrorMessage.Contains("voucher")).Should().BeTrue();
        }
    }
}
