-- ===============================================
-- TaksiWebApi - Sample SQL INSERT Statements
-- Based on existing TaxiDb structure
-- ===============================================

-- ===============================================
-- USERS TABLE (matching existing structure)
-- ===============================================

-- Insert sample users
INSERT INTO Users (FullName, PhoneNumber, Email, IsActive, CreatedDate)
VALUES 
    ('Ahmet Yılmaz', '+905551234567', 'ahmet.yilmaz@email.com', 1, GETDATE()),
    ('Fatma Demir', '+905551234568', 'fatma.demir@email.com', 1, GETDATE()),
    ('Mehmet Çelik', '+905551234569', 'mehmet.celik@email.com', 1, GETDATE()),
    ('Ayşe Kaya', '+905551234570', 'ayse.kaya@email.com', 1, GETDATE()),
    ('Ali Öztürk', '+905551234571', 'ali.ozturk@email.com', 0, GETDATE()),
    ('Deniz Kılıç', '+905551234572', 'deniz.kilic@email.com', 1, GETDATE()),
    ('Selin Aydın', '+905551234573', 'selin.aydin@email.com', 1, GETDATE()),
    ('Burak Tan', '+905551234574', 'burak.tan@email.com', 1, GETDATE()),
    ('Gizem Doğan', '+905551234575', 'gizem.dogan@email.com', 1, GETDATE()),
    ('Kerem Yıldız', '+905551234576', 'kerem.yildiz@email.com', 1, GETDATE());

-- ===============================================
-- DRIVERS TABLE (matching existing structure)
-- ===============================================

-- Insert sample drivers
INSERT INTO Drivers (FullName, PhoneNumber, Email, VehiclePlate, VehicleModel, VehicleColor, CreatedDate)
VALUES 
    ('Mustafa Şahin', '+905557654321', 'mustafa.sahin@email.com', '34ABC123', 'Toyota Corolla', 'Beyaz', GETDATE()),
    ('Elif Arslan', '+905557654322', 'elif.arslan@email.com', '06DEF456', 'Honda Civic', 'Gri', GETDATE()),
    ('Hasan Kurt', '+905557654323', 'hasan.kurt@email.com', '35GHI789', 'Volkswagen Passat', 'Siyah', GETDATE()),
    ('Zeynep Güler', '+905557654324', 'zeynep.guler@email.com', '16JKL012', 'Renault Megane', 'Mavi', GETDATE()),
    ('Emre Polat', '+905557654325', 'emre.polat@email.com', '07MNO345', 'Ford Focus', 'Kırmızı', GETDATE()),
    ('Okan Vural', '+905557654326', 'okan.vural@email.com', '41PQR678', 'Skoda Octavia', 'Gümüş', GETDATE()),
    ('Nilgün Erman', '+905557654327', 'nilgun.erman@email.com', '26STU901', 'Peugeot 301', 'Beyaz', GETDATE()),
    ('Serkan Bayram', '+905557654328', 'serkan.bayram@email.com', '58VWX234', 'Hyundai Accent', 'Siyah', GETDATE());

-- ===============================================
-- TRIPS TABLE (matching existing structure)
-- ===============================================

-- Insert sample trips
INSERT INTO Trips (PassengerID, DriverID, StartAddress, EndAddress, StartLatitude, StartLongitude, EndLatitude, EndLongitude, StartTime, EndTime, Cost, PaymentMethod, CreatedDate)
VALUES 
    (1, 1, 'Taksim Meydanı, İstanbul', 'Atatürk Havalimanı, İstanbul', 41.0369, 28.9840, 40.9769, 28.8189, DATEADD(hour, -2, GETDATE()), DATEADD(hour, -1, GETDATE()), 85.50, 'Kredi Kartı', GETDATE()),
    (2, 2, 'Kızılay, Ankara', 'Esenboğa Havalimanı, Ankara', 39.9208, 32.8541, 40.1281, 32.9951, DATEADD(hour, -4, GETDATE()), DATEADD(hour, -3, GETDATE()), 65.00, 'Nakit', GETDATE()),
    (3, 3, 'Konak, İzmir', 'Adnan Menderes Havalimanı, İzmir', 38.4189, 27.1287, 38.2924, 27.1569, DATEADD(hour, -6, GETDATE()), DATEADD(hour, -5, GETDATE()), 75.25, 'Kredi Kartı', GETDATE()),
    (4, 4, 'Osmangazi, Bursa', 'Mudanya, Bursa', 40.1885, 29.0610, 40.3686, 28.8794, DATEADD(hour, -8, GETDATE()), DATEADD(hour, -7, GETDATE()), 45.00, 'Nakit', GETDATE()),
    (1, 5, 'Beşiktaş, İstanbul', 'Kadıköy, İstanbul', 41.0422, 29.0084, 40.9833, 29.0333, DATEADD(hour, -10, GETDATE()), DATEADD(hour, -9, GETDATE()), 35.75, 'Banka Kartı', GETDATE()),
    (5, 6, 'Çankaya, Ankara', 'Ulus, Ankara', 39.9208, 32.8541, 39.9388, 32.8597, DATEADD(hour, -12, GETDATE()), DATEADD(hour, -11, GETDATE()), 28.50, 'Dijital Cüzdan', GETDATE()),
    (6, 7, 'Bornova, İzmir', 'Alsancak, İzmir', 38.4697, 27.2112, 38.4391, 27.1434, DATEADD(hour, -14, GETDATE()), DATEADD(hour, -13, GETDATE()), 32.00, 'Kredi Kartı', GETDATE()),
    (7, 8, 'Nilüfer, Bursa', 'Osmangazi, Bursa', 40.2039, 28.9869, 40.1885, 29.0610, DATEADD(hour, -16, GETDATE()), DATEADD(hour, -15, GETDATE()), 22.75, 'Nakit', GETDATE());

