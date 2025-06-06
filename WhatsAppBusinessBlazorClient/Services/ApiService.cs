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



        // Message Templates API methods
        public async Task<List<MessageTemplate>> GetMessageTemplatesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching message templates");
                var response = await _httpClient.GetAsync("api/templates");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var templates = JsonSerializer.Deserialize<List<MessageTemplate>>(json, _jsonOptions);
                    return templates ?? new List<MessageTemplate>();
                }
                else
                {
                    _logger.LogWarning("Failed to fetch templates. Status: {StatusCode}", response.StatusCode);
                    return new List<MessageTemplate>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching message templates");
                return new List<MessageTemplate>();
            }
        }

        public async Task<MessageTemplate?> GetMessageTemplateAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching message template {TemplateId}", id);
                var response = await _httpClient.GetAsync($"api/templates/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<MessageTemplate>(json, _jsonOptions);
                }
                else
                {
                    _logger.LogWarning("Failed to fetch template {TemplateId}. Status: {StatusCode}", id, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching message template {TemplateId}", id);
                return null;
            }
        }

        public async Task<bool> SaveMessageTemplateAsync(MessageTemplate template)
        {
            try
            {
                _logger.LogInformation("Saving message template: {TemplateName}", template.Name);
                
                var json = JsonSerializer.Serialize(template, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                HttpResponseMessage response;
                if (template.Id == 0)
                {
                    response = await _httpClient.PostAsync("api/templates", content);
                }
                else
                {
                    response = await _httpClient.PutAsync($"api/templates/{template.Id}", content);
                }
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Message template saved successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to save template. Status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving message template");
                return false;
            }
        }

        public async Task<bool> DeleteMessageTemplateAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting message template {TemplateId}", id);
                var response = await _httpClient.DeleteAsync($"api/templates/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Message template deleted successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to delete template {TemplateId}. Status: {StatusCode}", id, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message template {TemplateId}", id);
                return false;
            }
        }

        // Export API methods
        public async Task<byte[]?> ExportToursAsync(ExportRequest request)
        {
            try
            {
                _logger.LogInformation("Exporting tours - Format: {Format}", request.Format);
                
                var queryParams = new List<string>();
                if (request.FromDate.HasValue) queryParams.Add($"fromDate={request.FromDate.Value:yyyy-MM-dd}");
                if (request.ToDate.HasValue) queryParams.Add($"toDate={request.ToDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(request.TourTypeFilter)) queryParams.Add($"tourTypeFilter={Uri.EscapeDataString(request.TourTypeFilter)}");
                queryParams.Add($"format={request.Format}");
                queryParams.Add($"groupByTour={request.GroupByTour}");
                queryParams.Add($"includeParticipants={request.IncludeParticipants}");
                
                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/exports/tours?{queryString}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Tours exported successfully");
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    _logger.LogWarning("Failed to export tours. Status: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting tours");
                return null;
            }
        }

        public async Task<byte[]?> ExportContactsAsync(ExportRequest request)
        {
            try
            {
                _logger.LogInformation("Exporting contacts - Format: {Format}", request.Format);
                
                var queryParams = new List<string>();
                if (request.FromDate.HasValue) queryParams.Add($"fromDate={request.FromDate.Value:yyyy-MM-dd}");
                if (request.ToDate.HasValue) queryParams.Add($"toDate={request.ToDate.Value:yyyy-MM-dd}");
                queryParams.Add($"format={request.Format}");
                
                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/exports/contacts?{queryString}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Contacts exported successfully");
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    _logger.LogWarning("Failed to export contacts. Status: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting contacts");
                return null;
            }
        }

        public async Task<byte[]?> ExportGuideMessagesAsync(ExportRequest request)
        {
            try
            {
                _logger.LogInformation("Exporting guide messages - Format: {Format}", request.Format);
                
                var queryParams = new List<string>();
                if (request.FromDate.HasValue) queryParams.Add($"fromDate={request.FromDate.Value:yyyy-MM-dd}");
                if (request.ToDate.HasValue) queryParams.Add($"toDate={request.ToDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(request.TourTypeFilter)) queryParams.Add($"tourTypeFilter={Uri.EscapeDataString(request.TourTypeFilter)}");
                queryParams.Add($"format={request.Format}");
                
                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/exports/guide-messages?{queryString}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Guide messages exported successfully");
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    _logger.LogWarning("Failed to export guide messages. Status: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting guide messages");
                return null;
            }
        }

        public async Task<byte[]?> ExportMessagesAsync(ExportRequest request)
        {
            try
            {
                _logger.LogInformation("Exporting messages - Format: {Format}", request.Format);
                
                var queryParams = new List<string>();
                if (request.FromDate.HasValue) queryParams.Add($"fromDate={request.FromDate.Value:yyyy-MM-dd}");
                if (request.ToDate.HasValue) queryParams.Add($"toDate={request.ToDate.Value:yyyy-MM-dd}");
                queryParams.Add($"format={request.Format}");
                
                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/exports/messages?{queryString}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Messages exported successfully");
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    _logger.LogWarning("Failed to export messages. Status: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting messages");
                return null;
            }
        }

        public async Task<byte[]?> ExportAutomatedResponsesAsync(ExportRequest request)
        {
            try
            {
                _logger.LogInformation("Exporting automated responses - Format: {Format}", request.Format);
                
                var queryParams = new List<string>();
                if (request.FromDate.HasValue) queryParams.Add($"fromDate={request.FromDate.Value:yyyy-MM-dd}");
                if (request.ToDate.HasValue) queryParams.Add($"toDate={request.ToDate.Value:yyyy-MM-dd}");
                queryParams.Add($"format={request.Format}");
                
                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"api/exports/automated-responses?{queryString}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Automated responses exported successfully");
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    _logger.LogWarning("Failed to export automated responses. Status: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting automated responses");
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