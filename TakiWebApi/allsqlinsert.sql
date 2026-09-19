-- ================================================================
-- TakiWebApi - ÖRNEK VERİ (SEED DATA)
-- allsql.sql ile oluşturulan tablolara insert
-- PasswordHash = BCrypt("Test1234!")
-- ================================================================

USE TaxiDb;
GO

SET IDENTITY_INSERT AdminRoles OFF;

-- ================================================================
-- 1. USERS (5 kullanıcı)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Users)
BEGIN
    INSERT INTO Users (FullName, PhoneNumber, Email, PasswordHash, IsActive, CreatedDate) VALUES
        (N'Ahmet Yılmaz',    '05301234567', 'ahmet@example.com',   '$2a$11$xKp8Q9zL3mN1vR7tY2wOuOHqJ5kP6bE8cF4dG3hI0jA1lM9nK7sT', 1, GETDATE()),
        (N'Ayşe Kaya',       '05302345678', 'ayse@example.com',    '$2a$11$xKp8Q9zL3mN1vR7tY2wOuOHqJ5kP6bE8cF4dG3hI0jA1lM9nK7sT', 1, GETDATE()),
        (N'Mehmet Demir',    '05303456789', 'mehmet@example.com',  '$2a$11$xKp8Q9zL3mN1vR7tY2wOuOHqJ5kP6bE8cF4dG3hI0jA1lM9nK7sT', 1, GETDATE()),
        (N'Fatma Çelik',     '05304567890', 'fatma@example.com',   '$2a$11$xKp8Q9zL3mN1vR7tY2wOuOHqJ5kP6bE8cF4dG3hI0jA1lM9nK7sT', 1, GETDATE()),
        (N'Ali Şahin',       '05305678901', 'ali@example.com',     '$2a$11$xKp8Q9zL3mN1vR7tY2wOuOHqJ5kP6bE8cF4dG3hI0jA1lM9nK7sT', 1, GETDATE());
END
GO

-- ================================================================
-- 2. DRIVERS (4 sürücü)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Drivers)
BEGIN
    INSERT INTO Drivers (FullName, PhoneNumber, Email, PasswordHash, VehiclePlate, VehicleModel, VehicleColor, CreatedDate) VALUES
        (N'Kadir Arslan',    '05321111111', 'kadir@driver.com',  '$2a$11$xKp8Q9zL3mN1vR7tY2wOuOHqJ5kP6bE8cF4dG3hI0jA1lM9nK7sT', '34 ABC 001', 'Toyota Corolla',  N'Beyaz',  GETDATE()),
        (N'Hasan Öztürk',    '05322222222', 'hasan@driver.com',  '$2a$11$xKp8Q9zL3mN1vR7tY2wOuOHqJ5kP6bE8cF4dG3hI0jA1lM9nK7sT', '34 DEF 002', 'Hyundai Elantra', N'Siyah',  GETDATE()),
        (N'İbrahim Kılıç',   '05323333333', 'ibrahim@driver.com','$2a$11$xKp8Q9zL3mN1vR7tY2wOuOHqJ5kP6bE8cF4dG3hI0jA1lM9nK7sT', '34 GHI 003', 'Renault Symbol',  N'Gri',    GETDATE()),
        (N'Murat Yıldız',    '05324444444', 'murat@driver.com',  '$2a$11$xKp8Q9zL3mN1vR7tY2wOuOHqJ5kP6bE8cF4dG3hI0jA1lM9nK7sT', '34 JKL 004', 'Fiat Egea',       N'Beyaz',  GETDATE());
END
GO

