using System;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using StackExchange.Redis;

namespace Infrastructure.Data
{
  public class BasketRepository : IBasketRepository
  {
    private readonly IDatabase _database;

    public BasketRepository(IConnectionMultiplexer redis)
    {
      _database = redis.GetDatabase();
    }

    public async Task<bool> DeleteBasketAsync(string basketId)
    {
      var result = await _database.KeyDeleteAsync(basketId);

      return result;
    }

    public async Task<CustomerBasket> GetBasketAsync(string basketId)
    {
      var data = await _database.StringGetAsync(basketId);
      var customerBasket = data.IsNullOrEmpty
        ? null
        : JsonSerializer.Deserialize<CustomerBasket>(data);

      return customerBasket;
    }

    public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
    {
      var serialised = JsonSerializer.Serialize(basket);
      var timeToLive = TimeSpan.FromDays(30);
      var created = await _database.StringSetAsync(basket.Id, serialised, timeToLive);
      if (!created) { return null; }

      var result = await GetBasketAsync(basket.Id);

      return result;
    }
  }
}