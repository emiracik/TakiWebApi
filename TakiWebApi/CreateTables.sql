-- ===============================================
-- TaksiWebApi - Database Creation Script
-- ===============================================

-- Create Database (uncomment if needed)
-- CREATE DATABASE TaxiDb;
-- GO
-- USE TaxiDb;
-- GO

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

-- ===============================================

-- USER RATINGS TABLE
CREATE TABLE UserRatings (
    UserRatingID int IDENTITY(1,1) PRIMARY KEY,
    TripID int NOT NULL,
    RatedUserID int NOT NULL,
    RatedByDriverID int NOT NULL,
    Rating decimal(2,1) NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment nvarchar(1000) NULL,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    -- Foreign Keys
    FOREIGN KEY (TripID) REFERENCES Trips(TripID),
    FOREIGN KEY (RatedUserID) REFERENCES Users(UserID),
    FOREIGN KEY (RatedByDriverID) REFERENCES Drivers(DriverID)
);

-- USER RATINGS SAMPLE DATA
INSERT INTO UserRatings (TripID, RatedUserID, RatedByDriverID, Rating, Comment, CreatedBy)
VALUES (1, 2, 1, 4.5, N'Çok iyi yolcu, zamanında geldi.', 1);
INSERT INTO UserRatings (TripID, RatedUserID, RatedByDriverID, Rating, Comment, CreatedBy)
VALUES (2, 3, 2, 5.0, N'Kibar ve sorunsuz.', 2);
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
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    FOREIGN KEY (PassengerID) REFERENCES Users(UserID),
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
);

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

-- ===============================================
-- ADDITIONAL TABLES (Optional - for future use)
-- ===============================================

-- ADMINS TABLE
CREATE TABLE Admins (
    AdminID int IDENTITY(1,1) PRIMARY KEY,
    FullName nvarchar(200) NOT NULL,
    Email nvarchar(200) NOT NULL UNIQUE,
    PasswordHash nvarchar(255) NOT NULL,
    Role nvarchar(50) NOT NULL DEFAULT 'Admin',
    IsActive bit NOT NULL DEFAULT 1,
    LastLoginDate datetime2 NULL,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0
);

-- ADMIN ROLES TABLE
CREATE TABLE AdminRoles (
    AdminRoleID int IDENTITY(1,1) PRIMARY KEY,
    RoleName nvarchar(100) NOT NULL UNIQUE,
    Description nvarchar(500) NULL,
    Permissions ntext NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0
);

-- NOTIFICATIONS TABLE
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
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
);

-- SUPPORT TICKETS TABLE
CREATE TABLE SupportTickets (
    SupportTicketID int IDENTITY(1,1) PRIMARY KEY,
    Subject nvarchar(300) NOT NULL,
    Description ntext NOT NULL,
    UserID int NULL,
    DriverID int NULL,
    Status nvarchar(50) NOT NULL DEFAULT 'Open',
    Priority nvarchar(20) NOT NULL DEFAULT 'Medium',
    AssignedTo int NULL,
    ResolvedDate datetime2 NULL,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
    FOREIGN KEY (AssignedTo) REFERENCES Admins(AdminID)
);

