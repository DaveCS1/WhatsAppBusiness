namespace WhatsAppBusinessAPI.Models
{
    public class MessageTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TemplateText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "TourConfirmation", "General", "NoMatch", etc.
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public string? PlaceholderVariables { get; set; } // JSON array of available placeholders
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class TemplateVariable
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DefaultValue { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = false;
    }
} 