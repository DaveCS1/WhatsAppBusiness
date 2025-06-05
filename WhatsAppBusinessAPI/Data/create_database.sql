-- WhatsApp Business Database Creation Script
-- SQLite Database Schema for WhatsApp Business Automation System
-- Created: 2025-01-27

-- Enable foreign key constraints
PRAGMA foreign_keys = ON;

-- =====================================================
-- TABLE: Contacts
-- Stores WhatsApp contact information and AI-extracted data
-- =====================================================
CREATE TABLE IF NOT EXISTS Contacts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WaId TEXT UNIQUE NOT NULL,                    -- WhatsApp ID (phone number like "14155552671")
    DisplayName TEXT,                             -- User's display name from WhatsApp
    LastMessageTimestamp DATETIME,               -- When the last message was sent/received
    ExtractedUserName TEXT,                      -- AI-extracted user name from messages
    LastExtractedTourType TEXT,                  -- AI-extracted tour type preference
    LastExtractedTourDate TEXT,                  -- AI-extracted preferred tour date
    LastExtractedTourTime TEXT,                  -- AI-extracted preferred tour time
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Index for faster lookups by WaId
CREATE INDEX IF NOT EXISTS idx_contacts_waid ON Contacts(WaId);
CREATE INDEX IF NOT EXISTS idx_contacts_last_message ON Contacts(LastMessageTimestamp DESC);

-- =====================================================
-- TABLE: Messages
-- Stores all WhatsApp messages (incoming and outgoing)
-- =====================================================
CREATE TABLE IF NOT EXISTS Messages (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WaMessageId TEXT UNIQUE,                     -- WhatsApp's unique message ID
    ContactId INTEGER NOT NULL,                  -- Foreign Key to Contacts table
    Body TEXT,                                   -- Message content/text
    IsFromMe BOOLEAN NOT NULL,                   -- TRUE if sent by our system, FALSE if from user
    Timestamp DATETIME NOT NULL,                 -- When the message was sent/received
    Status TEXT,                                 -- Message status: "sent", "delivered", "read", "failed"
    MessageType TEXT DEFAULT 'text',             -- Type: "text", "image", "document", etc.
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ContactId) REFERENCES Contacts(Id) ON DELETE CASCADE
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_messages_contact ON Messages(ContactId);
CREATE INDEX IF NOT EXISTS idx_messages_timestamp ON Messages(Timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_messages_wa_id ON Messages(WaMessageId);
CREATE INDEX IF NOT EXISTS idx_messages_status ON Messages(Status);

-- =====================================================
-- TABLE: TourDetails
-- Stores preset tour information for automated responses
-- =====================================================
CREATE TABLE IF NOT EXISTS TourDetails (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TourType TEXT NOT NULL,                      -- e.g., "Walking Tour", "Food Tour", "Historical Tour"
    Date TEXT,                                   -- e.g., "tomorrow", "2025-07-01", "weekends"
    TimeSlot TEXT,                              -- e.g., "9 AM", "1 PM", "afternoon", "evening"
    GuideName TEXT NOT NULL,                    -- Tour guide's name
    MeetingLocation TEXT NOT NULL,              -- Where to meet the guide
    IdentifiableObject TEXT NOT NULL,           -- What the guide will be holding/wearing
    GuidePhoneNumber TEXT NOT NULL,             -- Guide's contact number
    IsActive BOOLEAN DEFAULT TRUE,              -- Whether this tour slot is currently available
    MaxCapacity INTEGER DEFAULT 10,             -- Maximum number of people for this tour
    Price DECIMAL(10,2),                        -- Tour price (optional)
    Description TEXT,                           -- Additional tour details
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(TourType, Date, TimeSlot)            -- Prevent duplicate tour slots
);

-- Index for tour lookups
CREATE INDEX IF NOT EXISTS idx_tour_details_type ON TourDetails(TourType);
CREATE INDEX IF NOT EXISTS idx_tour_details_date ON TourDetails(Date);
CREATE INDEX IF NOT EXISTS idx_tour_details_active ON TourDetails(IsActive);

-- =====================================================
-- TABLE: AutomatedResponseLog
-- Comprehensive logging of all automated responses
-- =====================================================
CREATE TABLE IF NOT EXISTS AutomatedResponseLog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    IncomingMessageId INTEGER,                   -- FK to Messages table (nullable for system-generated responses)
    ContactWaId TEXT NOT NULL,                  -- WhatsApp ID for easy reference
    RequestReceivedTime DATETIME NOT NULL,      -- When the incoming message was received
    ResponseSentTime DATETIME NOT NULL,         -- When the automated response was sent
    ProcessingDurationMs INTEGER NOT NULL,      -- Total processing time in milliseconds
    AiApiCallDurationMs INTEGER,                -- Time spent calling AI API (nullable)
    TemplateUsed TEXT,                          -- Which response template was used
    CompanyNameUsed TEXT,                       -- Company name used in response
    GuideNameUsed TEXT,                         -- Guide name used in response
    TourLocationUsed TEXT,                      -- Meeting location used in response
    TourTimeUsed TEXT,                          -- Tour time used in response
    IdentifiableObjectUsed TEXT,                -- Identifiable object used in response
    GuideNumberUsed TEXT,                       -- Guide phone number used in response
    FullResponseText TEXT NOT NULL,             -- Complete automated response text
    Status TEXT NOT NULL,                       -- "Sent", "Failed", "Pending"
    ErrorMessage TEXT,                          -- Error details if Status = "Failed"
    AiExtractedData TEXT,                       -- JSON of AI-extracted information
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (IncomingMessageId) REFERENCES Messages(Id) ON DELETE SET NULL
);

