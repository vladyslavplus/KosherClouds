namespace KosherClouds.OrderService.Controllers;

using Microsoft.AspNetCore.Mvc;
using KosherClouds.OrderService.Services.Interfaces;
using KosherClouds.OrderService.DTOs.PaymentRecord;
using Microsoft.AspNetCore.Authorization;
using System.Threading;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin, Manager")]
public class PaymentRecordsController : ControllerBase
{
    private readonly IPaymentRecordService _paymentRecordService;

    public PaymentRecordsController(IPaymentRecordService paymentRecordService)
    {
        _paymentRecordService = paymentRecordService;
    }

    [HttpGet("by-order/{orderId:guid}")]
    public async Task<IActionResult> GetPaymentsByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var payments = await _paymentRecordService.GetPaymentsByOrderIdAsync(orderId, cancellationToken);
        return Ok(payments);
    }

    [HttpPost("for-order/{orderId:guid}")]
    public async Task<IActionResult> CreatePaymentRecord(
        Guid orderId,
        [FromBody] PaymentRecordCreateDto request, 
        CancellationToken cancellationToken)
    {
        var createdPayment = await _paymentRecordService.CreatePaymentRecordAsync(request, orderId, cancellationToken);

        return CreatedAtAction(
            nameof(GetPaymentsByOrderId), 
            new { orderId = orderId },
            createdPayment
        );
    }
}