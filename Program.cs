using Microsoft.AspNetCore.Http.Features;
using QwenChatBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Adaugă Health Checks
builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<ILogService, LogService>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100_000_000;
    options.ValueLengthLimit = int.MaxValue;
    options.BufferBody = false;
});

var app = builder.Build();

// Configurează pipeline-ul HTTP
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Adaugă endpoint pentru health check
app.MapHealthChecks("/health");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();