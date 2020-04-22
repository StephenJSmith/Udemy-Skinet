using System.IO;
using System.Net;
using System.Threading.Tasks;
using API.Errors;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Order = Core.Entities.OrderAggregate;

namespace API.Controllers
{
  public class PaymentsController : BaseApiController
  {
    private readonly IPaymentService _paymentService;
    private readonly string _whSecret;
    private readonly ILogger<IPaymentService> _logger;

    public PaymentsController(
      IPaymentService paymentService,
      ILogger<IPaymentService> logger,
      IConfiguration config)
    {
      _logger = logger;
      _paymentService = paymentService;
      _whSecret = config.GetSection("StripeSettings:WhSecret").Value;
    }

    [Authorize]
    [HttpPost("{basketId}")]
    public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
    {
      var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);
      if (basket == null)
      {
        var badRequestCode = (int)HttpStatusCode.BadRequest;
        var apiResponse = new ApiResponse(badRequestCode, "Problem with your basket");

        return BadRequest(apiResponse);
      }

      return basket;
    }

    [HttpPost("webhook")]
    public async Task<ActionResult> StripeWebhook()
    {
      var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
      var stripeEvent = EventUtility.ConstructEvent(
        json, Request.Headers["Stripe-Signature"], _whSecret);

      PaymentIntent intent;
      Order.Order order;

      switch (stripeEvent.Type)
      {
        case "payment_intent.succeeded":
          intent = (PaymentIntent)stripeEvent.Data.Object;
          _logger.LogInformation("Payment Succeeded: ", intent.Id);
          order = await _paymentService.UpdatePaymentSucceeded(intent.Id);
          _logger.LogInformation("Order updated to payment received: ", order.Id);
          break;

        case "payment_intent.payment_failed":
          intent = (PaymentIntent)stripeEvent.Data.Object;
          _logger.LogInformation("Payment Failed: ", intent.Id);
          order = await _paymentService.UpdatePaymentFailed(intent.Id);
          _logger.LogInformation("Payment Failed: ", order.Id);
          break;
      }

      return new EmptyResult();
    }
  }
}