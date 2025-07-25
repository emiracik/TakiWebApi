-- =====================================================
-- TaxiDb Sample Data INSERT Scripts
-- Execute these scripts in the order shown below
-- =====================================================

-- =====================================================
-- 1. AdminRoles - Execute First
-- =====================================================
INSERT INTO AdminRoles (RoleName, Description) VALUES 
('SuperAdmin', 'Full system access'),
('Admin', 'Administrative access'),
('Moderator', 'Content moderation access'),
('Support', 'Customer support access');

-- =====================================================
-- 2. Admins - Execute After AdminRoles
-- =====================================================
INSERT INTO Admins (FullName, Email, Phone, PasswordHash, RoleID, IsActive, CreatedDate) VALUES 
('John Doe', 'john.doe@taxi.com', '+1234567890', 'hashed_password_123', 1, 1, GETDATE()),
('Jane Smith', 'jane.smith@taxi.com', '+1234567891', 'hashed_password_456', 2, 1, GETDATE()),
('Mike Johnson', 'mike.johnson@taxi.com', '+1234567892', 'hashed_password_789', 3, 1, GETDATE());

-- =====================================================
-- 3. Users - Execute Third
-- =====================================================
INSERT INTO Users (FullName, PhoneNumber, Email, IsActive, CreatedDate) VALUES 
('Ahmed Ali', '+201234567890', 'ahmed.ali@email.com', 1, GETDATE()),
('Sara Mohamed', '+201234567891', 'sara.mohamed@email.com', 1, GETDATE()),
('Omar Hassan', '+201234567892', 'omar.hassan@email.com', 1, GETDATE()),
('Fatma Ibrahim', '+201234567893', 'fatma.ibrahim@email.com', 1, GETDATE()),
('Mahmoud Khaled', '+201234567894', 'mahmoud.khaled@email.com', 1, GETDATE());

-- =====================================================
-- 4. Drivers - Execute Fourth
-- =====================================================
INSERT INTO Drivers (FullName, PhoneNumber, Email, VehiclePlate, VehicleModel, VehicleColor, CreatedDate) VALUES 
('Hassan Ahmed', '+201111111111', 'hassan.ahmed@email.com', 'ABC-123', 'Toyota Corolla', 'White', GETDATE()),
('Mohamed Ali', '+201111111112', 'mohamed.ali@email.com', 'DEF-456', 'Hyundai Elantra', 'Silver', GETDATE()),
('Khaled Omar', '+201111111113', 'khaled.omar@email.com', 'GHI-789', 'Nissan Sunny', 'Blue', GETDATE()),
('Amr Saeed', '+201111111114', 'amr.saeed@email.com', 'JKL-012', 'Kia Cerato', 'Black', GETDATE()),
('Tamer Farouk', '+201111111115', 'tamer.farouk@email.com', 'MNO-345', 'Chevrolet Aveo', 'Red', GETDATE());

-- =====================================================
-- 5. DriverDetails - Execute After Users
-- =====================================================
INSERT INTO DriverDetails (UserID, VehiclePlate, VehicleModel, VehicleColor, LicenseNumber, ExperienceYears, RatingAverage, CreatedDate) VALUES 
(1, 'ABC-123', 'Toyota Corolla', 'White', 'DL123456789', 5, 4.5, GETDATE()),
(2, 'DEF-456', 'Hyundai Elantra', 'Silver', 'DL987654321', 3, 4.2, GETDATE()),
(3, 'GHI-789', 'Nissan Sunny', 'Blue', 'DL456789123', 7, 4.8, GETDATE());

-- =====================================================
-- 6. Trips - Execute After Users and Drivers
-- =====================================================
INSERT INTO Trips (PassengerID, DriverID, StartAddress, EndAddress, StartLatitude, StartLongitude, EndLatitude, EndLongitude, StartTime, EndTime, Cost, PaymentMethod, CreatedDate) VALUES 
(1, 1, 'Nasr City, Cairo', 'Downtown Cairo', 30.0444, 31.2357, 30.0626, 31.2497, '2024-01-15 08:30:00', '2024-01-15 09:15:00', 45.50, 'Cash', GETDATE()),
(2, 2, 'Maadi, Cairo', 'Zamalek, Cairo', 29.9602, 31.2569, 30.0626, 31.2174, '2024-01-15 14:20:00', '2024-01-15 15:00:00', 65.00, 'Credit Card', GETDATE()),
(3, 1, 'Heliopolis, Cairo', 'New Cairo', 30.0808, 31.3230, 30.0131, 31.4969, '2024-01-16 10:00:00', '2024-01-16 10:45:00', 80.25, 'Digital Wallet', GETDATE()),
(4, 3, 'Giza, Egypt', 'Cairo Airport', 30.0131, 31.2089, 30.1219, 31.4056, '2024-01-16 16:30:00', '2024-01-16 17:30:00', 120.00, 'Cash', GETDATE());

