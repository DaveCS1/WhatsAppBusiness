namespace WhatsAppBusinessBlazorClient.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string WaMessageId { get; set; } = string.Empty;
        public int ContactId { get; set; }
        public string Body { get; set; } = string.Empty;
        public bool IsFromMe { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MessageType { get; set; } = "text";
        
        // Navigation property
        public Contact? Contact { get; set; }
    }
} 