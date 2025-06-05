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

            // Ensure the Data directory exists
            var dbFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "whatsapp.db");
            var dbDirectory = Path.GetDirectoryName(dbFilePath);
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory!);
                logger.LogInformation($"Created database directory: {dbDirectory}");
            }

            try
            {
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();
                logger.LogInformation("Database connection opened successfully.");

                // Read the SQL script file
                var sqlScriptPath = Path.Combine(AppContext.BaseDirectory, "Data", "create_database.sql");
                if (!File.Exists(sqlScriptPath))
                {
                    logger.LogError($"SQL script file not found at: {sqlScriptPath}");
                    throw new FileNotFoundException($"SQL script file not found: {sqlScriptPath}");
                }

                var sqlScript = await File.ReadAllTextAsync(sqlScriptPath);
                logger.LogInformation("SQL script loaded successfully.");

                // Split the script into individual statements (simple approach)
                var statements = sqlScript.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var statement in statements)
                {
                    var trimmedStatement = statement.Trim();
                    if (!string.IsNullOrEmpty(trimmedStatement) && 
                        !trimmedStatement.StartsWith("--") && 
                        !trimmedStatement.StartsWith("PRAGMA table_info") &&
                        !trimmedStatement.StartsWith("SELECT name FROM sqlite_master") &&
                        !trimmedStatement.StartsWith("SELECT 'TourDetails Count") &&
                        !trimmedStatement.StartsWith("SELECT 'SystemSettings Count") &&
                        !trimmedStatement.StartsWith("SELECT TourType") &&
                        !trimmedStatement.StartsWith("SELECT 'WhatsApp Business Database"))
                    {
                        try
                        {
                            await connection.ExecuteAsync(trimmedStatement);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning($"Failed to execute SQL statement: {trimmedStatement.Substring(0, Math.Min(50, trimmedStatement.Length))}... Error: {ex.Message}");
                        }
                    }
                }

                logger.LogInformation("Database initialization completed successfully.");
                
                // Verify tables were created
                var tableCount = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'");
                logger.LogInformation($"Database initialized with {tableCount} tables.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during database initialization.");
                throw;
            }
        }
    }
} 