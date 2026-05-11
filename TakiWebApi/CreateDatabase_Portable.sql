-- ===============================================
-- PORTABLE DATABASE CREATION SCRIPT
-- TaxiDb - Works on any SQL Server instance
-- ===============================================
-- Bu script herhangi bir SQL Server cihazında çalışır
-- Sabit disk yolu kullanmaz, SQL Server'ın varsayılan yolunu kullanır
-- ===============================================

USE [master]
GO

-- ===============================================
-- 1. MEVCUT VERİTABANINI KONTROL ET VE SİL (GEREKİRSE)
-- ===============================================
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'TaxiDb')
BEGIN
    PRINT '⚠️ TaxiDb veritabanı zaten mevcut. Bağlantılar kapatılıyor...'
    
    -- Tüm bağlantıları zorla kapat
    ALTER DATABASE [TaxiDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    
    PRINT '🗑️ Eski veritabanı siliniyor...'
    DROP DATABASE [TaxiDb];
    
    PRINT '✅ Eski veritabanı silindi.'
END
GO

-- ===============================================
-- 2. YENİ VERİTABANI OLUŞTUR (PORTABLE)
-- ===============================================
PRINT '📦 Yeni TaxiDb veritabanı oluşturuluyor...'

CREATE DATABASE [TaxiDb]
GO

-- Veritabanı ayarları
ALTER DATABASE [TaxiDb] SET RECOVERY SIMPLE;
ALTER DATABASE [TaxiDb] SET COMPATIBILITY_LEVEL = 150; -- SQL Server 2019+
GO

USE [TaxiDb]
GO

PRINT '✅ TaxiDb veritabanı başarıyla oluşturuldu!'
PRINT '📍 Varsayılan SQL Server yolu kullanılıyor.'
GO

-- ===============================================
-- 3. TABLOLARI OLUŞTUR
-- ===============================================

PRINT '🏗️ Tablolar oluşturuluyor...'
GO

-- ===============================================
-- USERS TABLE
-- ===============================================
CREATE TABLE Users (
    UserID int IDENTITY(1,1) PRIMARY KEY,
    FullName nvarchar(200) NOT NULL,
    PhoneNumber nvarchar(20) NOT NULL,
    Email nvarchar(200) NULL,
    PasswordHash nvarchar(255) NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0
);
PRINT '✅ Users tablosu oluşturuldu'
GO

-- ===============================================
-- DRIVERS TABLE
-- ===============================================
CREATE TABLE Drivers (
    DriverID int IDENTITY(1,1) PRIMARY KEY,
    FullName nvarchar(200) NOT NULL,
    PhoneNumber nvarchar(20) NOT NULL,
    Email nvarchar(200) NULL,
    PasswordHash nvarchar(255) NULL,
    VehiclePlate nvarchar(20) NULL,
    VehicleModel nvarchar(100) NULL,
    VehicleColor nvarchar(50) NULL,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0
);
PRINT '✅ Drivers tablosu oluşturuldu'
GO

-- ===============================================
-- TRIPS TABLE
-- ===============================================
CREATE TABLE Trips (
    TripID int IDENTITY(1,1) PRIMARY KEY,
    PassengerID int NULL,
    DriverID int NULL,
    StartAddress nvarchar(500) NULL,
    EndAddress nvarchar(500) NULL,
    StartLatitude float NULL,
    StartLongitude float NULL,
    EndLatitude float NULL,
    EndLongitude float NULL,
    StartTime datetime2 NULL,
    EndTime datetime2 NULL,
    Cost decimal(10,2) NULL,
    PaymentMethod nvarchar(50) NULL,
    Distance float NULL,
    Status nvarchar(50) NULL,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    CONSTRAINT FK_Trips_PassengerID FOREIGN KEY (PassengerID) REFERENCES Users(UserID),
    CONSTRAINT FK_Trips_DriverID FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
);
PRINT '✅ Trips tablosu oluşturuldu'
GO

-- ===============================================
-- ANNOUNCEMENTS TABLE
-- ===============================================
CREATE TABLE Announcements (
    AnnouncementID int IDENTITY(1,1) PRIMARY KEY,
    Title nvarchar(300) NOT NULL,
    Content ntext NULL,
    PublishedAt datetime2 NOT NULL DEFAULT GETDATE(),
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0
);
PRINT '✅ Announcements tablosu oluşturuldu'
GO

-- ===============================================
-- BLOGS TABLE
-- ===============================================
CREATE TABLE Blogs (
    BlogID int IDENTITY(1,1) PRIMARY KEY,
    Title nvarchar(300) NOT NULL,
    Content ntext NULL,
    ImageUrl nvarchar(500) NULL,
    PublishedAt datetime2 NULL,
    IsPublished bit NOT NULL DEFAULT 0,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0
);
PRINT '✅ Blogs tablosu oluşturuldu'
GO

-- ===============================================
-- FAQs TABLE
-- ===============================================
CREATE TABLE FAQs (
    FAQID int IDENTITY(1,1) PRIMARY KEY,
    Question nvarchar(500) NOT NULL,
    Answer nvarchar(2000) NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    SortOrder int NULL,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0
);
PRINT '✅ FAQs tablosu oluşturuldu'
GO

-- ===============================================
-- NOTIFICATIONS TABLE
-- ===============================================
CREATE TABLE Notifications (
    NotificationID int IDENTITY(1,1) PRIMARY KEY,
    Title nvarchar(200) NOT NULL,
    Message ntext NOT NULL,
    UserID int NULL,
    DriverID int NULL,
    NotificationType nvarchar(50) NOT NULL,
    IsRead bit NOT NULL DEFAULT 0,
    ReadDate datetime2 NULL,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    CONSTRAINT FK_Notifications_UserID FOREIGN KEY (UserID) REFERENCES Users(UserID),
    CONSTRAINT FK_Notifications_DriverID FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
);
PRINT '✅ Notifications tablosu oluşturuldu'
GO

-- ===============================================
-- USER ADDRESSES TABLE
-- ===============================================
CREATE TABLE UserAddresses (
    UserAddressID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NOT NULL,
    AddressName nvarchar(100) NOT NULL,
    FullAddress nvarchar(500) NOT NULL,
    Latitude float NULL,
    Longitude float NULL,
    IsDefault bit NOT NULL DEFAULT 0,
    AddressType nvarchar(50) NULL,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    CONSTRAINT FK_UserAddresses_UserID FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
PRINT '✅ UserAddresses tablosu oluşturuldu'
GO

-- ===============================================
-- DRIVER RATINGS TABLE
-- ===============================================
CREATE TABLE DriverRatings (
    DriverRatingID int IDENTITY(1,1) PRIMARY KEY,
    TripID int NOT NULL,
    DriverID int NOT NULL,
    UserID int NOT NULL,
    Rating decimal(2,1) NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment nvarchar(1000) NULL,
    RatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    CONSTRAINT FK_DriverRatings_TripID FOREIGN KEY (TripID) REFERENCES Trips(TripID),
    CONSTRAINT FK_DriverRatings_DriverID FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
    CONSTRAINT FK_DriverRatings_UserID FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
PRINT '✅ DriverRatings tablosu oluşturuldu'
GO

-- ===============================================
-- USER CREDIT CARDS TABLE
-- ===============================================
CREATE TABLE UserCreditCards (
    CreditCardID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NOT NULL,
    CardHolderName nvarchar(100) NOT NULL,
    CardNumberMasked nvarchar(20) NOT NULL,
    ExpiryMonth int NOT NULL,
    ExpiryYear int NOT NULL,
    IsDefault bit NOT NULL DEFAULT 0,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    CONSTRAINT FK_UserCreditCards_UserID FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
PRINT '✅ UserCreditCards tablosu oluşturuldu'
GO

-- ===============================================
-- USER NOTIFICATION SETTINGS TABLE
-- ===============================================
CREATE TABLE UserNotificationSettings (
    SettingID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NOT NULL,
    AllowPromotions bit NOT NULL DEFAULT 1,
    AllowTripUpdates bit NOT NULL DEFAULT 1,
    AllowNews bit NOT NULL DEFAULT 1,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    CONSTRAINT FK_UserNotificationSettings_UserID FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
PRINT '✅ UserNotificationSettings tablosu oluşturuldu'
GO

-- ===============================================
-- WALLET TRANSACTIONS TABLE
-- ===============================================
CREATE TABLE WalletTransactions (
    TransactionID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NOT NULL,
    Amount decimal(10,2) NOT NULL,
    TransactionType nvarchar(20) NOT NULL,
    Description nvarchar(200) NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    
    -- Foreign Keys
    CONSTRAINT FK_WalletTransactions_UserID FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
PRINT '✅ WalletTransactions tablosu oluşturuldu'
GO

-- ===============================================
-- MATCHING REQUESTS TABLE (MATCHING SYSTEM)
-- ===============================================
CREATE TABLE MatchingRequests (
    MatchingRequestID int IDENTITY(1,1) PRIMARY KEY,
    PassengerID int NOT NULL,
    DriverID int NULL,
    PickupAddress nvarchar(500) NOT NULL,
    DropoffAddress nvarchar(500) NOT NULL,
    PickupLatitude float NOT NULL,
    PickupLongitude float NOT NULL,
    DropoffLatitude float NOT NULL,
    DropoffLongitude float NOT NULL,
    EstimatedCost decimal(10,2) NULL,
    EstimatedDistance float NULL,
    EstimatedDuration int NULL,
    Status int NOT NULL DEFAULT 0,
    RequestTime datetime2 NOT NULL DEFAULT GETUTCDATE(),
    AcceptedTime datetime2 NULL,
    ExpiryTime datetime2 NULL,
    Notes nvarchar(1000) NULL,
    MaxWaitTime float NULL,
    MinRating float NULL,
    MaxDistance float NULL,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    CONSTRAINT FK_MatchingRequests_PassengerID FOREIGN KEY (PassengerID) REFERENCES Users(UserID),
    CONSTRAINT FK_MatchingRequests_DriverID FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
);
PRINT '✅ MatchingRequests tablosu oluşturuldu'
GO

-- ===============================================
-- DRIVER LOCATIONS TABLE (MATCHING SYSTEM)
-- ===============================================
CREATE TABLE DriverLocations (
    LocationID int IDENTITY(1,1) PRIMARY KEY,
    DriverID int NOT NULL,
    Latitude float NOT NULL,
    Longitude float NOT NULL,
    IsAvailable bit NOT NULL DEFAULT 1,
    CurrentAddress nvarchar(100) NULL,
    LocationTime datetime2 NOT NULL DEFAULT GETUTCDATE(),
    Speed float NULL,
    Heading float NULL,
    IsOnline bit NOT NULL DEFAULT 1,
    CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate datetime2 NULL,
    
    -- Foreign Keys
    CONSTRAINT FK_DriverLocations_DriverID FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
    CONSTRAINT UQ_DriverLocations_DriverID UNIQUE (DriverID)
);
PRINT '✅ DriverLocations tablosu oluşturuldu'
GO

-- ===============================================
-- 4. İNDEKSLER
-- ===============================================
PRINT '🔍 İndeksler oluşturuluyor...'
GO

-- Users Indexes
CREATE INDEX IX_Users_PhoneNumber ON Users(PhoneNumber);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsDeleted ON Users(IsDeleted);

-- Drivers Indexes
CREATE INDEX IX_Drivers_PhoneNumber ON Drivers(PhoneNumber);
CREATE INDEX IX_Drivers_IsDeleted ON Drivers(IsDeleted);

-- Trips Indexes
CREATE INDEX IX_Trips_PassengerID ON Trips(PassengerID);
CREATE INDEX IX_Trips_DriverID ON Trips(DriverID);
CREATE INDEX IX_Trips_StartTime ON Trips(StartTime);
CREATE INDEX IX_Trips_IsDeleted ON Trips(IsDeleted);

-- MatchingRequests Indexes
CREATE INDEX IX_MatchingRequests_PassengerID ON MatchingRequests(PassengerID);
CREATE INDEX IX_MatchingRequests_DriverID ON MatchingRequests(DriverID);
CREATE INDEX IX_MatchingRequests_Status ON MatchingRequests(Status);
CREATE INDEX IX_MatchingRequests_RequestTime ON MatchingRequests(RequestTime);
CREATE INDEX IX_MatchingRequests_Status_IsDeleted ON MatchingRequests(Status, IsDeleted);

-- DriverLocations Indexes
CREATE INDEX IX_DriverLocations_IsAvailable ON DriverLocations(IsAvailable);
CREATE INDEX IX_DriverLocations_IsOnline ON DriverLocations(IsOnline);
CREATE INDEX IX_DriverLocations_LocationTime ON DriverLocations(LocationTime);
CREATE INDEX IX_DriverLocations_Available_Online ON DriverLocations(IsAvailable, IsOnline, LocationTime);

PRINT '✅ İndeksler oluşturuldu'
GO

-- ===============================================
-- 5. FONKSIYONLAR VE STORED PROCEDURES
-- ===============================================
PRINT '⚙️ Fonksiyonlar oluşturuluyor...'
GO

-- Distance calculation function (Haversine)
CREATE FUNCTION dbo.CalculateDistance(
    @lat1 FLOAT,
    @lng1 FLOAT,
    @lat2 FLOAT,
    @lng2 FLOAT
)
RETURNS FLOAT
AS
BEGIN
    DECLARE @earthRadius FLOAT = 6371;
    DECLARE @dLat FLOAT = RADIANS(@lat2 - @lat1);
    DECLARE @dLng FLOAT = RADIANS(@lng2 - @lng1);
    DECLARE @a FLOAT;
    DECLARE @c FLOAT;
    DECLARE @distance FLOAT;

    SET @a = SIN(@dLat / 2) * SIN(@dLat / 2) + 
             COS(RADIANS(@lat1)) * COS(RADIANS(@lat2)) * 
             SIN(@dLng / 2) * SIN(@dLng / 2);
    
    SET @c = 2 * ATN2(SQRT(@a), SQRT(1 - @a));
    SET @distance = @earthRadius * @c;

    RETURN @distance;
END
GO
PRINT '✅ CalculateDistance fonksiyonu oluşturuldu'
GO

-- Find nearby drivers procedure
CREATE PROCEDURE dbo.FindNearbyDrivers
    @Latitude FLOAT,
    @Longitude FLOAT,
    @RadiusKm FLOAT = 5.0,
    @MaxResults INT = 10,
    @MinRating FLOAT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@MaxResults)
        d.DriverID,
        d.FullName,
        d.PhoneNumber,
        d.VehiclePlate,
        d.VehicleModel,
        d.VehicleColor,
        dl.Latitude,
        dl.Longitude,
        dl.IsAvailable,
        dl.LocationTime,
        dbo.CalculateDistance(@Latitude, @Longitude, dl.Latitude, dl.Longitude) AS Distance,
        COALESCE(AVG(CAST(dr.Rating as FLOAT)), 0) as AverageRating,
        COUNT(dr.DriverRatingID) as TotalRatings
    FROM Drivers d
    INNER JOIN DriverLocations dl ON d.DriverID = dl.DriverID
    LEFT JOIN DriverRatings dr ON d.DriverID = dr.DriverID AND dr.IsDeleted = 0
    WHERE d.IsDeleted = 0 
        AND dl.IsAvailable = 1 
        AND dl.IsOnline = 1
        AND dl.LocationTime > DATEADD(MINUTE, -30, GETUTCDATE())
        AND dbo.CalculateDistance(@Latitude, @Longitude, dl.Latitude, dl.Longitude) <= @RadiusKm
    GROUP BY d.DriverID, d.FullName, d.PhoneNumber, d.VehiclePlate, d.VehicleModel, 
             d.VehicleColor, dl.Latitude, dl.Longitude, dl.IsAvailable, dl.LocationTime
    HAVING (@MinRating IS NULL OR COALESCE(AVG(CAST(dr.Rating as FLOAT)), 0) >= @MinRating)
    ORDER BY Distance ASC, AverageRating DESC;
END
GO
PRINT '✅ FindNearbyDrivers procedure oluşturuldu'
GO

-- ===============================================
-- 6. ÖRNEK VERİ EKLEME (OPSİYONEL)
-- ===============================================
PRINT '📝 Örnek veriler ekleniyor...'
GO

-- Sample Users
INSERT INTO Users (FullName, PhoneNumber, Email, PasswordHash, IsActive)
VALUES 
    ('Ahmet Yılmaz', '+905551234567', 'ahmet@example.com', '8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92', 1),
    ('Ayşe Demir', '+905559876543', 'ayse@example.com', '8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92', 1);

-- Sample Drivers
INSERT INTO Drivers (FullName, PhoneNumber, Email, PasswordHash, VehiclePlate, VehicleModel, VehicleColor)
VALUES 
    ('Mehmet Yılmaz', '+905556789012', 'mehmet@example.com', 'F6E0A1E2AC41945A9AA7FF8A8AAA0CEBC12A3BCC981A929AD5CF810A090E11AE', '34ABC123', 'Toyota Corolla', 'Beyaz'),
    ('Fatma Öz', '+905554567890', 'fatma@example.com', 'F6E0A1E2AC41945A9AA7FF8A8AAA0CEBC12A3BCC981A929AD5CF810A090E11AE', '34XYZ789', 'Renault Clio', 'Gri');

-- Sample Driver Locations
INSERT INTO DriverLocations (DriverID, Latitude, Longitude, IsAvailable, CurrentAddress, IsOnline)
VALUES 
    (1, 41.0082, 28.9784, 1, 'Beşiktaş Merkez', 1),
    (2, 40.9833, 29.0167, 1, 'Kadıköy İskele', 1);

PRINT '✅ Örnek veriler eklendi'
GO

-- ===============================================
-- 7. BAŞARI MESAJI
-- ===============================================
PRINT ''
PRINT '=========================================='
PRINT '✅ TaxiDb KURULUMU TAMAMLANDI!'
PRINT '=========================================='
PRINT ''
PRINT '📊 Veritabanı: TaxiDb'
PRINT '🗄️ Tablolar: Users, Drivers, Trips, MatchingRequests, DriverLocations, ve daha fazlası'
PRINT '🔧 Fonksiyonlar: CalculateDistance'
PRINT '⚡ Stored Procedures: FindNearbyDrivers'
PRINT ''
PRINT '🚀 Sistem kullanıma hazır!'
PRINT ''
PRINT '📝 Test için:'
PRINT '   - SELECT * FROM Users;'
PRINT '   - SELECT * FROM Drivers;'
PRINT '   - EXEC FindNearbyDrivers @Latitude=41.0082, @Longitude=28.9784;'
PRINT ''
PRINT '=========================================='
GO