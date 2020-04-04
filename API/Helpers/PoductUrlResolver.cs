using API.Dtos;
using AutoMapper;
using Core.Entities;
using Microsoft.Extensions.Configuration;

namespace API.Helpers
{
  public class PoductUrlResolver : IValueResolver<Product, ProductToReturnDto, string>
  {
    private readonly IConfiguration _config;

    public PoductUrlResolver(IConfiguration config)
    {
      _config = config;
    }

    public string Resolve(
        Product source,
        ProductToReturnDto destination,
        string destMember,
        ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.PictureUrl)) { return null; }

        var resolved = _config["ApiUrl"] + source.PictureUrl;

        return resolved;
    }
  }
}