-- ===============================================
-- ANNOUNCEMENTS TABLE (matching existing structure)
-- ===============================================

-- Insert sample announcements
INSERT INTO Announcements (Title, Content, PublishedAt, CreatedDate)
VALUES 
    ('Yeni Güncelleme Yayınlandı', 'Uygulamamızın yeni versiyonu artık mevcut. Lütfen güncellemeyi indirin.', GETDATE(), GETDATE()),
    ('Bakım Çalışması Duyurusu', 'Sistem bakımı nedeniyle 15 Ağustos 02:00-04:00 saatleri arasında hizmet verilemeyecektir.', DATEADD(day, -1, GETDATE()), DATEADD(day, -1, GETDATE())),
    ('Yeni Özellik: Favori Adresler', 'Artık sık kullandığınız adresleri favorilerinize ekleyebilirsiniz!', DATEADD(day, -3, GETDATE()), DATEADD(day, -3, GETDATE())),
    ('İndirim Kampanyası', 'Bu hafta tüm yolculuklarda %20 indirim! Kaçırmayın.', DATEADD(day, -5, GETDATE()), DATEADD(day, -5, GETDATE())),
    ('Müşteri Hizmetleri Saatleri', 'Müşteri hizmetlerimiz hafta içi 09:00-18:00 saatleri arasında hizmetinizdedir.', DATEADD(day, -7, GETDATE()), DATEADD(day, -7, GETDATE()));

-- ===============================================
-- BLOGS TABLE (matching existing structure)
-- ===============================================

-- Insert sample blogs
INSERT INTO Blogs (Title, Content, ImageUrl, PublishedAt, IsPublished, CreatedDate)
VALUES 
    ('Güvenli Taksi Yolculuğu İçin İpuçları', 'Taksi ile seyahat ederken dikkat edilmesi gereken önemli güvenlik kuralları ve tavsiyeleri. Seyahatinizi daha güvenli hale getirmek için bu ipuçlarını takip edin.', '/images/blog/guvenli-yolculuk.jpg', GETDATE(), 1, GETDATE()),
    ('Şehir İçi Ulaşımda Tasarruf Yöntemleri', 'Şehir içi ulaşımda bütçenizi korumak için pratik öneriler ve ekonomik seyahat ipuçları. Akıllı seçimlerle tasarruf yapmanın yolları.', '/images/blog/tasarruf.jpg', DATEADD(day, -2, GETDATE()), 1, DATEADD(day, -2, GETDATE())),
    ('Taksi Uygulaması Kullanım Rehberi', 'Uygulamamızı en verimli şekilde nasıl kullanacağınızı öğrenin. Tüm özellikler ve kullanım ipuçları ile taksi çağırmak artık çok kolay.', '/images/blog/kullanim-rehberi.jpg', DATEADD(day, -5, GETDATE()), 1, DATEADD(day, -5, GETDATE())),
    ('Çevre Dostu Ulaşım Seçenekleri', 'Çevreye duyarlı ulaşım alternatifleri ve hibrit araçlar hakkında bilmeniz gerekenler. Doğa dostu seyahat seçenekleri.', '/images/blog/cevre-dostu.jpg', DATEADD(day, -7, GETDATE()), 0, DATEADD(day, -7, GETDATE())),
    ('Gece Taksi Kullanımı', 'Gece saatlerinde taksi kullanırken güvenliğinizi sağlama yolları ve dikkat edilmesi gereken noktalar.', '/images/blog/gece-taksi.jpg', DATEADD(day, -10, GETDATE()), 1, DATEADD(day, -10, GETDATE()));

-- ===============================================
-- FAQs TABLE (matching existing structure)
-- ===============================================

