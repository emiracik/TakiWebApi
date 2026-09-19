
USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TaxiDb')
BEGIN
    CREATE DATABASE TaxiDb;
END
GO

USE TaxiDb;
GO

-- ================================================================
-- 1. ADMIN ROLES TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AdminRoles' AND xtype = 'U')
BEGIN
    CREATE TABLE AdminRoles (
        RoleID       int           IDENTITY(1,1) PRIMARY KEY,
        RoleName     nvarchar(100) NOT NULL UNIQUE,
        Description  nvarchar(500) NULL
    );

    INSERT INTO AdminRoles (RoleName, Description) VALUES
        ('SuperAdmin', N'Tüm yetkilere sahip süper yönetici'),
        ('Admin',      N'Genel yönetici yetkileri'),
        ('Moderator',  N'İçerik moderasyon yetkileri'),
        ('Support',    N'Müşteri destek yetkileri');
END
GO

-- ================================================================
-- 2. USERS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Users' AND xtype = 'U')
BEGIN
    CREATE TABLE Users (
        UserID        int           IDENTITY(1,1) PRIMARY KEY,
        FullName      nvarchar(200) NOT NULL,
        PhoneNumber   nvarchar(20)  NOT NULL,
        Email         nvarchar(200) NULL,
        PasswordHash  nvarchar(255) NULL,
        IsActive      bit           NOT NULL DEFAULT 1,
        CreatedBy     int           NULL,
        CreatedDate   datetime2     NOT NULL DEFAULT GETDATE(),
        UpdatedBy     int           NULL,
        UpdatedDate   datetime2     NULL,
        DeletedBy     int           NULL,
        DeletedDate   datetime2     NULL,
        IsDeleted     bit           NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_Users_PhoneNumber ON Users(PhoneNumber);
    CREATE INDEX IX_Users_Email       ON Users(Email);
    CREATE INDEX IX_Users_IsDeleted   ON Users(IsDeleted);
END
GO

-- ================================================================
-- 3. DRIVERS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Drivers' AND xtype = 'U')
BEGIN
    CREATE TABLE Drivers (
        DriverID      int           IDENTITY(1,1) PRIMARY KEY,
        FullName      nvarchar(200) NOT NULL,
        PhoneNumber   nvarchar(20)  NOT NULL,
        Email         nvarchar(200) NULL,
        PasswordHash  nvarchar(255) NULL,
        VehiclePlate  nvarchar(20)  NULL,
        VehicleModel  nvarchar(100) NULL,
        VehicleColor  nvarchar(50)  NULL,
        CreatedBy     int           NULL,
        CreatedDate   datetime2     NOT NULL DEFAULT GETDATE(),
        UpdatedBy     int           NULL,
        UpdatedDate   datetime2     NULL,
        DeletedBy     int           NULL,
        DeletedDate   datetime2     NULL,
        IsDeleted     bit           NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_Drivers_PhoneNumber  ON Drivers(PhoneNumber);
    CREATE INDEX IX_Drivers_VehiclePlate ON Drivers(VehiclePlate);
    CREATE INDEX IX_Drivers_IsDeleted    ON Drivers(IsDeleted);
END
GO

-- ================================================================
-- 4. ADMINS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Admins' AND xtype = 'U')
BEGIN
    CREATE TABLE Admins (
        AdminID        int           IDENTITY(1,1) PRIMARY KEY,
        FullName       nvarchar(200) NOT NULL,
        Email          nvarchar(200) NOT NULL UNIQUE,
        Phone          nvarchar(20)  NULL,
        PasswordHash   nvarchar(500) NOT NULL,
        RoleID         int           NULL,
        IsActive       bit           NOT NULL DEFAULT 1,
        LastLoginDate  datetime2     NULL,
        CreatedBy      int           NULL,
        CreatedDate    datetime2     NOT NULL DEFAULT GETDATE(),
        UpdatedBy      int           NULL,
        UpdatedDate    datetime2     NULL,
        DeletedBy      int           NULL,
        DeletedDate    datetime2     NULL,
        IsDeleted      bit           NOT NULL DEFAULT 0,
        FOREIGN KEY (RoleID) REFERENCES AdminRoles(RoleID)
    );
END
GO

-- ================================================================
-- 5. TRIPS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Trips' AND xtype = 'U')
BEGIN
    CREATE TABLE Trips (
        TripID         int            IDENTITY(1,1) PRIMARY KEY,
        PassengerID    int            NULL,
        DriverID       int            NULL,
        StartAddress   nvarchar(500)  NULL,
        EndAddress     nvarchar(500)  NULL,
        StartLatitude  float          NULL,
        StartLongitude float          NULL,
        EndLatitude    float          NULL,
        EndLongitude   float          NULL,
        StartTime      datetime2      NULL,
        EndTime        datetime2      NULL,
        Cost           decimal(10,2)  NULL,
        PaymentMethod  nvarchar(50)   NULL,
        Distance       float          NULL,
        Status         nvarchar(20)   NULL, -- completed, cancelled, inProgress
        CreatedBy      int            NULL,
        CreatedDate    datetime2      NOT NULL DEFAULT GETDATE(),
        UpdatedBy      int            NULL,
        UpdatedDate    datetime2      NULL,
        DeletedBy      int            NULL,
        DeletedDate    datetime2      NULL,
        IsDeleted      bit            NOT NULL DEFAULT 0,
        FOREIGN KEY (PassengerID) REFERENCES Users(UserID),
        FOREIGN KEY (DriverID)    REFERENCES Drivers(DriverID)
    );

    CREATE INDEX IX_Trips_PassengerID ON Trips(PassengerID);
    CREATE INDEX IX_Trips_DriverID    ON Trips(DriverID);
    CREATE INDEX IX_Trips_StartTime   ON Trips(StartTime);
    CREATE INDEX IX_Trips_IsDeleted   ON Trips(IsDeleted);
END
GO

-- ================================================================
-- 6. WALLETS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Wallets' AND xtype = 'U')
BEGIN
    CREATE TABLE Wallets (
        WalletID     int           IDENTITY(1,1) PRIMARY KEY,
        UserID       int           NOT NULL UNIQUE,
        Balance      decimal(18,2) NOT NULL DEFAULT 0,
        TotalIn      decimal(18,2) NOT NULL DEFAULT 0,
        TotalOut     decimal(18,2) NOT NULL DEFAULT 0,
        LastUpdated  datetime2     NOT NULL DEFAULT GETDATE(),
        CreatedBy    int           NULL,
        CreatedDate  datetime2     NOT NULL DEFAULT GETDATE(),
        UpdatedBy    int           NULL,
        UpdatedDate  datetime2     NULL,
        DeletedBy    int           NULL,
        DeletedDate  datetime2     NULL,
        IsDeleted    bit           NOT NULL DEFAULT 0,
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END
GO

-- ================================================================
-- 7. WALLET TRANSACTIONS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'WalletTransactions' AND xtype = 'U')
BEGIN
    CREATE TABLE WalletTransactions (
        TransactionID    int           IDENTITY(1,1) PRIMARY KEY,
        WalletID         int           NOT NULL,
        UserID           int           NOT NULL,
        TransactionType  nvarchar(50)  NOT NULL, -- Deposit, Withdraw, Refund, Payment
        Amount           decimal(18,2) NOT NULL,
        Description      nvarchar(500) NULL,
        TransactionDate  datetime2     NOT NULL DEFAULT GETDATE(),
        CreatedBy        int           NULL,
        CreatedDate      datetime2     NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (WalletID) REFERENCES Wallets(WalletID),
        FOREIGN KEY (UserID)   REFERENCES Users(UserID)
    );
END
GO

-- ================================================================
-- 8. MATCHING REQUESTS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'MatchingRequests' AND xtype = 'U')
BEGIN
    CREATE TABLE MatchingRequests (
        MatchingRequestID   int            IDENTITY(1,1) PRIMARY KEY,
        PassengerID         int            NOT NULL,
        DriverID            int            NULL,
        PickupAddress       nvarchar(500)  NOT NULL,
        DropoffAddress      nvarchar(500)  NOT NULL,
        PickupLatitude      float          NOT NULL,
        PickupLongitude     float          NOT NULL,
        DropoffLatitude     float          NOT NULL,
        DropoffLongitude    float          NOT NULL,
        EstimatedCost       decimal(10,2)  NULL,
        EstimatedDistance   float          NULL,
        EstimatedDuration   int            NULL, -- dakika
        Status              int            NOT NULL DEFAULT 0, -- 0:Pending 1:Accepted 2:Rejected 3:Cancelled 4:Expired
        RequestTime         datetime2      NOT NULL DEFAULT GETUTCDATE(),
        AcceptedTime        datetime2      NULL,
        ExpiryTime          datetime2      NULL,
        Notes               nvarchar(1000) NULL,
        MaxWaitTime         float          NULL,
        MinRating           float          NULL,
        MaxDistance         float          NULL,
        CreatedBy           int            NULL,
        CreatedDate         datetime2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedBy           int            NULL,
        UpdatedDate         datetime2      NULL,
        DeletedBy           int            NULL,
        DeletedDate         datetime2      NULL,
        IsDeleted           bit            NOT NULL DEFAULT 0,
        CONSTRAINT FK_MatchingRequests_PassengerID FOREIGN KEY (PassengerID) REFERENCES Users(UserID),
        CONSTRAINT FK_MatchingRequests_DriverID    FOREIGN KEY (DriverID)    REFERENCES Drivers(DriverID)
    );

    CREATE INDEX IX_MatchingRequests_PassengerID        ON MatchingRequests(PassengerID);
    CREATE INDEX IX_MatchingRequests_DriverID           ON MatchingRequests(DriverID);
    CREATE INDEX IX_MatchingRequests_Status             ON MatchingRequests(Status);
    CREATE INDEX IX_MatchingRequests_RequestTime        ON MatchingRequests(RequestTime);
    CREATE INDEX IX_MatchingRequests_IsDeleted          ON MatchingRequests(IsDeleted);
    CREATE INDEX IX_MatchingRequests_Status_IsDeleted   ON MatchingRequests(Status, IsDeleted);
END
GO

-- ================================================================
-- 9. DRIVER LOCATIONS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'DriverLocations' AND xtype = 'U')
BEGIN
    CREATE TABLE DriverLocations (
        LocationID      int            IDENTITY(1,1) PRIMARY KEY,
        DriverID        int            NOT NULL,
        Latitude        float          NOT NULL,
        Longitude       float          NOT NULL,
        IsAvailable     bit            NOT NULL DEFAULT 1,
        CurrentAddress  nvarchar(100)  NULL,
        LocationTime    datetime2      NOT NULL DEFAULT GETUTCDATE(),
        Speed           float          NULL,
        Heading         float          NULL,
        IsOnline        bit            NOT NULL DEFAULT 1,
        CreatedDate     datetime2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate     datetime2      NULL,
        CONSTRAINT FK_DriverLocations_DriverID   FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
        CONSTRAINT UQ_DriverLocations_DriverID   UNIQUE (DriverID)
    );

    CREATE INDEX IX_DriverLocations_DriverID         ON DriverLocations(DriverID);
    CREATE INDEX IX_DriverLocations_IsAvailable      ON DriverLocations(IsAvailable);
    CREATE INDEX IX_DriverLocations_IsOnline         ON DriverLocations(IsOnline);
    CREATE INDEX IX_DriverLocations_LocationTime     ON DriverLocations(LocationTime);
    CREATE INDEX IX_DriverLocations_Available_Online ON DriverLocations(IsAvailable, IsOnline, LocationTime);
