using Microsoft.AspNetCore.Mvc;
using QwenChatBackend.Models;
using QwenChatBackend.Services;
using System.Text;

[ApiController]
[Route("api/qwen")]
public class QwenController(ILogService logService) : ControllerBase
{
    private readonly ILogService _logService = logService;

    private readonly HttpClient client = new HttpClient
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    private string _apiKey = "";
    private const string QwenApiUrl = "https://dashscope-intl.aliyuncs.com/compatible-mode/v1/chat/completions";

    [HttpPost("chat")]
    public async Task Chat()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _apiKey = Environment.GetEnvironmentVariable("QwenAiApiKey");
        }
        
        await _logService.LogAsync(new Log
        {
            Method = "POST",
            Endpoint = "/api/qwen/chat"
        });

        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var requestBody = await reader.ReadToEndAsync();

        var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, QwenApiUrl)
        {
            Content = httpContent
        };

        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");

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