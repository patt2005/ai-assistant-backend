using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;

namespace QwenChatBackend.ApiControllers;

[ApiController]
[Route("api/file")]
public class FileController : ControllerBase
{
    private readonly string _bucketName = "ai-assistant-macos-app";
    private readonly GoogleCredential _credential;

    public FileController()
    {
        var authJson = Environment.GetEnvironmentVariable("GCPStorageAuthFile") ?? "";
        _credential = GoogleCredential.FromJson(authJson);
    }
    
    [HttpPost("upload-file")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty or missing");
        }
        
        var client = StorageClient.Create(_credential);
    
        using var stream = file.OpenReadStream() ;
        var obj = await client.UploadObjectAsync(_bucketName, file.FileName, file.ContentType, stream);

        return Ok(new { message = "File uploaded successfully!", fileName = obj.Name });
    }
    
    [HttpGet("get-file")]
    public async Task<IActionResult> GetFile(string fileName)
    {
        var client = StorageClient.Create(_credential);
        var stream = new MemoryStream();
        var obj = await client.DownloadObjectAsync(_bucketName, fileName, stream);
        stream.Position = 0;
        
        return File(stream, obj.ContentType, obj.Name);
    }
}