-- ================================================================
-- 3. TRIPS (6 yolculuk — Users 1-5, Drivers 1-4)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Trips)
BEGIN
    INSERT INTO Trips (PassengerID, DriverID, StartAddress, EndAddress, StartLatitude, StartLongitude, EndLatitude, EndLongitude, StartTime, EndTime, Cost, PaymentMethod, Distance, Status, CreatedDate) VALUES
        (1, 1, N'Beşiktaş Meydanı',      N'Kadıköy İskele',      41.0428, 29.0094, 40.9905, 29.0231, DATEADD(DAY,-3,GETDATE()), DATEADD(DAY,-3,DATEADD(MINUTE,22,GETDATE())), 85.50,  'Nakit',        8.2,  'completed',  GETDATE()),
        (2, 2, N'Taksim Meydanı',         N'Üsküdar Merkez',      41.0369, 28.9850, 41.0228, 29.0128, DATEADD(DAY,-2,GETDATE()), DATEADD(DAY,-2,DATEADD(MINUTE,18,GETDATE())), 72.00,  'Kart',         6.5,  'completed',  GETDATE()),
        (3, 3, N'Atatürk Havalimanı',     N'Şişli Merkez',        40.9760, 28.8143, 41.0600, 28.9864, DATEADD(DAY,-1,GETDATE()), DATEADD(DAY,-1,DATEADD(MINUTE,35,GETDATE())), 140.00, 'Nakit',       19.3,  'completed',  GETDATE()),
        (4, 1, N'Levent Metro',           N'Maslak AVM',          41.0797, 29.0117, 41.1101, 29.0228, DATEADD(HOUR,-5,GETDATE()), DATEADD(HOUR,-5,DATEADD(MINUTE,12,GETDATE())), 45.00, 'Cüzdan',       3.8,  'completed',  GETDATE()),
        (5, 4, N'Bağcılar Terminal',      N'Bakırköy Sahil',      41.0392, 28.8543, 40.9825, 28.8700, DATEADD(HOUR,-2,GETDATE()), DATEADD(HOUR,-2,DATEADD(MINUTE,20,GETDATE())), 68.75, 'Kart',         7.1,  'completed',  GETDATE()),
        (1, 2, N'Sarıyer İskele',         N'Emirgan Korusu',      41.1667, 29.0500, 41.1108, 29.0561, DATEADD(MINUTE,-30,GETDATE()), NULL,                                       NULL,   'Nakit',        NULL, 'inProgress', GETDATE());
END
GO

-- ================================================================
-- 4. WALLETS (her user için 1 cüzdan)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Wallets)
BEGIN
    INSERT INTO Wallets (UserID, Balance, TotalIn, TotalOut, LastUpdated, CreatedDate) VALUES
        (1, 250.00,  500.00, 250.00, GETDATE(), GETDATE()),
        (2, 120.50,  300.00, 179.50, GETDATE(), GETDATE()),
        (3, 0.00,    150.00, 150.00, GETDATE(), GETDATE()),
        (4, 75.25,   200.00, 124.75, GETDATE(), GETDATE()),
        (5, 400.00,  400.00,   0.00, GETDATE(), GETDATE());
END
GO

-- ================================================================
-- 5. WALLET TRANSACTIONS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM WalletTransactions)
BEGIN
    INSERT INTO WalletTransactions (WalletID, UserID, TransactionType, Amount, Description, TransactionDate) VALUES
        (1, 1, 'Deposit',  500.00, N'Kredi kartı ile yükleme',    DATEADD(DAY,-10,GETDATE())),
        (1, 1, 'Payment',  -85.50, N'Yolculuk #1 ödemesi',        DATEADD(DAY,-3,GETDATE())),
        (1, 1, 'Payment',  -68.75, N'Yolculuk #5 için paylaşım',  DATEADD(HOUR,-2,GETDATE())),
        (2, 2, 'Deposit',  300.00, N'Banka transferi',             DATEADD(DAY,-5,GETDATE())),
        (2, 2, 'Payment',  -72.00, N'Yolculuk #2 ödemesi',        DATEADD(DAY,-2,GETDATE())),
        (3, 3, 'Deposit',  150.00, N'İlk yükleme',                DATEADD(DAY,-7,GETDATE())),
        (3, 3, 'Payment', -150.00, N'Yolculuk #3 ödemesi',        DATEADD(DAY,-1,GETDATE())),
        (4, 4, 'Deposit',  200.00, N'Kredi kartı ile yükleme',    DATEADD(DAY,-4,GETDATE())),
        (4, 4, 'Payment',  -45.00, N'Yolculuk #4 ödemesi',        DATEADD(HOUR,-5,GETDATE())),
        (5, 5, 'Deposit',  400.00, N'Banka transferi',             DATEADD(DAY,-1,GETDATE()));
END
GO

-- ================================================================
-- 6. DRIVER LOCATIONS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM DriverLocations)
BEGIN
    INSERT INTO DriverLocations (DriverID, Latitude, Longitude, IsAvailable, CurrentAddress, LocationTime, Speed, Heading, IsOnline, CreatedDate) VALUES
        (1, 41.0428, 29.0094, 0, N'Beşiktaş Meydanı',  GETUTCDATE(), 0.0,  0.0,  1, GETUTCDATE()),
        (2, 41.0369, 28.9850, 0, N'Taksim Meydanı',     GETUTCDATE(), 32.5, 180.0,1, GETUTCDATE()),
        (3, 40.9760, 28.8143, 1, N'Bağcılar Terminal',  GETUTCDATE(), 0.0,  0.0,  1, GETUTCDATE()),
        (4, 41.0600, 28.9864, 1, N'Şişli Merkez',       GETUTCDATE(), 0.0,  90.0, 1, GETUTCDATE());