END
GO

-- ================================================================
-- 10. DRIVER RATINGS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'DriverRatings' AND xtype = 'U')
BEGIN
    CREATE TABLE DriverRatings (
        DriverRatingID  int            IDENTITY(1,1) PRIMARY KEY,
        TripID          int            NOT NULL,
        DriverID        int            NOT NULL,
        UserID          int            NOT NULL,
        Rating          decimal(2,1)   NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
        Comment         nvarchar(1000) NULL,
        CreatedBy       int            NULL,
        CreatedDate     datetime2      NOT NULL DEFAULT GETDATE(),
        UpdatedBy       int            NULL,
        UpdatedDate     datetime2      NULL,
        DeletedBy       int            NULL,
        DeletedDate     datetime2      NULL,
        IsDeleted       bit            NOT NULL DEFAULT 0,
        FOREIGN KEY (TripID)   REFERENCES Trips(TripID),
        FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
        FOREIGN KEY (UserID)   REFERENCES Users(UserID)
    );
END
GO

-- ================================================================
-- 11. USER RATINGS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'UserRatings' AND xtype = 'U')
BEGIN
    CREATE TABLE UserRatings (
        UserRatingID      int            IDENTITY(1,1) PRIMARY KEY,
        TripID            int            NOT NULL,
        RatedUserID       int            NOT NULL,
        RatedByDriverID   int            NOT NULL,
        Rating            decimal(2,1)   NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
        Comment           nvarchar(1000) NULL,
        CreatedBy         int            NULL,
        CreatedDate       datetime2      NOT NULL DEFAULT GETDATE(),
        UpdatedBy         int            NULL,
        UpdatedDate       datetime2      NULL,
        DeletedBy         int            NULL,
        DeletedDate       datetime2      NULL,
        IsDeleted         bit            NOT NULL DEFAULT 0,
        FOREIGN KEY (TripID)          REFERENCES Trips(TripID),
        FOREIGN KEY (RatedUserID)     REFERENCES Users(UserID),
        FOREIGN KEY (RatedByDriverID) REFERENCES Drivers(DriverID)
    );
