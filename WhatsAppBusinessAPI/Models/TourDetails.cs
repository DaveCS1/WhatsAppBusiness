namespace WhatsAppBusinessAPI.Models
{
    public class TourDetails
    {
        public int Id { get; set; }
        public string TourType { get; set; } = string.Empty; // e.g., "Walking Tour", "Food Tour", "Historical Tour"
        public string? Date { get; set; } // e.g., "tomorrow", "2025-07-01", "weekends"
        public string? TimeSlot { get; set; } // e.g., "9 AM", "1 PM", "afternoon", "evening"
        public string GuideName { get; set; } = string.Empty; // Tour guide's name
        public string MeetingLocation { get; set; } = string.Empty; // Where to meet the guide
        public string IdentifiableObject { get; set; } = string.Empty; // What the guide will be holding/wearing
        public string GuidePhoneNumber { get; set; } = string.Empty; // Guide's contact number
        public bool IsActive { get; set; } = true; // Whether this tour slot is currently available
        public int MaxCapacity { get; set; } = 10; // Maximum number of people for this tour
        public decimal? Price { get; set; } // Tour price (optional)
        public string? Description { get; set; } // Additional tour details
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 