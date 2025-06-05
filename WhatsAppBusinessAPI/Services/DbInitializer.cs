using Dapper;
using Microsoft.Data.Sqlite;

namespace WhatsAppBusinessAPI.Services
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                logger.LogError("DefaultConnection string is not configured.");
                throw new InvalidOperationException("Database connection string is missing.");
            }

            try
            {
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();
                logger.LogInformation("Database connection opened successfully.");

                // Verify that the required tables exist
                var requiredTables = new[] { "Contacts", "Messages", "TourDetails", "AutomatedResponseLog" };
                var existingTables = await connection.QueryAsync<string>(
                    "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'");
                
                var missingTables = requiredTables.Except(existingTables, StringComparer.OrdinalIgnoreCase).ToList();
                
                if (missingTables.Any())
                {
                    logger.LogError($"Missing required tables: {string.Join(", ", missingTables)}");
                    throw new InvalidOperationException($"Database is missing required tables: {string.Join(", ", missingTables)}");
                }

                // Verify table count and log success
                var tableCount = existingTables.Count();
                logger.LogInformation($"Database verification completed successfully. Found {tableCount} tables.");
                
                // Optional: Log some basic stats
                var contactCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Contacts");
                var messageCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Messages");
                var tourCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM TourDetails");
                
                logger.LogInformation($"Database stats - Contacts: {contactCount}, Messages: {messageCount}, Tours: {tourCount}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during database verification.");
                throw;
            }
        }
    }
} 