END
GO

-- ================================================================
-- 12. TRIP RATINGS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'TripRatings' AND xtype = 'U')
BEGIN
    CREATE TABLE TripRatings (
        TripRatingID    int            IDENTITY(1,1) PRIMARY KEY,
        TripID          int            NOT NULL,
        RatedByUserID   int            NOT NULL,
        Rating          decimal(2,1)   NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
        Comment         nvarchar(1000) NULL,
        CreatedBy       int            NULL,
        CreatedDate     datetime2      NOT NULL DEFAULT GETDATE(),
        UpdatedBy       int            NULL,
        UpdatedDate     datetime2      NULL,
        DeletedBy       int            NULL,
        DeletedDate     datetime2      NULL,
        IsDeleted       bit            NOT NULL DEFAULT 0,
        FOREIGN KEY (TripID)        REFERENCES Trips(TripID),
        FOREIGN KEY (RatedByUserID) REFERENCES Users(UserID)
    );
END
GO

-- ================================================================
-- 13. DRIVER REVIEWS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'DriverReviews' AND xtype = 'U')
BEGIN
    CREATE TABLE DriverReviews (
        ReviewID     int            IDENTITY(1,1) PRIMARY KEY,
        TripID       int            NULL,
        DriverID     int            NULL,
        UserID       int            NULL,
        ReviewText   nvarchar(1000) NULL,
        CreatedAt    datetime2      NOT NULL DEFAULT GETDATE(),
        CreatedBy    int            NULL,
        UpdatedBy    int            NULL,
        UpdatedDate  datetime2      NULL,
        DeletedBy    int            NULL,
        DeletedDate  datetime2      NULL,
        IsDeleted    bit            NOT NULL DEFAULT 0,
        FOREIGN KEY (TripID)   REFERENCES Trips(TripID),
        FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
        FOREIGN KEY (UserID)   REFERENCES Users(UserID)
    );
