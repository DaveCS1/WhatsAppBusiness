namespace WhatsAppBusinessBlazorClient.Models
{
    public class AutomatedResponseLog
    {
        public int Id { get; set; }
        public int IncomingMessageId { get; set; }
        public string ContactWaId { get; set; } = string.Empty;
        public DateTime RequestReceivedTime { get; set; }
        public DateTime? ResponseSentTime { get; set; }
        public int ProcessingDurationMs { get; set; }
        public int AiApiCallDurationMs { get; set; }
        public string TemplateUsed { get; set; } = string.Empty;
        public string CompanyNameUsed { get; set; } = string.Empty;
        public string GuideNameUsed { get; set; } = string.Empty;
        public string TourLocationUsed { get; set; } = string.Empty;
        public string TourTimeUsed { get; set; } = string.Empty;
        public string IdentifiableObjectUsed { get; set; } = string.Empty;
        public string GuideNumberUsed { get; set; } = string.Empty;
        public string FullResponseText { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? AiExtractedData { get; set; }
    }
} 