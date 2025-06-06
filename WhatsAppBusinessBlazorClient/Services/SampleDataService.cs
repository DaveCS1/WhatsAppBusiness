using WhatsAppBusinessBlazorClient.Models;

namespace WhatsAppBusinessBlazorClient.Services
{
    public class SampleDataService
    {
        private readonly List<Contact> _sampleContacts;
        private readonly List<Message> _sampleMessages;
        private readonly List<AutomatedResponseLog> _sampleLogs;

        public SampleDataService()
        {
            _sampleContacts = GenerateSampleContacts();
            _sampleMessages = GenerateSampleMessages();
            _sampleLogs = GenerateSampleLogs();
        }

        public async Task<List<Contact>> GetContactsAsync()
        {
            await Task.Delay(100); // Simulate API delay
            return _sampleContacts.OrderByDescending(c => c.LastMessageTimestamp).ToList();
        }

        public async Task<List<Message>> GetMessagesForContactAsync(int contactId)
        {
            await Task.Delay(100); // Simulate API delay
            return _sampleMessages
                .Where(m => m.ContactId == contactId)
                .OrderBy(m => m.Timestamp)
                .ToList();
        }

        public async Task<List<AutomatedResponseLog>> GetAutomatedResponseLogsAsync()
        {
            await Task.Delay(100); // Simulate API delay
            return _sampleLogs.OrderByDescending(l => l.RequestReceivedTime).ToList();
        }

        public async Task<Dictionary<string, object>> GetSystemStatsAsync()
        {
            await Task.Delay(100); // Simulate API delay
            
            var stats = new Dictionary<string, object>
            {
                ["TotalContacts"] = _sampleContacts.Count,
                ["TotalMessages"] = _sampleMessages.Count,
                ["AutomatedResponsesToday"] = _sampleLogs.Count(l => l.RequestReceivedTime.Date == DateTime.Today),
                ["TotalAutomatedResponses"] = _sampleLogs.Count,
                ["SuccessfulResponses"] = _sampleLogs.Count(l => l.Status == "Sent"),
                ["SuccessRate"] = _sampleLogs.Count > 0 ? (double)_sampleLogs.Count(l => l.Status == "Sent") / _sampleLogs.Count * 100 : 0,
                ["AverageProcessingTimeMs"] = _sampleLogs.Count > 0 ? _sampleLogs.Average(l => l.ProcessingDurationMs) : 0
            };

            return stats;
        }

        public async Task<bool> SendMessageAsync(int contactId, string message)
        {
            await Task.Delay(200); // Simulate API delay
            
            var contact = _sampleContacts.FirstOrDefault(c => c.Id == contactId);
            if (contact == null) return false;

            var newMessage = new Message
            {
                Id = _sampleMessages.Count + 1,
                WaMessageId = Guid.NewGuid().ToString(),
                ContactId = contactId,
                Body = message,
                IsFromMe = true,
                Timestamp = DateTime.Now,
                Status = "sent",
                MessageType = "text"
            };

            _sampleMessages.Add(newMessage);
            contact.LastMessageTimestamp = DateTime.Now;

            return true;
        }