-- Insert sample FAQs
INSERT INTO FAQs (Question, Answer, IsActive, SortOrder, CreatedDate)
VALUES 
    ('Nasıl taksi çağırabilirim?', 'Uygulamamızı açın, konumunuzu belirleyin ve "Taksi Çağır" butonuna tıklayın. En yakın sürücü size yönlendirilecektir.', 1, 1, GETDATE()),
    ('Ödeme nasıl yapılır?', 'Nakit, kredi kartı, banka kartı veya dijital cüzdan ile ödeme yapabilirsiniz. Ödeme yöntemini yolculuk öncesi seçebilirsiniz.', 1, 2, GETDATE()),
    ('Yolculuğumu iptal edebilir miyim?', 'Evet, sürücü henüz size ulaşmadıysa yolculuğunuzu ücretsiz olarak iptal edebilirsiniz.', 1, 3, GETDATE()),
    ('Şikayetimi nasıl iletebilirim?', 'Uygulama içindeki "Destek" bölümünden veya müşteri hizmetleri numaramızdan şikayetinizi iletebilirsiniz.', 1, 4, GETDATE()),
    ('Fatura alabilir miyim?', 'Evet, yolculuk sonrası uygulama üzerinden e-fatura talep edebilirsiniz.', 1, 5, GETDATE()),
    ('Çocuk koltuğu talep edebilir miyim?', 'Evet, taksi çağırırken özel talep olarak çocuk koltuğu seçeneğini işaretleyebilirsiniz.', 1, 6, GETDATE()),
    ('Havalimanı transferi var mı?', 'Evet, tüm büyük havalimanlarına özel transfer hizmetimiz bulunmaktadır.', 1, 7, GETDATE()),
    ('Uygulama ücretsiz mi?', 'Evet, uygulamamızı indirmek ve kullanmak tamamen ücretsizdir. Sadece kullandığınız taksi hizmeti için ödeme yaparsınız.', 1, 8, GETDATE()),
    ('Sürücü puanlama sistemi nasıl çalışır?', 'Her yolculuk sonrası sürücünüzü 1-5 yıldız arasında puanlayabilirsiniz. Bu puanlar diğer kullanıcılara yardımcı olur.', 1, 9, GETDATE()),
    ('Kayıp eşyam için ne yapmalıyım?', 'Uygulama içindeki "Kayıp Eşya" bölümünden bildirim yapabilir veya müşteri hizmetlerimizi arayabilirsiniz.', 1, 10, GETDATE());

-- ===============================================
-- ADDITIONAL SAMPLE DATA FOR TESTING
-- ===============================================

-- Insert driver ratings (if DriverRatings table exists)
-- INSERT INTO DriverRatings (TripID, DriverID, UserID, Rating, RatedAt, CreatedDate) VALUES 
-- (1, 1, 1, 5, DATEADD(minute, 10, DATEADD(hour, -1, GETDATE())), GETDATE()),
-- (2, 2, 2, 4, DATEADD(minute, 15, DATEADD(hour, -3, GETDATE())), GETDATE()),
-- (3, 3, 3, 5, DATEADD(minute, 20, DATEADD(hour, -5, GETDATE())), GETDATE());

-- Insert user addresses (if UserAddresses table exists)
-- INSERT INTO UserAddresses (UserID, Title, AddressText, Latitude, Longitude, CreatedDate) VALUES 
-- (1, 'Ev', 'Taksim Mahallesi, Gümüşsuyu Caddesi No:15, Beyoğlu/İstanbul', 41.0369, 28.9840, GETDATE()),
-- (1, 'İş', 'Levent Mahallesi, Büyükdere Caddesi No:100, Şişli/İstanbul', 41.0766, 29.0142, GETDATE()),
-- (2, 'Ev', 'Kızılay Mahallesi, Atatürk Bulvarı No:50, Çankaya/Ankara', 39.9208, 32.8541, GETDATE());

-- Insert promotions (if Promotions table exists)
-- INSERT INTO Promotions (Code, Description, DiscountAmount, ExpiryDate, MaxUsageCount, UsedCount, IsActive) VALUES 
-- ('HOSGELDIN10', 'Hoş geldin bonusu - İlk yolculuğunuzda 10 TL indirim', 10.00, '2024-12-31', 1000, 0, 1),
-- ('TASARRUF20', '100 TL üzeri yolculuklarda 20 TL indirim', 20.00, '2024-06-30', 500, 0, 1),
-- ('HAFTASONU15', 'Hafta sonu özel - 15 TL indirim', 15.00, '2024-12-31', 2000, 0, 1);

-- ===============================================
-- VERIFICATION QUERIES
-- ===============================================

