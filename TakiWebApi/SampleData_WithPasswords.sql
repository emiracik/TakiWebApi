-- ===============================================
-- TakiWebApi - Sample Data with Hashed Passwords
-- ===============================================

-- Clear existing data
DELETE FROM Users;
DELETE FROM Drivers;

-- Reset identity seeds
DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('Drivers', RESEED, 0);

-- ===============================================
-- SAMPLE USERS WITH HASHED PASSWORDS
-- ===============================================

-- Password hashes for common passwords (SHA256):
-- "password123" = "EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F"
-- "user123" = "6CA13D52CA70C883E0F0BB101E425A89E8624DE51DB2D2392593AF6A84118090"
-- "123456" = "8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92"

INSERT INTO Users (FullName, PhoneNumber, Email, PasswordHash, IsActive, CreatedDate, IsDeleted) VALUES
-- User with password "password123"
('Ahmed Hassan', '05551234567', 'ahmed@example.com', 'EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F', 1, GETDATE(), 0),

-- User with password "user123"  
('Sara Mohammed', '05559876543', 'sara@example.com', '6CA13D52CA70C883E0F0BB101E425A89E8624DE51DB2D2392593AF6A84118090', 1, GETDATE(), 0),

-- User with password "123456"
('Omar Ali', '05551111111', 'omar@example.com', '8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92', 1, GETDATE(), 0),

-- User with password "password123"
('Fatima Khalil', '05552222222', 'fatima@example.com', 'EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F', 1, GETDATE(), 0),

-- User with password "user123"
('Youssef Nasser', '05553333333', 'youssef@example.com', '6CA13D52CA70C883E0F0BB101E425A89E8624DE51DB2D2392593AF6A84118090', 1, GETDATE(), 0);

-- ===============================================
-- SAMPLE DRIVERS WITH HASHED PASSWORDS
-- ===============================================

-- Password hashes for driver passwords:
-- "driver123" = "F6E0A1E2AC41945A9AA7FF8A8AAA0CEBC12A3BCC981A929AD5CF810A090E11AE"
-- "password123" = "EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F"

INSERT INTO Drivers (FullName, PhoneNumber, Email, PasswordHash, VehiclePlate, VehicleModel, VehicleColor, CreatedDate, IsDeleted) VALUES
-- Driver with password "driver123"
('Mohammed Abdullah', '05556789012', 'mohammed.driver@example.com', 'F6E0A1E2AC41945A9AA7FF8A8AAA0CEBC12A3BCC981A929AD5CF810A090E11AE', 'ABC123', 'Toyota Camry', 'White', GETDATE(), 0),

-- Driver with password "driver123"
('Hassan Rashid', '05554567890', 'hassan.driver@example.com', 'F6E0A1E2AC41945A9AA7FF8A8AAA0CEBC12A3BCC981A929AD5CF810A090E11AE', 'XYZ789', 'Honda Civic', 'Black', GETDATE(), 0),

-- Driver with password "password123"
('Khalil Ahmed', '05557777777', 'khalil.driver@example.com', 'EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F', 'DEF456', 'Nissan Altima', 'Silver', GETDATE(), 0),

-- Driver with password "driver123"
('Amira Said', '05558888888', 'amira.driver@example.com', 'F6E0A1E2AC41945A9AA7FF8A8AAA0CEBC12A3BCC981A929AD5CF810A090E11AE', 'GHI789', 'Hyundai Elantra', 'Blue', GETDATE(), 0),

-- Driver with password "123456"
('Nour Hassan', '05559999999', 'nour.driver@example.com', '8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92', 'JKL012', 'Kia Cerato', 'Red', GETDATE(), 0);

-- ===============================================
-- VERIFICATION QUERIES
-- ===============================================

-- Check inserted users
SELECT UserID, FullName, PhoneNumber, Email, 
       CASE 
           WHEN PasswordHash = 'EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F' THEN 'password123'
           WHEN PasswordHash = '6CA13D52CA70C883E0F0BB101E425A89E8624DE51DB2D2392593AF6A84118090' THEN 'user123'
           WHEN PasswordHash = '8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92' THEN '123456'
           ELSE 'Unknown'
       END as PlainPassword,
       IsActive, CreatedDate
FROM Users
ORDER BY UserID;

-- Check inserted drivers
SELECT DriverID, FullName, PhoneNumber, Email,
       CASE 
           WHEN PasswordHash = 'F6E0A1E2AC41945A9AA7FF8A8AAA0CEBC12A3BCC981A929AD5CF810A090E11AE' THEN 'driver123'
           WHEN PasswordHash = 'EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F' THEN 'password123'
           WHEN PasswordHash = '8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92' THEN '123456'
           ELSE 'Unknown'
       END as PlainPassword,
       VehiclePlate, VehicleModel, VehicleColor, CreatedDate
FROM Drivers  
ORDER BY DriverID;


