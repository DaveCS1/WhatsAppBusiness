namespace WhatsAppBusinessAPI.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string WaId { get; set; } = string.Empty; // User's WhatsApp phone number (e.g., "14155552671")
        public string? DisplayName { get; set; } // User's name from WhatsApp or extracted
        public DateTime LastMessageTimestamp { get; set; }
        public string? ExtractedUserName { get; set; } // AI-extracted user name
        public string? LastExtractedTourType { get; set; } // AI-extracted tour type
        public string? LastExtractedTourDate { get; set; } // AI-extracted tour date
        public string? LastExtractedTourTime { get; set; } // AI-extracted tour time
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 