namespace WhatsAppBusinessAPI.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string? WaMessageId { get; set; } // WhatsApp's unique message ID
        public int ContactId { get; set; } // Foreign Key to Contacts
        public string? Body { get; set; } // Message content/text
        public bool IsFromMe { get; set; } // True if sent by our app, false if from user
        public DateTime Timestamp { get; set; } // When the message was sent/received
        public string? Status { get; set; } // e.g., "sent", "delivered", "read", "failed"
        public string MessageType { get; set; } = "text"; // Type: "text", "image", "document", etc.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 