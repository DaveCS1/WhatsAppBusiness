using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatsAppBusinessAPI.Services
{
    public class WhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly string _apiToken;
        private readonly string _phoneNumberId;

        public WhatsAppService(HttpClient httpClient, IConfiguration configuration, ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _apiToken = _configuration["WhatsApp:ApiToken"] ?? string.Empty;
            _phoneNumberId = _configuration["WhatsApp:PhoneNumberId"] ?? string.Empty;

            if (string.IsNullOrEmpty(_apiToken) || string.IsNullOrEmpty(_phoneNumberId))
            {
                _logger.LogWarning("WhatsApp API Token or Phone Number ID is not configured. WhatsApp messaging will not function.");
            }
            else
            {
                _httpClient.BaseAddress = new Uri($"https://graph.facebook.com/v19.0/{_phoneNumberId}/");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        public async Task<bool> SendMessageAsync(string toWaId, string messageBody)
        {
            if (string.IsNullOrEmpty(_apiToken) || string.IsNullOrEmpty(_phoneNumberId))
            {
                _logger.LogWarning("WhatsApp API not configured. Cannot send message to {ToWaId}", toWaId);
                return false;
            }

            try
            {
                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = toWaId,
                    type = "text",
                    text = new { body = messageBody }
                };

                var jsonContent = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending WhatsApp message to {ToWaId}. Message length: {MessageLength} characters", 
                    toWaId, messageBody.Length);

                var response = await _httpClient.PostAsync("messages", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("WhatsApp message sent successfully to {ToWaId}. Response: {Response}", 
                        toWaId, responseContent);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send WhatsApp message to {ToWaId}. Status: {StatusCode}. Error: {ErrorContent}", 
                        toWaId, response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error while sending WhatsApp message to {ToWaId}: {Message}", 
                    toWaId, httpEx.Message);
                return false;
            }
            catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException)
            {
                _logger.LogError(tcEx, "Timeout while sending WhatsApp message to {ToWaId}", toWaId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending WhatsApp message to {ToWaId}: {Message}", 
                    toWaId, ex.Message);
                return false;
            }
        }

        public async Task<bool> SendTemplateMessageAsync(string toWaId, string templateName, string languageCode = "en_US", object[]? parameters = null)
        {
            if (string.IsNullOrEmpty(_apiToken) || string.IsNullOrEmpty(_phoneNumberId))
            {
                _logger.LogWarning("WhatsApp API not configured. Cannot send template message to {ToWaId}", toWaId);
                return false;
            }

            try
            {
                var templatePayload = new
                {
                    messaging_product = "whatsapp",
                    to = toWaId,
                    type = "template",
                    template = new
                    {
                        name = templateName,
                        language = new { code = languageCode },
                        components = parameters != null ? new[]
                        {
                            new
                            {
                                type = "body",
                                parameters = parameters.Select(p => new { type = "text", text = p.ToString() }).ToArray()
                            }
                        } : null
                    }
                };

                var jsonContent = JsonSerializer.Serialize(templatePayload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending WhatsApp template message '{TemplateName}' to {ToWaId}", 
                    templateName, toWaId);

                var response = await _httpClient.PostAsync("messages", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("WhatsApp template message sent successfully to {ToWaId}. Response: {Response}", 
                        toWaId, responseContent);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send WhatsApp template message to {ToWaId}. Status: {StatusCode}. Error: {ErrorContent}", 
                        toWaId, response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp template message to {ToWaId}: {Message}", 
                    toWaId, ex.Message);
                return false;
            }
        }

        public async Task<bool> MarkMessageAsReadAsync(string messageId)
        {
            if (string.IsNullOrEmpty(_apiToken) || string.IsNullOrEmpty(_phoneNumberId))
            {
                _logger.LogWarning("WhatsApp API not configured. Cannot mark message as read");
                return false;
            }

            try
            {
                var payload = new
                {
                    messaging_product = "whatsapp",
                    status = "read",
                    message_id = messageId
                };

                var jsonContent = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("messages", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Marked WhatsApp message {MessageId} as read", messageId);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to mark WhatsApp message {MessageId} as read. Status: {StatusCode}. Error: {ErrorContent}", 
                        messageId, response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking WhatsApp message {MessageId} as read: {Message}", 
                    messageId, ex.Message);
                return false;
            }
        }
    }
} 