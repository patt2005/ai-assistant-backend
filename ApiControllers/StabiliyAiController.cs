using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using QwenChatBackend.Models;
using QwenChatBackend.Services;

namespace QwenChatBackend.ApiControllers;

[ApiController]
[Route("api/stability")]
public class StabiliyAiController : ControllerBase
{
    private readonly ILogService _logService;

    private readonly HttpClient client = new HttpClient
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    private readonly string _apiKey;

    public StabiliyAiController(ILogService logService)
    {
        _logService = logService;
        _apiKey = Environment.GetEnvironmentVariable("StabilityAiApiKey");
    }
    
    [HttpPost("generate-image")]
    public async Task<IActionResult> GenerateImage([FromQuery] string prompt, [FromQuery] string aspectRatio, [FromQuery] string style)
    {
        await _logService.LogAsync(new Log
        {
            Method = "POST",
            Endpoint = "/api/stability/generate-image"
        });
        
        var stabilityAiUrl = "https://api.stability.ai/v2beta/stable-image/generate/core";

        using var request = new HttpRequestMessage(HttpMethod.Post, stabilityAiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/*"));
        
        var boundary = $"Boundary-{Guid.NewGuid()}";
        var content = new MultipartFormDataContent(boundary);

        var promptContent = new StringContent(prompt);
        promptContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "\"prompt\""
        };
        content.Add(promptContent);

        var aspectRatioContent = new StringContent(aspectRatio);
        aspectRatioContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "\"aspect_ratio\""
        };
        content.Add(aspectRatioContent);

        var stylePresetContent = new StringContent(style);
        stylePresetContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "\"style_preset\""
        };
        content.Add(stylePresetContent);

        var outputFormatContent = new StringContent("png");
        outputFormatContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "\"output_format\""
        };
        content.Add(outputFormatContent);

        request.Content = content;

        var response = await client.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorMsg = await response.Content.ReadAsStringAsync();
            await _logService.LogAsync(new Log
            {
                Method = "POST",
                Endpoint = "/api/stability/generate-image",
                ResponseBody = errorMsg, 
            });
            return StatusCode((int)response.StatusCode, $"Error: {errorMsg}");
        }

        var imageData = await response.Content.ReadAsByteArrayAsync();
        return File(imageData, "image/png");
    }
}