-- Check record counts for main tables
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users WHERE IsDeleted = 0
UNION ALL
SELECT 'Drivers', COUNT(*) FROM Drivers WHERE IsDeleted = 0
UNION ALL
SELECT 'Trips', COUNT(*) FROM Trips WHERE IsDeleted = 0
UNION ALL
SELECT 'Announcements', COUNT(*) FROM Announcements WHERE IsDeleted = 0
UNION ALL
SELECT 'Blogs', COUNT(*) FROM Blogs WHERE IsDeleted = 0
UNION ALL
SELECT 'FAQs', COUNT(*) FROM FAQs WHERE IsDeleted = 0
ORDER BY TableName;

-- ===============================================
-- TEST QUERIES FOR API ENDPOINTS
-- ===============================================

-- Test Users queries
-- SELECT * FROM Users WHERE IsDeleted = 0 ORDER BY CreatedDate DESC;
-- SELECT * FROM Users WHERE FullName LIKE '%Ahmet%';
-- SELECT COUNT(*) as ActiveUsers FROM Users WHERE IsActive = 1 AND IsDeleted = 0;

-- Test Drivers queries  
-- SELECT * FROM Drivers WHERE IsDeleted = 0 ORDER BY CreatedDate DESC;
-- SELECT * FROM Drivers WHERE VehiclePlate LIKE '%34%';
-- SELECT COUNT(*) as TotalDrivers FROM Drivers WHERE IsDeleted = 0;

-- Test Trips queries
-- SELECT * FROM Trips WHERE IsDeleted = 0 ORDER BY StartTime DESC;
-- SELECT * FROM Trips WHERE PassengerID = 1;
-- SELECT * FROM Trips WHERE DriverID = 1;
-- SELECT SUM(Cost) as TotalRevenue FROM Trips WHERE IsDeleted = 0;
-- SELECT * FROM Trips WHERE StartTime >= '2024-01-01' AND StartTime <= '2024-12-31';

-- Test Announcements queries
-- SELECT * FROM Announcements WHERE IsDeleted = 0 ORDER BY PublishedAt DESC;
-- SELECT * FROM Announcements WHERE Title LIKE '%güncelleme%';

-- Test Blogs queries
-- SELECT * FROM Blogs WHERE IsPublished = 1 AND IsDeleted = 0 ORDER BY PublishedAt DESC;
-- SELECT * FROM Blogs WHERE Title LIKE '%taksi%';

-- Test FAQs queries
-- SELECT * FROM FAQs WHERE IsActive = 1 AND IsDeleted = 0 ORDER BY SortOrder;
-- SELECT * FROM FAQs WHERE Question LIKE '%ödeme%' OR Answer LIKE '%ödeme%';

-- ===============================================
-- PAGINATION TEST QUERIES
-- ===============================================

-- Test pagination (Page 1, 5 records per page)
-- SELECT * FROM Users WHERE IsDeleted = 0 ORDER BY CreatedDate DESC OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY;

-- Test pagination (Page 2, 5 records per page)  
-- SELECT * FROM Users WHERE IsDeleted = 0 ORDER BY CreatedDate DESC OFFSET 5 ROWS FETCH NEXT 5 ROWS ONLY;

-- ===============================================
-- SUCCESS MESSAGE
-- ===============================================

PRINT '✅ Sample data inserted successfully!';
PRINT '📊 Total Users: ' + CAST((SELECT COUNT(*) FROM Users WHERE IsDeleted = 0) AS VARCHAR(10));
PRINT '🚗 Total Drivers: ' + CAST((SELECT COUNT(*) FROM Drivers WHERE IsDeleted = 0) AS VARCHAR(10));
PRINT '🚖 Total Trips: ' + CAST((SELECT COUNT(*) FROM Trips WHERE IsDeleted = 0) AS VARCHAR(10));
PRINT '📢 Total Announcements: ' + CAST((SELECT COUNT(*) FROM Announcements WHERE IsDeleted = 0) AS VARCHAR(10));
PRINT '📝 Total Blogs: ' + CAST((SELECT COUNT(*) FROM Blogs WHERE IsDeleted = 0) AS VARCHAR(10));
PRINT '❓ Total FAQs: ' + CAST((SELECT COUNT(*) FROM FAQs WHERE IsDeleted = 0) AS VARCHAR(10));
PRINT '';
PRINT '🎉 You can now test your TaksiWebApi endpoints!';
PRINT '🔗 API Base URL: https://localhost:5001/api';
PRINT '';
PRINT 'Sample test URLs:';
PRINT '• GET https://localhost:5001/api/users';
PRINT '• GET https://localhost:5001/api/drivers';
PRINT '• GET https://localhost:5001/api/trips';
PRINT '• GET https://localhost:5001/api/announcements';
PRINT '• GET https://localhost:5001/api/blogs';
PRINT '• GET https://localhost:5001/api/faqs';
