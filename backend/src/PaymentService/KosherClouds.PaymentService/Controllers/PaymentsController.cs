using KosherClouds.PaymentService.DTOs;
using KosherClouds.PaymentService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace KosherClouds.PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
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

        [HttpPost("webhook")]
        [AllowAnonymous]
#pragma warning disable S6932
        public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"].ToString();
#pragma warning restore S6932

            try
            {
                var webhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
                Event stripeEvent;

                if (!string.IsNullOrEmpty(webhookSecret))
                {
                    stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
                }
                else
                {
                    stripeEvent = Event.FromJson(json);
                }

                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        await _paymentService.HandlePaymentSuccessAsync(
                            paymentIntent.Id,
                            cancellationToken
                        );
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing failed");
                return Ok();
            }
        }
    }
}