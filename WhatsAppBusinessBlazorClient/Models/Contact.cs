namespace WhatsAppBusinessBlazorClient.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string WaId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime LastMessageTimestamp { get; set; }
        public string? ExtractedUserName { get; set; }
        public string? LastExtractedTourType { get; set; }
        public string? LastExtractedTourDate { get; set; }
        public string? LastExtractedTourTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 