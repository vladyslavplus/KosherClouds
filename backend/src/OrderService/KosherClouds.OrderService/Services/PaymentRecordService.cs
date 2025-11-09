using KosherClouds.OrderService.Data;
using KosherClouds.OrderService.Entities;
using KosherClouds.OrderService.DTOs.PaymentRecord;
using KosherClouds.OrderService.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.OrderService.Services
{
    public class PaymentRecordService : IPaymentRecordService
    {
        private readonly OrderDbContext _dbContext;
        private readonly IMapper _mapper;

        public PaymentRecordService(OrderDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PaymentRecordResponseDto>> GetPaymentsByOrderIdAsync(
            Guid orderId, 
            CancellationToken cancellationToken = default)
        {
            var payments = await _dbContext.PaymentRecords
                .Where(pr => pr.OrderId == orderId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<PaymentRecordResponseDto>>(payments);
        }


        public async Task<PaymentRecordResponseDto> CreatePaymentRecordAsync(
            PaymentRecordCreateDto paymentDto, 
            Guid orderId, 
            CancellationToken cancellationToken = default)
        {
            var payment = _mapper.Map<PaymentRecord>(paymentDto);
            payment.OrderId = orderId;
            
            await _dbContext.PaymentRecords.AddAsync(payment, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken); 

            return _mapper.Map<PaymentRecordResponseDto>(payment);
        }
        
    }
}