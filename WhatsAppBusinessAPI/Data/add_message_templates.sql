-- =====================================================
-- MIGRATION SCRIPT: Add MessageTemplates Table
-- Run this script on your existing database to add the new MessageTemplates functionality
-- =====================================================

-- Create MessageTemplates table
CREATE TABLE IF NOT EXISTS MessageTemplates (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT NOT NULL,
    TemplateText TEXT NOT NULL,
    Category TEXT NOT NULL,                     -- "TourConfirmation", "General", "NoMatch", etc.
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    IsDefault BOOLEAN NOT NULL DEFAULT 0,
    PlaceholderVariables TEXT,                  -- JSON array of available placeholders
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for message templates
CREATE INDEX IF NOT EXISTS idx_templates_category ON MessageTemplates(Category);
CREATE INDEX IF NOT EXISTS idx_templates_active ON MessageTemplates(IsActive);
CREATE INDEX IF NOT EXISTS idx_templates_default ON MessageTemplates(IsDefault);

-- Insert sample message templates
INSERT OR IGNORE INTO MessageTemplates (Name, Description, TemplateText, Category, IsActive, IsDefault, PlaceholderVariables) VALUES
('Tour Confirmation', 'Standard tour confirmation message', 
'Hello! Thank you for booking your tour with {company_name}. 

Your tour guide {guide_name} will meet you at {meeting_location} at {tour_time}. Look for {identifiable_object}.

If you need to reach your guide directly, you can contact them at: {guide_phone}

{tour_description}

We look forward to showing you an amazing time! If you have any questions before your tour, feel free to reach out.

PS - We have many more tours available! Ask us about our other offerings including food tours, historical walks, and specialty experiences.

Have a wonderful day!
{company_name} Team', 
'TourConfirmation', 1, 1, 
'[{"name":"company_name","description":"Company name","defaultValue":"NYC Adventure Tours","required":true},{"name":"guide_name","description":"Tour guide name","defaultValue":"","required":true},{"name":"meeting_location","description":"Meeting location","defaultValue":"","required":true},{"name":"tour_time","description":"Tour time","defaultValue":"","required":true},{"name":"identifiable_object","description":"What guide will be holding/wearing","defaultValue":"","required":true},{"name":"guide_phone","description":"Guide phone number","defaultValue":"","required":true},{"name":"tour_description","description":"Tour description","defaultValue":"","required":false}]'),

('General Welcome', 'General welcome message when no tour match found',
'Hello! Thank you for contacting {company_name}. We''ve received your message and will get back to you shortly with tour information.

We offer amazing tours including:
• Walking Tours of Central Park and Times Square
• Food Tours through Greenwich Village and Little Italy  
• Historical Tours of Brooklyn Bridge and Lower Manhattan
• Art Tours of Museum Mile and galleries

Please let us know what type of experience interests you and when you''d like to visit!

{company_name} Team',
'General', 1, 0,
'[{"name":"company_name","description":"Company name","defaultValue":"NYC Adventure Tours","required":true}]'),

('Booking Confirmation', 'Message sent after successful booking',
'Great news! Your booking has been confirmed for {tour_type} on {tour_date} at {tour_time}.

Tour Details:
• Guide: {guide_name}
• Meeting Point: {meeting_location}
• Look for: {identifiable_object}
• Guide Contact: {guide_phone}

Important reminders:
- Please arrive 10 minutes early
- Wear comfortable walking shoes
- Bring water and a camera
- Contact your guide directly if you''re running late

We''re excited to show you the best of NYC!

{company_name}',
'BookingConfirmation', 1, 0,
'[{"name":"company_name","description":"Company name","defaultValue":"NYC Adventure Tours","required":true},{"name":"tour_type","description":"Type of tour","defaultValue":"","required":true},{"name":"tour_date","description":"Tour date","defaultValue":"","required":true},{"name":"tour_time","description":"Tour time","defaultValue":"","required":true},{"name":"guide_name","description":"Guide name","defaultValue":"","required":true},{"name":"meeting_location","description":"Meeting location","defaultValue":"","required":true},{"name":"identifiable_object","description":"Identifiable object","defaultValue":"","required":true},{"name":"guide_phone","description":"Guide phone","defaultValue":"","required":true}]');

-- Create trigger for automatic timestamp updates on MessageTemplates
CREATE TRIGGER IF NOT EXISTS update_message_templates_timestamp 
    AFTER UPDATE ON MessageTemplates
BEGIN
    UPDATE MessageTemplates SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
END;

-- Verify the migration
SELECT 'MessageTemplates table created successfully!' as Status;
SELECT 'Templates inserted: ' || COUNT(*) as TemplateCount FROM MessageTemplates;

-- Show the new templates
SELECT Name, Category, IsActive, IsDefault FROM MessageTemplates ORDER BY Category, Name;

PRAGMA table_info(MessageTemplates); 