-- FAQs
INSERT INTO FAQs (Question, Answer, CreatedDate)
VALUES 
('Uygulama nasıl kullanılır?', 'Mobil uygulamayı indirip giriş yapmanız yeterli.', GETDATE()),
('İptal ücreti ne kadar?', 'Yolculuk başlamadan iptal ücretsizdir.', GETDATE()),
('Şoför bilgilerini nereden görebilirim?', 'Rezervasyon sonrası ekranınızda görünür.', GETDATE());

-- Notifications
INSERT INTO Notifications (UserID, Title, Message, NotificationType, IsRead, CreatedDate)
VALUES 
(1, 'Yeni sürüşünüz puanlandı!', 'Sürüşünüz başarıyla tamamlandı.', 'Info', 0, GETDATE()),
(2, 'Hesabınıza yeni promosyon tanımlandı.', 'Hoşgeldin indiriminizi kullanabilirsiniz.', 'Promotion', 0, GETDATE()),
(3, 'Destek talebiniz yanıtlandı.', 'Talebiniz incelendi ve çözüldü.', 'Support', 1, GETDATE());

-- Promotions
INSERT INTO Promotions (Title, Description, DiscountType, DiscountValue, StartDate, EndDate, CreatedDate)
VALUES 
('Hoşgeldin İndirimi', 'İlk yolculukta %30 indirim!', 'Percentage', 30, GETDATE(), DATEADD(DAY, 30, GETDATE()), GETDATE()),
('Arkadaşını Getir', 'Arkadaşın kayıt olursa 25 TL kazan!', 'FixedAmount', 25, GETDATE(), DATEADD(DAY, 60, GETDATE()), GETDATE()),
('Hafta Sonuna Özel', 'Cumartesi ve Pazar geçerli %15 indirim.', 'Percentage', 15, GETDATE(), DATEADD(DAY, 15, GETDATE()), GETDATE());

-- SupportTickets
INSERT INTO SupportTickets (UserID, Subject, Description, Status, Priority, CreatedDate)
VALUES 
(1, 'Şoför Geç Geldi', 'Bekleme süresi çok uzundu.', 'Open', 'Medium', GETDATE()),
(2, 'Yanlış ücret', 'Benden fazla ücret kesildi.', 'Closed', 'High', GETDATE()),
(3, 'Uygulama hatası', 'Yolculuk görünmüyor.', 'Open', 'Low', GETDATE());

-- Trips
INSERT INTO Trips (PassengerID, DriverID, StartAddress, EndAddress, StartTime, EndTime, Cost, PaymentMethod, CreatedDate)
VALUES 
(1, 1, 'Beşiktaş', 'Şişli', GETDATE(), DATEADD(MINUTE, 30, GETDATE()), 45.50, 'Cash', GETDATE()),
(2, 2, 'Kadıköy', 'Bostancı', GETDATE(), DATEADD(MINUTE, 20, GETDATE()), 32.00, 'CreditCard', GETDATE()),
(3, 1, 'Üsküdar', 'Taksim', GETDATE(), DATEADD(MINUTE, 25, GETDATE()), 67.75, 'Cash', GETDATE());

-- UserAddresses
INSERT INTO UserAddresses (UserID, AddressName, FullAddress, Latitude, Longitude, IsDefault, AddressType, CreatedDate)
VALUES 
(1, 'Ev', 'Cumhuriyet Mah. No:12, İstanbul', 41.01224, 28.97602, 1, 'Home', GETDATE()),
(2, 'İş', 'Atatürk Bulvarı No:45, Ankara', 39.92077, 32.85411, 0, 'Work', GETDATE()),
(3, 'Diğer', 'İzmir Cad. No:3, İzmir', 38.4192, 27.1287, 0, 'Other', GETDATE());

-- DriverRatings
INSERT INTO DriverRatings (TripID, DriverID, UserID, Rating, Comment, CreatedDate)
VALUES 
(1, 1, 1, 5, 'Harika sürüş', GETDATE()),
(2, 1, 2, 4, 'Güzel ama biraz hızlıydı', GETDATE()),
(3, 2, 3, 3, 'Orta seviye', GETDATE());

-- ===============================================
-- DEMO LOGIN CREDENTIALS SUMMARY
-- ===============================================

/*
USERS (for /api/Auth/login):
Phone: 05551234567, Password: password123 (Ahmed Hassan)
Phone: 05559876543, Password: user123 (Sara Mohammed)  
Phone: 05551111111, Password: 123456 (Omar Ali)
Phone: 05552222222, Password: password123 (Fatima Khalil)
Phone: 05553333333, Password: user123 (Youssef Nasser)

DRIVERS (for /api/Auth/driver-login):
Phone: 05556789012, Password: driver123 (Mohammed Abdullah)
Phone: 05554567890, Password: driver123 (Hassan Rashid)
Phone: 05557777777, Password: password123 (Khalil Ahmed)
Phone: 05558888888, Password: driver123 (Amira Said)
Phone: 05559999999, Password: 123456 (Nour Hassan)
*/
