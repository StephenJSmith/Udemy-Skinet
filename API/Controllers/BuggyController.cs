using System.Net;
using API.Errors;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  public class BuggyController : BaseApiController
  {
    private readonly StoreContext _context;
    public BuggyController(StoreContext context)
    {
      _context = context;
    }

    [HttpGet("notfound")]
    public ActionResult GetNotFoundRequest()
    {
        var item = _context.Products.Find(42);
        if (item == null)
        {
            return NotFound( new ApiResponse((int)HttpStatusCode.NotFound));
        }

        return Ok();
    }

    [HttpGet("testauth")]
    [Authorize]
    public ActionResult<string> GetSecretText() {
      return "secret stuff";
    }


    [HttpGet("servererror")]
    public ActionResult GetServerError()
    {
        var item = _context.Products.Find(42);
        var result = item.ToString();

        return Ok();
    }

    [HttpGet("badrequest")]
    public ActionResult GetBadRequest()
    {
        return BadRequest(new ApiResponse((int)HttpStatusCode.BadRequest));
    }

    [HttpGet("badrequest/{id}")]
    public ActionResult GetIncorrectTypeRequest(int id)
    {
        return Ok();
    }
  }
}