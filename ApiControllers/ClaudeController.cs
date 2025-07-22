using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        // _apiKey = Environment.GetEnvironmentVariable("ClaudeAiApiKey");
        _apiKey = Environment.GetEnvironmentVariable("OpenAiApiKey");
        _logService = logService;
    }

    private readonly string _apiKey;

    private const string QwenApiUrl = "https://api.openai.com/v1/chat/completions";

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
        json["model"] = "gpt-4o-mini";
        
        if (json["messages"] is JArray messagesArray)
        {
            var system = messagesArray.FirstOrDefault(m => m["role"]?.ToString() == "system");
            var recent = new JArray(messagesArray
                .Where(m => m["role"]?.ToString() != "system")
                .TakeLast(10));
        
            var finalMessages = new JArray();
            if (system != null) finalMessages.Add(system);
            foreach (var m in recent) finalMessages.Add(m);
        
            json["messages"] = finalMessages;
        }

        var modifiedRequestBody = json.ToString();

        Console.WriteLine("--------------------------------------------");
        Console.WriteLine(requestBody);
        Console.WriteLine("--------------------------------------------");

        var httpContent = new StringContent(modifiedRequestBody, Encoding.UTF8, "application/json");

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
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

            var data = line["data:".Length..].Trim();

            if (data == "[DONE]")
                break;

            try
            {
                var parsed = JObject.Parse(data);
                var content = parsed["choices"]?[0]?["delta"]?["content"]?.ToString();

                if (!string.IsNullOrEmpty(content))
                {
                    var claudeCompatibleJson = new
                    {
                        delta = new
                        {
                            text = content
                        }
                    };

                    var adaptedLine = "data: " + JsonConvert.SerializeObject(claudeCompatibleJson);
                    await Response.WriteAsync(adaptedLine + "\n\n");
                    await Response.Body.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to parse line: " + line);
                Console.WriteLine(ex.Message);
            }
        }
    }
}