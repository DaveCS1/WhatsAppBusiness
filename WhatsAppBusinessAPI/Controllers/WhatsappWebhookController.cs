using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WhatsAppBusinessAPI.Repositories;
using WhatsAppBusinessAPI.Services;
using WhatsAppBusinessAPI.Models;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace WhatsAppBusinessAPI.Controllers
{
    [Route("api/whatsapp")]
    [ApiController]
    public class WhatsappWebhookController : ControllerBase
    {
        private readonly ChatRepository _chatRepository;
        private readonly WhatsAppService _whatsAppService;
        private readonly AiExtractionService _aiExtractionService;
        private readonly TourPresetsService _tourPresetsService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsappWebhookController> _logger;

        public WhatsappWebhookController(
            ChatRepository chatRepository,
            WhatsAppService whatsAppService,
            AiExtractionService aiExtractionService,
            TourPresetsService tourPresetsService,
            IConfiguration configuration,
            ILogger<WhatsappWebhookController> logger)
        {
            _chatRepository = chatRepository;
            _whatsAppService = whatsAppService;
            _aiExtractionService = aiExtractionService;
            _tourPresetsService = tourPresetsService;
            _configuration = configuration;
            _logger = logger;
        }

        // GET method for Webhook Verification
        [HttpGet("webhook")]
        public IActionResult GetWebhook([FromQuery(Name = "hub.mode")] string hubMode,
                                        [FromQuery(Name = "hub.challenge")] string hubChallenge,
                                        [FromQuery(Name = "hub.verify_token")] string hubVerifyToken)
        {
            _logger.LogInformation("Webhook verification request received. Mode: {HubMode}, Challenge: {HubChallenge}", 
                hubMode, hubChallenge);

            var verifyToken = _configuration["WhatsApp:VerifyToken"];

            if (hubMode == "subscribe" && hubVerifyToken == verifyToken)
            {
                _logger.LogInformation("Webhook verified successfully.");
                return Ok(hubChallenge);
            }
            else
            {
                _logger.LogWarning("Webhook verification failed: Invalid mode or verify token. Expected: {ExpectedToken}, Received: {ReceivedToken}", 
                    verifyToken, hubVerifyToken);
                return Forbid("Invalid verify token");
            }
        }

        // POST method for Incoming Messages and Status Updates
        [HttpPost("webhook")]
        public async Task<IActionResult> PostWebhook()
        {
            DateTime requestReceivedTime = DateTime.UtcNow;
            int? aiApiCallDurationMs = null;
            string contactWaId = "N/A";
            int incomingMessageDbId = 0;
            string automatedResponseStatus = "Failed";
            string? errorMessage = null;
            string fullAutomatedResponseText = "Automated response generation failed.";
            string templateUsed = "TourConfirmation";
            string? companyNameUsed = null;
            string? guideNameUsed = null;
            string? tourLocationUsed = null;
            string? tourTimeUsed = null;
            string? identifiableObjectUsed = null;
            string? guideNumberUsed = null;
            string? aiExtractedData = null;

            try
            {
                var rawPayload = await new StreamReader(Request.Body).ReadToEndAsync();
                _logger.LogInformation("Received webhook payload: {RawPayload}", rawPayload);

                // Optional: X-Hub-Signature-256 verification (recommended for production)
                var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
                if (!string.IsNullOrEmpty(signature))
                {
                    var appSecret = _configuration["WhatsApp:AppSecret"];
                    if (!string.IsNullOrEmpty(appSecret) && !VerifySignature(rawPayload, signature, appSecret))
                    {
                        _logger.LogWarning("Webhook signature verification failed.");
                        return Forbid("Invalid signature");
                    }
                }

                using (JsonDocument document = JsonDocument.Parse(rawPayload))
                {
                    var root = document.RootElement;
                    
                    if (root.TryGetProperty("entry", out JsonElement entryArray) && entryArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var entry in entryArray.EnumerateArray())
                        {
                            if (entry.TryGetProperty("changes", out JsonElement changesArray) && changesArray.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var change in changesArray.EnumerateArray())
                                {
                                    if (change.TryGetProperty("field", out JsonElement fieldElement) && fieldElement.GetString() == "messages")
                                    {
                                        if (change.TryGetProperty("value", out JsonElement valueElement))
                                        {
                                            // Handle incoming messages
                                            if (valueElement.TryGetProperty("messages", out JsonElement messagesArray) && messagesArray.ValueKind == JsonValueKind.Array)
                                            {
                                                foreach (var messageJson in messagesArray.EnumerateArray())
                                                {
                                                    await ProcessIncomingMessage(messageJson, requestReceivedTime);
                                                }
                                            }

                                            // Handle status updates
                                            if (valueElement.TryGetProperty("statuses", out JsonElement statusesArray) && statusesArray.ValueKind == JsonValueKind.Array)
                                            {
                                                foreach (var statusJson in statusesArray.EnumerateArray())
                                                {
                                                    await ProcessStatusUpdate(statusJson);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.LogError(ex, "Error processing WhatsApp webhook: {ErrorMessage}", ex.Message);
                return StatusCode(500, "Internal server error processing webhook.");
            }
        }

        private async Task ProcessIncomingMessage(JsonElement messageJson, DateTime requestReceivedTime)
        {
            int? aiApiCallDurationMs = null;
            string contactWaId = "N/A";
            int incomingMessageDbId = 0;
            string automatedResponseStatus = "Failed";
            string? errorMessage = null;
            string fullAutomatedResponseText = "Automated response generation failed.";
            string templateUsed = "TourConfirmation";
            string? companyNameUsed = null;
            string? guideNameUsed = null;
            string? tourLocationUsed = null;
            string? tourTimeUsed = null;
            string? identifiableObjectUsed = null;
            string? guideNumberUsed = null;
            string? aiExtractedData = null;

            try
            {
                string messageType = messageJson.GetProperty("type").GetString() ?? "unknown";
                
                if (messageType == "text")
                {
                    string from = messageJson.GetProperty("from").GetString() ?? "";
                    string messageBody = messageJson.GetProperty("text").GetProperty("body").GetString() ?? "";
                    string waMessageId = messageJson.GetProperty("id").GetString() ?? "";
                    long timestamp = messageJson.GetProperty("timestamp").GetInt64();
                    DateTime messageTimestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;

                    contactWaId = from;

                    _logger.LogInformation("Processing text message from {From}: {MessageBody}", from, messageBody);

                    // 1. Get or Create Contact
                    var contact = await _chatRepository.GetOrCreateContactAsync(from, from);
                    contact.LastMessageTimestamp = messageTimestamp;

                    // 2. Save incoming message to DB
                    var incomingMessage = new Message
                    {
                        WaMessageId = waMessageId,
                        ContactId = contact.Id,
                        Body = messageBody,
                        IsFromMe = false,
                        Timestamp = messageTimestamp,
                        Status = "received"
                    };
                    incomingMessageDbId = await _chatRepository.SaveMessageAsync(incomingMessage);
                    _logger.LogInformation("Incoming message saved with ID: {MessageId}", incomingMessageDbId);

                    // 3. AI Extraction
                    DateTime aiStartTime = DateTime.UtcNow;
                    var extractedInfo = await _aiExtractionService.ExtractUserInfoAsync(messageBody);
                    aiApiCallDurationMs = (int)(DateTime.UtcNow - aiStartTime).TotalMilliseconds;

                    if (extractedInfo != null)
                    {
                        contact.ExtractedUserName = extractedInfo.UserName != "N/A" ? extractedInfo.UserName : contact.DisplayName;
                        contact.LastExtractedTourType = extractedInfo.TourType;
                        contact.LastExtractedTourDate = extractedInfo.TourDate;
                        contact.LastExtractedTourTime = extractedInfo.TourTime;
                        await _chatRepository.UpdateContactAsync(contact);

                        aiExtractedData = JsonSerializer.Serialize(extractedInfo);
                        _logger.LogInformation("AI extracted info for {From}: {ExtractedInfo}", from, aiExtractedData);
                    }

                    // 4. Retrieve Tour Presets
                    var tourDetails = await _tourPresetsService.FindBestMatchAsync(
                        extractedInfo?.TourType,
                        extractedInfo?.TourDate,
                        extractedInfo?.TourTime
                    );

                    // 5. Construct Templated Response
                    companyNameUsed = _configuration["WhatsApp:CompanyName"] ?? "NYC Adventure Tours";
                    
                    if (tourDetails != null)
                    {
                        guideNameUsed = tourDetails.GuideName;
                        tourLocationUsed = tourDetails.MeetingLocation;
                        tourTimeUsed = tourDetails.TimeSlot;
                        identifiableObjectUsed = tourDetails.IdentifiableObject;
                        guideNumberUsed = tourDetails.GuidePhoneNumber;

                        fullAutomatedResponseText = _tourPresetsService.GenerateTourResponseMessage(tourDetails, companyNameUsed);
                    }
                    else
                    {
                        fullAutomatedResponseText = $"Hello! Thank you for contacting {companyNameUsed}. We've received your message and will get back to you shortly with tour information.";
                    }

                    // 6. Send WhatsApp Response
                    bool sent = await _whatsAppService.SendMessageAsync(from, fullAutomatedResponseText);
                    automatedResponseStatus = sent ? "Sent" : "Failed";

                    if (!sent)
                    {
                        errorMessage = "Failed to send WhatsApp message";
                    }

                    // 7. Save Outgoing Message to DB
                    var outgoingMessage = new Message
                    {
                        WaMessageId = $"auto-reply-{Guid.NewGuid()}",
                        ContactId = contact.Id,
                        Body = fullAutomatedResponseText,
                        IsFromMe = true,
                        Timestamp = DateTime.UtcNow,
                        Status = sent ? "sent" : "failed"
                    };
                    await _chatRepository.SaveMessageAsync(outgoingMessage);

                    // 8. Mark original message as read
                    await _whatsAppService.MarkMessageAsReadAsync(waMessageId);

                    _logger.LogInformation("Automated response {Status} for message from {From}", automatedResponseStatus, from);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.LogError(ex, "Error processing incoming message from {ContactWaId}: {ErrorMessage}", contactWaId, ex.Message);
            }
            finally
            {
                // Always log the automated response attempt
                await LogAutomatedResponse(
                    incomingMessageDbId, contactWaId, requestReceivedTime, DateTime.UtcNow,
                    aiApiCallDurationMs, templateUsed, companyNameUsed, guideNameUsed,
                    tourLocationUsed, tourTimeUsed, identifiableObjectUsed, guideNumberUsed,
                    fullAutomatedResponseText, automatedResponseStatus, errorMessage, aiExtractedData);
            }
        }

        private async Task ProcessStatusUpdate(JsonElement statusJson)
        {
            try
            {
                string status = statusJson.GetProperty("status").GetString() ?? "";
                string messageId = statusJson.GetProperty("id").GetString() ?? "";
                string recipientId = statusJson.GetProperty("recipient_id").GetString() ?? "";

                _logger.LogInformation("WhatsApp message {MessageId} status updated to {Status} for recipient {RecipientId}", 
                    messageId, status, recipientId);

                // Update message status in database
                await _chatRepository.UpdateMessageStatusAsync(messageId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing status update: {ErrorMessage}", ex.Message);
            }
        }

        private async Task LogAutomatedResponse(
            int incomingMessageId, string contactWaId, DateTime requestReceivedTime, DateTime responseSentTime,
            int? aiApiCallDurationMs, string templateUsed, string? companyNameUsed, string? guideNameUsed,
            string? tourLocationUsed, string? tourTimeUsed, string? identifiableObjectUsed, string? guideNumberUsed,
            string fullResponseText, string status, string? errorMessage, string? aiExtractedData)
        {
            try
            {
                var processingDuration = (int)(responseSentTime - requestReceivedTime).TotalMilliseconds;

                var logEntry = new AutomatedResponseLog
                {
                    IncomingMessageId = incomingMessageId > 0 ? incomingMessageId : null,
                    ContactWaId = contactWaId,
                    RequestReceivedTime = requestReceivedTime,
                    ResponseSentTime = responseSentTime,
                    ProcessingDurationMs = processingDuration,
                    AiApiCallDurationMs = aiApiCallDurationMs,
                    TemplateUsed = templateUsed,
                    CompanyNameUsed = companyNameUsed ?? "N/A",
                    GuideNameUsed = guideNameUsed ?? "N/A",
                    TourLocationUsed = tourLocationUsed ?? "N/A",
                    TourTimeUsed = tourTimeUsed ?? "N/A",
                    IdentifiableObjectUsed = identifiableObjectUsed ?? "N/A",
                    GuideNumberUsed = guideNumberUsed ?? "N/A",
                    FullResponseText = fullResponseText,
                    Status = status,
                    ErrorMessage = errorMessage,
                    AiExtractedData = aiExtractedData
                };

                await _chatRepository.LogAutomatedResponseAsync(logEntry);
                _logger.LogDebug("Automated response logged successfully for contact {ContactWaId}", contactWaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log automated response for contact {ContactWaId}: {ErrorMessage}", 
                    contactWaId, ex.Message);
            }
        }

        private bool VerifySignature(string payload, string signature, string appSecret)
        {
            try
            {
                if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(appSecret)) 
                    return false;

                var elements = signature.Split('=');
                if (elements.Length != 2 || elements[0] != "sha256") 
                    return false;

                var hash = Convert.FromHexString(elements[1]);

                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret)))
                {
                    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                    return computedHash.SequenceEqual(hash);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying webhook signature: {ErrorMessage}", ex.Message);
                return false;
            }
        }
    }
} 