using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  [Authorize]
  public class OrdersController : BaseApiController
  {
    private readonly IOrderService _orderService;
    private readonly IMapper _mapper;

    public OrdersController(
        IOrderService orderService,
        IMapper mapper)
    {
      _mapper = mapper;
      _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(OrderDto orderDto)
    {
      var email = HttpContext.User.RetrieveEmailFromPrincipal();
      var address = _mapper.Map<AddressDto, Address>(orderDto.ShipToAddress);
      var order = await _orderService.CreateOrderAsync(email,
        orderDto.DeliveryMethodId, orderDto.BasketId, address);
      if (order == null)
      {
        var apiResponse = new ApiResponse((int)HttpStatusCode.BadRequest,
            "Problem creating order");

        return BadRequest(apiResponse);
      }

      return Ok(order);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetOrdersForUser() {
      var email = HttpContext.User.RetrieveEmailFromPrincipal();
      var orders = await _orderService.GetOrdersForUserAsync(email);
      var orderDtos = _mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDto>>(orders);

      return Ok(orderDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderByIdForUser(int id) {
      var email = HttpContext.User.RetrieveEmailFromPrincipal();
      var order = await _orderService.GetOrderByIdAsync(id, email);
      if (order == null)
      {
        var apiResponse = new ApiResponse((int)HttpStatusCode.NotFound);

        return NotFound(apiResponse);
      }

      var orderDto = _mapper.Map<Order, OrderToReturnDto>(order);

      return Ok(orderDto);
    }

    [HttpGet("deliveryMethods")]
    public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods() {
      var methods = await _orderService.GetDeliveryMethodsAsync();

      return Ok(methods);
    }
  }
}