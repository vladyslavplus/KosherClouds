namespace KosherClouds.OrderService.Services.Interfaces;
using KosherClouds.OrderService.DTOs.PaymentRecord;
public interface IPaymentRecordService
{
    Task<IEnumerable<PaymentRecordResponseDto>> GetPaymentsByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<PaymentRecordResponseDto> CreatePaymentRecordAsync(
        PaymentRecordCreateDto paymentDto,
        Guid orderId,
        CancellationToken cancellationToken = default);
    

}