-- USER ADDRESSES TABLE
CREATE TABLE UserAddresses (
    UserAddressID int IDENTITY(1,1) PRIMARY KEY,
    UserID int NOT NULL,
    AddressName nvarchar(100) NOT NULL,
    FullAddress nvarchar(500) NOT NULL,
    Latitude float NULL,
    Longitude float NULL,
    IsDefault bit NOT NULL DEFAULT 0,
    AddressType nvarchar(50) NULL, -- Home, Work, Other
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- DRIVER RATINGS TABLE
CREATE TABLE DriverRatings (
    DriverRatingID int IDENTITY(1,1) PRIMARY KEY,
    TripID int NOT NULL,
    DriverID int NOT NULL,
    UserID int NOT NULL,
    Rating decimal(2,1) NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment nvarchar(1000) NULL,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    
    -- Foreign Keys
    FOREIGN KEY (TripID) REFERENCES Trips(TripID),
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- PROMOTIONS TABLE
CREATE TABLE Promotions (
    PromotionID int IDENTITY(1,1) PRIMARY KEY,
    Title nvarchar(200) NOT NULL,
    Description ntext NULL,
    DiscountType nvarchar(20) NOT NULL, -- Percentage, FixedAmount
    DiscountValue decimal(10,2) NOT NULL,
    MinimumAmount decimal(10,2) NULL,
    MaximumDiscount decimal(10,2) NULL,
    StartDate datetime2 NOT NULL,
    EndDate datetime2 NOT NULL,
    UsageLimit int NULL,
    UsedCount int NOT NULL DEFAULT 0,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0
);

-- TRIP RATINGS TABLE
CREATE TABLE TripRatings (
    TripRatingID int IDENTITY(1,1) PRIMARY KEY,
    TripID int NOT NULL,
    RatedByUserID int NOT NULL,
    Rating decimal(2,1) NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment nvarchar(1000) NULL,
    CreatedBy int NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    -- Foreign Keys
    FOREIGN KEY (TripID) REFERENCES Trips(TripID),
    FOREIGN KEY (RatedByUserID) REFERENCES Users(UserID)
);

-- TRIP RATINGS SAMPLE DATA
INSERT INTO TripRatings (TripID, RatedByUserID, Rating, Comment, CreatedBy)
VALUES (1, 2, 4.5, N'Güzel bir yolculuktu.', 2);
INSERT INTO TripRatings (TripID, RatedByUserID, Rating, Comment, CreatedBy)
VALUES (2, 3, 5.0, N'Çok memnun kaldım.', 3);

-- ===============================================
-- INDEXES FOR PERFORMANCE
-- ===============================================

-- Users Indexes
CREATE INDEX IX_Users_PhoneNumber ON Users(PhoneNumber);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsDeleted ON Users(IsDeleted);

-- Drivers Indexes
CREATE INDEX IX_Drivers_PhoneNumber ON Drivers(PhoneNumber);
CREATE INDEX IX_Drivers_VehiclePlate ON Drivers(VehiclePlate);
CREATE INDEX IX_Drivers_IsDeleted ON Drivers(IsDeleted);

-- Trips Indexes
CREATE INDEX IX_Trips_PassengerID ON Trips(PassengerID);
CREATE INDEX IX_Trips_DriverID ON Trips(DriverID);
CREATE INDEX IX_Trips_StartTime ON Trips(StartTime);
CREATE INDEX IX_Trips_IsDeleted ON Trips(IsDeleted);

-- Announcements Indexes
CREATE INDEX IX_Announcements_PublishedAt ON Announcements(PublishedAt);
CREATE INDEX IX_Announcements_IsDeleted ON Announcements(IsDeleted);

-- Blogs Indexes
CREATE INDEX IX_Blogs_IsPublished ON Blogs(IsPublished);
CREATE INDEX IX_Blogs_PublishedAt ON Blogs(PublishedAt);
CREATE INDEX IX_Blogs_IsDeleted ON Blogs(IsDeleted);

-- FAQs Indexes
CREATE INDEX IX_FAQs_IsActive ON FAQs(IsActive);
CREATE INDEX IX_FAQs_SortOrder ON FAQs(SortOrder);
CREATE INDEX IX_FAQs_IsDeleted ON FAQs(IsDeleted);

-- ===============================================
-- INITIAL ADMIN USER (Optional)
-- ===============================================

-- Insert default admin user
INSERT INTO Admins (FullName, Email, PasswordHash, Role, IsActive, CreatedDate)
VALUES ('Sistem Yöneticisi', 'admin@taksiwebapi.com', 'TEMP_PASSWORD_HASH', 'SuperAdmin', 1, GETDATE());

-- Insert default admin roles
INSERT INTO AdminRoles (RoleName, Description, IsActive, CreatedDate)
VALUES 
    ('SuperAdmin', 'Tüm yetkilere sahip süper yönetici', 1, GETDATE()),
    ('Admin', 'Genel yönetici yetkileri', 1, GETDATE()),
    ('Moderator', 'İçerik moderasyon yetkileri', 1, GETDATE()),
    ('Support', 'Müşteri destek yetkileri', 1, GETDATE());

-- ===============================================
-- SUCCESS MESSAGE
-- ===============================================

PRINT 'Database tables created successfully!';
PRINT 'You can now run the SampleInserts.sql file to add sample data.';
