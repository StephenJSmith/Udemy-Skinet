using System.Linq;
using System.Security.Claims;

namespace API.Extensions
{
  public static class ClaimsPrincipalEtensions
  {
    public static string RetrieveEmailFromPrincipal(
        this ClaimsPrincipal user)
    {
      var value = user?.Claims?
          .FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;

      return value;
    }
  }
}