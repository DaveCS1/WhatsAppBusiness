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