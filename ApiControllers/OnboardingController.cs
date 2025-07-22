using Microsoft.AspNetCore.Mvc;
using QwenChatBackend.Models;
using QwenChatBackend.Services;

namespace QwenChatBackend.ApiControllers;

[ApiController]
[Route("api/onboarding")]
public class OnboardingController : ControllerBase
{
    private readonly ILogService _logService;

    public OnboardingController(ILogService logService)
    {
        _logService = logService;
    }

    [HttpGet("variant")]
    public async Task<IActionResult> GetOnboardingVariant()
    {
        await _logService.LogAsync(new Log
        {
            Method = "GET",
            Endpoint = "/api/onboarding/variant"
        });

        var response = new
        {
            onboarding_variant = "C"
        };

        return Ok(response);
    }
}