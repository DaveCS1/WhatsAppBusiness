using Dapper;
using Microsoft.Data.Sqlite;
using WhatsAppBusinessAPI.Models;
using System.Data;

namespace WhatsAppBusinessAPI.Repositories
{
    public class ChatRepository
    {
        private readonly string _connectionString;

        public ChatRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        private async Task<IDbConnection> GetOpenConnectionAsync()
        {
            var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<int> SaveMessageAsync(Message message)
        {
            using var connection = await GetOpenConnectionAsync();
            
            // Check if message with WaMessageId already exists
            var existingMessage = await connection.QueryFirstOrDefaultAsync<Message>(
                "SELECT Id FROM Messages WHERE WaMessageId = @WaMessageId", new { message.WaMessageId });

            if (existingMessage != null)
            {
                // Update existing message (e.g., status)
                await connection.ExecuteAsync(
                    "UPDATE Messages SET Status = @Status, Timestamp = @Timestamp, Body = @Body WHERE Id = @Id",
                    new { message.Status, message.Timestamp, message.Body, Id = existingMessage.Id });
                return existingMessage.Id;
            }
            else
            {
                // Insert new message
                var sql = @"
                    INSERT INTO Messages (WaMessageId, ContactId, Body, IsFromMe, Timestamp, Status, MessageType)
                    VALUES (@WaMessageId, @ContactId, @Body, @IsFromMe, @Timestamp, @Status, @MessageType);
                    SELECT last_insert_rowid();";
                return await connection.ExecuteScalarAsync<int>(sql, message);
            }
        }

        public async Task<IEnumerable<Message>> GetMessagesForContactAsync(int contactId)
        {
            using var connection = await GetOpenConnectionAsync();
            return await connection.QueryAsync<Message>(
                "SELECT * FROM Messages WHERE ContactId = @ContactId ORDER BY Timestamp ASC", new { ContactId = contactId });
        }

        public async Task<Contact> GetOrCreateContactAsync(string waId, string displayName)
        {
            using var connection = await GetOpenConnectionAsync();
            
            var contact = await connection.QueryFirstOrDefaultAsync<Contact>(
                "SELECT * FROM Contacts WHERE WaId = @WaId", new { WaId = waId });

            if (contact == null)
            {
                // Create new contact
                var sql = @"
                    INSERT INTO Contacts (WaId, DisplayName, LastMessageTimestamp, CreatedAt, UpdatedAt)
                    VALUES (@WaId, @DisplayName, @LastMessageTimestamp, @CreatedAt, @UpdatedAt);
                    SELECT last_insert_rowid();";
                var newContactId = await connection.ExecuteScalarAsync<int>(
                    sql, new { 
                        WaId = waId, 
                        DisplayName = displayName, 
                        LastMessageTimestamp = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                contact = new Contact { 
                    Id = newContactId, 
                    WaId = waId, 
                    DisplayName = displayName, 
                    LastMessageTimestamp = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            return contact;
        }

        public async Task UpdateContactAsync(Contact contact)
        {
            using var connection = await GetOpenConnectionAsync();
            var sql = @"
                UPDATE Contacts
                SET DisplayName = @DisplayName,
                    LastMessageTimestamp = @LastMessageTimestamp,
                    ExtractedUserName = @ExtractedUserName,
                    LastExtractedTourType = @LastExtractedTourType,
                    LastExtractedTourDate = @LastExtractedTourDate,
                    LastExtractedTourTime = @LastExtractedTourTime,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
            
            contact.UpdatedAt = DateTime.UtcNow;
            await connection.ExecuteAsync(sql, contact);
        }

        public async Task<IEnumerable<Contact>> GetContactsAsync()
        {
            using var connection = await GetOpenConnectionAsync();
            return await connection.QueryAsync<Contact>(
                "SELECT * FROM Contacts ORDER BY LastMessageTimestamp DESC");
        }

        public async Task LogAutomatedResponseAsync(AutomatedResponseLog log)
        {
            using var connection = await GetOpenConnectionAsync();
            var sql = @"
                INSERT INTO AutomatedResponseLog (
                    IncomingMessageId, ContactWaId, RequestReceivedTime, ResponseSentTime,
                    ProcessingDurationMs, AiApiCallDurationMs, TemplateUsed, CompanyNameUsed,
                    GuideNameUsed, TourLocationUsed, TourTimeUsed, IdentifiableObjectUsed,
                    GuideNumberUsed, FullResponseText, Status, ErrorMessage, AiExtractedData
                ) VALUES (
                    @IncomingMessageId, @ContactWaId, @RequestReceivedTime, @ResponseSentTime,
                    @ProcessingDurationMs, @AiApiCallDurationMs, @TemplateUsed, @CompanyNameUsed,
                    @GuideNameUsed, @TourLocationUsed, @TourTimeUsed, @IdentifiableObjectUsed,
                    @GuideNumberUsed, @FullResponseText, @Status, @ErrorMessage, @AiExtractedData
                )";
            await connection.ExecuteAsync(sql, log);
        }

        public async Task<TourDetails?> GetTourDetailsAsync(string tourType, string date, string timeSlot)
        {
            using var connection = await GetOpenConnectionAsync();
            return await connection.QueryFirstOrDefaultAsync<TourDetails>(
                "SELECT * FROM TourDetails WHERE TourType = @TourType AND Date = @Date AND TimeSlot = @TimeSlot AND IsActive = 1",
                new { TourType = tourType, Date = date, TimeSlot = timeSlot });
        }

        public async Task<TourDetails?> GetTourDetailsByIdAsync(int id)
        {
            using var connection = await GetOpenConnectionAsync();
            return await connection.QueryFirstOrDefaultAsync<TourDetails>(
                "SELECT * FROM TourDetails WHERE Id = @Id", new { id });
        }

        public async Task<TourDetails?> GetBestMatchTourDetailsAsync(string? tourType, string? date, string? timeSlot)
        {
            using var connection = await GetOpenConnectionAsync();
            
            // Try exact match first
            if (!string.IsNullOrEmpty(tourType) && !string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(timeSlot))
            {
                var exactMatch = await connection.QueryFirstOrDefaultAsync<TourDetails>(
                    "SELECT * FROM TourDetails WHERE TourType LIKE @TourType AND Date LIKE @Date AND TimeSlot LIKE @TimeSlot AND IsActive = 1",
                    new { TourType = $"%{tourType}%", Date = $"%{date}%", TimeSlot = $"%{timeSlot}%" });
                if (exactMatch != null) return exactMatch;
            }

            // Try partial matches
            if (!string.IsNullOrEmpty(tourType))
            {
                var typeMatch = await connection.QueryFirstOrDefaultAsync<TourDetails>(
                    "SELECT * FROM TourDetails WHERE TourType LIKE @TourType AND IsActive = 1 ORDER BY Id LIMIT 1",
                    new { TourType = $"%{tourType}%" });
                if (typeMatch != null) return typeMatch;
            }

            // Return first available tour as fallback
            return await connection.QueryFirstOrDefaultAsync<TourDetails>(
                "SELECT * FROM TourDetails WHERE IsActive = 1 ORDER BY Id LIMIT 1");
        }

        public async Task UpdateMessageStatusAsync(string waMessageId, string status)
        {
            using var connection = await GetOpenConnectionAsync();
            await connection.ExecuteAsync(
                "UPDATE Messages SET Status = @Status WHERE WaMessageId = @WaMessageId",
                new { Status = status, WaMessageId = waMessageId });
        }

        public async Task<IEnumerable<AutomatedResponseLog>> GetAutomatedResponseLogsAsync(int limit = 100)
        {
            using var connection = await GetOpenConnectionAsync();
            return await connection.QueryAsync<AutomatedResponseLog>(
                "SELECT * FROM AutomatedResponseLog ORDER BY RequestReceivedTime DESC LIMIT @Limit",
                new { Limit = limit });
        }

        public async Task<AutomatedResponseLog?> GetAutomatedResponseLogByIdAsync(int id)
        {
            using var connection = await GetOpenConnectionAsync();
            return await connection.QueryFirstOrDefaultAsync<AutomatedResponseLog>(
                "SELECT * FROM AutomatedResponseLog WHERE Id = @Id", new { id });
        }

        public async Task<IEnumerable<TourDetails>> GetAllActiveTourDetailsAsync()
        {
            using var connection = await GetOpenConnectionAsync();
            return await connection.QueryAsync<TourDetails>(
                "SELECT * FROM TourDetails WHERE IsActive = 1 ORDER BY TourType, Date, TimeSlot");
        }

        // Message Templates methods
        public async Task<IEnumerable<MessageTemplate>> GetMessageTemplatesAsync()
        {
            using var connection = await GetOpenConnectionAsync();
            return await connection.QueryAsync<MessageTemplate>(
                "SELECT * FROM MessageTemplates ORDER BY Category, Name");
        }

        public async Task<MessageTemplate?> GetMessageTemplateByIdAsync(int id)
        {
            using var connection = await GetOpenConnectionAsync();
            return await connection.QueryFirstOrDefaultAsync<MessageTemplate>(
                "SELECT * FROM MessageTemplates WHERE Id = @Id", new { Id = id });
        }

        public async Task<MessageTemplate?> GetDefaultTemplateByCategory(string category)
        {
            using var connection = await GetOpenConnectionAsync();
            return await connection.QueryFirstOrDefaultAsync<MessageTemplate>(
                "SELECT * FROM MessageTemplates WHERE Category = @Category AND IsDefault = 1 AND IsActive = 1", 
                new { Category = category });
        }

        public async Task<int> SaveMessageTemplateAsync(MessageTemplate template)
        {
            using var connection = await GetOpenConnectionAsync();
            
            if (template.Id == 0)
            {
                // Insert new template
                var sql = @"
                    INSERT INTO MessageTemplates (Name, Description, TemplateText, Category, IsActive, IsDefault, PlaceholderVariables)
                    VALUES (@Name, @Description, @TemplateText, @Category, @IsActive, @IsDefault, @PlaceholderVariables);
                    SELECT last_insert_rowid();";
                return await connection.QuerySingleAsync<int>(sql, template);
            }
            else
            {
                // Update existing template
                var sql = @"
                    UPDATE MessageTemplates 
                    SET Name = @Name, Description = @Description, TemplateText = @TemplateText, 
                        Category = @Category, IsActive = @IsActive, IsDefault = @IsDefault, 
                        PlaceholderVariables = @PlaceholderVariables, UpdatedAt = CURRENT_TIMESTAMP
                    WHERE Id = @Id";
                await connection.ExecuteAsync(sql, template);
                return template.Id;
            }
        }

        public async Task DeleteMessageTemplateAsync(int id)
        {
            using var connection = await GetOpenConnectionAsync();
            await connection.ExecuteAsync("DELETE FROM MessageTemplates WHERE Id = @Id", new { Id = id });
        }

        // Export methods
        public async Task<IEnumerable<TourExportData>> GetTourExportDataAsync(DateTime? fromDate = null, DateTime? toDate = null, string? tourTypeFilter = null)
        {
            using var connection = await GetOpenConnectionAsync();
            
            // Get all tours that have had bookings (from AutomatedResponseLog)
            var sql = @"
                SELECT DISTINCT 
                    COALESCE(arl.TourLocationUsed, td.MeetingLocation) as MeetingLocation,
                    COALESCE(arl.GuideNameUsed, td.GuideName) as GuideName,
                    COALESCE(arl.GuideNumberUsed, td.GuidePhoneNumber) as GuidePhoneNumber,
                    COALESCE(arl.TourTimeUsed, td.TimeSlot) as TimeSlot,
                    COALESCE(arl.IdentifiableObjectUsed, td.IdentifiableObject) as IdentifiableObject,
                    CASE 
                        WHEN arl.TourLocationUsed IS NOT NULL THEN 
                            CASE 
                                WHEN arl.TourLocationUsed LIKE '%Central Park%' OR arl.GuideNameUsed LIKE '%Alice%' OR arl.GuideNameUsed LIKE '%Bob%' THEN 'Walking Tour'
                                WHEN arl.TourLocationUsed LIKE '%Greenwich%' OR arl.TourLocationUsed LIKE '%Little Italy%' OR arl.GuideNameUsed LIKE '%Maria%' OR arl.GuideNameUsed LIKE '%David%' THEN 'Food Tour'
                                WHEN arl.TourLocationUsed LIKE '%Brooklyn%' OR arl.GuideNameUsed LIKE '%Sarah%' THEN 'Historical Tour'
                                WHEN arl.TourLocationUsed LIKE '%Museum%' OR arl.GuideNameUsed LIKE '%Michael%' THEN 'Art Tour'
                                ELSE 'General Tour'
                            END
                        ELSE td.TourType 
                    END as TourType,
                    CASE 
                        WHEN arl.RequestReceivedTime IS NOT NULL THEN DATE(arl.RequestReceivedTime)
                        ELSE td.Date 
                    END as Date
                FROM AutomatedResponseLog arl
                LEFT JOIN TourDetails td ON (
                    td.GuideName = arl.GuideNameUsed OR 
                    td.MeetingLocation = arl.TourLocationUsed OR
                    td.TourType LIKE '%' || CASE 
                        WHEN arl.TourLocationUsed LIKE '%Central Park%' THEN 'Walking'
                        WHEN arl.TourLocationUsed LIKE '%Greenwich%' THEN 'Food'
                        WHEN arl.TourLocationUsed LIKE '%Brooklyn%' THEN 'Historical'
                        WHEN arl.TourLocationUsed LIKE '%Museum%' THEN 'Art'
                        ELSE 'Tour'
                    END || '%'
                )
                WHERE arl.Status = 'Sent' AND arl.TourLocationUsed IS NOT NULL";

            var parameters = new DynamicParameters();
            
            if (fromDate.HasValue)
            {
                sql += " AND arl.RequestReceivedTime >= @FromDate";
                parameters.Add("FromDate", fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                sql += " AND arl.RequestReceivedTime <= @ToDate";
                parameters.Add("ToDate", toDate.Value);
            }
            
            if (!string.IsNullOrEmpty(tourTypeFilter))
            {
                sql += " AND (td.TourType = @TourType OR arl.TourLocationUsed LIKE '%' || @TourType || '%')";
                parameters.Add("TourType", tourTypeFilter);
            }

            sql += " ORDER BY Date, TimeSlot, TourType";

            var tourData = await connection.QueryAsync<TourExportData>(sql, parameters);
            
            // Get participants for each tour
            foreach (var tour in tourData)
            {
                tour.Participants = (await GetTourParticipantsAsync(tour.TourType, tour.Date, tour.TimeSlot, fromDate, toDate)).ToList();
            }

            return tourData;
        }

        public async Task<IEnumerable<TourParticipant>> GetTourParticipantsAsync(string tourType, string date, string timeSlot, DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = await GetOpenConnectionAsync();
            
            var sql = @"
                SELECT 
                    COALESCE(c.ExtractedUserName, c.DisplayName, 'Guest') as Name,
                    c.WaId as PhoneNumber,
                    c.WaId as WhatsAppId,
                    arl.RequestReceivedTime as BookingTime,
                    arl.AiExtractedData as SpecialRequests,
                    1 as NumberOfPeople
                FROM AutomatedResponseLog arl
                INNER JOIN Contacts c ON c.WaId = arl.ContactWaId
                WHERE arl.Status = 'Sent' 
                AND arl.TourLocationUsed IS NOT NULL
                AND (
                    -- Match by tour type
                    (@TourType LIKE '%Walking%' AND (arl.TourLocationUsed LIKE '%Central Park%' OR arl.GuideNameUsed LIKE '%Alice%' OR arl.GuideNameUsed LIKE '%Bob%')) OR
                    (@TourType LIKE '%Food%' AND (arl.TourLocationUsed LIKE '%Greenwich%' OR arl.TourLocationUsed LIKE '%Little Italy%' OR arl.GuideNameUsed LIKE '%Maria%' OR arl.GuideNameUsed LIKE '%David%')) OR
                    (@TourType LIKE '%Historical%' AND (arl.TourLocationUsed LIKE '%Brooklyn%' OR arl.GuideNameUsed LIKE '%Sarah%')) OR
                    (@TourType LIKE '%Art%' AND (arl.TourLocationUsed LIKE '%Museum%' OR arl.GuideNameUsed LIKE '%Michael%')) OR
                    -- Fallback: match by any part of the tour type
                    (arl.TourLocationUsed LIKE '%' || @TourType || '%' OR arl.GuideNameUsed LIKE '%' || @TourType || '%')
                )";

            var parameters = new DynamicParameters();
            parameters.Add("TourType", tourType);
            
            if (fromDate.HasValue)
            {
                sql += " AND arl.RequestReceivedTime >= @FromDate";
                parameters.Add("FromDate", fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                sql += " AND arl.RequestReceivedTime <= @ToDate";
                parameters.Add("ToDate", toDate.Value);
            }

            // Also filter by date if provided
            if (!string.IsNullOrEmpty(date) && date != "tomorrow" && date != "today")
            {
                sql += " AND DATE(arl.RequestReceivedTime) = @Date";
                parameters.Add("Date", date);
            }

            // Also filter by time slot if provided
            if (!string.IsNullOrEmpty(timeSlot))
            {
                sql += " AND arl.TourTimeUsed LIKE '%' || @TimeSlot || '%'";
                parameters.Add("TimeSlot", timeSlot);
            }

            sql += " ORDER BY arl.RequestReceivedTime DESC";

            return await connection.QueryAsync<TourParticipant>(sql, parameters);
        }

        public async Task<IEnumerable<ContactExportData>> GetContactExportDataAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = await GetOpenConnectionAsync();
            
            var sql = @"
                SELECT 
                    COALESCE(c.ExtractedUserName, c.DisplayName, 'Unknown') as Name,
                    c.WaId as PhoneNumber,
                    c.WaId as WhatsAppId,
                    c.LastMessageTimestamp as LastContact,
                    COALESCE(c.LastExtractedTourType, 'Unknown') as LastTourType,
                    COALESCE(c.LastExtractedTourDate, 'Unknown') as LastTourDate,
                    COALESCE(c.LastExtractedTourTime, 'Unknown') as LastTourTime,
                    (SELECT COUNT(*) FROM Messages m WHERE m.ContactId = c.Id) as TotalMessages,
                    CASE 
                        WHEN EXISTS(SELECT 1 FROM AutomatedResponseLog arl WHERE arl.ContactWaId = c.WaId AND arl.Status = 'Sent') 
                        THEN 'Contacted' 
                        ELSE 'New' 
                    END as Status
                FROM Contacts c
                WHERE 1=1";

            var parameters = new DynamicParameters();
            
            if (fromDate.HasValue)
            {
                sql += " AND c.LastMessageTimestamp >= @FromDate";
                parameters.Add("FromDate", fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                sql += " AND c.LastMessageTimestamp <= @ToDate";
                parameters.Add("ToDate", toDate.Value);
            }

            sql += " ORDER BY c.LastMessageTimestamp DESC";

            return await connection.QueryAsync<ContactExportData>(sql, parameters);
        }

        public async Task<IEnumerable<MessageExportData>> GetMessageExportDataAsync(DateTime? fromDate = null, DateTime? toDate = null, string? contactFilter = null)
        {
            using var connection = await GetOpenConnectionAsync();
            
            var sql = @"
                SELECT 
                    m.Id as MessageId,
                    m.WaMessageId,
                    c.WaId as ContactWaId,
                    COALESCE(c.ExtractedUserName, c.DisplayName, 'Unknown') as ContactName,
                    m.Body as MessageText,
                    m.IsFromMe,
                    m.Timestamp as MessageTimestamp,
                    m.Status as MessageStatus,
                    m.MessageType,
                    CASE WHEN m.IsFromMe = 1 THEN 'Outgoing' ELSE 'Incoming' END as Direction
                FROM Messages m
                INNER JOIN Contacts c ON c.Id = m.ContactId
                WHERE 1=1";

            var parameters = new DynamicParameters();
            
            if (fromDate.HasValue)
            {
                sql += " AND m.Timestamp >= @FromDate";
                parameters.Add("FromDate", fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                sql += " AND m.Timestamp <= @ToDate";
                parameters.Add("ToDate", toDate.Value);
            }
            
            if (!string.IsNullOrEmpty(contactFilter))
            {
                sql += " AND (c.WaId LIKE '%' || @ContactFilter || '%' OR c.DisplayName LIKE '%' || @ContactFilter || '%' OR c.ExtractedUserName LIKE '%' || @ContactFilter || '%')";
                parameters.Add("ContactFilter", contactFilter);
            }

            sql += " ORDER BY m.Timestamp DESC";

            return await connection.QueryAsync<MessageExportData>(sql, parameters);
        }

        public async Task<IEnumerable<AutomatedResponseExportData>> GetAutomatedResponseExportDataAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = await GetOpenConnectionAsync();
            
            var sql = @"
                SELECT 
                    arl.Id as LogId,
                    arl.ContactWaId,
                    COALESCE(c.ExtractedUserName, c.DisplayName, 'Unknown') as ContactName,
                    arl.RequestReceivedTime,
                    arl.ResponseSentTime,
                    arl.ProcessingDurationMs,
                    arl.AiApiCallDurationMs,
                    arl.TemplateUsed,
                    arl.CompanyNameUsed,
                    arl.GuideNameUsed,
                    arl.TourLocationUsed,
                    arl.TourTimeUsed,
                    arl.IdentifiableObjectUsed,
                    arl.GuideNumberUsed,
                    arl.FullResponseText,
                    arl.Status,
                    arl.ErrorMessage,
                    arl.AiExtractedData
                FROM AutomatedResponseLog arl
                LEFT JOIN Contacts c ON c.WaId = arl.ContactWaId
                WHERE 1=1";

            var parameters = new DynamicParameters();
            
            if (fromDate.HasValue)
            {
                sql += " AND arl.RequestReceivedTime >= @FromDate";
                parameters.Add("FromDate", fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                sql += " AND arl.RequestReceivedTime <= @ToDate";
                parameters.Add("ToDate", toDate.Value);
            }

            sql += " ORDER BY arl.RequestReceivedTime DESC";

            return await connection.QueryAsync<AutomatedResponseExportData>(sql, parameters);
        }

        public async Task<Dictionary<string, object>> GetSystemStatsAsync()
        {
            using var connection = await GetOpenConnectionAsync();
            
            var stats = new Dictionary<string, object>();
            
            // Get total contacts
            var totalContacts = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Contacts");
            stats["TotalContacts"] = totalContacts;
            
            // Get total messages
            var totalMessages = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Messages");
            stats["TotalMessages"] = totalMessages;
            
            // Get automated responses today
            var responsesToday = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM AutomatedResponseLog WHERE DATE(RequestReceivedTime) = DATE('now')");
            stats["AutomatedResponsesToday"] = responsesToday;
            
            // Get success rate
            var totalResponses = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM AutomatedResponseLog");
            var successfulResponses = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM AutomatedResponseLog WHERE Status = 'Sent'");
            stats["TotalAutomatedResponses"] = totalResponses;
            stats["SuccessfulResponses"] = successfulResponses;
            stats["SuccessRate"] = totalResponses > 0 ? (double)successfulResponses / totalResponses * 100 : 0;
            
            // Get average processing time
            var avgProcessingTime = await connection.ExecuteScalarAsync<double?>(
                "SELECT AVG(ProcessingDurationMs) FROM AutomatedResponseLog WHERE ProcessingDurationMs > 0");
            stats["AverageProcessingTimeMs"] = avgProcessingTime ?? 0;
            
            return stats;
        }
    }
} 