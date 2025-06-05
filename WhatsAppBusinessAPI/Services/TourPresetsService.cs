using WhatsAppBusinessAPI.Models;
using WhatsAppBusinessAPI.Repositories;

namespace WhatsAppBusinessAPI.Services
{
    public class TourPresetsService
    {
        private readonly ChatRepository _chatRepository;
        private readonly ILogger<TourPresetsService> _logger;

        public TourPresetsService(ChatRepository chatRepository, ILogger<TourPresetsService> logger)
        {
            _chatRepository = chatRepository;
            _logger = logger;
        }

        public async Task<TourDetails?> GetTourDetailsAsync(string? tourType, string? date, string? timeSlot)
        {
            try
            {
                _logger.LogInformation("Searching for tour details - Type: {TourType}, Date: {Date}, Time: {TimeSlot}", 
                    tourType, date, timeSlot);

                // Use the repository's best match method which handles fuzzy matching
                var tourDetails = await _chatRepository.GetBestMatchTourDetailsAsync(tourType, date, timeSlot);

                if (tourDetails != null)
                {
                    _logger.LogInformation("Found matching tour: {TourType} on {Date} at {TimeSlot} with guide {GuideName}", 
                        tourDetails.TourType, tourDetails.Date, tourDetails.TimeSlot, tourDetails.GuideName);
                    return tourDetails;
                }

                _logger.LogWarning("No tour details found for Type: {TourType}, Date: {Date}, Time: {TimeSlot}. Using fallback.", 
                    tourType, date, timeSlot);

                // Return a fallback tour if no match found
                return GetFallbackTourDetails();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tour details for Type: {TourType}, Date: {Date}, Time: {TimeSlot}", 
                    tourType, date, timeSlot);
                return GetFallbackTourDetails();
            }
        }

        public async Task<TourDetails?> GetTourDetailsByIdAsync(int id)
        {
            try
            {
                var tourDetails = await _chatRepository.GetTourDetailsByIdAsync(id);
                if (tourDetails != null)
                {
                    _logger.LogInformation("Retrieved tour details by ID {Id}: {TourType}", id, tourDetails.TourType);
                }
                else
                {
                    _logger.LogWarning("No tour details found for ID: {Id}", id);
                }
                return tourDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tour details by ID: {Id}", id);
                return null;
            }
        }

        public async Task<IEnumerable<TourDetails>> GetAllActiveTourDetailsAsync()
        {
            try
            {
                var tours = await _chatRepository.GetAllActiveTourDetailsAsync();
                _logger.LogInformation("Retrieved {Count} active tour details", tours.Count());
                return tours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all active tour details");
                return Enumerable.Empty<TourDetails>();
            }
        }

        public async Task<TourDetails?> FindBestMatchAsync(string? extractedTourType, string? extractedDate, string? extractedTime)
        {
            try
            {
                // Normalize the extracted data for better matching
                var normalizedTourType = NormalizeTourType(extractedTourType);
                var normalizedDate = NormalizeDate(extractedDate);
                var normalizedTime = NormalizeTime(extractedTime);

                _logger.LogInformation("Normalized search criteria - Type: {TourType}, Date: {Date}, Time: {Time}", 
                    normalizedTourType, normalizedDate, normalizedTime);

                return await GetTourDetailsAsync(normalizedTourType, normalizedDate, normalizedTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding best tour match for extracted data");
                return GetFallbackTourDetails();
            }
        }

        private string? NormalizeTourType(string? tourType)
        {
            if (string.IsNullOrEmpty(tourType) || tourType.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return null;

            // Convert common variations to standard tour types
            var normalized = tourType.ToLowerInvariant().Trim();
            
            return normalized switch
            {
                var t when t.Contains("walk") => "Walking Tour",
                var t when t.Contains("food") || t.Contains("eat") || t.Contains("culinary") => "Food Tour",
                var t when t.Contains("history") || t.Contains("historical") => "Historical Tour",
                var t when t.Contains("art") || t.Contains("museum") => "Art Tour",
                var t when t.Contains("photo") => "Photography Tour",
                var t when t.Contains("night") => "Night Tour",
                var t when t.Contains("bike") || t.Contains("cycling") => "Bike Tour",
                _ => tourType // Return original if no match
            };
        }

        private string? NormalizeDate(string? date)
        {
            if (string.IsNullOrEmpty(date) || date.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return null;

            var normalized = date.ToLowerInvariant().Trim();
            
            return normalized switch
            {
                var d when d.Contains("today") => "today",
                var d when d.Contains("tomorrow") => "tomorrow",
                var d when d.Contains("weekend") => "weekend",
                var d when d.Contains("monday") => "Monday",
                var d when d.Contains("tuesday") => "Tuesday",
                var d when d.Contains("wednesday") => "Wednesday",
                var d when d.Contains("thursday") => "Thursday",
                var d when d.Contains("friday") => "Friday",
                var d when d.Contains("saturday") => "Saturday",
                var d when d.Contains("sunday") => "Sunday",
                _ => date // Return original if no match
            };
        }

        private string? NormalizeTime(string? time)
        {
            if (string.IsNullOrEmpty(time) || time.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return null;

            var normalized = time.ToLowerInvariant().Trim();
            
            return normalized switch
            {
                var t when t.Contains("morning") || t.Contains("9") || t.Contains("10") => "9 AM",
                var t when t.Contains("lunch") || t.Contains("noon") || t.Contains("12") => "12 PM",
                var t when t.Contains("afternoon") || t.Contains("1") || t.Contains("2") => "2 PM",
                var t when t.Contains("evening") || t.Contains("6") || t.Contains("7") => "6 PM",
                var t when t.Contains("night") || t.Contains("8") || t.Contains("9") => "8 PM",
                _ => time // Return original if no match
            };
        }

        private TourDetails GetFallbackTourDetails()
        {
            return new TourDetails
            {
                Id = 0,
                TourType = "General Tour",
                Date = "tomorrow",
                TimeSlot = "9 AM",
                GuideName = "our friendly team",
                MeetingLocation = "your hotel lobby",
                IdentifiableObject = "a small sign with our company logo",
                GuidePhoneNumber = "Please contact our main office",
                IsActive = true,
                Description = "We'll arrange a wonderful tour experience for you!"
            };
        }

        public string GenerateTourResponseMessage(TourDetails tourDetails, string companyName)
        {
            try
            {
                var template = @"Hello! Thank you for booking your tour with {0}. 

Your tour guide {1} will meet you at {2} at {3}. Look for {4}.

If you need to reach your guide directly, you can contact them at: {5}

{6}

We look forward to showing you an amazing time! If you have any questions before your tour, feel free to reach out.

PS - We have many more tours available! Ask us about our other offerings including food tours, historical walks, and specialty experiences.

Have a wonderful day!
{0} Team";

                var message = string.Format(template,
                    companyName,
                    tourDetails.GuideName,
                    tourDetails.MeetingLocation,
                    tourDetails.TimeSlot,
                    tourDetails.IdentifiableObject,
                    tourDetails.GuidePhoneNumber,
                    !string.IsNullOrEmpty(tourDetails.Description) ? tourDetails.Description : "We're excited to share our city with you!"
                );

                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tour response message");
                return $"Thank you for booking with {companyName}! We'll send you tour details shortly.";
            }
        }
    }
} 