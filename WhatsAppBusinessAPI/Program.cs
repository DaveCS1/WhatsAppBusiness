using WhatsAppBusinessAPI.Services;
using WhatsAppBusinessAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register Repositories
builder.Services.AddScoped<ChatRepository>();

// Register Services with HttpClient
builder.Services.AddHttpClient<WhatsAppService>();
builder.Services.AddHttpClient<AiExtractionService>();
builder.Services.AddScoped<TourPresetsService>();

// Configure CORS (important for Blazor client if on different origin)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy => policy
            .WithOrigins(
                "https://localhost:7001", // Your Blazor dev server (HTTPS)
                "http://localhost:5000",  // Your Blazor dev server (HTTP, if applicable)
                "https://localhost:5001"  // Alternative HTTPS port
                // Add your deployed Blazor client URL here for production
                // e.g., "https://your-blazor-app.azurewebsites.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // Required for SignalR if used
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Initialize the database
await DbInitializer.Initialize(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting(); // Important for CORS

app.UseCors("AllowBlazorClient"); // Apply the CORS policy

app.UseAuthorization();

app.MapControllers();

app.Run();
