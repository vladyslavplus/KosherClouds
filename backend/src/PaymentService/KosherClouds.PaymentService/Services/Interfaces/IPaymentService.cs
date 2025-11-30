using KosherClouds.PaymentService.DTOs;

namespace KosherClouds.PaymentService.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CreatePaymentAsync(
            PaymentRequestDto request,
            Guid userId,
            CancellationToken cancellationToken);

        Task HandlePaymentSuccessAsync(
            string transactionId,
            CancellationToken cancellationToken);
    }
}