END
GO

-- ================================================================
-- 14. DRIVER EARNINGS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'DriverEarnings' AND xtype = 'U')
BEGIN
    CREATE TABLE DriverEarnings (
        EarningID    int           IDENTITY(1,1) PRIMARY KEY,
        DriverID     int           NULL,
        TripID       int           NULL,
        Amount       decimal(10,2) NULL,
        EarnedAt     datetime2     NOT NULL DEFAULT GETDATE(),
        CreatedBy    int           NULL,
        CreatedDate  datetime2     NOT NULL DEFAULT GETDATE(),
        UpdatedBy    int           NULL,
        UpdatedDate  datetime2     NULL,
        DeletedBy    int           NULL,
        DeletedDate  datetime2     NULL,
        IsDeleted    bit           NOT NULL DEFAULT 0,
        FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
        FOREIGN KEY (TripID)   REFERENCES Trips(TripID)
    );
END
GO

-- ================================================================
-- 15. DRIVER DETAILS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'DriverDetails' AND xtype = 'U')
BEGIN
    CREATE TABLE DriverDetails (
        DriverDetailID   int           IDENTITY(1,1) PRIMARY KEY,
        UserID           int           NULL,
        VehiclePlate     nvarchar(20)  NULL,
        VehicleModel     nvarchar(100) NULL,
        VehicleColor     nvarchar(50)  NULL,
        LicenseNumber    nvarchar(50)  NULL,
        ExperienceYears  int           NULL,
        RatingAverage    float         NULL,
        CreatedBy        int           NULL,
        CreatedDate      datetime2     NOT NULL DEFAULT GETDATE(),
        UpdatedBy        int           NULL,
        UpdatedDate      datetime2     NULL,
        DeletedBy        int           NULL,
        DeletedDate      datetime2     NULL,
        IsDeleted        bit           NOT NULL DEFAULT 0,
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END
GO

-- ================================================================
-- 16. DRIVER DOCUMENTS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'DriverDocuments' AND xtype = 'U')
BEGIN
    CREATE TABLE DriverDocuments (
        DocumentID     int           IDENTITY(1,1) PRIMARY KEY,
        DriverID       int           NOT NULL,
        DocumentType   nvarchar(50)  NOT NULL,
        DocumentUrl    nvarchar(500) NOT NULL,
        UploadedDate   datetime2     NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
    );
END
GO

-- ================================================================
-- 17. BANK INFO TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'BankInfo' AND xtype = 'U')
BEGIN
    CREATE TABLE BankInfo (
        BankInfoID      int           IDENTITY(1,1) PRIMARY KEY,
        DriverID        int           NOT NULL,
        BankName        nvarchar(100) NOT NULL,
        IBAN            nvarchar(34)  NOT NULL,
        AccountNumber   nvarchar(30)  NOT NULL,
        CreatedDate     datetime2     NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
    );
END
GO

