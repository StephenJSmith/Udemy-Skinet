using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class FallbackController : Controller
    {
        public IActionResult Index() {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
            var contentType = "text/HTML";

            return PhysicalFile(path, contentType);
        }
    }
}