-- =====================================================
-- 7. DriverRatings - Execute After Trips
-- =====================================================
INSERT INTO DriverRatings (TripID, DriverID, UserID, Rating, RatedAt, CreatedDate) VALUES 
(1, 1, 1, 5, '2024-01-15 09:20:00', GETDATE()),
(2, 2, 2, 4, '2024-01-15 15:05:00', GETDATE()),
(3, 1, 3, 5, '2024-01-16 10:50:00', GETDATE()),
(4, 3, 4, 4, '2024-01-16 17:35:00', GETDATE());

-- =====================================================
-- 8. DriverReviews - Execute After Trips
-- =====================================================
INSERT INTO DriverReviews (TripID, DriverID, UserID, ReviewText, CreatedAt) VALUES 
(1, 1, 1, 'Excellent driver, very professional and safe driving', '2024-01-15 09:25:00'),
(2, 2, 2, 'Good service, clean car and on time', '2024-01-15 15:10:00'),
(3, 1, 3, 'Amazing experience, highly recommend this driver', '2024-01-16 10:55:00'),
(4, 3, 4, 'Professional driver, smooth ride to the airport', '2024-01-16 17:40:00');

-- =====================================================
-- 9. DriverEarnings - Execute After Trips
-- =====================================================
INSERT INTO DriverEarnings (DriverID, TripID, Amount, EarnedAt, CreatedDate) VALUES 
(1, 1, 36.40, '2024-01-15 09:15:00', GETDATE()),
(2, 2, 52.00, '2024-01-15 15:00:00', GETDATE()),
(1, 3, 64.20, '2024-01-16 10:45:00', GETDATE()),
(3, 4, 96.00, '2024-01-16 17:30:00', GETDATE());

-- =====================================================
-- 10. UserAddresses - Execute After Users
-- =====================================================
INSERT INTO UserAddresses (UserID, Title, AddressText, Latitude, Longitude, CreatedDate) VALUES 
(1, 'Home', 'Apartment 15, Building 20, Nasr City, Cairo', 30.0444, 31.2357, GETDATE()),
(1, 'Work', 'Office Tower, Downtown Cairo', 30.0626, 31.2497, GETDATE()),
(2, 'Home', 'Villa 25, Maadi, Cairo', 29.9602, 31.2569, GETDATE()),
(3, 'Home', 'Flat 8, Heliopolis, Cairo', 30.0808, 31.3230, GETDATE()),
(4, 'Home', 'House 12, New Cairo', 30.0131, 31.4969, GETDATE());

-- =====================================================
-- 11. UserCreditCards - Execute After Users
-- =====================================================
INSERT INTO UserCreditCards (UserID, CardHolderName, CardNumberMasked, ExpiryMonth, ExpiryYear, CreatedDate) VALUES 
(1, 'Ahmed Ali', '**** **** **** 1234', 12, 2026, GETDATE()),
(2, 'Sara Mohamed', '**** **** **** 5678', 8, 2025, GETDATE()),
(3, 'Omar Hassan', '**** **** **** 9012', 3, 2027, GETDATE()),
(4, 'Fatma Ibrahim', '**** **** **** 3456', 11, 2026, GETDATE());

-- =====================================================
-- 12. UserNotificationSettings - Execute After Users
-- =====================================================
INSERT INTO UserNotificationSettings (UserID, AllowPromotions, AllowTripUpdates, AllowNews, CreatedDate) VALUES 
(1, 1, 1, 1, GETDATE()),
(2, 0, 1, 1, GETDATE()),
(3, 1, 1, 0, GETDATE()),
(4, 1, 1, 1, GETDATE()),
(5, 0, 1, 0, GETDATE());

