using Microsoft.AspNetCore.Mvc;
using System;

namespace QwenChatBackend.ApiControllers;

[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("api/test")]
    public IActionResult Get()
    {
        string apiKey = Environment.GetEnvironmentVariable("OpenAiApiKey") ?? "No key was found";
        
        return Ok("hello api key");
    }
}