        private List<Contact> GenerateSampleContacts()
        {
            return new List<Contact>
            {
                new Contact
                {
                    Id = 1,
                    WaId = "1234567890",
                    DisplayName = "John Smith",
                    LastMessageTimestamp = DateTime.Now.AddMinutes(-5),
                    ExtractedUserName = "John",
                    LastExtractedTourType = "Food Tour",
                    LastExtractedTourDate = "tomorrow",
                    LastExtractedTourTime = "2 PM",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    UpdatedAt = DateTime.Now.AddMinutes(-5)
                },
                new Contact
                {
                    Id = 2,
                    WaId = "0987654321",
                    DisplayName = "Sarah Johnson",
                    LastMessageTimestamp = DateTime.Now.AddMinutes(-15),
                    ExtractedUserName = "Sarah",
                    LastExtractedTourType = "Walking Tour",
                    LastExtractedTourDate = "weekend",
                    LastExtractedTourTime = "morning",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now.AddMinutes(-15)
                },
                new Contact
                {
                    Id = 3,
                    WaId = "5555555555",
                    DisplayName = "Mike Chen",
                    LastMessageTimestamp = DateTime.Now.AddHours(-2),
                    ExtractedUserName = "Mike",
                    LastExtractedTourType = "Historical Tour",
                    LastExtractedTourDate = "Friday",
                    LastExtractedTourTime = "6 PM",
                    CreatedAt = DateTime.Now.AddDays(-3),
                    UpdatedAt = DateTime.Now.AddHours(-2)
                },
                new Contact
                {
                    Id = 4,
                    WaId = "7777777777",
                    DisplayName = "Emma Wilson",
                    LastMessageTimestamp = DateTime.Now.AddHours(-4),
                    ExtractedUserName = "Emma",
                    LastExtractedTourType = "Art Tour",
                    LastExtractedTourDate = "today",
                    LastExtractedTourTime = "afternoon",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now.AddHours(-4)
                },
                new Contact
                {
                    Id = 5,
                    WaId = "9999999999",
                    DisplayName = "David Brown",
                    LastMessageTimestamp = DateTime.Now.AddDays(-1),
                    ExtractedUserName = "David",
                    LastExtractedTourType = "Photography Tour",
                    LastExtractedTourDate = "Monday",
                    LastExtractedTourTime = "9 AM",
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UpdatedAt = DateTime.Now.AddDays(-1)
                }
            };
        }

        private List<Message> GenerateSampleMessages()
        {
            return new List<Message>
            {
                // John Smith conversation
                new Message { Id = 1, WaMessageId = "msg1", ContactId = 1, Body = "Hi! I'm interested in a food tour for tomorrow around 2 PM. My name is John.", IsFromMe = false, Timestamp = DateTime.Now.AddMinutes(-10), Status = "delivered" },
                new Message { Id = 2, WaMessageId = "msg2", ContactId = 1, Body = "Hello John! Thank you for booking your tour with NYC Adventure Tours.\n\nYour tour guide Maria will meet you at Little Italy at 2 PM. Look for a small sign with our company logo.\n\nIf you need to reach your guide directly, you can contact them at: (555) 123-4567\n\nOur food tour will take you through the best authentic Italian restaurants and hidden gems in the neighborhood. You'll taste amazing pasta, pizza, and gelato while learning about the area's rich history.\n\nWe look forward to showing you an amazing time! If you have any questions before your tour, feel free to reach out.", IsFromMe = true, Timestamp = DateTime.Now.AddMinutes(-9), Status = "sent" },
                new Message { Id = 3, WaMessageId = "msg3", ContactId = 1, Body = "Perfect! Thank you so much. Looking forward to it!", IsFromMe = false, Timestamp = DateTime.Now.AddMinutes(-5), Status = "delivered" },

                // Sarah Johnson conversation
                new Message { Id = 4, WaMessageId = "msg4", ContactId = 2, Body = "Hello, I'd like to book a walking tour for this weekend morning. I'm Sarah.", IsFromMe = false, Timestamp = DateTime.Now.AddMinutes(-20), Status = "delivered" },
                new Message { Id = 5, WaMessageId = "msg5", ContactId = 2, Body = "Hello Sarah! Thank you for booking your tour with NYC Adventure Tours.\n\nYour tour guide Tom will meet you at Central Park Entrance at 9 AM. Look for a small sign with our company logo.\n\nIf you need to reach your guide directly, you can contact them at: (555) 234-5678\n\nOur walking tour covers the most scenic routes through Central Park and surrounding neighborhoods. You'll discover hidden paths, historical landmarks, and beautiful viewpoints perfect for photos.\n\nWe look forward to showing you an amazing time! If you have any questions before your tour, feel free to reach out.", IsFromMe = true, Timestamp = DateTime.Now.AddMinutes(-18), Status = "sent" },
                new Message { Id = 6, WaMessageId = "msg6", ContactId = 2, Body = "Sounds great! What should I bring?", IsFromMe = false, Timestamp = DateTime.Now.AddMinutes(-15), Status = "delivered" },

                // Mike Chen conversation
                new Message { Id = 7, WaMessageId = "msg7", ContactId = 3, Body = "Hi there! I'm Mike and I'm interested in a historical tour on Friday evening around 6 PM.", IsFromMe = false, Timestamp = DateTime.Now.AddHours(-3), Status = "delivered" },
                new Message { Id = 8, WaMessageId = "msg8", ContactId = 3, Body = "Hello Mike! Thank you for booking your tour with NYC Adventure Tours.\n\nYour tour guide Professor Williams will meet you at City Hall at 6 PM. Look for a small sign with our company logo.\n\nIf you need to reach your guide directly, you can contact them at: (555) 345-6789\n\nOur historical tour will take you through centuries of New York history, from colonial times to modern day. You'll visit significant landmarks and hear fascinating stories about the people who shaped this great city.\n\nWe look forward to showing you an amazing time! If you have any questions before your tour, feel free to reach out.", IsFromMe = true, Timestamp = DateTime.Now.AddHours(-2), Status = "sent" },

                // Emma Wilson conversation
                new Message { Id = 9, WaMessageId = "msg9", ContactId = 4, Body = "Hello! I'm Emma and I'd love to do an art tour today in the afternoon if possible.", IsFromMe = false, Timestamp = DateTime.Now.AddHours(-5), Status = "delivered" },
                new Message { Id = 10, WaMessageId = "msg10", ContactId = 4, Body = "Hello Emma! Thank you for booking your tour with NYC Adventure Tours.\n\nYour tour guide Isabella will meet you at MoMA Entrance at 2 PM. Look for a small sign with our company logo.\n\nIf you need to reach your guide directly, you can contact them at: (555) 456-7890\n\nOur art tour includes visits to world-renowned museums and galleries, showcasing everything from classical masterpieces to contemporary installations. You'll gain insights into artistic techniques and the stories behind famous works.\n\nWe look forward to showing you an amazing time! If you have any questions before your tour, feel free to reach out.", IsFromMe = true, Timestamp = DateTime.Now.AddHours(-4), Status = "sent" },

                // David Brown conversation
                new Message { Id = 11, WaMessageId = "msg11", ContactId = 5, Body = "Hi! I'm David and I'm interested in a photography tour on Monday morning at 9 AM.", IsFromMe = false, Timestamp = DateTime.Now.AddDays(-1).AddHours(-1), Status = "delivered" },
                new Message { Id = 12, WaMessageId = "msg12", ContactId = 5, Body = "Hello David! Thank you for booking your tour with NYC Adventure Tours.\n\nYour tour guide Alex will meet you at Brooklyn Bridge at 9 AM. Look for a small sign with our company logo.\n\nIf you need to reach your guide directly, you can contact them at: (555) 567-8901\n\nOur photography tour will take you to the most Instagram-worthy spots in the city. You'll learn professional photography techniques while capturing stunning shots of iconic landmarks and hidden gems.\n\nWe look forward to showing you an amazing time! If you have any questions before your tour, feel free to reach out.", IsFromMe = true, Timestamp = DateTime.Now.AddDays(-1), Status = "sent" }
            };
        }

