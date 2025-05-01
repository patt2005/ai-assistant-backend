using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using QwenChatBackend.Models;
using QwenChatBackend.Services;

namespace QwenChatBackend.ApiControllers;

[ApiController]
[Route("api/claude")]
public class ClaudeController : ControllerBase
{
    private readonly ILogService _logService;

    private readonly HttpClient client = new HttpClient
    {
        Timeout = Timeout.InfiniteTimeSpan
    };

    public ClaudeController(ILogService logService)
    {
        _apiKey = Environment.GetEnvironmentVariable("ClaudeAiApiKey");
        _logService = logService;
    }

    private readonly string _apiKey;

    private const string QwenApiUrl = "https://api.anthropic.com/v1/messages";

    [HttpPost("chat")]
    public async Task Chat()
    {
        await _logService.LogAsync(new Log
        {
            Method = "POST",
            Endpoint = "/api/claude/chat"
        });
        
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var requestBody = await reader.ReadToEndAsync();
        
        var json = JObject.Parse(requestBody);
        json["model"] = "claude-3-5-haiku-20241022";
        
        var modifiedRequestBody = json.ToString();

        Console.WriteLine("--------------------------------------------");
        Console.WriteLine(requestBody);
        Console.WriteLine("--------------------------------------------");

        var httpContent = new StringContent(modifiedRequestBody, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, QwenApiUrl)
        {
            Content = httpContent
        };

        httpRequest.Headers.Add("x-api-key", _apiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");

        var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            Response.StatusCode = (int)response.StatusCode;
            await Response.WriteAsync($"data: Error {response.StatusCode}\n\n");
            return;
        }

        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");

        var responseStream = await response.Content.ReadAsStreamAsync();
        using var responseReader = new StreamReader(responseStream);

        while (!responseReader.EndOfStream)
        {
            var line = await responseReader.ReadLineAsync();
            if (string.IsNullOrEmpty(line)) continue;
            await Response.WriteAsync($"{line}\n\n");
            await Response.Body.FlushAsync();
        }
    }
}