END
GO

-- ================================================================
-- 7. DRIVER RATINGS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM DriverRatings)
BEGIN
    INSERT INTO DriverRatings (TripID, DriverID, UserID, Rating, Comment, CreatedDate) VALUES
        (1, 1, 1, 4.5, N'Çok temiz araç, sürücü çok kibardı.',          GETDATE()),
        (2, 2, 2, 5.0, N'Mükemmel sürüş, kesinlikle tavsiye ederim.',   GETDATE()),
        (3, 3, 3, 4.0, N'İyi sürücü ama trafik yüzünden geç kaldı.',    GETDATE()),
        (4, 1, 4, 5.0, N'Süper! Her zaman bu şoförü seçeceğim.',        GETDATE()),
        (5, 4, 5, 3.5, N'Araç biraz eski ama temizdi.',                  GETDATE());
END
GO

-- ================================================================
-- 8. USER RATINGS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM UserRatings)
BEGIN
    INSERT INTO UserRatings (TripID, RatedUserID, RatedByDriverID, Rating, Comment, CreatedDate) VALUES
        (1, 1, 1, 5.0, N'Çok nazik ve saygılı yolcu.',   GETDATE()),
        (2, 2, 2, 4.5, N'Zamanında hazırdı, teşekkürler.',GETDATE()),
        (3, 3, 3, 4.0, N'Güzel yolculuktu.',              GETDATE()),
        (4, 4, 1, 5.0, N'Mükemmel yolcu.',                GETDATE()),
        (5, 5, 4, 4.0, N'İyi bir yolculuktu.',            GETDATE());
END
GO

-- ================================================================
-- 9. TRIP RATINGS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM TripRatings)
BEGIN
    INSERT INTO TripRatings (TripID, RatedByUserID, Rating, Comment, CreatedDate) VALUES
        (1, 1, 4.5, N'Konforlu ve hızlı yolculuktu.',      GETDATE()),
        (2, 2, 5.0, N'Harika bir deneyim!',                GETDATE()),
        (3, 3, 3.5, N'Yolculuk biraz uzun sürdü.',         GETDATE()),
        (4, 4, 5.0, N'Mükemmeldi, tekrar kullanacağım.',   GETDATE()),
        (5, 5, 4.0, N'Gayet iyiydi.',                      GETDATE());
END
GO

-- ================================================================
-- 10. DRIVER REVIEWS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM DriverReviews)
BEGIN
    INSERT INTO DriverReviews (TripID, DriverID, UserID, ReviewText, CreatedAt) VALUES
        (1, 1, 1, N'Kadir Bey gerçekten profesyonel bir şoför. Arabası çok temiz.',  GETDATE()),
        (2, 2, 2, N'Hasan Bey trafik bilgisi çok iyi, kısa yoldan gitti.',           GETDATE()),
        (3, 3, 3, N'İbrahim Bey müzik konusunda yolcuya soran nadir şoförlerden.', GETDATE()),
        (4, 1, 4, N'Yine mükemmel bir yolculuktu Kadir Bey ile.',                   GETDATE());
END
GO

-- ================================================================
-- 11. DRIVER EARNINGS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM DriverEarnings)
BEGIN
    INSERT INTO DriverEarnings (DriverID, TripID, Amount, EarnedAt, CreatedDate) VALUES
        (1, 1, 72.68, DATEADD(DAY,-3,GETDATE()), GETDATE()),  -- %85 komisyon
        (2, 2, 61.20, DATEADD(DAY,-2,GETDATE()), GETDATE()),
        (3, 3,119.00, DATEADD(DAY,-1,GETDATE()), GETDATE()),
        (1, 4, 38.25, DATEADD(HOUR,-5,GETDATE()),GETDATE()),
        (4, 5, 58.44, DATEADD(HOUR,-2,GETDATE()),GETDATE());
END
GO

-- ================================================================
-- 12. DRIVER DETAILS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM DriverDetails)
BEGIN
    INSERT INTO DriverDetails (UserID, VehiclePlate, VehicleModel, VehicleColor, LicenseNumber, ExperienceYears, RatingAverage, CreatedDate) VALUES
        (1, '34 ABC 001', 'Toyota Corolla',  N'Beyaz', 'E-123456', 7, 4.75, GETDATE()),
        (2, '34 DEF 002', 'Hyundai Elantra', N'Siyah', 'E-234567', 4, 4.85, GETDATE()),
        (3, '34 GHI 003', 'Renault Symbol',  N'Gri',   'E-345678', 5, 4.30, GETDATE()),
        (4, '34 JKL 004', 'Fiat Egea',       N'Beyaz', 'E-456789', 3, 4.10, GETDATE());
