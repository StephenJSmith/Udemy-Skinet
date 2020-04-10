using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using Core.Entities.Identity;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  public class AccountController : BaseApiController
  {
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
        IMapper mapper
    )
    {
      _signInManager = signInManager;
      _userManager = userManager;
      _tokenService = tokenService;
      _mapper = mapper;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
      var user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);

      var userDto = new UserDto
      {
        Email = user.Email,
        Token = _tokenService.CreateToken(user),
        DisplayName = user.DisplayName
      };

      return userDto;
    }

    [HttpGet("emailexists")]
    public async Task<ActionResult<bool>> CheckEmailExists([FromQuery] string email)
    {
      var result = await _userManager.FindByEmailAsync(email) != null;

      return result;
    }

    [Authorize]
    [HttpGet("address")]
    public async Task<ActionResult<AddressDto>> GetUserAddress()
    {
      var user = await _userManager.FindUserByClaimsPrincipalWithAddressAsync(HttpContext.User);
      var addressDto = _mapper.Map<Address, AddressDto>(user.Address);

      return addressDto;
    }

    [Authorize]
    [HttpPut("address")]
    public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto address)
    {
      var user = await _userManager.FindUserByClaimsPrincipalWithAddressAsync(HttpContext.User);
      user.Address = _mapper.Map<AddressDto, Address>(address);

      var result = await _userManager.UpdateAsync(user);
      if (!result.Succeeded) { return BadRequest("Problem updating the user"); }

      var addressDto = _mapper.Map<Address, AddressDto>(user.Address);

      return addressDto;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
      var unauthorisedCode = (int)HttpStatusCode.Unauthorized;
      var user = await _userManager.FindByEmailAsync(loginDto.Email);
      if (user == null) { return Unauthorized(new ApiResponse(unauthorisedCode)); }

      var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
      if (!result.Succeeded) { return Unauthorized(new ApiResponse(unauthorisedCode)); }

      var userDto = new UserDto
      {
        Email = user.Email,
        Token = _tokenService.CreateToken(user),
        DisplayName = user.DisplayName
      };

      return userDto;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
      var badRequestCode = (int)HttpStatusCode.BadRequest;
      var user = new AppUser
      {
        DisplayName = registerDto.DisplayName,
        Email = registerDto.Email,
        UserName = registerDto.Email
      };

      var result = await _userManager.CreateAsync(user, registerDto.Password);
      if (!result.Succeeded) { return BadRequest(new ApiResponse(badRequestCode)); }

      var userDto = new UserDto
      {
        Email = user.Email,
        Token = _tokenService.CreateToken(user),
        DisplayName = user.DisplayName
      };

      return userDto;
    }
  }
}