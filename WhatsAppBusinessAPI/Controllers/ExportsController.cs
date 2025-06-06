using Microsoft.AspNetCore.Mvc;
using WhatsAppBusinessAPI.Models;
using WhatsAppBusinessAPI.Repositories;
using System.Text;
using System.Text.Json;
using Dapper;

namespace WhatsAppBusinessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportsController : ControllerBase
    {
        private readonly ChatRepository _chatRepository;
        private readonly ILogger<ExportsController> _logger;

        public ExportsController(ChatRepository chatRepository, ILogger<ExportsController> logger)
        {
            _chatRepository = chatRepository;
            _logger = logger;
        }

        // GET: api/exports/tours
        [HttpGet("tours")]
        public async Task<IActionResult> ExportTours([FromQuery] ExportRequest request)
        {
            try
            {
                var tourData = await _chatRepository.GetTourExportDataAsync(request.FromDate, request.ToDate, request.TourTypeFilter);

                return request.Format.ToLower() switch
                {
                    "csv" => File(GenerateToursCsv(tourData, request), "text/csv", $"tours_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"),
                    "json" => Ok(tourData),
                    "guide-messages" => File(GenerateGuideMessages(tourData), "text/plain", $"guide_messages_{DateTime.Now:yyyyMMdd_HHmmss}.txt"),
                    _ => BadRequest("Unsupported format. Use 'csv', 'json', or 'guide-messages'.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting tours");
                return StatusCode(500, "Internal server error while exporting tours.");
            }
        }

        // GET: api/exports/contacts
        [HttpGet("contacts")]
        public async Task<IActionResult> ExportContacts([FromQuery] ExportRequest request)
        {
            try
            {
                var contactData = await _chatRepository.GetContactExportDataAsync(request.FromDate, request.ToDate);

                return request.Format.ToLower() switch
                {
                    "csv" => File(GenerateContactsCsv(contactData), "text/csv", $"contacts_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"),
                    "json" => Ok(contactData),
                    _ => BadRequest("Unsupported format. Use 'csv' or 'json'.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting contacts");
                return StatusCode(500, "Internal server error while exporting contacts.");
            }
        }

        // GET: api/exports/guide-messages
        [HttpGet("guide-messages")]
        public async Task<IActionResult> ExportGuideMessages([FromQuery] ExportRequest request)
        {
            try
            {
                var tourData = await _chatRepository.GetTourExportDataAsync(request.FromDate, request.ToDate, request.TourTypeFilter);
                var guideMessages = GenerateGuideMessagesData(tourData);

                return request.Format.ToLower() switch
                {
                    "csv" => File(GenerateGuideMessagesCsv(guideMessages), "text/csv", $"guide_messages_{DateTime.Now:yyyyMMdd_HHmmss}.csv"),
                    "txt" => File(GenerateGuideMessages(tourData), "text/plain", $"guide_messages_{DateTime.Now:yyyyMMdd_HHmmss}.txt"),
                    "json" => Ok(guideMessages),
                    _ => BadRequest("Unsupported format. Use 'csv', 'txt', or 'json'.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting guide messages");
                return StatusCode(500, "Internal server error while exporting guide messages.");
            }
        }

        // GET: api/exports/messages
        [HttpGet("messages")]
        public async Task<IActionResult> ExportMessages([FromQuery] ExportRequest request, [FromQuery] string? contactFilter = null)
        {
            try
            {
                var messageData = await _chatRepository.GetMessageExportDataAsync(request.FromDate, request.ToDate, contactFilter);

                return request.Format.ToLower() switch
                {
                    "csv" => File(GenerateMessagesCsv(messageData), "text/csv", $"messages_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"),
                    "json" => Ok(messageData),
                    _ => BadRequest("Unsupported format. Use 'csv' or 'json'.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting messages");
                return StatusCode(500, "Internal server error while exporting messages.");
            }
        }

        // GET: api/exports/automated-responses
        [HttpGet("automated-responses")]
        public async Task<IActionResult> ExportAutomatedResponses([FromQuery] ExportRequest request)
        {
            try
            {
                var responseData = await _chatRepository.GetAutomatedResponseExportDataAsync(request.FromDate, request.ToDate);

                return request.Format.ToLower() switch
                {
                    "csv" => File(GenerateAutomatedResponsesCsv(responseData), "text/csv", $"automated_responses_{DateTime.Now:yyyyMMdd_HHmmss}.csv"),
                    "json" => Ok(responseData),
                    _ => BadRequest("Unsupported format. Use 'csv' or 'json'.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting automated responses");
                return StatusCode(500, "Internal server error while exporting automated responses.");
            }
        }

        private byte[] GenerateToursCsv(IEnumerable<TourExportData> tours, ExportRequest request)
        {
            var csv = new StringBuilder();
            
            if (request.GroupByTour)
            {
                // Group by tour with participants
                csv.AppendLine("Tour Type,Date,Time,Guide Name,Guide Phone,Meeting Location,Participant Name,Participant Phone,Booking Time,Number of People");
                
                foreach (var tour in tours)
                {
                    if (tour.Participants.Any())
                    {
                        foreach (var participant in tour.Participants)
                        {
                            csv.AppendLine($"\"{tour.TourType}\",\"{tour.Date}\",\"{tour.TimeSlot}\",\"{tour.GuideName}\",\"{tour.GuidePhoneNumber}\",\"{tour.MeetingLocation}\",\"{participant.Name}\",\"{participant.PhoneNumber}\",\"{participant.BookingTime:yyyy-MM-dd HH:mm}\",{participant.NumberOfPeople}");
                        }
                    }
                    else
                    {
                        csv.AppendLine($"\"{tour.TourType}\",\"{tour.Date}\",\"{tour.TimeSlot}\",\"{tour.GuideName}\",\"{tour.GuidePhoneNumber}\",\"{tour.MeetingLocation}\",\"No participants\",\"\",\"\",0");
                    }
                }
            }
            else
            {
                // Summary view
                csv.AppendLine("Tour Type,Date,Time,Guide Name,Guide Phone,Meeting Location,Total Participants");
                
                foreach (var tour in tours)
                {
                    csv.AppendLine($"\"{tour.TourType}\",\"{tour.Date}\",\"{tour.TimeSlot}\",\"{tour.GuideName}\",\"{tour.GuidePhoneNumber}\",\"{tour.MeetingLocation}\",{tour.TotalParticipants}");
                }
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private byte[] GenerateContactsCsv(IEnumerable<ContactExportData> contacts)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Name,Phone Number,WhatsApp ID,Last Contact,Last Tour Type,Last Tour Date,Last Tour Time,Total Messages,Status");

            foreach (var contact in contacts)
            {
                csv.AppendLine($"\"{contact.Name}\",\"{contact.PhoneNumber}\",\"{contact.WhatsAppId}\",\"{contact.LastContact:yyyy-MM-dd HH:mm}\",\"{contact.LastTourType}\",\"{contact.LastTourDate}\",\"{contact.LastTourTime}\",{contact.TotalMessages},\"{contact.Status}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private IEnumerable<GuideMessageExport> GenerateGuideMessagesData(IEnumerable<TourExportData> tours)
        {
            return tours.Select(tour => new GuideMessageExport
            {
                GuideName = tour.GuideName,
                GuidePhoneNumber = tour.GuidePhoneNumber,
                TourType = tour.TourType,
                Date = tour.Date,
                TimeSlot = tour.TimeSlot,
                MeetingLocation = tour.MeetingLocation,
                Participants = tour.Participants.ToList(),
                FormattedMessage = GenerateGuideMessage(tour)
            });
        }

        private byte[] GenerateGuideMessagesCsv(IEnumerable<GuideMessageExport> guideMessages)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Guide Name,Guide Phone,Tour Type,Date,Time,Meeting Location,Participants Count,Formatted Message");

            foreach (var message in guideMessages)
            {
                var formattedMessage = message.FormattedMessage.Replace("\"", "\"\"").Replace("\n", " | ");
                csv.AppendLine($"\"{message.GuideName}\",\"{message.GuidePhoneNumber}\",\"{message.TourType}\",\"{message.Date}\",\"{message.TimeSlot}\",\"{message.MeetingLocation}\",{message.Participants.Count},\"{formattedMessage}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private byte[] GenerateGuideMessages(IEnumerable<TourExportData> tours)
        {
            var messages = new StringBuilder();
            
            foreach (var tour in tours.Where(t => t.Participants.Any()))
            {
                messages.AppendLine($"=== MESSAGE FOR {tour.GuideName.ToUpper()} ===");
                messages.AppendLine($"Tour: {tour.TourType}");
                messages.AppendLine($"Date: {tour.Date}");
                messages.AppendLine($"Time: {tour.TimeSlot}");
                messages.AppendLine($"Location: {tour.MeetingLocation}");
                messages.AppendLine($"Your Phone: {tour.GuidePhoneNumber}");
                messages.AppendLine();
                messages.AppendLine("PARTICIPANTS:");
                
                foreach (var participant in tour.Participants)
                {
                    messages.AppendLine($"• {participant.Name} - {participant.PhoneNumber} ({participant.NumberOfPeople} people)");
                }
                
                messages.AppendLine();
                messages.AppendLine("COPY/PASTE MESSAGE TO SEND:");
                messages.AppendLine("---");
                messages.AppendLine(GenerateGuideMessage(tour));
                messages.AppendLine("---");
                messages.AppendLine();
                messages.AppendLine("".PadRight(50, '='));
                messages.AppendLine();
            }

            return Encoding.UTF8.GetBytes(messages.ToString());
        }

        private string GenerateGuideMessage(TourExportData tour)
        {
            var participantNames = string.Join(", ", tour.Participants.Select(p => p.Name));
            var totalPeople = tour.Participants.Sum(p => p.NumberOfPeople);
            
            return $@"Hi {tour.GuideName}! 

You have a {tour.TourType} scheduled for {tour.Date} at {tour.TimeSlot}.

Meeting Location: {tour.MeetingLocation}
Look for: {tour.IdentifiableObject}

Participants ({totalPeople} people):
{string.Join("\n", tour.Participants.Select(p => $"• {p.Name} - {p.PhoneNumber} ({p.NumberOfPeople} people)"))}

Please confirm you received this and are ready for the tour. Contact the office if you have any questions!

Thanks!";
        }

        private byte[] GenerateMessagesCsv(IEnumerable<MessageExportData> messages)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Message ID,WhatsApp ID,Contact ID,Contact Name,Message Text,Direction,Timestamp,Status,Type");

            foreach (var message in messages)
            {
                var messageText = message.MessageText.Replace("\"", "\"\"").Replace("\n", " | ");
                csv.AppendLine($"{message.MessageId},\"{message.WaMessageId}\",\"{message.ContactWaId}\",\"{message.ContactName}\",\"{messageText}\",\"{message.Direction}\",\"{message.MessageTimestamp:yyyy-MM-dd HH:mm:ss}\",\"{message.MessageStatus}\",\"{message.MessageType}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private byte[] GenerateAutomatedResponsesCsv(IEnumerable<AutomatedResponseExportData> responses)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Log ID,Contact ID,Contact Name,Request Time,Response Time,Processing Time (ms),AI Call Time (ms),Template,Company,Guide,Location,Tour Time,Status,Response Text");

            foreach (var response in responses)
            {
                var responseText = response.FullResponseText.Replace("\"", "\"\"").Replace("\n", " | ");
                csv.AppendLine($"{response.LogId},\"{response.ContactWaId}\",\"{response.ContactName}\",\"{response.RequestReceivedTime:yyyy-MM-dd HH:mm:ss}\",\"{response.ResponseSentTime:yyyy-MM-dd HH:mm:ss}\",{response.ProcessingDurationMs},{response.AiApiCallDurationMs ?? 0},\"{response.TemplateUsed}\",\"{response.CompanyNameUsed}\",\"{response.GuideNameUsed}\",\"{response.TourLocationUsed}\",\"{response.TourTimeUsed}\",\"{response.Status}\",\"{responseText}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }
    }
} 