END
GO

-- ================================================================
-- 13. DRIVER DOCUMENTS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM DriverDocuments)
BEGIN
    INSERT INTO DriverDocuments (DriverID, DocumentType, DocumentUrl, UploadedDate) VALUES
        (1, N'Ehliyet',         '/docs/drivers/1/license.jpg',       GETDATE()),
        (1, N'Ruhsat',          '/docs/drivers/1/registration.jpg',  GETDATE()),
        (1, N'SRC Belgesi',     '/docs/drivers/1/src.jpg',           GETDATE()),
        (2, N'Ehliyet',         '/docs/drivers/2/license.jpg',       GETDATE()),
        (2, N'Ruhsat',          '/docs/drivers/2/registration.jpg',  GETDATE()),
        (3, N'Ehliyet',         '/docs/drivers/3/license.jpg',       GETDATE()),
        (3, N'Sigorta Poliçesi','/docs/drivers/3/insurance.jpg',     GETDATE()),
        (4, N'Ehliyet',         '/docs/drivers/4/license.jpg',       GETDATE());
END
GO

-- ================================================================
-- 14. BANK INFO
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM BankInfo)
BEGIN
    INSERT INTO BankInfo (DriverID, BankName, IBAN, AccountNumber, CreatedDate) VALUES
        (1, N'Ziraat Bankası', 'TR330006100519786457841326', '5197864578', GETDATE()),
        (2, N'İş Bankası',     'TR610006400000143999931201', '1439999312', GETDATE()),
        (3, N'Garanti BBVA',   'TR320006200004503268600001', '4503268600', GETDATE()),
        (4, N'Yapı Kredi',     'TR980006701000000095736311', '9573631100', GETDATE());
END
GO

-- ================================================================
-- 15. ANNOUNCEMENTS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Announcements)
BEGIN
    INSERT INTO Announcements (Title, Content, PublishedAt, CreatedDate) VALUES
        (N'Yeni Fiyat Tarifesi', N'1 Temmuz 2026 itibarıyla yeni fiyat tarifemiz uygulamaya girmektedir. Km başı ücret 8.50₺ olarak güncellenmiştir.',         DATEADD(DAY,-5,GETDATE()), GETDATE()),
        (N'Uygulama Güncellemesi v2.1', N'Yeni sürümümüzde cüzdan ödemesi, favori sürücü ekleme ve rota önizleme özellikleri eklendi.',                         DATEADD(DAY,-3,GETDATE()), GETDATE()),
        (N'Bayram Tatili Hizmetleri', N'Kurban Bayramı süresince 7/24 hizmet vermeye devam ediyoruz. Sürücülerimize bayram tazminatı uygulanacaktır.',          DATEADD(DAY,-1,GETDATE()), GETDATE()),
        (N'Güvenli Yolculuk Kampanyası', N'Bu ay yapılan her 5 yolculuğa 1 bedava yolculuk hakkı kazanıyorsunuz. Detaylar uygulama içinde.',                   GETDATE(),                GETDATE());
END
GO

-- ================================================================
-- 16. ANNOUNCEMENT READ
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM AnnouncementRead)
BEGIN
    INSERT INTO AnnouncementRead (userId, announcementId, createdDate) VALUES
        (1, 1, DATEADD(DAY,-4,GETDATE())),
        (1, 2, DATEADD(DAY,-2,GETDATE())),
        (2, 1, DATEADD(DAY,-4,GETDATE())),
        (3, 1, DATEADD(DAY,-3,GETDATE())),
        (3, 2, DATEADD(DAY,-2,GETDATE())),
        (4, 3, GETDATE());
END
GO

-- ================================================================
-- 17. BLOGS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Blogs)
BEGIN
    INSERT INTO Blogs (Title, Content, ImageUrl, PublishedAt, IsPublished, CreatedDate) VALUES
        (N'Güvenli Taksi Yolculuğu İpuçları',       N'Taksi binerken dikkat etmeniz gereken 10 önemli ipucu...', '/images/blog/safe-taxi.jpg',   DATEADD(DAY,-10,GETDATE()), 1, GETDATE()),
        (N'İstanbul''un En Yoğun Saatleri',         N'Trafiğin en yoğun olduğu saatler ve alternatif rotalar...', '/images/blog/traffic.jpg',    DATEADD(DAY,-5,GETDATE()),  1, GETDATE()),
        (N'Elektrikli Araçlarla Geleceğimiz',       N'Filomuzda elektrikli araç oranını artırmayı planlıyoruz.', '/images/blog/ev-taxi.jpg',     DATEADD(DAY,-2,GETDATE()),  1, GETDATE()),
        (N'Sürücü Olmanın Avantajları',             N'TakiWebApi bünyesinde sürücü olarak çalışmanın faydaları.', '/images/blog/driver.jpg',     NULL,                       0, GETDATE());
