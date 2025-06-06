using Microsoft.AspNetCore.Mvc;
using WhatsAppBusinessAPI.Services;
using WhatsAppBusinessAPI.Repositories;

namespace WhatsAppBusinessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ChatRepository _chatRepository;
        private readonly AiExtractionService _aiExtractionService;
        private readonly TourPresetsService _tourPresetsService;
        private readonly ILogger<TestController> _logger;

        public TestController(
            ChatRepository chatRepository,
            AiExtractionService aiExtractionService,
            TourPresetsService tourPresetsService,
            ILogger<TestController> logger)
        {
            _chatRepository = chatRepository;
            _aiExtractionService = aiExtractionService;
            _tourPresetsService = tourPresetsService;
            _logger = logger;
        }

        [HttpPost("simulate-message")]
        public async Task<IActionResult> SimulateMessage([FromBody] SimulateMessageRequest request)
        {
            try
            {
                _logger.LogInformation("Simulating message from {ContactName}: {Message}", 
                    request.ContactName, request.Message);

                // Create or get contact
                var contact = await _chatRepository.GetOrCreateContactAsync(
                    request.ContactId, request.ContactName);

                // Save incoming message
                var incomingMessage = new Models.Message
                {
                    WaMessageId = Guid.NewGuid().ToString(),
                    ContactId = contact.Id,
                    Body = request.Message,
                    IsFromMe = false,
                    Timestamp = DateTime.UtcNow,
                    Status = "received",
                    MessageType = "text"
                };

                var messageId = await _chatRepository.SaveMessageAsync(incomingMessage);

                // Extract information using AI
                var extractedData = await _aiExtractionService.ExtractUserInfoAsync(request.Message);

                // Update contact with extracted information
                if (extractedData != null)
                {
                    contact.ExtractedUserName = extractedData.UserName;
                    contact.LastExtractedTourType = extractedData.TourType;
                    contact.LastExtractedTourDate = extractedData.TourDate;
                    contact.LastExtractedTourTime = extractedData.TourTime;
                    await _chatRepository.UpdateContactAsync(contact);
                }

                // Find best matching tour
                var tourDetails = await _tourPresetsService.FindBestMatchAsync(
                    extractedData?.TourType, extractedData?.TourDate, extractedData?.TourTime);

                // Generate response
                string responseText = "Thank you for your message! We'll get back to you soon.";
                if (tourDetails != null)
                {
                    responseText = _tourPresetsService.GenerateTourResponseMessage(tourDetails, "NYC Adventure Tours");
                }

                // Save automated response
                var responseMessage = new Models.Message
                {
                    WaMessageId = Guid.NewGuid().ToString(),
                    ContactId = contact.Id,
                    Body = responseText,
                    IsFromMe = true,
                    Timestamp = DateTime.UtcNow,
                    Status = "sent",
                    MessageType = "text"
                };

                await _chatRepository.SaveMessageAsync(responseMessage);

                return Ok(new
                {
                    success = true,
                    message = "Message processed successfully",
                    extractedData = extractedData,
                    tourMatched = tourDetails?.TourType,
                    responseText = responseText
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating message");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("sample-messages")]
        public IActionResult GetSampleMessages()
        {
            var sampleMessages = new[]
            {
                new { contactId = "test001", contactName = "Alice Johnson", message = "Hi! I'm Alice and I'd love to book a food tour for tomorrow around 2 PM." },
                new { contactId = "test002", contactName = "Bob Smith", message = "Hello, my name is Bob. I'm interested in a walking tour this weekend morning." },
                new { contactId = "test003", contactName = "Carol Davis", message = "Hi there! I'm Carol and I want to do a historical tour on Friday evening." },
                new { contactId = "test004", contactName = "David Wilson", message = "Hello! I'm David. Can I book an art tour for today afternoon?" },
                new { contactId = "test005", contactName = "Eva Brown", message = "Hi! I'm Eva and I'd like a photography tour on Monday at 9 AM." }
            };

            return Ok(sampleMessages);
        }

        [HttpPost("create-sample-data")]
        public async Task<IActionResult> CreateSampleData()
        {
            try
            {
                _logger.LogInformation("Creating sample data for testing");

                // Create sample contacts and conversations
                var sampleConversations = new[]
                {
                    new { contactId = "1234567890", contactName = "John Smith", 
                          messages = new[] {
                              new { body = "Hi! I'm John and I'd love to book a food tour for tomorrow around 2 PM.", isFromMe = false, timestamp = DateTime.UtcNow.AddHours(-2) },
                              new { body = "Hello John! Thank you for booking your tour with NYC Adventure Tours.\n\nYour tour guide Maria will meet you at Greenwich Village (Washington Square Park) at 11 AM. Look for a green tote bag with \"Foodie Tours\" logo.\n\nIf you need to reach your guide directly, you can contact them at: +1-555-0201\n\nOur food tour will take you through the best restaurants in Greenwich Village. You'll taste amazing cuisine while learning about the area's rich history.\n\nWe look forward to showing you an amazing time! If you have any questions before your tour, feel free to reach out.", isFromMe = true, timestamp = DateTime.UtcNow.AddHours(-2).AddMinutes(2) },
                              new { body = "Perfect! Thank you so much. Looking forward to it!", isFromMe = false, timestamp = DateTime.UtcNow.AddHours(-1) }
                          }
                    },
                    new { contactId = "0987654321", contactName = "Sarah Johnson", 
                          messages = new[] {
                              new { body = "Hello, I'd like to book a walking tour for this weekend morning. I'm Sarah.", isFromMe = false, timestamp = DateTime.UtcNow.AddHours(-3) },
                              new { body = "Hello Sarah! Thank you for booking your tour with NYC Adventure Tours.\n\nYour tour guide Alice will meet you at Central Park Entrance (59th St & 5th Ave) at 9 AM. Look for a red umbrella and \"NYC Tours\" sign.\n\nIf you need to reach your guide directly, you can contact them at: +1-555-0101\n\nOur walking tour covers the most scenic routes through Central Park. You'll discover hidden paths, historical landmarks, and beautiful viewpoints perfect for photos.\n\nWe look forward to showing you an amazing time!", isFromMe = true, timestamp = DateTime.UtcNow.AddHours(-3).AddMinutes(3) },
                              new { body = "Sounds great! What should I bring?", isFromMe = false, timestamp = DateTime.UtcNow.AddHours(-2).AddMinutes(-30) }
                          }
                    },
                    new { contactId = "5555555555", contactName = "Mike Chen", 
                          messages = new[] {
                              new { body = "Hi there! I'm Mike and I want to do a historical tour on Friday evening.", isFromMe = false, timestamp = DateTime.UtcNow.AddHours(-5) },
                              new { body = "Hello Mike! Thank you for booking your tour with NYC Adventure Tours.\n\nYour tour guide Sarah will meet you at Brooklyn Bridge (Manhattan side entrance) at 10 AM. Look for a yellow folder and \"History Walks\" badge.\n\nIf you need to reach your guide directly, you can contact them at: +1-555-0301\n\nOur historical tour will take you through centuries of New York history, from colonial times to modern day. You'll visit significant landmarks and hear fascinating stories.\n\nWe look forward to showing you an amazing time!", isFromMe = true, timestamp = DateTime.UtcNow.AddHours(-5).AddMinutes(5) }
                          }
                    }
                };

                foreach (var conversation in sampleConversations)
                {
                    // Create or get contact
                    var contact = await _chatRepository.GetOrCreateContactAsync(conversation.contactId, conversation.contactName);
                    
                    // Add messages
                    foreach (var msg in conversation.messages)
                    {
                        var message = new Models.Message
                        {
                            WaMessageId = Guid.NewGuid().ToString(),
                            ContactId = contact.Id,
                            Body = msg.body,
                            IsFromMe = msg.isFromMe,
                            Timestamp = msg.timestamp,
                            Status = msg.isFromMe ? "sent" : "delivered",
                            MessageType = "text"
                        };

                        await _chatRepository.SaveMessageAsync(message);
                    }

                    // Update contact with extracted info (simulate AI extraction)
                    if (conversation.contactName == "John Smith")
                    {
                        contact.ExtractedUserName = "John";
                        contact.LastExtractedTourType = "Food Tour";
                        contact.LastExtractedTourDate = "tomorrow";
                        contact.LastExtractedTourTime = "2 PM";
                    }
                    else if (conversation.contactName == "Sarah Johnson")
                    {
                        contact.ExtractedUserName = "Sarah";
                        contact.LastExtractedTourType = "Walking Tour";
                        contact.LastExtractedTourDate = "weekend";
                        contact.LastExtractedTourTime = "morning";
                    }
                    else if (conversation.contactName == "Mike Chen")
                    {
                        contact.ExtractedUserName = "Mike";
                        contact.LastExtractedTourType = "Historical Tour";
                        contact.LastExtractedTourDate = "Friday";
                        contact.LastExtractedTourTime = "evening";
                    }

                    contact.LastMessageTimestamp = conversation.messages.Max(m => m.timestamp);
                    await _chatRepository.UpdateContactAsync(contact);
                }

                return Ok(new { success = true, message = "Sample data created successfully", contactsCreated = sampleConversations.Length });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sample data");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("reset-data")]
        public async Task<IActionResult> ResetTestData()
        {
            try
            {
                // This would reset test data - implement if needed
                _logger.LogInformation("Test data reset requested");
                
                return Ok(new { success = true, message = "Test data reset (not implemented)" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting test data");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }

    public class SimulateMessageRequest
    {
        public string ContactId { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
} 