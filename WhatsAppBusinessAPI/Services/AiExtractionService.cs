using System.Text.Json;

namespace WhatsAppBusinessAPI.Services
{
    public record ExtractedUserInfo(string UserName, string TourType, string TourDate, string TourTime);

    public class AiExtractionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiExtractionService> _logger;
        private readonly string _aiApiEndpoint;
        private readonly string _aiApiKey;
        private readonly string _aiModel;

        public AiExtractionService(HttpClient httpClient, IConfiguration configuration, ILogger<AiExtractionService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _aiApiEndpoint = _configuration["AiApi:Endpoint"] ?? "https://generativelanguage.googleapis.com/v1beta/models/";
            _aiApiKey = _configuration["AiApi:ApiKey"] ?? string.Empty;
            _aiModel = _configuration["AiApi:Model"] ?? "gemini-2.0-flash";

            if (string.IsNullOrEmpty(_aiApiKey))
            {
                _logger.LogWarning("AI API Key is not configured. AI extraction will not function.");
            }
        }

        public async Task<ExtractedUserInfo?> ExtractUserInfoAsync(string messageText)
        {
            if (string.IsNullOrEmpty(_aiApiKey))
            {
                _logger.LogWarning("AI API Key is missing. Skipping AI extraction for message: {MessageText}", messageText);
                return new ExtractedUserInfo("N/A", "N/A", "N/A", "N/A"); // Return default values for development
            }

            try
            {
                var prompt = $@"Extract the user's name, tour type, tour date, and tour time from this WhatsApp message. 
If any information is not present or unclear, use 'N/A' for that field.

Tour types might include: Walking Tour, Food Tour, Historical Tour, Art Tour, Photography Tour, etc.
Tour dates might be: today, tomorrow, specific dates like 'July 1st', 'next Monday', etc.
Tour times might be: morning, afternoon, evening, or specific times like '9 AM', '2 PM', etc.

Message: '{messageText}'

Please extract:
- UserName: The person's name if mentioned
- TourType: What kind of tour they're interested in
- TourDate: When they want the tour
- TourTime: What time they prefer";

                var chatHistory = new[] {
                    new { role = "user", parts = new[] { new { text = prompt } } }
                };

                var payload = new
                {
                    contents = chatHistory,
                    generationConfig = new
                    {
                        responseMimeType = "application/json",
                        responseSchema = new
                        {
                            type = "OBJECT",
                            properties = new Dictionary<string, object>
                            {
                                { "UserName", new { type = "STRING", description = "The user's name if mentioned, otherwise 'N/A'" } },
                                { "TourType", new { type = "STRING", description = "Type of tour requested (Walking, Food, Historical, etc.) or 'N/A'" } },
                                { "TourDate", new { type = "STRING", description = "When they want the tour (today, tomorrow, specific date) or 'N/A'" } },
                                { "TourTime", new { type = "STRING", description = "Preferred time (morning, afternoon, 9 AM, etc.) or 'N/A'" } }
                            },
                            required = new[] { "UserName", "TourType", "TourDate", "TourTime" }
                        }
                    }
                };

                var requestUri = $"{_aiApiEndpoint}{_aiModel}:generateContent?key={_aiApiKey}";
                var jsonContent = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation("Calling AI API for message extraction. Message: {MessageText}", messageText);

                var response = await _httpClient.PostAsync(requestUri, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AI API request failed. Status: {StatusCode}. Error: {ErrorContent}", 
                        response.StatusCode, errorContent);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("AI API raw response: {Response}", jsonResponse);

                // Parse the Gemini API response structure
                using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                {
                    if (doc.RootElement.TryGetProperty("candidates", out JsonElement candidatesElement) &&
                        candidatesElement.EnumerateArray().Any())
                    {
                        var firstCandidate = candidatesElement.EnumerateArray().First();
                        
                        if (firstCandidate.TryGetProperty("content", out JsonElement contentElement) &&
                            contentElement.TryGetProperty("parts", out JsonElement partsElement) &&
                            partsElement.EnumerateArray().Any())
                        {
                            var firstPart = partsElement.EnumerateArray().First();
                            
                            if (firstPart.TryGetProperty("text", out JsonElement textElement))
                            {
                                var extractedJson = textElement.GetString();
                                _logger.LogInformation("AI extracted JSON: {ExtractedJson}", extractedJson);

                                if (!string.IsNullOrEmpty(extractedJson))
                                {
                                    var extractedInfo = JsonSerializer.Deserialize<ExtractedUserInfo>(extractedJson, 
                                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                    
                                    _logger.LogInformation("Successfully extracted user info - Name: {UserName}, Tour: {TourType}, Date: {TourDate}, Time: {TourTime}",
                                        extractedInfo?.UserName, extractedInfo?.TourType, extractedInfo?.TourDate, extractedInfo?.TourTime);
                                    
                                    return extractedInfo;
                                }
                            }
                        }
                    }
                }

                _logger.LogWarning("AI API response did not contain expected structure for message: {MessageText}", messageText);
                return null;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error calling AI API for message: {MessageText}. Error: {Message}", 
                    messageText, httpEx.Message);
                return null;
            }
            catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException)
            {
                _logger.LogError(tcEx, "Timeout calling AI API for message: {MessageText}", messageText);
                return null;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON parsing error from AI API response for message: {MessageText}. Error: {Message}", 
                    messageText, jsonEx.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during AI extraction for message: {MessageText}. Error: {Message}", 
                    messageText, ex.Message);
                return null;
            }
        }


    }
} 