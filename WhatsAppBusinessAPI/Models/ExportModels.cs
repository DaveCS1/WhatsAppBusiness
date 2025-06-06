namespace WhatsAppBusinessAPI.Models
{
    public class TourExportData
    {
        public string TourType { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public string GuideName { get; set; } = string.Empty;
        public string GuidePhoneNumber { get; set; } = string.Empty;
        public string MeetingLocation { get; set; } = string.Empty;
        public string IdentifiableObject { get; set; } = string.Empty;
        public List<TourParticipant> Participants { get; set; } = new List<TourParticipant>();
        public int TotalParticipants => Participants.Count;
    }

    public class TourParticipant
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string WhatsAppId { get; set; } = string.Empty;
        public DateTime BookingTime { get; set; }
        public string? SpecialRequests { get; set; }
        public int NumberOfPeople { get; set; } = 1;
    }

    public class GuideMessageExport
    {
        public string GuideName { get; set; } = string.Empty;
        public string GuidePhoneNumber { get; set; } = string.Empty;
        public string TourType { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public string MeetingLocation { get; set; } = string.Empty;
        public List<TourParticipant> Participants { get; set; } = new List<TourParticipant>();
        public string FormattedMessage { get; set; } = string.Empty;
    }

    public class ContactExportData
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string WhatsAppId { get; set; } = string.Empty;
        public DateTime LastContact { get; set; }
        public string LastTourType { get; set; } = string.Empty;
        public string LastTourDate { get; set; } = string.Empty;
        public string LastTourTime { get; set; } = string.Empty;
        public int TotalMessages { get; set; }
        public string Status { get; set; } = string.Empty; // "Active", "Booked", "Completed", etc.
    }

    public class ExportRequest
    {
        public string ExportType { get; set; } = string.Empty; // "Tours", "Contacts", "GuideMessages"
        public string Format { get; set; } = "CSV"; // "CSV", "Excel", "JSON"
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? TourTypeFilter { get; set; }
        public string? GuideFilter { get; set; }
        public bool IncludeParticipants { get; set; } = true;
        public bool GroupByTour { get; set; } = true;
        public bool FormatForGuides { get; set; } = false;
    }

    public class MessageExportData
    {
        public int MessageId { get; set; }
        public string WaMessageId { get; set; } = string.Empty;
        public string ContactWaId { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public bool IsFromMe { get; set; }
        public DateTime MessageTimestamp { get; set; }
        public string MessageStatus { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
    }

    public class AutomatedResponseExportData
    {
        public int LogId { get; set; }
        public string ContactWaId { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public DateTime RequestReceivedTime { get; set; }
        public DateTime ResponseSentTime { get; set; }
        public int ProcessingDurationMs { get; set; }
        public int? AiApiCallDurationMs { get; set; }
        public string? TemplateUsed { get; set; }
        public string? CompanyNameUsed { get; set; }
        public string? GuideNameUsed { get; set; }
        public string? TourLocationUsed { get; set; }
        public string? TourTimeUsed { get; set; }
        public string? IdentifiableObjectUsed { get; set; }
        public string? GuideNumberUsed { get; set; }
        public string FullResponseText { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? AiExtractedData { get; set; }
    }
} 