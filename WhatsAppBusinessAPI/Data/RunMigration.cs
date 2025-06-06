using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace WhatsAppBusinessAPI.Data
{
    public class RunMigration
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("WhatsApp Business Database Migration");
            Console.WriteLine("Adding MessageTemplates table...");
            
            var dbPath = "Data/whatsapp.db";
            var migrationScript = "Data/add_message_templates.sql";
            
            // Check if files exist
            if (!File.Exists(dbPath))
            {
                Console.WriteLine($"Database not found at: {dbPath}");
                Console.WriteLine("Please make sure you're running this from the WhatsAppBusinessAPI directory");
                return;
            }
            
            if (!File.Exists(migrationScript))
            {
                Console.WriteLine($"Migration script not found at: {migrationScript}");
                return;
            }
            
            try
            {
                var connectionString = $"Data Source={dbPath}";
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();
                
                Console.WriteLine("Connected to database successfully");
                
                // Read and execute the migration script
                var migrationSql = await File.ReadAllTextAsync(migrationScript);
                
                // Split by semicolon and execute each statement
                var statements = migrationSql.Split(';', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var statement in statements)
                {
                    var trimmedStatement = statement.Trim();
                    if (string.IsNullOrEmpty(trimmedStatement) || trimmedStatement.StartsWith("--"))
                        continue;
                        
                    using var command = connection.CreateCommand();
                    command.CommandText = trimmedStatement;
                    await command.ExecuteNonQueryAsync();
                }
                
                Console.WriteLine("Migration completed successfully!");
                Console.WriteLine("MessageTemplates table has been added to your database.");
                Console.WriteLine("You can now restart your API application.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running migration: {ex.Message}");
                Console.WriteLine("Please check the error and try again.");
            }
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
} 