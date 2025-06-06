using System.Text;
using System.Text.Json;
using WhatsAppBusinessBlazorClient.Models;

namespace WhatsAppBusinessBlazorClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            // Log the base address for debugging
            Console.WriteLine($"🔧 ApiService constructor - HttpClient BaseAddress: {_httpClient.BaseAddress}");
            _logger.LogInformation("ApiService initialized with base address: {BaseAddress}", _httpClient.BaseAddress);
            
            if (_httpClient.BaseAddress == null)
            {
                Console.WriteLine("❌ ERROR: HttpClient BaseAddress is NULL!");
                _logger.LogError("HttpClient BaseAddress is null - this will cause API calls to fail");
            }
        }

        public async Task<List<Contact>> GetContactsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching contacts from API");
                var response = await _httpClient.GetAsync("api/chat/contacts");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var contacts = JsonSerializer.Deserialize<List<Contact>>(json, _jsonOptions);
                    return contacts ?? new List<Contact>();
                }
                else
                {
                    _logger.LogWarning("Failed to fetch contacts. Status: {StatusCode}", response.StatusCode);
                    return new List<Contact>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching contacts");
                return new List<Contact>();
            }
        }

        public async Task<List<Message>> GetMessagesAsync(int contactId)
        {
            try
            {
                _logger.LogInformation("Fetching messages for contact {ContactId}", contactId);
                var response = await _httpClient.GetAsync($"api/chat/messages/{contactId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var messages = JsonSerializer.Deserialize<List<Message>>(json, _jsonOptions);
                    return messages ?? new List<Message>();
                }
                else
                {
                    _logger.LogWarning("Failed to fetch messages for contact {ContactId}. Status: {StatusCode}", contactId, response.StatusCode);
                    return new List<Message>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching messages for contact {ContactId}", contactId);
                return new List<Message>();
            }
        }

        public async Task<bool> SendMessageAsync(int contactId, string message)
        {
            try
            {
                _logger.LogInformation("Sending message to contact {ContactId}", contactId);
                
                var request = new
                {
                    ContactId = contactId,
                    Message = message
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/chat/send-reply", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Message sent successfully to contact {ContactId}", contactId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to send message to contact {ContactId}. Status: {StatusCode}", contactId, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to contact {ContactId}", contactId);
                return false;
            }
        }

        public async Task<List<AutomatedResponseLog>> GetAutomatedResponseLogsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching automated response logs");
                var response = await _httpClient.GetAsync("api/chat/logs");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var logs = JsonSerializer.Deserialize<List<AutomatedResponseLog>>(json, _jsonOptions);
                    return logs ?? new List<AutomatedResponseLog>();
                }
                else
                {
                    _logger.LogWarning("Failed to fetch logs. Status: {StatusCode}", response.StatusCode);
                    return new List<AutomatedResponseLog>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching automated response logs");
                return new List<AutomatedResponseLog>();
            }
        }

        public async Task<SystemStats?> GetSystemStatsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching system statistics");
                var response = await _httpClient.GetAsync("api/chat/stats");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var stats = JsonSerializer.Deserialize<SystemStats>(json, _jsonOptions);
                    return stats;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch stats. Status: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching system statistics");
                return null;
            }
        }

        public async Task<bool> TestApiConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing API connection");
                var response = await _httpClient.GetAsync("api/test/sample-messages");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API connection test failed");
                return false;
            }
        }

        public async Task<bool> ResendMessageAsync(int logId, string? customMessage = null)
        {
            try
            {
                _logger.LogInformation("Resending message for log {LogId}", logId);
                
                var request = new
                {
                    LogId = logId,
                    CustomMessage = customMessage
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/chat/resend-message", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Message resent successfully for log {LogId}", logId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to resend message for log {LogId}. Status: {StatusCode}", logId, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending message for log {LogId}", logId);
                return false;
            }
        }

        public async Task<bool> ReviewLogAsync(int logId, string status, string notes, string reviewer)
        {
            try
            {
                _logger.LogInformation("Reviewing log {LogId}", logId);
                
                var request = new
                {
                    LogId = logId,
                    Status = status,
                    Notes = notes,
                    Reviewer = reviewer,
                    ReviewedAt = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/chat/review-log", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Log {LogId} reviewed successfully", logId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to review log {LogId}. Status: {StatusCode}", logId, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing log {LogId}", logId);
                return false;
            }
        }

        public async Task<byte[]?> ExportLogDataAsync(int logId, string format, ExportOptions options)
        {
            try
            {
                _logger.LogInformation("Exporting data for log {LogId} in {Format} format", logId, format);
                
                var request = new
                {
                    LogId = logId,
                    Format = format,
                    Options = options
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/chat/export-log", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();
                    _logger.LogInformation("Data exported successfully for log {LogId}", logId);
                    return data;
                }
                else
                {
                    _logger.LogWarning("Failed to export data for log {LogId}. Status: {StatusCode}", logId, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data for log {LogId}", logId);
                return null;
            }
        }

        public async Task<bool> ContactCustomerAsync(string contactWaId, string method, string? message = null)
        {
            try
            {
                _logger.LogInformation("Contacting customer {ContactWaId} via {Method}", contactWaId, method);
                
                var request = new
                {
                    ContactWaId = contactWaId,
                    Method = method,
                    Message = message,
                    InitiatedAt = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/chat/contact-customer", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Customer contact initiated successfully for {ContactWaId}", contactWaId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to contact customer {ContactWaId}. Status: {StatusCode}", contactWaId, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contacting customer {ContactWaId}", contactWaId);
                return false;
            }
        }

        public async Task<byte[]?> ExportBulkLogDataAsync(string exportType, string format, DateTime fromDate, DateTime toDate, string? contactFilter, ExportOptions options)
        {
            try
            {
                _logger.LogInformation("Exporting bulk data - Type: {ExportType}, Format: {Format}", exportType, format);
                
                var request = new
                {
                    ExportType = exportType,
                    Format = format,
                    FromDate = fromDate,
                    ToDate = toDate,
                    ContactFilter = contactFilter,
                    Options = options
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/chat/export-bulk", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();
                    _logger.LogInformation("Bulk data exported successfully - Type: {ExportType}", exportType);
                    return data;
                }
                else
                {
                    _logger.LogWarning("Failed to export bulk data. Status: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting bulk data - Type: {ExportType}", exportType);
                return null;
            }
        }
    }

    public class SystemStats
    {
        public int TotalContacts { get; set; }
        public int TotalAutomatedResponses { get; set; }
        public int SuccessfulResponses { get; set; }
        public int FailedResponses { get; set; }
        public double AverageProcessingTime { get; set; }
        public double AverageAiCallTime { get; set; }
        public DateTime? LastResponseTime { get; set; }
        public int ResponsesLast24Hours { get; set; }
    }

    public class ExportOptions
    {
        public bool IncludeResponseText { get; set; } = true;
        public bool IncludeAiData { get; set; } = true;
        public bool IncludePerformanceData { get; set; } = true;
        public bool IncludeTourDetails { get; set; } = true;
        public string ExportedBy { get; set; } = "Admin User";
    }
} 