-- Indexes for analytics and monitoring
CREATE INDEX IF NOT EXISTS idx_response_log_contact ON AutomatedResponseLog(ContactWaId);
CREATE INDEX IF NOT EXISTS idx_response_log_status ON AutomatedResponseLog(Status);
CREATE INDEX IF NOT EXISTS idx_response_log_time ON AutomatedResponseLog(RequestReceivedTime DESC);
CREATE INDEX IF NOT EXISTS idx_response_log_template ON AutomatedResponseLog(TemplateUsed);

-- =====================================================
-- TABLE: SystemSettings
-- Store application configuration and settings
-- =====================================================
CREATE TABLE IF NOT EXISTS SystemSettings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SettingKey TEXT UNIQUE NOT NULL,
    SettingValue TEXT,
    Description TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- SAMPLE DATA INSERTION
-- =====================================================

-- Insert sample tour details
INSERT OR IGNORE INTO TourDetails (TourType, Date, TimeSlot, GuideName, MeetingLocation, IdentifiableObject, GuidePhoneNumber, Description, Price) VALUES
('Walking Tour', 'tomorrow', '9 AM', 'Alice Smith', 'Central Park Entrance (59th St & 5th Ave)', 'a red umbrella and "NYC Tours" sign', '+1-555-0101', 'Explore the highlights of Central Park with our experienced guide', 25.00),
('Walking Tour', 'tomorrow', '2 PM', 'Bob Johnson', 'Central Park Entrance (59th St & 5th Ave)', 'a blue cap and "NYC Tours" sign', '+1-555-0102', 'Afternoon walking tour of Central Park', 25.00),
('Food Tour', 'tomorrow', '11 AM', 'Maria Garcia', 'Greenwich Village (Washington Square Park)', 'a green tote bag with "Foodie Tours" logo', '+1-555-0201', 'Taste the best of Greenwich Village cuisine', 45.00),
('Food Tour', 'tomorrow', '6 PM', 'David Chen', 'Little Italy (Mulberry St & Grand St)', 'a white chef hat and clipboard', '+1-555-0202', 'Evening food tour through Little Italy and Chinatown', 50.00),
('Historical Tour', 'tomorrow', '10 AM', 'Sarah Williams', 'Brooklyn Bridge (Manhattan side entrance)', 'a yellow folder and "History Walks" badge', '+1-555-0301', 'Discover the history of Brooklyn Bridge and surrounding area', 30.00),
('Art Tour', 'tomorrow', '1 PM', 'Michael Brown', 'Museum Mile (82nd St & 5th Ave)', 'a purple scarf and art portfolio', '+1-555-0401', 'Explore world-class museums and galleries', 35.00),
('Walking Tour', 'today', '3 PM', 'Jennifer Lee', 'Times Square (Red Steps)', 'a bright orange vest and megaphone', '+1-555-0103', 'Last-minute walking tour of Times Square area', 20.00),
('Food Tour', 'weekend', '12 PM', 'Carlos Rodriguez', 'Chelsea Market (75 9th Ave)', 'a black apron and "Weekend Eats" sign', '+1-555-0203', 'Weekend special food tour of Chelsea Market', 40.00);

