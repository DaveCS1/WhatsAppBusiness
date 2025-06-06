-- =====================================================
-- SAMPLE DATA FOR TESTING
-- Run this script in DB Browser for SQLite to add sample contacts and messages
-- =====================================================

-- Insert sample contacts
INSERT OR REPLACE INTO Contacts (Id, WaId, DisplayName, ExtractedUserName, LastExtractedTourType, LastExtractedTourDate, LastExtractedTourTime, LastMessageTimestamp, CreatedAt, UpdatedAt) VALUES
(1, '1234567890', 'John Smith', 'John', 'Food Tour', 'tomorrow', '2 PM', '2024-12-19 16:00:00', '2024-12-17 10:00:00', '2024-12-19 16:00:00'),
(2, '0987654321', 'Sarah Johnson', 'Sarah', 'Walking Tour', 'weekend', 'morning', '2024-12-19 14:30:00', '2024-12-18 09:00:00', '2024-12-19 14:30:00'),
(3, '5555555555', 'Mike Chen', 'Mike', 'Historical Tour', 'Friday', 'evening', '2024-12-19 12:00:00', '2024-12-16 15:00:00', '2024-12-19 12:00:00'),
(4, '7777777777', 'Emma Wilson', 'Emma', 'Art Tour', 'today', 'afternoon', '2024-12-19 10:00:00', '2024-12-18 14:00:00', '2024-12-19 10:00:00'),
(5, '9999999999', 'David Brown', 'David', 'Photography Tour', 'Monday', '9 AM', '2024-12-19 08:00:00', '2024-12-17 11:00:00', '2024-12-19 08:00:00');