-- =====================================================
-- 13. OtpRequests - Execute After Users
-- =====================================================
INSERT INTO OtpRequests (UserID, PhoneNumber, OTPCode, ExpirationTime, IsUsed, RequestIP, RequestSource, CreatedDate) VALUES 
(1, '+201234567890', '123456', DATEADD(MINUTE, 5, GETDATE()), 1, '192.168.1.100', 'Mobile App', GETDATE()),
(2, '+201234567891', '789012', DATEADD(MINUTE, 5, GETDATE()), 0, '192.168.1.101', 'Web App', GETDATE());

-- =====================================================
-- 14. SmsLogs - Execute After OtpRequests
-- =====================================================
INSERT INTO SmsLogs (PhoneNumber, MessageBody, IsSuccess, SentAt, ProviderResponse, OTPID, CreatedDate) VALUES 
('+201234567890', 'Your OTP code is: 123456', 1, GETDATE(), 'Message sent successfully', 1, GETDATE()),
('+201234567891', 'Your OTP code is: 789012', 1, GETDATE(), 'Message sent successfully', 2, GETDATE());

-- =====================================================
-- 15. Notifications - Execute After Users
-- =====================================================
INSERT INTO Notifications (UserID, Title, Message, IsRead, SentAt, CreatedDate) VALUES 
(1, 'Trip Completed', 'Your trip from Nasr City to Downtown has been completed successfully', 1, '2024-01-15 09:20:00', GETDATE()),
(2, 'Payment Received', 'Payment of 65.00 EGP has been processed for your trip', 0, '2024-01-15 15:05:00', GETDATE()),
(3, 'Driver Assigned', 'Driver Hassan Ahmed has been assigned to your trip', 1, '2024-01-16 10:00:00', GETDATE()),
(4, 'Trip Started', 'Your trip to Cairo Airport has started', 0, '2024-01-16 16:30:00', GETDATE());

-- =====================================================
-- 16. Promotions - Independent Table
-- =====================================================
INSERT INTO Promotions (Code, Description, DiscountAmount, ExpiryDate, MaxUsageCount, UsedCount, IsActive) VALUES 
('WELCOME10', 'Welcome bonus - 10 EGP off your first ride', 10.00, '2024-12-31', 1000, 0, 1),
('SAVE20', '20 EGP discount on rides above 100 EGP', 20.00, '2024-06-30', 500, 0, 1),
('WEEKEND15', 'Weekend special - 15 EGP off', 15.00, '2024-12-31', 2000, 0, 1),
('NEWUSER25', 'New user special - 25 EGP off', 25.00, '2024-12-31', 1500, 0, 1);

-- =====================================================
-- 17. SupportTickets - Execute After Users and Admins
-- =====================================================
INSERT INTO SupportTickets (UserID, Subject, Message, Status, CreatedDate) VALUES 
(1, 'Payment Issue', 'I was charged twice for the same trip', 'Open', GETDATE()),
(2, 'Driver Complaint', 'Driver was late and unprofessional', 'In Progress', GETDATE()),
(3, 'App Bug', 'The app crashes when I try to book a ride', 'Resolved', GETDATE()),
(4, 'Refund Request', 'Please refund my cancelled trip', 'Open', GETDATE());

-- =====================================================
-- 18. FAQs - Independent Table
-- =====================================================
INSERT INTO FAQs (Question, Answer, IsActive, SortOrder, CreatedDate) VALUES 
('How do I book a ride?', 'Open the app, enter your destination, select a driver, and confirm your booking.', 1, 1, GETDATE()),
('What payment methods are accepted?', 'We accept cash, credit cards, and digital wallets.', 1, 2, GETDATE()),
('How is the fare calculated?', 'Fare is calculated based on distance, time, and current demand.', 1, 3, GETDATE()),
('Can I cancel my ride?', 'Yes, you can cancel your ride before the driver arrives without any charges.', 1, 4, GETDATE()),
('How do I contact customer support?', 'You can reach us through the app''s support section or call our hotline.', 1, 5, GETDATE());

