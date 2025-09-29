using FIAP.CloudGames.Core.Messages;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using FIAP.CloudGames.Order.API.Application.DTO;
using FIAP.CloudGames.Order.API.Application.Events;
using FIAP.CloudGames.Order.Domain.Order;
using FIAP.CloudGames.Order.Domain.Voucher;
using FIAP.CloudGames.Order.Domain.Voucher.Specs;
using FluentValidation.Results;
using MediatR;

namespace FIAP.CloudGames.Order.API.Application.Commands
{
    public class OrderCommandHandler : CommandHandler, IRequestHandler<AddOrderCommand, ValidationResult>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IMessageBus _bus;

        public OrderCommandHandler(IVoucherRepository voucherRepository,
                                    IOrderRepository orderRepository,
                                    IMessageBus bus)
        {
            _voucherRepository = voucherRepository;
            _orderRepository = orderRepository;
            _bus = bus;
        }

        public async Task<ValidationResult> Handle(AddOrderCommand message, CancellationToken cancellationToken)
        {
            // Validação do comando
            if (!message.IsValid()) return message.ValidationResult;

            // Mapear Pedido
            var order = MapOrder(message);

            // Aplicar voucher se houver
            if (!await ApplyVoucher(message, order)) return ValidationResult;

            // Validar pedido
            if (!ValidateOrder(order)) return ValidationResult;

            // Processar pagamento
            if (!await ProcessPayment(order, message)) return ValidationResult;

            // Se pagamento tudo ok!
             order.AuthorizeOrder();

            // Adicionar Evento
            order.AddEvent(new OrderFinishedEvent(order.Id, order.CustomerId));

            // Adicionar Pedido Repositorio
            _orderRepository.Add(order);

            // Persistir dados de pedido e voucher
            return await PersistData(_orderRepository.UnitOfWork);
        }

        private Domain.Order.Order MapOrder(AddOrderCommand message)
        {
            var address = new Address
            {
                Street = message.Address.Street,
                Number = message.Address.Number,
                AdditionalInfo = message.Address.AdditionalInfo,
                Neighborhood = message.Address.Neighborhood,
                PostalCode = message.Address.PostalCode,
                City = message.Address.City,
                State = message.Address.State
            };

            var pedido = new Domain.Order.Order(message.CustomerId, message.TotalValue, message.OrderItems.Select(OrderItemDTO.ToOrderItem).ToList(),
                message.VoucherUsed, message.Discount);

            pedido.AssignAddress(address);
            return pedido;
        }

        private async Task<bool> ApplyVoucher(AddOrderCommand message, Domain.Order.Order order)
        {
            if (!message.VoucherUsed) return true;

            var voucher = await _voucherRepository.GetVoucherByCode(message.VoucherCode);
            if (voucher == null)
            {
                AddError("O voucher informado não existe!");
                return false;
            }

            var voucherValidation = new VoucherValidation().Validate(voucher);
            if (!voucherValidation.IsValid)
            {
                voucherValidation.Errors.ToList().ForEach(m => AddError(m.ErrorMessage));
                return false;
            }

            order.AssignVoucher(voucher);
            voucher.DebitQuantity();

            _voucherRepository.Update(voucher);

            return true;
        }

        private bool ValidateOrder(Domain.Order.Order order)
        {
            var orderOriginalValue = order.TotalValue;
            var orderDiscount = order.Discount;

            order.CalculateOrderValue();

            if (order.TotalValue != orderOriginalValue)
            {
                AddError("O valor total do pedido não confere com o cálculo do pedido");
                return false;
            }

            if (order.Discount != orderDiscount)
            {
                AddError("O valor total não confere com o cálculo do pedido");
                return false;
            }

            return true;
        }

        public async Task<bool> ProcessPayment(Domain.Order.Order order, AddOrderCommand message)
        {
            var orderStarted = new OrderProcessingStartedIntegrationEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                Value = order.TotalValue,
                PaymentType = 1,
                CardName = message.CardName,
                CardNumber = message.CardNumber,
                CardExpirationDate = message.CardExpirationDate,
                CvvCard = message.CvvCard
            };

            var result = await _bus.RequestAsync<OrderProcessingStartedIntegrationEvent, ResponseMessage>(orderStarted);

            if (result.ValidationResult.IsValid) return true;

            foreach (var erro in result.ValidationResult.Errors)
            {
                AddError(erro.ErrorMessage);
            }

            return false;
        }
    }
}