-- ================================================================
-- 18. ANNOUNCEMENTS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Announcements' AND xtype = 'U')
BEGIN
    CREATE TABLE Announcements (
        AnnouncementID  int           IDENTITY(1,1) PRIMARY KEY,
        Title           nvarchar(300) NOT NULL,
        Content         ntext         NULL,
        PublishedAt     datetime2     NOT NULL DEFAULT GETDATE(),
        CreatedBy       int           NULL,
        CreatedDate     datetime2     NOT NULL DEFAULT GETDATE(),
        UpdatedBy       int           NULL,
        UpdatedDate     datetime2     NULL,
        DeletedBy       int           NULL,
        DeletedDate     datetime2     NULL,
        IsDeleted       bit           NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_Announcements_PublishedAt ON Announcements(PublishedAt);
    CREATE INDEX IX_Announcements_IsDeleted   ON Announcements(IsDeleted);
END
GO

-- ================================================================
-- 19. ANNOUNCEMENT READ TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AnnouncementRead' AND xtype = 'U')
BEGIN
    CREATE TABLE AnnouncementRead (
        id               int       IDENTITY(1,1) PRIMARY KEY,
        userId           int       NOT NULL,
        announcementId   int       NOT NULL,
        createdDate      datetime2 NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (userId)         REFERENCES Users(UserID),
        FOREIGN KEY (announcementId) REFERENCES Announcements(AnnouncementID)
    );
END
GO

-- ================================================================
-- 20. BLOGS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Blogs' AND xtype = 'U')
BEGIN
    CREATE TABLE Blogs (
        BlogID       int           IDENTITY(1,1) PRIMARY KEY,
        Title        nvarchar(300) NOT NULL,
        Content      ntext         NULL,
        ImageUrl     nvarchar(500) NULL,
        PublishedAt  datetime2     NULL,
        IsPublished  bit           NOT NULL DEFAULT 0,
        CreatedBy    int           NULL,
        CreatedDate  datetime2     NOT NULL DEFAULT GETDATE(),
        UpdatedBy    int           NULL,
        UpdatedDate  datetime2     NULL,
        DeletedBy    int           NULL,
        DeletedDate  datetime2     NULL,
        IsDeleted    bit           NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_Blogs_IsPublished ON Blogs(IsPublished);
    CREATE INDEX IX_Blogs_PublishedAt ON Blogs(PublishedAt);
    CREATE INDEX IX_Blogs_IsDeleted   ON Blogs(IsDeleted);
END
GO

-- ================================================================
-- 21. FAQs TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'FAQs' AND xtype = 'U')
BEGIN
    CREATE TABLE FAQs (
        FAQID       int            IDENTITY(1,1) PRIMARY KEY,
        Question    nvarchar(500)  NOT NULL,
        Answer      nvarchar(2000) NOT NULL,
        IsActive    bit            NOT NULL DEFAULT 1,
        SortOrder   int            NULL,
        CreatedBy   int            NULL,
        CreatedDate datetime2      NOT NULL DEFAULT GETDATE(),
        UpdatedBy   int            NULL,
        UpdatedDate datetime2      NULL,
        DeletedBy   int            NULL,
        DeletedDate datetime2      NULL,
        IsDeleted   bit            NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_FAQs_IsActive  ON FAQs(IsActive);
    CREATE INDEX IX_FAQs_SortOrder ON FAQs(SortOrder);
    CREATE INDEX IX_FAQs_IsDeleted ON FAQs(IsDeleted);
END
GO

-- ================================================================
-- 22. NOTIFICATIONS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Notifications' AND xtype = 'U')
BEGIN
    CREATE TABLE Notifications (
        NotificationID    int           IDENTITY(1,1) PRIMARY KEY,
        Title             nvarchar(200) NOT NULL,
        Message           ntext         NOT NULL,
        UserID            int           NULL,
        DriverID          int           NULL,
        NotificationType  nvarchar(50)  NOT NULL,
        IsRead            bit           NOT NULL DEFAULT 0,
        ReadDate          datetime2     NULL,
        CreatedBy         int           NULL,
        CreatedDate       datetime2     NOT NULL DEFAULT GETDATE(),
        UpdatedBy         int           NULL,
        UpdatedDate       datetime2     NULL,
        DeletedBy         int           NULL,
        DeletedDate       datetime2     NULL,
        IsDeleted         bit           NOT NULL DEFAULT 0,
        FOREIGN KEY (UserID)   REFERENCES Users(UserID),
        FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
    );
END
GO

-- ================================================================
-- 23. SUPPORT TICKETS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'SupportTickets' AND xtype = 'U')
BEGIN
    CREATE TABLE SupportTickets (
        SupportTicketID  int           IDENTITY(1,1) PRIMARY KEY,
        Subject          nvarchar(300) NOT NULL,
        Description      ntext         NOT NULL,
        UserID           int           NULL,
        DriverID         int           NULL,
        Status           nvarchar(50)  NOT NULL DEFAULT 'Open',
        Priority         nvarchar(20)  NOT NULL DEFAULT 'Medium',
        AssignedTo       int           NULL,
        ResolvedDate     datetime2     NULL,
        CreatedBy        int           NULL,
        CreatedDate      datetime2     NOT NULL DEFAULT GETDATE(),
        UpdatedBy        int           NULL,
        UpdatedDate      datetime2     NULL,
        DeletedBy        int           NULL,
        DeletedDate      datetime2     NULL,
        IsDeleted        bit           NOT NULL DEFAULT 0,
        FOREIGN KEY (UserID)     REFERENCES Users(UserID),
        FOREIGN KEY (DriverID)   REFERENCES Drivers(DriverID),
        FOREIGN KEY (AssignedTo) REFERENCES Admins(AdminID)
    );
END
GO

-- ================================================================
-- 24. USER ADDRESSES TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'UserAddresses' AND xtype = 'U')
BEGIN
    CREATE TABLE UserAddresses (
        UserAddressID  int           IDENTITY(1,1) PRIMARY KEY,
        UserID         int           NOT NULL,
        AddressName    nvarchar(100) NOT NULL,
        FullAddress    nvarchar(500) NOT NULL,
        Latitude       float         NULL,
        Longitude      float         NULL,
        IsDefault      bit           NOT NULL DEFAULT 0,
        AddressType    nvarchar(50)  NULL, -- Home, Work, Other
        CreatedBy      int           NULL,
        CreatedDate    datetime2     NOT NULL DEFAULT GETDATE(),
        UpdatedBy      int           NULL,
        UpdatedDate    datetime2     NULL,
        DeletedBy      int           NULL,
        DeletedDate    datetime2     NULL,
        IsDeleted      bit           NOT NULL DEFAULT 0,
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END
GO

-- ================================================================
-- 25. PROMOTIONS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Promotions' AND xtype = 'U')
BEGIN
    CREATE TABLE Promotions (
        PromotionID       int           IDENTITY(1,1) PRIMARY KEY,
        Title             nvarchar(200) NOT NULL,
        Description       ntext         NULL,
        DiscountType      nvarchar(20)  NOT NULL, -- Percentage, FixedAmount
        DiscountValue     decimal(10,2) NOT NULL,
        MinimumAmount     decimal(10,2) NULL,
        MaximumDiscount   decimal(10,2) NULL,
        StartDate         datetime2     NOT NULL,
        EndDate           datetime2     NOT NULL,
        UsageLimit        int           NULL,
        UsedCount         int           NOT NULL DEFAULT 0,
        IsActive          bit           NOT NULL DEFAULT 1,
        CreatedBy         int           NULL,
        CreatedDate       datetime2     NOT NULL DEFAULT GETDATE(),
        UpdatedBy         int           NULL,
        UpdatedDate       datetime2     NULL,
        DeletedBy         int           NULL,
        DeletedDate       datetime2     NULL,
        IsDeleted         bit           NOT NULL DEFAULT 0
    );
END
GO

-- ================================================================
-- 26. CREDIT CARDS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'CreditCards' AND xtype = 'U')
BEGIN
    CREATE TABLE CreditCards (
        CardID       int           IDENTITY(1,1) PRIMARY KEY,
        UserID       int           NOT NULL,
        CardNumber   nvarchar(20)  NOT NULL,
        CardHolder   nvarchar(100) NOT NULL,
        ExpiryMonth  int           NOT NULL,
        ExpiryYear   int           NOT NULL,
        CVC          nvarchar(4)   NOT NULL,
        IsDefault    bit           NOT NULL DEFAULT 0,
        CreatedDate  datetime2     NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END
GO

-- ================================================================
-- 27. INVOICES TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Invoices' AND xtype = 'U')
BEGIN
    CREATE TABLE Invoices (
        InvoiceID    int           IDENTITY(1,1) PRIMARY KEY,
        UserID       int           NOT NULL,
        TripID       int           NOT NULL,
        Amount       decimal(10,2) NOT NULL,
        InvoiceDate  datetime2     NOT NULL DEFAULT GETDATE(),
        Status       nvarchar(20)  NOT NULL DEFAULT 'Paid',
        FOREIGN KEY (UserID) REFERENCES Users(UserID),
        FOREIGN KEY (TripID) REFERENCES Trips(TripID)
    );
END
GO

-- ================================================================
-- 28. FEEDBACKS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Feedbacks' AND xtype = 'U')
BEGIN
    CREATE TABLE Feedbacks (
        FeedbackID   int            IDENTITY(1,1) PRIMARY KEY,
        UserID       int            NULL,
        DriverID     int            NULL,
        TripID       int            NULL,
        Rating       decimal(2,1)   NOT NULL,
        Comment      nvarchar(1000) NULL,
        CreatedDate  datetime2      NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (UserID)   REFERENCES Users(UserID),
        FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
        FOREIGN KEY (TripID)   REFERENCES Trips(TripID)
    );
END
GO

-- ================================================================
-- 29. SHARED RIDES TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'SharedRides' AND xtype = 'U')
BEGIN
    CREATE TABLE SharedRides (
        SharedRideID    int          IDENTITY(1,1) PRIMARY KEY,
        TripID          int          NOT NULL,
        PassengerCount  int          NOT NULL,
        Status          nvarchar(20) NOT NULL,
        CreatedDate     datetime2    NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (TripID) REFERENCES Trips(TripID)
    );
