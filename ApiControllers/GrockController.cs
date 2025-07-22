using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace QwenChatBackend.ApiControllers;

[ApiController]
[Route("api/grok")]
public class GrockController : ControllerBase
{
    private readonly string _apiKey;
    private const string QwenApiUrl = "https://api.x.ai/v1/chat/completions";
    
    private readonly HttpClient client = new HttpClient
    {
        Timeout = Timeout.InfiniteTimeSpan
    };

    public GrockController()
    {
        _apiKey = Environment.GetEnvironmentVariable("GrokAiApiKey");
    }
    
    [HttpPost("chat")]
    public async Task Chat()
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var requestBody = await reader.ReadToEndAsync();
        
        Console.WriteLine("--------------------------------------------");
        Console.WriteLine(requestBody);
        Console.WriteLine("--------------------------------------------");

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
