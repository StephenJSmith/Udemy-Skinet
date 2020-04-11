using System.Threading.Tasks;
using API.Dtos;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  public class BasketController : BaseApiController
  {
    private readonly IBasketRepository _basketRepository;
    private readonly IMapper _mapper;

    public BasketController(
      IBasketRepository basketRepository,
      IMapper mapper)
    {
      _mapper = mapper;
      _basketRepository = basketRepository;
    }

    [HttpGet]
    public async Task<ActionResult<CustomerBasket>> GetBasketById(string id)
    {
      var basket = await _basketRepository.GetBasketAsync(id);
      var result = basket ?? new CustomerBasket(id);

      return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerBasket>> UpdateBasket(CustomerBasketDto basketDto)
    {
      var customerBasket = _mapper.Map<CustomerBasketDto, CustomerBasket>(basketDto);
      var updateBasket = await _basketRepository.UpdateBasketAsync(customerBasket);

      return Ok(updateBasket);
    }

    [HttpDelete]
    public async Task DeleteBasket(string id)
    {
      await _basketRepository.DeleteBasketAsync(id);
    }
  }
}