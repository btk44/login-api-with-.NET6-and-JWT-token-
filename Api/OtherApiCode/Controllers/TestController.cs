using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    public TestController() { }

    [HttpGet("public")]
    public ActionResult<string> PublicGet()
    {
        return Ok("hello public");
    }

    [Authorize]
    [HttpGet("private")]
    public ActionResult<string> PrivateGet()
    {
        return Ok("hello private");
    }
}