-- Insert sample messages for John Smith (Contact ID 1)
INSERT OR REPLACE INTO Messages (Id, WaMessageId, ContactId, Body, IsFromMe, Timestamp, Status, MessageType, CreatedAt) VALUES
(1, 'msg_john_001', 1, 'Hi! I''m John and I''d love to book a food tour for tomorrow around 2 PM.', 0, '2024-12-19 14:00:00', 'delivered', 'text', '2024-12-19 14:00:00'),
(2, 'msg_john_002', 1, 'Hello John! Thank you for booking your tour with NYC Adventure Tours.

Your tour guide Maria will meet you at Greenwich Village (Washington Square Park) at 11 AM. Look for a green tote bag with "Foodie Tours" logo.

If you need to reach your guide directly, you can contact them at: +1-555-0201

Our food tour will take you through the best restaurants in Greenwich Village. You''ll taste amazing cuisine while learning about the area''s rich history.

We look forward to showing you an amazing time! If you have any questions before your tour, feel free to reach out.', 1, '2024-12-19 14:02:00', 'sent', 'text', '2024-12-19 14:02:00'),
(3, 'msg_john_003', 1, 'Perfect! Thank you so much. Looking forward to it!', 0, '2024-12-19 16:00:00', 'delivered', 'text', '2024-12-19 16:00:00');

-- Insert sample messages for Sarah Johnson (Contact ID 2)
INSERT OR REPLACE INTO Messages (Id, WaMessageId, ContactId, Body, IsFromMe, Timestamp, Status, MessageType, CreatedAt) VALUES
(4, 'msg_sarah_001', 2, 'Hello, I''d like to book a walking tour for this weekend morning. I''m Sarah.', 0, '2024-12-19 13:00:00', 'delivered', 'text', '2024-12-19 13:00:00'),
(5, 'msg_sarah_002', 2, 'Hello Sarah! Thank you for booking your tour with NYC Adventure Tours.

Your tour guide Alice will meet you at Central Park Entrance (59th St & 5th Ave) at 9 AM. Look for a red umbrella and "NYC Tours" sign.

If you need to reach your guide directly, you can contact them at: +1-555-0101

Our walking tour covers the most scenic routes through Central Park. You''ll discover hidden paths, historical landmarks, and beautiful viewpoints perfect for photos.

We look forward to showing you an amazing time!', 1, '2024-12-19 13:03:00', 'sent', 'text', '2024-12-19 13:03:00'),
(6, 'msg_sarah_003', 2, 'Sounds great! What should I bring?', 0, '2024-12-19 14:30:00', 'delivered', 'text', '2024-12-19 14:30:00');

-- Insert sample messages for Mike Chen (Contact ID 3)
INSERT OR REPLACE INTO Messages (Id, WaMessageId, ContactId, Body, IsFromMe, Timestamp, Status, MessageType, CreatedAt) VALUES
(7, 'msg_mike_001', 3, 'Hi there! I''m Mike and I want to do a historical tour on Friday evening.', 0, '2024-12-19 11:00:00', 'delivered', 'text', '2024-12-19 11:00:00'),
(8, 'msg_mike_002', 3, 'Hello Mike! Thank you for booking your tour with NYC Adventure Tours.

Your tour guide Sarah will meet you at Brooklyn Bridge (Manhattan side entrance) at 10 AM. Look for a yellow folder and "History Walks" badge.

If you need to reach your guide directly, you can contact them at: +1-555-0301

Our historical tour will take you through centuries of New York history, from colonial times to modern day. You''ll visit significant landmarks and hear fascinating stories.

We look forward to showing you an amazing time!', 1, '2024-12-19 11:05:00', 'sent', 'text', '2024-12-19 11:05:00'),
(9, 'msg_mike_003', 3, 'That sounds perfect! See you Friday.', 0, '2024-12-19 12:00:00', 'delivered', 'text', '2024-12-19 12:00:00');

-- Insert sample messages for Emma Wilson (Contact ID 4)
INSERT OR REPLACE INTO Messages (Id, WaMessageId, ContactId, Body, IsFromMe, Timestamp, Status, MessageType, CreatedAt) VALUES
(10, 'msg_emma_001', 4, 'Hello! I''m Emma and I''d love to book an art tour for today afternoon.', 0, '2024-12-19 09:00:00', 'delivered', 'text', '2024-12-19 09:00:00'),
(11, 'msg_emma_002', 4, 'Hello Emma! Thank you for booking your tour with NYC Adventure Tours.

Your tour guide Michael will meet you at Museum Mile (82nd St & 5th Ave) at 1 PM. Look for a purple scarf and art portfolio.

If you need to reach your guide directly, you can contact them at: +1-555-0401

Our art tour will explore world-class museums and galleries. You''ll discover amazing artworks and learn about different artistic movements and techniques.

We look forward to showing you an amazing time!', 1, '2024-12-19 09:03:00', 'sent', 'text', '2024-12-19 09:03:00'),
(12, 'msg_emma_003', 4, 'Wonderful! I''m really excited about this.', 0, '2024-12-19 10:00:00', 'delivered', 'text', '2024-12-19 10:00:00');

-- Insert sample messages for David Brown (Contact ID 5)
INSERT OR REPLACE INTO Messages (Id, WaMessageId, ContactId, Body, IsFromMe, Timestamp, Status, MessageType, CreatedAt) VALUES
(13, 'msg_david_001', 5, 'Hi! I''m David and I''d like a photography tour on Monday at 9 AM.', 0, '2024-12-19 07:00:00', 'delivered', 'text', '2024-12-19 07:00:00'),
(14, 'msg_david_002', 5, 'Hello David! Thank you for your interest in our photography tours.

Unfortunately, we don''t currently have a photography tour scheduled for Monday at 9 AM. However, we have other amazing tours available:

• Walking Tour - Monday 9 AM at Central Park
• Food Tour - Monday 11 AM in Greenwich Village
• Historical Tour - Monday 10 AM at Brooklyn Bridge

Would any of these interest you? We can also try to arrange a custom photography tour if you have specific requirements.

Please let us know how we can help!', 1, '2024-12-19 07:05:00', 'sent', 'text', '2024-12-19 07:05:00'),
(15, 'msg_david_003', 5, 'The walking tour sounds good as a backup. Let me think about it.', 0, '2024-12-19 08:00:00', 'delivered', 'text', '2024-12-19 08:00:00');

-- Insert sample automated response logs
INSERT OR REPLACE INTO AutomatedResponseLog (Id, IncomingMessageId, ContactWaId, RequestReceivedTime, ResponseSentTime, ProcessingDurationMs, AiApiCallDurationMs, TemplateUsed, CompanyNameUsed, GuideNameUsed, TourLocationUsed, TourTimeUsed, IdentifiableObjectUsed, GuideNumberUsed, FullResponseText, Status, AiExtractedData, CreatedAt) VALUES
(1, 1, '1234567890', '2024-12-19 14:00:00', '2024-12-19 14:02:00', 1250, 800, 'Food Tour Template', 'NYC Adventure Tours', 'Maria', 'Greenwich Village (Washington Square Park)', '11 AM', 'a green tote bag with "Foodie Tours" logo', '+1-555-0201', 'Hello John! Thank you for booking your tour with NYC Adventure Tours...', 'Sent', '{"userName":"John","tourType":"Food Tour","date":"tomorrow","time":"2 PM"}', '2024-12-19 14:02:00'),
(2, 4, '0987654321', '2024-12-19 13:00:00', '2024-12-19 13:03:00', 1100, 750, 'Walking Tour Template', 'NYC Adventure Tours', 'Alice', 'Central Park Entrance (59th St & 5th Ave)', '9 AM', 'a red umbrella and "NYC Tours" sign', '+1-555-0101', 'Hello Sarah! Thank you for booking your tour with NYC Adventure Tours...', 'Sent', '{"userName":"Sarah","tourType":"Walking Tour","date":"weekend","time":"morning"}', '2024-12-19 13:03:00'),
(3, 7, '5555555555', '2024-12-19 11:00:00', '2024-12-19 11:05:00', 1350, 900, 'Historical Tour Template', 'NYC Adventure Tours', 'Sarah', 'Brooklyn Bridge (Manhattan side entrance)', '10 AM', 'a yellow folder and "History Walks" badge', '+1-555-0301', 'Hello Mike! Thank you for booking your tour with NYC Adventure Tours...', 'Sent', '{"userName":"Mike","tourType":"Historical Tour","date":"Friday","time":"evening"}', '2024-12-19 11:05:00'),
(4, 10, '7777777777', '2024-12-19 09:00:00', '2024-12-19 09:03:00', 1200, 850, 'Art Tour Template', 'NYC Adventure Tours', 'Michael', 'Museum Mile (82nd St & 5th Ave)', '1 PM', 'a purple scarf and art portfolio', '+1-555-0401', 'Hello Emma! Thank you for booking your tour with NYC Adventure Tours...', 'Sent', '{"userName":"Emma","tourType":"Art Tour","date":"today","time":"afternoon"}', '2024-12-19 09:03:00'),
(5, 13, '9999999999', '2024-12-19 07:00:00', '2024-12-19 07:05:00', 1500, 950, 'No Match Template', 'NYC Adventure Tours', 'Support Team', 'Various Locations', 'Various Times', 'N/A', 'Main Office', 'Hello David! Thank you for your interest in our photography tours...', 'Sent', '{"userName":"David","tourType":"Photography Tour","date":"Monday","time":"9 AM"}', '2024-12-19 07:05:00');

-- Verify the data was inserted
SELECT 'Contacts inserted: ' || COUNT(*) FROM Contacts;
SELECT 'Messages inserted: ' || COUNT(*) FROM Messages;
SELECT 'Response logs inserted: ' || COUNT(*) FROM AutomatedResponseLog;

-- Show sample data
SELECT 'Sample Contacts:' as Info;
SELECT Id, WaId, DisplayName, ExtractedUserName, LastExtractedTourType FROM Contacts ORDER BY LastMessageTimestamp DESC;

SELECT 'Sample Messages:' as Info;
SELECT ContactId, Body, IsFromMe, Timestamp FROM Messages ORDER BY Timestamp DESC LIMIT 10; 