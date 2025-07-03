using System.Text;
using Microsoft.AspNetCore.Mvc;
using QwenChatBackend.Models;
using QwenChatBackend.Services;
using Newtonsoft.Json.Linq;

namespace QwenChatBackend.ApiControllers;

[ApiController]
[Route("api/chatgpt")]
public class ChatGptController : ControllerBase
{
    private readonly ILogService _logService;

    private readonly HttpClient client = new HttpClient
    {
        Timeout = Timeout.InfiniteTimeSpan
    };

    private readonly string _apiKey;
    private const string ChatApiUrl = "https://api.openai.com/v1/chat/completions";

    public ChatGptController(ILogService logService)
    {
        _apiKey = Environment.GetEnvironmentVariable("OpenAiApiKey");
        _logService = logService;
    }
    
    [HttpPost("generate-audio")]
    public async Task<IActionResult> GenerateAudio()
    {
        await _logService.LogAsync(new Log
        {
            Method = "POST",
            Endpoint = "/api/chatgpt/generate-audio"
        });
        
        var apiUrl = "https://api.openai.com/v1/audio/speech";

        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var requestBody = await reader.ReadToEndAsync();

        var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = httpContent
        };

        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, new { error = "OpenAI API error", details = errorResponse });
        }

        var audioData = await response.Content.ReadAsByteArrayAsync();
        return File(audioData, "audio/mpeg", "speech.mp3");
    }

    [HttpPost("solve-math-problem")]
    public async Task<IActionResult> SolveMathProblem()
    {
        await _logService.LogAsync(new Log
        {
            Method = "POST",
            Endpoint = "/api/chatgpt/solve-math-problem"
        });
        
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var requestBody = await reader.ReadToEndAsync();
        
        var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, ChatApiUrl)
        {
            Content = httpContent
        };

        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        var responseContent = await response.Content.ReadAsStringAsync();

        return Content(responseContent, "application/json");
    }

    [HttpPost("chat")]
    public async Task Chat()
    {
        await _logService.LogAsync(new Log
        {
            Method = "POST",
            Endpoint = "/api/chatgpt/chat"
        });

        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var requestBody = await reader.ReadToEndAsync();
        
        var json = JObject.Parse(requestBody);
        json["model"] = "gpt-4o-mini";
        
        var modifiedRequestBody = json.ToString();

        var httpContent = new StringContent(modifiedRequestBody, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, ChatApiUrl)
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

    [HttpPost("websearch")]
    public async Task WebSearch()
    {
        await _logService.LogAsync(new Log
        {
            Method = "POST",
            Endpoint = "/api/chatgpt/websearch"
        });

        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var requestBody = await reader.ReadToEndAsync();
        
        await _logService.LogAsync(new Log
        {
            Method = "POST",
            Endpoint = "/api/chatgpt/websearch",
            RequestBody = requestBody
        });

        var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses")
        {
            Content = httpContent
        };

        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            Response.StatusCode = (int)response.StatusCode;
            var errorContent = await response.Content.ReadAsStringAsync();
            await Response.WriteAsync($"data: Error {response.StatusCode}: {errorContent}\n\n");
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