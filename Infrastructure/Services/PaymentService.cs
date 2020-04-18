using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.Extensions.Configuration;
using Stripe;
using Order = Core.Entities.OrderAggregate.Order;
using Product = Core.Entities.Product;

namespace Infrastructure.Services
{
  public class PaymentService : IPaymentService
  {
    private readonly IBasketRepository _basketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public PaymentService(
        IBasketRepository basketRepository,
        IUnitOfWork unitOfWork,
        IConfiguration config)
    {
      _basketRepository = basketRepository;
      _unitOfWork = unitOfWork;
      _config = config;
    }
    public async Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId)
    {
      StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];
      var basket = await _basketRepository.GetBasketAsync(basketId);
      if (basket == null) { return null; }

      var shippingPrice = 0m;

      if (basket.DeliveryMethodId.HasValue)
      {
        var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>()
          .GetByIdAsync((int)basket.DeliveryMethodId);
        shippingPrice = deliveryMethod.Price;
      }

      foreach (var item in basket.Items)
      {
        var productItem = await _unitOfWork
          .Repository<Product>()
          .GetByIdAsync(item.Id);

        if (item.Price != productItem.Price)
        {
          item.Price = productItem.Price;
        }
      }

      var service = new PaymentIntentService();
      PaymentIntent intent;
      if (string.IsNullOrEmpty(basket.PaymentIntentId))
      {
        var options = new PaymentIntentCreateOptions
        {
          Amount = (long)basket.Items.Sum(i => i.Quantity * (i.Price * 100))
            + (long)shippingPrice * 100,
          Currency = "usd",
          PaymentMethodTypes = new List<string> { "card" }
        };

        intent = await service.CreateAsync(options);
        basket.PaymentIntentId = intent.Id;
        basket.ClientSecret = intent.ClientSecret;
      }
      else
      {
        var options = new PaymentIntentUpdateOptions
        {
          Amount = (long)basket.Items.Sum(i => i.Quantity * (i.Price * 100))
            + (long)shippingPrice * 100
        };

        await service.UpdateAsync(basket.PaymentIntentId, options);
      }

      await _basketRepository.UpdateBasketAsync(basket);

      return basket;
    }

    public async Task<CustomerBasket> xCreateOrUpdatePaymentIntent(string basketId)
    {
      StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];
      var basket = await _basketRepository.GetBasketAsync(basketId);
      if (basket == null) { return null; }

      var shippingPrice = await GetShippingPrice(basket);
      await VerifyBasketItemPrices(basket);
      await UpdateBasketWithPaymentIntent(basket, shippingPrice);

      return basket;
    }

    private async Task UpdateBasketWithPaymentIntent(CustomerBasket basket, decimal shippingPrice)
    {
      if (string.IsNullOrEmpty(basket.PaymentIntentId))
      {
        await ProcessNewPaymentIntent(basket, shippingPrice);
      }
      else
      {
        await ProcessExistingPaymentIntent(basket, shippingPrice);
      }

      await _basketRepository.UpdateBasketAsync(basket);
    }


    private async Task ProcessNewPaymentIntent(CustomerBasket basket, decimal shippingPrice)
    {
      var service = new PaymentIntentService();
      PaymentIntent intent;
      var options = new PaymentIntentCreateOptions
      {
        Amount = GetLongBasketAmount(basket, shippingPrice),
        Currency = "usd",
        PaymentMethodTypes = new List<string> { "card" }
      };

      intent = await service.CreateAsync(options);
      basket.PaymentIntentId = intent.Id;
      basket.ClientSecret = intent.ClientSecret;
    }

    private async Task ProcessExistingPaymentIntent(CustomerBasket basket, decimal shippingPrice)
    {
      var service = new PaymentIntentService();
      var options = new PaymentIntentUpdateOptions
      {
        Amount = GetLongBasketAmount(basket, shippingPrice)
      };

      await service.UpdateAsync(basket.PaymentIntentId, options);
    }
    private static long GetLongBasketAmount(CustomerBasket basket, decimal shippingPrice)
    {
      return (long)basket.Items
                      .Sum(i => i.Quantity * (i.Price * 100))
                      + (long)shippingPrice * 100;
    }
    private async Task<decimal> GetShippingPrice(CustomerBasket basket)
    {

      if (!basket.DeliveryMethodId.HasValue)
      {
        return 0m;
      }

      var deliveryMethod = await _unitOfWork
        .Repository<DeliveryMethod>()
        .GetByIdAsync((int)basket.DeliveryMethodId);

      return deliveryMethod.Price;
    }

    private async Task VerifyBasketItemPrices(CustomerBasket basket)
    {
      foreach (var item in basket.Items)
      {
        var productionItem = await _unitOfWork
            .Repository<Product>()
            .GetByIdAsync(item.Id);
        if (item.Price != productionItem.Price)
        {
          item.Price = productionItem.Price;
        }
      }
    }

    public async Task<Order> UpdatePaymentSucceeded(string paymentIntentId)
    {
      var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
      var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
      if (order == null) { return null; }

      order.Status = OrderStatus.PaymentReceived;
      _unitOfWork.Repository<Order>().Update(order);
      await _unitOfWork.Complete();

      return order;
    }

    public async Task<Order> UpdatePaymentFailed(string paymentIntentId)
    {
      var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
      var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
      if (order == null) { return null; }

      order.Status = OrderStatus.PaymentFailed;
      await _unitOfWork.Complete();

      return order;
    }
  }
}