-- Insert system settings
INSERT OR IGNORE INTO SystemSettings (SettingKey, SettingValue, Description) VALUES
('company_name', 'NYC Adventure Tours', 'Default company name for automated responses'),
('default_response_template', 'TourConfirmation', 'Default template to use for automated responses'),
('ai_extraction_enabled', 'true', 'Whether to use AI for extracting user information'),
('max_processing_time_ms', '30000', 'Maximum time allowed for processing a single message'),
('auto_response_enabled', 'true', 'Whether automated responses are enabled'),
('business_hours_start', '08:00', 'Business hours start time (24-hour format)'),
('business_hours_end', '20:00', 'Business hours end time (24-hour format)'),
('timezone', 'America/New_York', 'Business timezone for scheduling');

-- =====================================================
-- VIEWS FOR COMMON QUERIES
-- =====================================================

-- View: Recent conversations with message counts
CREATE VIEW IF NOT EXISTS RecentConversations AS
SELECT 
    c.Id,
    c.WaId,
    c.DisplayName,
    c.ExtractedUserName,
    c.LastMessageTimestamp,
    COUNT(m.Id) as MessageCount,
    MAX(CASE WHEN m.IsFromMe = 0 THEN m.Timestamp END) as LastIncomingMessage,
    MAX(CASE WHEN m.IsFromMe = 1 THEN m.Timestamp END) as LastOutgoingMessage
FROM Contacts c
LEFT JOIN Messages m ON c.Id = m.ContactId
GROUP BY c.Id, c.WaId, c.DisplayName, c.ExtractedUserName, c.LastMessageTimestamp
ORDER BY c.LastMessageTimestamp DESC;

-- View: Automated response statistics
CREATE VIEW IF NOT EXISTS ResponseStats AS
SELECT 
    DATE(RequestReceivedTime) as ResponseDate,
    COUNT(*) as TotalResponses,
    COUNT(CASE WHEN Status = 'Sent' THEN 1 END) as SuccessfulResponses,
    COUNT(CASE WHEN Status = 'Failed' THEN 1 END) as FailedResponses,
    AVG(ProcessingDurationMs) as AvgProcessingTime,
    AVG(AiApiCallDurationMs) as AvgAiCallTime
FROM AutomatedResponseLog
GROUP BY DATE(RequestReceivedTime)
ORDER BY ResponseDate DESC;

-- =====================================================
-- TRIGGERS FOR AUTOMATIC TIMESTAMP UPDATES
-- =====================================================

-- Update Contacts.UpdatedAt when record is modified
CREATE TRIGGER IF NOT EXISTS update_contacts_timestamp 
    AFTER UPDATE ON Contacts
BEGIN
    UPDATE Contacts SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
END;

-- Update TourDetails.UpdatedAt when record is modified
CREATE TRIGGER IF NOT EXISTS update_tour_details_timestamp 
    AFTER UPDATE ON TourDetails
BEGIN
    UPDATE TourDetails SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
END;

-- Update SystemSettings.UpdatedAt when record is modified
CREATE TRIGGER IF NOT EXISTS update_system_settings_timestamp 
    AFTER UPDATE ON SystemSettings
BEGIN
    UPDATE SystemSettings SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
END;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Check if all tables were created successfully
SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;

-- Check sample data
SELECT 'TourDetails Count: ' || COUNT(*) FROM TourDetails;
SELECT 'SystemSettings Count: ' || COUNT(*) FROM SystemSettings;

-- Display sample tour data
SELECT TourType, Date, TimeSlot, GuideName, MeetingLocation FROM TourDetails LIMIT 5;

PRAGMA table_info(Contacts);
PRAGMA table_info(Messages);
PRAGMA table_info(TourDetails);
PRAGMA table_info(AutomatedResponseLog);

-- =====================================================
-- SCRIPT COMPLETION MESSAGE
-- =====================================================
SELECT 'WhatsApp Business Database Created Successfully!' as Status; 