END
GO

-- ================================================================
-- 18. FAQs
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM FAQs)
BEGIN
    INSERT INTO FAQs (Question, Answer, IsActive, SortOrder, CreatedDate) VALUES
        (N'Nasıl yolculuk rezervasyonu yapabilirim?',  N'Uygulamayı açıp konumunuzu ve gideceğiniz yeri girdikten sonra "Taksi Bul" butonuna basmanız yeterlidir.',        1, 1, GETDATE()),
        (N'Hangi ödeme yöntemlerini kabul ediyorsunuz?', N'Nakit, kredi/banka kartı ve uygulama içi cüzdan ile ödeme yapabilirsiniz.',                                    1, 2, GETDATE()),
        (N'Yolculuğumu nasıl iptal edebilirim?',       N'Sürücü yola çıkmadan önce iptal ücreti alınmaz. Sürücü yola çıktıktan sonra iptal ücreti 15₺ olarak uygulanır.',  1, 3, GETDATE()),
        (N'Sürücü puanlaması nasıl çalışır?',          N'Yolculuk sonunda 1-5 yıldız arası puanlama yapabilirsiniz. Puanlamalar sürücünün profiline yansır.',              1, 4, GETDATE()),
        (N'Faturamı nasıl alabilirim?',                N'Hesabım > Yolculuklarım bölümünden ilgili yolculuğun faturasını PDF olarak indirebilirsiniz.',                    1, 5, GETDATE()),
        (N'Kaybettiğim eşyamı nasıl bulabilirim?',     N'Uygulama üzerinden sürücünüzle iletişime geçebilir ya da destek hattımızı arayabilirsiniz.',                     1, 6, GETDATE());
END
GO

-- ================================================================
-- 19. NOTIFICATIONS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Notifications)
BEGIN
    INSERT INTO Notifications (Title, Message, UserID, DriverID, NotificationType, IsRead, CreatedDate) VALUES
        (N'Yolculuğunuz Tamamlandı',    N'Beşiktaş → Kadıköy yolculuğunuz tamamlandı. İyi günler!',             1, NULL, 'TripCompleted',   1, DATEADD(DAY,-3,GETDATE())),
        (N'Sürücünüz Yolda',            N'Kadir Arslan 3 dk içinde yanınızda olacak.',                           2, NULL, 'DriverOnTheWay',  1, DATEADD(DAY,-2,GETDATE())),
        (N'Ödeme Alındı',               N'72.00₺ ödemeniz başarıyla alındı.',                                    2, NULL, 'PaymentReceived',  1, DATEADD(DAY,-2,GETDATE())),
        (N'Cüzdanınıza Yükleme Yapıldı',N'500.00₺ cüzdanınıza yüklendi.',                                       1, NULL, 'WalletTopUp',      1, DATEADD(DAY,-10,GETDATE())),
        (N'Yeni Yolculuk Talebi',       N'Beşiktaş''tan yolculuk talebi var. Kabul etmek ister misiniz?',        NULL, 3, 'NewTripRequest',  0, GETDATE()),
        (N'Haftalık Kazanç Özeti',      N'Bu hafta 5 yolculuk tamamladınız. Toplam kazancınız: 349.57₺',         NULL, 1, 'EarningsSummary', 0, GETDATE());
END
GO

-- ================================================================
-- 20. SUPPORT TICKETS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM SupportTickets)
BEGIN
    INSERT INTO SupportTickets (Subject, Description, UserID, DriverID, Status, Priority, AssignedTo, CreatedDate) VALUES
        (N'Yolculuk ücreti fazla kesildi',     N'34 ABC 001 plakalı araç ile yaptığım yolculukta 85₺ yerine 120₺ kesildi.',   1, NULL, 'Open',     'High',   1, DATEADD(DAY,-2,GETDATE())),
        (N'Sürücü rude davrandı',              N'Hasan Öztürk yolculuk sırasında agresif davrandı.',                           2, NULL, 'InReview', 'Medium', 1, DATEADD(DAY,-1,GETDATE())),
        (N'Araçta eşya unuttum',               N'Telefon şarj kablosunu araçta unuttum.',                                      3, NULL, 'Resolved', 'Low',    1, DATEADD(DAY,-3,GETDATE())),
        (N'Uygulama çöktü',                    N'Yolculuk sırasında uygulama kapandı ve ödeme yapamadım.',                     4, NULL, 'Open',     'Medium', NULL, GETDATE());