        private List<AutomatedResponseLog> GenerateSampleLogs()
        {
            return new List<AutomatedResponseLog>
            {
                new AutomatedResponseLog
                {
                    Id = 1,
                    IncomingMessageId = 1,
                    ContactWaId = "1234567890",
                    RequestReceivedTime = DateTime.Now.AddMinutes(-10),
                    ResponseSentTime = DateTime.Now.AddMinutes(-9),
                    ProcessingDurationMs = 1250,
                    AiApiCallDurationMs = 800,
                    TemplateUsed = "Food Tour Template",
                    CompanyNameUsed = "NYC Adventure Tours",
                    GuideNameUsed = "Maria",
                    TourLocationUsed = "Little Italy",
                    TourTimeUsed = "2 PM",
                    IdentifiableObjectUsed = "a small sign with our company logo",
                    GuideNumberUsed = "(555) 123-4567",
                    FullResponseText = "Hello John! Thank you for booking your tour with NYC Adventure Tours...",
                    Status = "Sent",
                    AiExtractedData = "{\"userName\":\"John\",\"tourType\":\"Food Tour\",\"date\":\"tomorrow\",\"time\":\"2 PM\"}"
                },
                new AutomatedResponseLog
                {
                    Id = 2,
                    IncomingMessageId = 4,
                    ContactWaId = "0987654321",
                    RequestReceivedTime = DateTime.Now.AddMinutes(-20),
                    ResponseSentTime = DateTime.Now.AddMinutes(-18),
                    ProcessingDurationMs = 1100,
                    AiApiCallDurationMs = 750,
                    TemplateUsed = "Walking Tour Template",
                    CompanyNameUsed = "NYC Adventure Tours",
                    GuideNameUsed = "Tom",
                    TourLocationUsed = "Central Park Entrance",
                    TourTimeUsed = "9 AM",
                    IdentifiableObjectUsed = "a small sign with our company logo",
                    GuideNumberUsed = "(555) 234-5678",
                    FullResponseText = "Hello Sarah! Thank you for booking your tour with NYC Adventure Tours...",
                    Status = "Sent",
                    AiExtractedData = "{\"userName\":\"Sarah\",\"tourType\":\"Walking Tour\",\"date\":\"weekend\",\"time\":\"morning\"}"
                },
                new AutomatedResponseLog
                {
                    Id = 3,
                    IncomingMessageId = 7,
                    ContactWaId = "5555555555",
                    RequestReceivedTime = DateTime.Now.AddHours(-3),
                    ResponseSentTime = DateTime.Now.AddHours(-2),
                    ProcessingDurationMs = 1350,
                    AiApiCallDurationMs = 900,
                    TemplateUsed = "Historical Tour Template",
                    CompanyNameUsed = "NYC Adventure Tours",
                    GuideNameUsed = "Professor Williams",
                    TourLocationUsed = "City Hall",
                    TourTimeUsed = "6 PM",
                    IdentifiableObjectUsed = "a small sign with our company logo",
                    GuideNumberUsed = "(555) 345-6789",
                    FullResponseText = "Hello Mike! Thank you for booking your tour with NYC Adventure Tours...",
                    Status = "Sent",
                    AiExtractedData = "{\"userName\":\"Mike\",\"tourType\":\"Historical Tour\",\"date\":\"Friday\",\"time\":\"6 PM\"}"
                },
                new AutomatedResponseLog
                {
                    Id = 4,
                    IncomingMessageId = 9,
                    ContactWaId = "7777777777",
                    RequestReceivedTime = DateTime.Now.AddHours(-5),
                    ResponseSentTime = DateTime.Now.AddHours(-4),
                    ProcessingDurationMs = 1180,
                    AiApiCallDurationMs = 820,
                    TemplateUsed = "Art Tour Template",
                    CompanyNameUsed = "NYC Adventure Tours",
                    GuideNameUsed = "Isabella",
                    TourLocationUsed = "MoMA Entrance",
                    TourTimeUsed = "2 PM",
                    IdentifiableObjectUsed = "a small sign with our company logo",
                    GuideNumberUsed = "(555) 456-7890",
                    FullResponseText = "Hello Emma! Thank you for booking your tour with NYC Adventure Tours...",
                    Status = "Sent",
                    AiExtractedData = "{\"userName\":\"Emma\",\"tourType\":\"Art Tour\",\"date\":\"today\",\"time\":\"afternoon\"}"
                },
                new AutomatedResponseLog
                {
                    Id = 5,
                    IncomingMessageId = 11,
                    ContactWaId = "9999999999",
                    RequestReceivedTime = DateTime.Now.AddDays(-1).AddHours(-1),
                    ResponseSentTime = DateTime.Now.AddDays(-1),
                    ProcessingDurationMs = 1420,
                    AiApiCallDurationMs = 950,
                    TemplateUsed = "Photography Tour Template",
                    CompanyNameUsed = "NYC Adventure Tours",
                    GuideNameUsed = "Alex",
                    TourLocationUsed = "Brooklyn Bridge",
                    TourTimeUsed = "9 AM",
                    IdentifiableObjectUsed = "a small sign with our company logo",
                    GuideNumberUsed = "(555) 567-8901",
                    FullResponseText = "Hello David! Thank you for booking your tour with NYC Adventure Tours...",
                    Status = "Sent",
                    AiExtractedData = "{\"userName\":\"David\",\"tourType\":\"Photography Tour\",\"date\":\"Monday\",\"time\":\"9 AM\"}"
                }
            };
        }
    }
} 