namespace WhatsAppBusinessBlazorClient.Models
{
    public class MessageTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TemplateText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public string? PlaceholderVariables { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class TemplateVariable
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DefaultValue { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = false;
    }

    public class ExportRequest
    {
        public string ExportType { get; set; } = string.Empty;
        public string Format { get; set; } = "CSV";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? TourTypeFilter { get; set; }
        public string? GuideFilter { get; set; }
        public bool IncludeParticipants { get; set; } = true;
        public bool GroupByTour { get; set; } = true;
        public bool FormatForGuides { get; set; } = false;
    }
} 