END
GO

-- ================================================================
-- 21. USER ADDRESSES
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM UserAddresses)
BEGIN
    INSERT INTO UserAddresses (UserID, AddressName, FullAddress, Latitude, Longitude, IsDefault, AddressType, CreatedDate) VALUES
        (1, N'Ev',       N'Beşiktaş Barbaros Bulvarı No:12 D:5, İstanbul',         41.0428, 29.0094, 1, 'Home',  GETDATE()),
        (1, N'İş',       N'Levent Büyükdere Cad. No:100 K:8, İstanbul',            41.0797, 29.0117, 0, 'Work',  GETDATE()),
        (2, N'Ev',       N'Kadıköy Moda Cad. No:45, İstanbul',                     40.9905, 29.0231, 1, 'Home',  GETDATE()),
        (3, N'Ev',       N'Şişli Halaskargazi Cad. No:78 D:3, İstanbul',           41.0600, 28.9864, 1, 'Home',  GETDATE()),
        (3, N'Spor Salonu',N'Maslak Atatürk Oto Sanayi, İstanbul',                 41.1101, 29.0228, 0, 'Other', GETDATE()),
        (4, N'Ev',       N'Üsküdar Bağlarbaşı Cad. No:22, İstanbul',              41.0228, 29.0128, 1, 'Home',  GETDATE()),
        (5, N'Ev',       N'Bakırköy İncirli Cad. No:5 D:1, İstanbul',             40.9825, 28.8700, 1, 'Home',  GETDATE());
END
GO

-- ================================================================
-- 22. PROMOTIONS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Promotions)
BEGIN
    INSERT INTO Promotions (Title, Description, DiscountType, DiscountValue, MinimumAmount, MaximumDiscount, StartDate, EndDate, UsageLimit, UsedCount, IsActive, CreatedDate) VALUES
        (N'İlk Yolculuk %20 İndirim', N'Uygulamayı ilk kez kullanan müşterilere özel.',     'Percentage',  20.00, 30.00, 50.00,  DATEADD(DAY,-30,GETDATE()), DATEADD(DAY,60,GETDATE()), 1000, 423, 1, GETDATE()),
        (N'Hafta Sonu 15₺ İndirim',   N'Cumartesi ve Pazar günleri geçerlidir.',             'FixedAmount', 15.00, 50.00, NULL,   DATEADD(DAY,-7,GETDATE()),  DATEADD(DAY,30,GETDATE()), NULL, 215, 1, GETDATE()),
        (N'Havalimanı Transferi %10', N'Havalimanına gidiş/geliş yolculuklarında geçerli.', 'Percentage',  10.00, 100.00,75.00, DATEADD(DAY,-14,GETDATE()), DATEADD(DAY,14,GETDATE()), 500,  87,  1, GETDATE()),
        (N'Eski Kampanya',             N'Süresi dolmuş promosyon.',                           'FixedAmount', 10.00, 40.00, NULL,   DATEADD(DAY,-60,GETDATE()), DATEADD(DAY,-1,GETDATE()),  200, 198, 0, GETDATE());
END
GO

-- ================================================================
-- 23. CREDIT CARDS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM CreditCards)
BEGIN
    INSERT INTO CreditCards (UserID, CardNumber, CardHolder, ExpiryMonth, ExpiryYear, CVC, IsDefault, CreatedDate) VALUES
        (1, '4111111111111111', N'AHMET YILMAZ', 12, 2027, '123', 1, GETDATE()),
        (2, '5500005555555559', N'AYSE KAYA',    6,  2026, '456', 1, GETDATE()),
        (3, '4012888888881881', N'MEHMET DEMIR', 9,  2028, '789', 1, GETDATE()),
        (4, '4222222222222',    N'FATMA CELIK',  3,  2027, '321', 1, GETDATE());
END
GO

-- ================================================================
-- 24. INVOICES
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Invoices)
BEGIN
    INSERT INTO Invoices (UserID, TripID, Amount, InvoiceDate, Status) VALUES
        (1, 1,  85.50, DATEADD(DAY,-3,GETDATE()), 'Paid'),
        (2, 2,  72.00, DATEADD(DAY,-2,GETDATE()), 'Paid'),
        (3, 3, 140.00, DATEADD(DAY,-1,GETDATE()), 'Paid'),
        (4, 4,  45.00, DATEADD(HOUR,-5,GETDATE()),'Paid'),
        (5, 5,  68.75, DATEADD(HOUR,-2,GETDATE()),'Paid');
