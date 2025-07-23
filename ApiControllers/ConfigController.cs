using Microsoft.AspNetCore.Mvc;
using QwenChatBackend.Models;
using QwenChatBackend.Services;

namespace QwenChatBackend.ApiControllers;

[ApiController]
[Route("api/config")]
public class ConfigController : ControllerBase
{
    private readonly ILogService _logService;
    private readonly IConfiguration _configuration;

    public ConfigController(ILogService logService, IConfiguration configuration)
    {
        _logService = logService;
        _configuration = configuration;
    }

    [HttpGet("base-url")]
    public async Task<IActionResult> GetBaseUrl()
    {
        await _logService.LogAsync(new Log
        {
            Method = "GET",
            Endpoint = "/api/config/base-url"
        });

        var baseUrl = "https://veo3-backend-118847640969.europe-west1.run.app";

        var response = new
        {
            base_url = baseUrl
        };

        return Ok(response);
    }
}