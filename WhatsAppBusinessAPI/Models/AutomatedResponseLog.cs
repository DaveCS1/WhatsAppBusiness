namespace WhatsAppBusinessAPI.Models
{
    public class AutomatedResponseLog
    {
        public int Id { get; set; }
        public int? IncomingMessageId { get; set; } // FK to Messages table (nullable for system-generated responses)
        public string ContactWaId { get; set; } = string.Empty; // WhatsApp ID for easy reference
        public DateTime RequestReceivedTime { get; set; } // When the incoming message was received
        public DateTime ResponseSentTime { get; set; } // When the automated response was sent
        public int ProcessingDurationMs { get; set; } // Total processing time in milliseconds
        public int? AiApiCallDurationMs { get; set; } // Time spent calling AI API (nullable)
        public string? TemplateUsed { get; set; } // Which response template was used
        public string? CompanyNameUsed { get; set; } // Company name used in response
        public string? GuideNameUsed { get; set; } // Guide name used in response
        public string? TourLocationUsed { get; set; } // Meeting location used in response
        public string? TourTimeUsed { get; set; } // Tour time used in response
        public string? IdentifiableObjectUsed { get; set; } // Identifiable object used in response
        public string? GuideNumberUsed { get; set; } // Guide phone number used in response
        public string FullResponseText { get; set; } = string.Empty; // Complete automated response text
        public string Status { get; set; } = string.Empty; // "Sent", "Failed", "Pending"
        public string? ErrorMessage { get; set; } // Error details if Status = "Failed"
        public string? AiExtractedData { get; set; } // JSON of AI-extracted information
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 