END
GO

-- ================================================================
-- 25. FEEDBACKS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Feedbacks)
BEGIN
    INSERT INTO Feedbacks (UserID, DriverID, TripID, Rating, Comment, CreatedDate) VALUES
        (1, 1, 1, 4.5, N'Araç temiz, sürücü güler yüzlüydü.',       GETDATE()),
        (2, 2, 2, 5.0, N'Mükemmel hizmet, kesinlikle tavsiye ederim.', GETDATE()),
        (3, 3, 3, 4.0, N'İyi ama biraz geç kaldı.',                  GETDATE()),
        (4, 1, 4, 5.0, N'Her zaman bu şoförü seçerim.',              GETDATE()),
        (5, 4, 5, 3.5, N'Ortalama bir yolculuktu.',                  GETDATE());
END
GO

-- ================================================================
-- 26. SHARED RIDES
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM SharedRides)
BEGIN
    INSERT INTO SharedRides (TripID, PassengerCount, Status, CreatedDate) VALUES
        (1, 2, 'Completed', GETDATE()),
        (3, 3, 'Completed', GETDATE());
END
GO

-- ================================================================
-- 27. MESSAGES
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Messages)
BEGIN
    INSERT INTO Messages (SenderID, ReceiverID, Content, SentAt, IsRead) VALUES
        (1, 2, N'Merhaba, yarın için ortak taksi planı yapalım mı?',     DATEADD(HOUR,-2,GETDATE()), 1),
        (2, 1, N'Tabii ki! Saat kaçta çıkmayı düşünüyorsun?',           DATEADD(HOUR,-1,GETDATE()), 1),
        (1, 2, N'Sabah 8:30 olsa uygun mu?',                            DATEADD(MINUTE,-45,GETDATE()),0),
        (3, 4, N'Bu uygulamayı nasıl buldun? Çok kullanışlı.',           DATEADD(DAY,-1,GETDATE()),   1),
        (4, 3, N'Evet, özellikle cüzdan özelliği çok pratik!',          DATEADD(DAY,-1,GETDATE()),   1);
END
GO

-- ================================================================
-- 28. OTP REQUESTS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM OtpRequests)
BEGIN
    INSERT INTO OtpRequests (UserID, PhoneNumber, OTPCode, ExpirationTime, IsUsed, UsedAt, RequestIP, RequestSource, CreatedDate) VALUES
        (1, '05301234567', '482916', DATEADD(MINUTE,-175,GETDATE()), 1, DATEADD(MINUTE,-174,GETDATE()), '192.168.1.101', 'MobileApp', DATEADD(MINUTE,-180,GETDATE())),
        (2, '05302345678', '753284', DATEADD(MINUTE,-85,GETDATE()),  1, DATEADD(MINUTE,-84,GETDATE()),  '192.168.1.102', 'MobileApp', DATEADD(MINUTE,-90,GETDATE())),
        (3, '05303456789', '194623', DATEADD(MINUTE,-10,GETDATE()),  0, NULL,                           '192.168.1.103', 'MobileApp', DATEADD(MINUTE,-15,GETDATE())),
        (NULL,'05309999999','000000', DATEADD(MINUTE,-5,GETDATE()),  0, NULL,                           '10.0.0.55',     'WebApp',    DATEADD(MINUTE,-5,GETDATE()));
END
GO

-- ================================================================
-- 29. SMS LOGS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM SmsLogs)
BEGIN
    INSERT INTO SmsLogs (PhoneNumber, MessageBody, IsSuccess, SentAt, ProviderResponse, OTPID, CreatedDate) VALUES
        ('05301234567', N'TakiWebApi doğrulama kodunuz: 482916 (5 dk geçerli)',  1, DATEADD(MINUTE,-180,GETDATE()), '{"status":"200","messageId":"abc123"}', 1, GETDATE()),
        ('05302345678', N'TakiWebApi doğrulama kodunuz: 753284 (5 dk geçerli)',  1, DATEADD(MINUTE,-90,GETDATE()),  '{"status":"200","messageId":"def456"}', 2, GETDATE()),
        ('05303456789', N'TakiWebApi doğrulama kodunuz: 194623 (5 dk geçerli)',  1, DATEADD(MINUTE,-15,GETDATE()),  '{"status":"200","messageId":"ghi789"}', 3, GETDATE()),
        ('05309999999', N'TakiWebApi doğrulama kodunuz: 000000 (5 dk geçerli)',  0, DATEADD(MINUTE,-5,GETDATE()),   '{"status":"429","error":"Rate limit"}', 4, GETDATE());
