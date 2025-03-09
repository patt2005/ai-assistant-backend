using Microsoft.AspNetCore.Mvc;

namespace QwenChatBackend.ApiControllers;

[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("api/test")]
    public IActionResult Get()
    {
        return Ok("Hello world!");
    }
}