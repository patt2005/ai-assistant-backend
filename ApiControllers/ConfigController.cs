using Microsoft.AspNetCore.Mvc;

namespace QwenChatBackend.ApiControllers;

[ApiController]
[Route("api/config")]
public class ConfigController : ControllerBase
{
    [HttpGet("base-url")]
    public async Task<IActionResult> GetBaseUrl()
    {
        var baseUrl = "https://veo3-backend-164860087792.us-central1.run.app";

        var response = new
        {
            base_url = baseUrl
        };

        return Ok(response);
    }
}