END
GO

-- ================================================================
-- 30. USER NOTIFICATION SETTINGS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM UserNotificationSettings)
BEGIN
    INSERT INTO UserNotificationSettings (UserID, AllowPromotions, AllowTripUpdates, AllowNews, CreatedDate) VALUES
        (1, 1, 1, 1, GETDATE()),
        (2, 1, 1, 0, GETDATE()),
        (3, 0, 1, 1, GETDATE()),
        (4, 1, 1, 1, GETDATE()),
        (5, 0, 1, 0, GETDATE());
END
GO

-- ================================================================
-- 31. MATCHING REQUESTS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM MatchingRequests)
BEGIN
    INSERT INTO MatchingRequests (PassengerID, DriverID, PickupAddress, DropoffAddress, PickupLatitude, PickupLongitude, DropoffLatitude, DropoffLongitude, EstimatedCost, EstimatedDistance, EstimatedDuration, Status, RequestTime, AcceptedTime, ExpiryTime, MaxWaitTime, MinRating, MaxDistance, CreatedDate) VALUES
        (1, 1, N'Beşiktaş İskele',     N'Kadıköy Merkez',      41.0428, 29.0094, 40.9905, 29.0231, 85.50,  8.2, 22, 1, DATEADD(DAY,-3,GETUTCDATE()), DATEADD(DAY,-3,DATEADD(MINUTE,2,GETUTCDATE())),  DATEADD(DAY,-3,DATEADD(MINUTE,10,GETUTCDATE())), 15.0, 3.5, 5.0, GETDATE()),
        (2, 2, N'Taksim Meydanı',      N'Üsküdar İskele',      41.0369, 28.9850, 41.0228, 29.0128, 72.00,  6.5, 18, 1, DATEADD(DAY,-2,GETUTCDATE()), DATEADD(DAY,-2,DATEADD(MINUTE,3,GETUTCDATE())),  DATEADD(DAY,-2,DATEADD(MINUTE,10,GETUTCDATE())), 10.0, 4.0, 4.0, GETDATE()),
        (3, NULL,N'Şişli Metro',       N'Maslak Plaza',        41.0600, 28.9864, 41.1101, 29.0228, 55.00,  4.8, 14, 3, DATEADD(DAY,-1,GETUTCDATE()), NULL,                                            DATEADD(DAY,-1,DATEADD(MINUTE,10,GETUTCDATE())), 15.0, 3.0, 6.0, GETDATE()),
        (4, NULL,N'Bakırköy Sahil',    N'Zeytinburnu Metro',   40.9825, 28.8700, 41.0028, 28.9019, 38.00,  3.5, 12, 0, DATEADD(MINUTE,-5,GETUTCDATE()), NULL,                                         DATEADD(MINUTE,10,GETUTCDATE()),                  20.0, 3.5, 8.0, GETDATE());
END
GO

-- ================================================================
-- 32. APP VERSIONS
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM AppVersions)
BEGIN
    INSERT INTO AppVersions (Platform, VersionNumber, IsMandatory, ReleaseNotes, ReleaseDate) VALUES
        ('Android', '2.1.0', 1, N'Cüzdan özelliği eklendi. Performans iyileştirmeleri yapıldı.', DATEADD(DAY,-7,GETDATE())),
        ('iOS',     '2.1.0', 1, N'Cüzdan özelliği eklendi. iOS 17 uyumluluğu sağlandı.',        DATEADD(DAY,-7,GETDATE())),
        ('Android', '2.0.5', 0, N'Kritik hata düzeltmesi.',                                      DATEADD(DAY,-30,GETDATE())),
        ('iOS',     '2.0.5', 0, N'Kritik hata düzeltmesi.',                                      DATEADD(DAY,-30,GETDATE())),
        ('Android', '2.0.0', 0, N'Eşleşme sistemi yenilendi. Harita performansı artırıldı.',    DATEADD(DAY,-60,GETDATE())),
        ('iOS',     '2.0.0', 0, N'Eşleşme sistemi yenilendi. Harita performansı artırıldı.',    DATEADD(DAY,-60,GETDATE()));
END
GO

-- ================================================================
PRINT 'Seed data basariyla eklendi.';
PRINT 'Users:5 | Drivers:4 | Trips:6 | Wallets:5 | Notifications:6 | ...';
-- ================================================================