-- =====================================================
-- 19. Blogs - Independent Table
-- =====================================================
INSERT INTO Blogs (Title, Content, ImageUrl, PublishedAt, IsPublished, CreatedDate) VALUES 
('Welcome to Our New Taxi Service', 'We are excited to introduce our new taxi booking platform that connects passengers with professional drivers across the city. Our service focuses on safety, reliability, and customer satisfaction.', '/images/blog1.jpg', GETDATE(), 1, GETDATE()),
('Safety First: Our Driver Training Program', 'Learn about our comprehensive driver training and safety protocols. Every driver undergoes extensive background checks and professional training to ensure your safety and comfort.', '/images/blog2.jpg', GETDATE(), 1, GETDATE()),
('Eco-Friendly Transportation Options', 'Discover our commitment to environmental sustainability through our eco-friendly vehicle options and carbon offset programs.', '/images/blog3.jpg', GETDATE(), 1, GETDATE());

-- =====================================================
-- 20. Announcements - Independent Table
-- =====================================================
INSERT INTO Announcements (Title, Content, PublishedAt, CreatedDate) VALUES 
('Service Update', 'We have improved our app performance and added new features including real-time tracking and improved payment options.', GETDATE(), GETDATE()),
('Holiday Schedule', 'Please note our modified service hours during the upcoming holidays. We will continue to serve you with slightly adjusted schedules.', GETDATE(), GETDATE()),
('New Coverage Areas', 'We have expanded our service to include new areas in the city. Check the app for updated coverage maps.', GETDATE(), GETDATE());

-- =====================================================
-- 21. AnnouncementRead - Execute After Users and Announcements
-- =====================================================
INSERT INTO AnnouncementRead (userId, announcementId, createdDate) VALUES 
(1, 1, GETDATE()),
(1, 2, GETDATE()),
(2, 1, GETDATE()),
(3, 1, GETDATE()),
(4, 2, GETDATE());

-- =====================================================
-- 22. AppVersions - Independent Table
-- =====================================================
INSERT INTO AppVersions (Platform, VersionNumber, IsMandatory, ReleaseNotes, ReleaseDate) VALUES 
('iOS', '1.0.0', 1, 'Initial release with basic booking functionality', GETDATE()),
('Android', '1.0.0', 1, 'Initial release with basic booking functionality', GETDATE()),
('iOS', '1.1.0', 0, 'Added payment integration and improved UI', GETDATE()),
('Android', '1.1.0', 0, 'Added payment integration and improved UI', GETDATE());

-- =====================================================
-- VERIFICATION QUERIES
-- Run these to verify your data was inserted correctly
-- =====================================================

-- Check record counts
SELECT 'AdminRoles' as TableName, COUNT(*) as RecordCount FROM AdminRoles
UNION ALL
SELECT 'Admins', COUNT(*) FROM Admins
UNION ALL
SELECT 'Users', COUNT(*) FROM Users
UNION ALL
SELECT 'Drivers', COUNT(*) FROM Drivers
UNION ALL
SELECT 'DriverDetails', COUNT(*) FROM DriverDetails
UNION ALL
SELECT 'Trips', COUNT(*) FROM Trips
UNION ALL
SELECT 'DriverRatings', COUNT(*) FROM DriverRatings
UNION ALL
SELECT 'DriverReviews', COUNT(*) FROM DriverReviews
UNION ALL
SELECT 'DriverEarnings', COUNT(*) FROM DriverEarnings
UNION ALL
SELECT 'UserAddresses', COUNT(*) FROM UserAddresses
UNION ALL
SELECT 'UserCreditCards', COUNT(*) FROM UserCreditCards
UNION ALL
SELECT 'UserNotificationSettings', COUNT(*) FROM UserNotificationSettings
UNION ALL
SELECT 'OtpRequests', COUNT(*) FROM OtpRequests
UNION ALL
SELECT 'SmsLogs', COUNT(*) FROM SmsLogs
UNION ALL
SELECT 'Notifications', COUNT(*) FROM Notifications
UNION ALL
SELECT 'Promotions', COUNT(*) FROM Promotions
UNION ALL
SELECT 'SupportTickets', COUNT(*) FROM SupportTickets
UNION ALL
SELECT 'FAQs', COUNT(*) FROM FAQs
UNION ALL
SELECT 'Blogs', COUNT(*) FROM Blogs
UNION ALL
SELECT 'Announcements', COUNT(*) FROM Announcements
UNION ALL
SELECT 'AnnouncementRead', COUNT(*) FROM AnnouncementRead
UNION ALL
SELECT 'AppVersions', COUNT(*) FROM AppVersions
ORDER BY TableName;

-- =====================================================
-- END OF SAMPLE DATA SCRIPT
-- =====================================================
