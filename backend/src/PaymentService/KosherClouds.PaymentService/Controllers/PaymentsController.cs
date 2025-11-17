using KosherClouds.PaymentService.DTOs;
using KosherClouds.PaymentService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KosherClouds.PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(
            [FromBody] PaymentRequestDto request,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _paymentService
                .CreatePaymentAsync(request, userId.Value, cancellationToken);

            return Ok(result);
        }
    }
}