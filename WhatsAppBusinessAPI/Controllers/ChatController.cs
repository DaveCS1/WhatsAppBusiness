using Microsoft.AspNetCore.Mvc;
using WhatsAppBusinessAPI.Models;
using WhatsAppBusinessAPI.Repositories;
using WhatsAppBusinessAPI.Services;
using System.Text.Json;

namespace WhatsAppBusinessAPI.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatRepository _chatRepository;
        private readonly WhatsAppService _whatsAppService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            ChatRepository chatRepository,
            WhatsAppService whatsAppService,
            ILogger<ChatController> logger)
        {
            _chatRepository = chatRepository;
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        [HttpGet("contacts")]
        public async Task<ActionResult<IEnumerable<Contact>>> GetContacts()
        {
            try
            {
                var contacts = await _chatRepository.GetContactsAsync();
                _logger.LogInformation("Retrieved {Count} contacts", contacts.Count());
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contacts: {ErrorMessage}", ex.Message);
                return StatusCode(500, "Internal server error while retrieving contacts.");
            }
        }

        [HttpGet("messages/{contactId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessagesForContact(int contactId)
        {
            try
            {
                var messages = await _chatRepository.GetMessagesForContactAsync(contactId);
                _logger.LogInformation("Retrieved {Count} messages for contact ID: {ContactId}", messages.Count(), contactId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for contact ID {ContactId}: {ErrorMessage}", contactId, ex.Message);
                return StatusCode(500, "Internal server error while retrieving messages.");
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrEmpty(request.ToWaId) || string.IsNullOrEmpty(request.MessageBody))
            {
                return BadRequest("Recipient ID and message body are required.");
            }

            try
            {
                _logger.LogInformation("Sending manual message to {ToWaId}: {MessageBody}", request.ToWaId, request.MessageBody);

                bool sent = await _whatsAppService.SendMessageAsync(request.ToWaId, request.MessageBody);
                
                if (sent)
                {
                    // Save outgoing message to DB
                    var contact = await _chatRepository.GetOrCreateContactAsync(request.ToWaId, request.ToWaId);
                    var outgoingMessage = new Message
                    {
                        WaMessageId = $"agent-reply-{Guid.NewGuid()}",
                        ContactId = contact.Id,
                        Body = request.MessageBody,
                        IsFromMe = true,
                        Timestamp = DateTime.UtcNow,
                        Status = "sent"
                    };
                    await _chatRepository.SaveMessageAsync(outgoingMessage);

                    _logger.LogInformation("Manual message sent successfully to {ToWaId}", request.ToWaId);
                    return Ok(new { success = true, message = "Message sent successfully.", messageId = outgoingMessage.Id });
                }
                else
                {
                    _logger.LogWarning("Failed to send manual message to {ToWaId}", request.ToWaId);
                    return StatusCode(500, "Failed to send message via WhatsApp API.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to {ToWaId}: {ErrorMessage}", request.ToWaId, ex.Message);
                return StatusCode(500, "Internal server error while sending message.");
            }
        }

        [HttpPost("send-reply")]
        public async Task<IActionResult> SendReply([FromBody] SendReplyRequest request)
        {
            if (request.ContactId <= 0 || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Contact ID and message are required.");
            }

            try
            {
                _logger.LogInformation("Sending reply to contact {ContactId}: {Message}", request.ContactId, request.Message);

                // Get the contact to find the WaId
                var contacts = await _chatRepository.GetContactsAsync();
                var contact = contacts.FirstOrDefault(c => c.Id == request.ContactId);
                
                if (contact == null)
                {
                    return NotFound($"Contact with ID {request.ContactId} not found.");
                }

                bool sent = await _whatsAppService.SendMessageAsync(contact.WaId, request.Message);
                
                if (sent)
                {
                    // Save outgoing message to DB
                    var outgoingMessage = new Message
                    {
                        WaMessageId = $"agent-reply-{Guid.NewGuid()}",
                        ContactId = contact.Id,
                        Body = request.Message,
                        IsFromMe = true,
                        Timestamp = DateTime.UtcNow,
                        Status = "sent"
                    };
                    await _chatRepository.SaveMessageAsync(outgoingMessage);

                    // Update contact's last message timestamp
                    contact.LastMessageTimestamp = DateTime.UtcNow;
                    await _chatRepository.UpdateContactAsync(contact);

                    _logger.LogInformation("Reply sent successfully to contact {ContactId}", request.ContactId);
                    return Ok(new { success = true, message = "Reply sent successfully.", messageId = outgoingMessage.Id });
                }
                else
                {
                    _logger.LogWarning("Failed to send reply to contact {ContactId}", request.ContactId);
                    return StatusCode(500, "Failed to send message via WhatsApp API.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reply to contact {ContactId}: {ErrorMessage}", request.ContactId, ex.Message);
                return StatusCode(500, "Internal server error while sending reply.");
            }
        }

        [HttpGet("logs")]
        public async Task<ActionResult<IEnumerable<AutomatedResponseLog>>> GetAutomatedResponseLogs([FromQuery] int limit = 100)
        {
            try
            {
                var logs = await _chatRepository.GetAutomatedResponseLogsAsync(limit);
                _logger.LogInformation("Retrieved {Count} automated response logs", logs.Count());
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automated response logs: {ErrorMessage}", ex.Message);
                return StatusCode(500, "Internal server error while retrieving logs.");
            }
        }

        [HttpGet("logs/{id}")]
        public async Task<ActionResult<AutomatedResponseLog>> GetAutomatedResponseLog(int id)
        {
            try
            {
                var log = await _chatRepository.GetAutomatedResponseLogByIdAsync(id);
                if (log == null)
                {
                    return NotFound($"Automated response log with ID {id} not found.");
                }
                return Ok(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automated response log {Id}: {ErrorMessage}", id, ex.Message);
                return StatusCode(500, "Internal server error while retrieving log.");
            }
        }

        [HttpGet("contact/{waId}")]
        public async Task<ActionResult<Contact>> GetContactByWaId(string waId)
        {
            try
            {
                var contact = await _chatRepository.GetOrCreateContactAsync(waId, waId);
                return Ok(contact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact by WaId {WaId}: {ErrorMessage}", waId, ex.Message);
                return StatusCode(500, "Internal server error while retrieving contact.");
            }
        }

        [HttpPut("contact/{id}")]
        public async Task<IActionResult> UpdateContact(int id, [FromBody] UpdateContactRequest request)
        {
            try
            {
                // First, get the existing contact
                var contacts = await _chatRepository.GetContactsAsync();
                var contact = contacts.FirstOrDefault(c => c.Id == id);
                
                if (contact == null)
                {
                    return NotFound($"Contact with ID {id} not found.");
                }

                // Update the contact properties
                if (!string.IsNullOrEmpty(request.DisplayName))
                    contact.DisplayName = request.DisplayName;
                
                if (!string.IsNullOrEmpty(request.ExtractedUserName))
                    contact.ExtractedUserName = request.ExtractedUserName;

                await _chatRepository.UpdateContactAsync(contact);
                
                _logger.LogInformation("Updated contact {Id} with new information", id);
                return Ok(new { success = true, message = "Contact updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact {Id}: {ErrorMessage}", id, ex.Message);
                return StatusCode(500, "Internal server error while updating contact.");
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetStats()
        {
            try
            {
                var contacts = await _chatRepository.GetContactsAsync();
                var logs = await _chatRepository.GetAutomatedResponseLogsAsync(1000); // Get more for stats

                var stats = new
                {
                    TotalContacts = contacts.Count(),
                    TotalAutomatedResponses = logs.Count(),
                    SuccessfulResponses = logs.Count(l => l.Status == "Sent"),
                    FailedResponses = logs.Count(l => l.Status == "Failed"),
                    AverageProcessingTime = logs.Any() ? logs.Average(l => l.ProcessingDurationMs) : 0,
                    AverageAiCallTime = logs.Where(l => l.AiApiCallDurationMs.HasValue).Any() 
                        ? logs.Where(l => l.AiApiCallDurationMs.HasValue).Average(l => l.AiApiCallDurationMs!.Value) 
                        : 0,
                    LastResponseTime = logs.Any() ? logs.Max(l => l.ResponseSentTime) : (DateTime?)null,
                    ResponsesLast24Hours = logs.Count(l => l.ResponseSentTime >= DateTime.UtcNow.AddDays(-1))
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats: {ErrorMessage}", ex.Message);
                return StatusCode(500, "Internal server error while retrieving stats.");
            }
        }

        [HttpPost("resend-message")]
        public async Task<IActionResult> ResendMessage([FromBody] ResendMessageRequest request)
        {
            try
            {
                _logger.LogInformation("Resending message for log {LogId}", request.LogId);

                // Get the original log
                var log = await _chatRepository.GetAutomatedResponseLogByIdAsync(request.LogId);
                if (log == null)
                {
                    return NotFound($"Log with ID {request.LogId} not found.");
                }

                // Determine message to send
                var messageToSend = !string.IsNullOrEmpty(request.CustomMessage) 
                    ? request.CustomMessage 
                    : log.FullResponseText;

                // Send the message
                bool sent = await _whatsAppService.SendMessageAsync(log.ContactWaId, messageToSend);
                
                if (sent)
                {
                    _logger.LogInformation("Message resent successfully for log {LogId}", request.LogId);
                    return Ok(new { success = true, message = "Message resent successfully." });
                }
                else
                {
                    _logger.LogWarning("Failed to resend message for log {LogId}", request.LogId);
                    return StatusCode(500, "Failed to resend message via WhatsApp API.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending message for log {LogId}: {ErrorMessage}", request.LogId, ex.Message);
                return StatusCode(500, "Internal server error while resending message.");
            }
        }

        [HttpPost("review-log")]
        public async Task<IActionResult> ReviewLog([FromBody] ReviewLogRequest request)
        {
            try
            {
                _logger.LogInformation("Reviewing log {LogId} with status {Status}", request.LogId, request.Status);

                // In a real app, you would save this to a reviews table
                // For demo purposes, we'll just log it
                var reviewData = new
                {
                    LogId = request.LogId,
                    Status = request.Status,
                    Notes = request.Notes,
                    Reviewer = request.Reviewer,
                    ReviewedAt = request.ReviewedAt
                };

                _logger.LogInformation("Log {LogId} reviewed successfully by {Reviewer}", request.LogId, request.Reviewer);
                return Ok(new { success = true, message = "Log reviewed successfully.", review = reviewData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing log {LogId}: {ErrorMessage}", request.LogId, ex.Message);
                return StatusCode(500, "Internal server error while reviewing log.");
            }
        }

        [HttpPost("export-log")]
        public async Task<IActionResult> ExportLog([FromBody] ExportLogRequest request)
        {
            try
            {
                _logger.LogInformation("Exporting data for log {LogId} in {Format} format", request.LogId, request.Format);

                // Get the log data
                var log = await _chatRepository.GetAutomatedResponseLogByIdAsync(request.LogId);
                if (log == null)
                {
                    return NotFound($"Log with ID {request.LogId} not found.");
                }

                // Create export data based on options
                var exportData = new
                {
                    LogId = log.Id,
                    ContactWaId = log.ContactWaId,
                    RequestReceivedTime = log.RequestReceivedTime,
                    ResponseSentTime = log.ResponseSentTime,
                    Status = log.Status,
                    FullResponseText = request.Options.IncludeResponseText ? log.FullResponseText : null,
                    AiExtractedData = request.Options.IncludeAiData ? log.AiExtractedData : null,
                    ProcessingDurationMs = request.Options.IncludePerformanceData ? log.ProcessingDurationMs : (int?)null,
                    AiApiCallDurationMs = request.Options.IncludePerformanceData ? log.AiApiCallDurationMs : null,
                    TourDetails = request.Options.IncludeTourDetails ? new
                    {
                        TemplateUsed = log.TemplateUsed,
                        GuideNameUsed = log.GuideNameUsed,
                        TourLocationUsed = log.TourLocationUsed,
                        TourTimeUsed = log.TourTimeUsed,
                        GuideNumberUsed = log.GuideNumberUsed
                    } : null,
                    ExportedAt = DateTime.UtcNow,
                    ExportedBy = request.Options.ExportedBy
                };

                // In a real app, you would generate the actual file based on format
                // For demo purposes, we'll return the data as JSON
                var jsonData = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
                var bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

                _logger.LogInformation("Data exported successfully for log {LogId}", request.LogId);
                return File(bytes, "application/json", $"log-{request.LogId}-export.{request.Format}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data for log {LogId}: {ErrorMessage}", request.LogId, ex.Message);
                return StatusCode(500, "Internal server error while exporting data.");
            }
        }

        [HttpPost("contact-customer")]
        public async Task<IActionResult> ContactCustomer([FromBody] ContactCustomerRequest request)
        {
            try
            {
                _logger.LogInformation("Contacting customer {ContactWaId} via {Method}", request.ContactWaId, request.Method);

                switch (request.Method.ToLower())
                {
                    case "whatsapp":
                        if (!string.IsNullOrEmpty(request.Message))
                        {
                            bool sent = await _whatsAppService.SendMessageAsync(request.ContactWaId, request.Message);
                            if (sent)
                            {
                                _logger.LogInformation("WhatsApp message sent successfully to {ContactWaId}", request.ContactWaId);
                                return Ok(new { success = true, message = "WhatsApp message sent successfully." });
                            }
                            else
                            {
                                return StatusCode(500, "Failed to send WhatsApp message.");
                            }
                        }
                        else
                        {
                            return BadRequest("Message is required for WhatsApp contact method.");
                        }

                    case "phone":
                        // In a real app, you would integrate with a phone system
                        _logger.LogInformation("Phone call initiated for {ContactWaId}", request.ContactWaId);
                        return Ok(new { success = true, message = "Phone call initiated successfully." });

                    case "guide":
                        // In a real app, you would forward to the tour guide
                        _logger.LogInformation("Contact forwarded to tour guide for {ContactWaId}", request.ContactWaId);
                        return Ok(new { success = true, message = "Contact forwarded to tour guide successfully." });

                    default:
                        return BadRequest($"Unsupported contact method: {request.Method}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contacting customer {ContactWaId}: {ErrorMessage}", request.ContactWaId, ex.Message);
                return StatusCode(500, "Internal server error while contacting customer.");
            }
        }
    }

    // Helper classes for incoming requests
    public class SendMessageRequest
    {
        public string ToWaId { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
    }

    public class SendReplyRequest
    {
        public int ContactId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class UpdateContactRequest
    {
        public string? DisplayName { get; set; }
        public string? ExtractedUserName { get; set; }
    }

    public class ResendMessageRequest
    {
        public int LogId { get; set; }
        public string? CustomMessage { get; set; }
    }

    public class ReviewLogRequest
    {
        public int LogId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Reviewer { get; set; } = string.Empty;
        public DateTime ReviewedAt { get; set; }
    }

    public class ExportLogRequest
    {
        public int LogId { get; set; }
        public string Format { get; set; } = string.Empty;
        public ExportLogOptions Options { get; set; } = new ExportLogOptions();
    }

    public class ExportLogOptions
    {
        public bool IncludeResponseText { get; set; } = true;
        public bool IncludeAiData { get; set; } = true;
        public bool IncludePerformanceData { get; set; } = true;
        public bool IncludeTourDetails { get; set; } = true;
        public string ExportedBy { get; set; } = string.Empty;
    }

    public class ContactCustomerRequest
    {
        public string ContactWaId { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
} 