END
GO

-- ================================================================
-- 30. MESSAGES TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Messages' AND xtype = 'U')
BEGIN
    CREATE TABLE Messages (
        MessageID   int            IDENTITY(1,1) PRIMARY KEY,
        SenderID    int            NOT NULL,
        ReceiverID  int            NOT NULL,
        Content     nvarchar(2000) NOT NULL,
        SentAt      datetime2      NOT NULL DEFAULT GETDATE(),
        IsRead      bit            NOT NULL DEFAULT 0,
        FOREIGN KEY (SenderID)   REFERENCES Users(UserID),
        FOREIGN KEY (ReceiverID) REFERENCES Users(UserID)
    );
END
GO

-- ================================================================
-- 31. OTP REQUESTS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'OtpRequests' AND xtype = 'U')
BEGIN
    CREATE TABLE OtpRequests (
        OtpID           int          IDENTITY(1,1) PRIMARY KEY,
        UserID          int          NULL,
        PhoneNumber     nvarchar(20) NOT NULL,
        OTPCode         nvarchar(10) NOT NULL,
        ExpirationTime  datetime2    NOT NULL,
        IsUsed          bit          NOT NULL DEFAULT 0,
        UsedAt          datetime2    NULL,
        RequestIP       nvarchar(50) NULL,
        RequestSource   nvarchar(100) NULL,
        CreatedDate     datetime2    NOT NULL DEFAULT GETDATE(),
        CreatedBy       int          NULL,
        IsDeleted       bit          NOT NULL DEFAULT 0,
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END
GO

-- ================================================================
-- 32. SMS LOGS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'SmsLogs' AND xtype = 'U')
BEGIN
    CREATE TABLE SmsLogs (
        SmsID            int            IDENTITY(1,1) PRIMARY KEY,
        PhoneNumber      nvarchar(20)   NOT NULL,
        MessageBody      nvarchar(1000) NULL,
        IsSuccess        bit            NOT NULL DEFAULT 0,
        SentAt           datetime2      NOT NULL DEFAULT GETDATE(),
        ProviderResponse nvarchar(1000) NULL,
        OTPID            int            NULL,
        CreatedBy        int            NULL,
        CreatedDate      datetime2      NOT NULL DEFAULT GETDATE(),
        IsDeleted        bit            NOT NULL DEFAULT 0,
        FOREIGN KEY (OTPID) REFERENCES OtpRequests(OtpID)
    );
END
GO

