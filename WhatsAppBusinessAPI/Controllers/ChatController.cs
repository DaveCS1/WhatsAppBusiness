using Microsoft.AspNetCore.Mvc;
using WhatsAppBusinessAPI.Models;
using WhatsAppBusinessAPI.Repositories;
using WhatsAppBusinessAPI.Services;

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
    }

    // Helper classes for incoming requests
    public class SendMessageRequest
    {
        public string ToWaId { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
    }

    public class UpdateContactRequest
    {
        public string? DisplayName { get; set; }
        public string? ExtractedUserName { get; set; }
    }
} 