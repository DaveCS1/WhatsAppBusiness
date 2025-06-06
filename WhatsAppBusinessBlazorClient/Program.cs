using WhatsAppBusinessBlazorClient.Components;
using WhatsAppBusinessBlazorClient.Models;
using WhatsAppBusinessBlazorClient.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure API base URL - hardcoded for now to ensure it works
var apiBaseUrl = "http://localhost:5260";
Console.WriteLine($"🔧 API Base URL: {apiBaseUrl}");

// Add HttpClient with base address
builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    Console.WriteLine($"🔧 HttpClient configured with base address: {client.BaseAddress}");
});

// Configure API settings
builder.Services.Configure<ApiSettings>(options =>
{
    options.BaseUrl = apiBaseUrl;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