-- ================================================================
-- 33. USER NOTIFICATION SETTINGS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'UserNotificationSettings' AND xtype = 'U')
BEGIN
    CREATE TABLE UserNotificationSettings (
        SettingID         int       IDENTITY(1,1) PRIMARY KEY,
        UserID            int       NULL,
        AllowPromotions   bit       NOT NULL DEFAULT 1,
        AllowTripUpdates  bit       NOT NULL DEFAULT 1,
        AllowNews         bit       NOT NULL DEFAULT 1,
        CreatedDate       datetime2 NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END
GO

-- ================================================================
-- 34. APP VERSIONS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AppVersions' AND xtype = 'U')
BEGIN
    CREATE TABLE AppVersions (
        VersionID      int           IDENTITY(1,1) PRIMARY KEY,
        Platform       nvarchar(50)  NOT NULL,
        VersionNumber  nvarchar(20)  NOT NULL,
        IsMandatory    bit           NOT NULL DEFAULT 0,
        ReleaseNotes   nvarchar(2000) NULL,
        ReleaseDate    datetime2     NOT NULL DEFAULT GETDATE()
    );
END
GO

-- ================================================================
-- FUNCTIONS
-- ================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'CalculateDistance' AND type = 'FN')
BEGIN
    EXEC('
    CREATE FUNCTION dbo.CalculateDistance(
        @lat1 FLOAT, @lng1 FLOAT,
        @lat2 FLOAT, @lng2 FLOAT
    )
    RETURNS FLOAT
    AS
    BEGIN
        DECLARE @earthRadius FLOAT = 6371;
        DECLARE @dLat  FLOAT = RADIANS(@lat2 - @lat1);
        DECLARE @dLng  FLOAT = RADIANS(@lng2 - @lng1);
        DECLARE @a     FLOAT;
        DECLARE @c     FLOAT;

        SET @a = SIN(@dLat/2)*SIN(@dLat/2) +
                 COS(RADIANS(@lat1))*COS(RADIANS(@lat2))*
                 SIN(@dLng/2)*SIN(@dLng/2);
        SET @c = 2 * ATN2(SQRT(@a), SQRT(1-@a));
        RETURN @earthRadius * @c;
    END
    ');
END
GO

-- ================================================================
-- STORED PROCEDURES
-- ================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'FindNearbyDrivers' AND type = 'P')
BEGIN
    EXEC('
    CREATE PROCEDURE dbo.FindNearbyDrivers
        @Latitude   FLOAT,
        @Longitude  FLOAT,
        @RadiusKm   FLOAT = 5.0,
        @MaxResults INT   = 10,
        @MinRating  FLOAT = NULL
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT TOP (@MaxResults)
            d.DriverID, d.FullName, d.PhoneNumber, d.VehiclePlate, d.VehicleModel, d.VehicleColor,
            dl.Latitude, dl.Longitude, dl.IsAvailable, dl.LocationTime,
            dbo.CalculateDistance(@Latitude, @Longitude, dl.Latitude, dl.Longitude) AS Distance,
            COALESCE(AVG(CAST(dr.Rating AS FLOAT)), 0) AS AverageRating,
            COUNT(dr.DriverRatingID) AS TotalRatings
        FROM Drivers d
        INNER JOIN DriverLocations dl ON d.DriverID = dl.DriverID
        LEFT  JOIN DriverRatings   dr ON d.DriverID = dr.DriverID AND dr.IsDeleted = 0
        WHERE d.IsDeleted = 0
          AND dl.IsAvailable = 1
          AND dl.IsOnline    = 1
          AND dl.LocationTime > DATEADD(MINUTE, -30, GETUTCDATE())
          AND dbo.CalculateDistance(@Latitude, @Longitude, dl.Latitude, dl.Longitude) <= @RadiusKm
        GROUP BY d.DriverID, d.FullName, d.PhoneNumber, d.VehiclePlate, d.VehicleModel,
                 d.VehicleColor, dl.Latitude, dl.Longitude, dl.IsAvailable, dl.LocationTime
        HAVING (@MinRating IS NULL OR COALESCE(AVG(CAST(dr.Rating AS FLOAT)), 0) >= @MinRating)
        ORDER BY Distance ASC, AverageRating DESC;
    END
    ');
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'GetMatchingStatistics' AND type = 'P')
BEGIN
    EXEC('
    CREATE PROCEDURE dbo.GetMatchingStatistics
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT ''ActiveRequests'' AS StatType, COUNT(*) AS Value
        FROM MatchingRequests WHERE Status IN (0,1) AND IsDeleted = 0
        UNION ALL
        SELECT ''AvailableDrivers'', COUNT(DISTINCT dl.DriverID)
        FROM DriverLocations dl
        INNER JOIN Drivers d ON dl.DriverID = d.DriverID
        WHERE dl.IsAvailable = 1 AND dl.IsOnline = 1 AND d.IsDeleted = 0
          AND dl.LocationTime > DATEADD(MINUTE, -30, GETUTCDATE())
        UNION ALL
        SELECT ''TotalRequestsToday'', COUNT(*)
        FROM MatchingRequests
        WHERE CAST(RequestTime AS DATE) = CAST(GETUTCDATE() AS DATE) AND IsDeleted = 0
        UNION ALL
        SELECT ''SuccessfulMatchesToday'', COUNT(*)
        FROM MatchingRequests
        WHERE CAST(RequestTime AS DATE) = CAST(GETUTCDATE() AS DATE)
          AND Status = 1 AND IsDeleted = 0;
    END
    ');
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'CleanupExpiredRequests' AND type = 'P')
BEGIN
    EXEC('
    CREATE PROCEDURE dbo.CleanupExpiredRequests
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE MatchingRequests
        SET Status = 4, UpdatedDate = GETUTCDATE()
        WHERE Status = 0 AND ExpiryTime < GETUTCDATE() AND IsDeleted = 0;
        SELECT @@ROWCOUNT AS ExpiredRequestsCount;
    END
    ');
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'UpdateOfflineDrivers' AND type = 'P')
BEGIN
    EXEC('
    CREATE PROCEDURE dbo.UpdateOfflineDrivers
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE DriverLocations
        SET IsOnline = 0, IsAvailable = 0, UpdatedDate = GETUTCDATE()
        WHERE LocationTime < DATEADD(MINUTE, -60, GETUTCDATE()) AND IsOnline = 1;
        SELECT @@ROWCOUNT AS OfflineDriversCount;
    END
    ');
END
GO

-- ================================================================
-- TRIGGERS
-- ================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'TR_MatchingRequests_AcceptRequest' AND type = 'TR')
BEGIN
    EXEC('
    CREATE TRIGGER TR_MatchingRequests_AcceptRequest
    ON MatchingRequests
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE dl
        SET IsAvailable = 0, UpdatedDate = GETUTCDATE()
        FROM DriverLocations dl
        INNER JOIN inserted i ON dl.DriverID = i.DriverID
        WHERE i.Status = 1
          AND EXISTS (SELECT 1 FROM deleted d WHERE d.MatchingRequestID = i.MatchingRequestID AND d.Status != 1);
    END
    ');
END
GO

-- ================================================================
-- VIEWS
-- ================================================================

IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_ActiveMatchingRequests')
BEGIN
    EXEC('
    CREATE VIEW vw_ActiveMatchingRequests AS
    SELECT
        mr.MatchingRequestID, mr.PassengerID,
        u.FullName  AS PassengerName,  u.PhoneNumber AS PassengerPhone,
        mr.DriverID,
        d.FullName  AS DriverName,     d.PhoneNumber AS DriverPhone,
        d.VehiclePlate,
        mr.PickupAddress, mr.DropoffAddress,
        mr.EstimatedCost, mr.EstimatedDistance,
        mr.Status,
        CASE mr.Status
            WHEN 0 THEN ''Pending''   WHEN 1 THEN ''Accepted''
            WHEN 2 THEN ''Rejected''  WHEN 3 THEN ''Cancelled''
            WHEN 4 THEN ''Expired''   ELSE ''Unknown''
        END AS StatusText,
        mr.RequestTime, mr.AcceptedTime, mr.ExpiryTime
    FROM MatchingRequests mr
    INNER JOIN Users   u ON mr.PassengerID = u.UserID
    LEFT  JOIN Drivers d ON mr.DriverID    = d.DriverID
    WHERE mr.Status IN (0,1) AND mr.IsDeleted = 0;
    ');
END
GO

IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_AvailableDrivers')
BEGIN
    EXEC('
    CREATE VIEW vw_AvailableDrivers AS
    SELECT
        d.DriverID, d.FullName, d.PhoneNumber, d.VehiclePlate, d.VehicleModel, d.VehicleColor,
        dl.Latitude, dl.Longitude, dl.CurrentAddress, dl.LocationTime,
        COALESCE(AVG(CAST(dr.Rating AS FLOAT)), 0) AS AverageRating,
        COUNT(dr.DriverRatingID) AS TotalRatings
    FROM Drivers d
    INNER JOIN DriverLocations dl ON d.DriverID  = dl.DriverID
    LEFT  JOIN DriverRatings   dr ON d.DriverID  = dr.DriverID AND dr.IsDeleted = 0
    WHERE d.IsDeleted = 0 AND dl.IsAvailable = 1 AND dl.IsOnline = 1
      AND dl.LocationTime > DATEADD(MINUTE, -30, GETUTCDATE())
    GROUP BY d.DriverID, d.FullName, d.PhoneNumber, d.VehiclePlate, d.VehicleModel,
             d.VehicleColor, dl.Latitude, dl.Longitude, dl.CurrentAddress, dl.LocationTime;
    ');
END
GO

-- ================================================================
-- INITIAL DATA
-- ================================================================

IF NOT EXISTS (SELECT * FROM Admins)
BEGIN
    INSERT INTO Admins (FullName, Email, PasswordHash, RoleID, IsActive, CreatedDate)
    SELECT N'Sistem Yöneticisi', 'admin@takiwebapi.com', 'CHANGE_THIS_HASH', RoleID, 1, GETDATE()
    FROM AdminRoles WHERE RoleName = 'SuperAdmin';
END
GO

-- ================================================================
PRINT 'TaxiDb veritabani basariyla olusturuldu.';
PRINT 'Toplam tablo: 34 | SP: 4 | Fonksiyon: 1 | View: 2 | Trigger: 1';
-- ================================================================
