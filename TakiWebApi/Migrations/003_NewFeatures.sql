-- 003: Yeni ekranlar için şema (iptal nedeni/ücreti, bahşiş, promosyon kodu,
-- ileri tarihli yolculuk, sürücü belgesi onay durumu).
-- API'nin bu sürümü yayına alınmadan ÖNCE VeloxTaxiDb üzerinde bir kez
-- çalıştırılmalı (tekrar çalıştırmak güvenli).

-- İptal nedeni ve iptal ücreti
IF COL_LENGTH(N'dbo.MatchingRequests', N'CancelReason') IS NULL
    ALTER TABLE dbo.MatchingRequests ADD CancelReason nvarchar(200) NULL;
IF COL_LENGTH(N'dbo.MatchingRequests', N'CancellationFee') IS NULL
    ALTER TABLE dbo.MatchingRequests ADD CancellationFee decimal(10,2) NULL;
GO

-- Bahşiş (yolculuk başına bir kez)
IF COL_LENGTH(N'dbo.Trips', N'TipAmount') IS NULL
    ALTER TABLE dbo.Trips ADD TipAmount decimal(10,2) NULL;
GO

-- Promosyon kodu: Promotions tablosunda kod sütunu yoktu (tablo yoksa oluştur).
IF OBJECT_ID(N'dbo.Promotions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Promotions (
        PromotionID      int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title            nvarchar(200) NOT NULL,
        Description      ntext         NULL,
        DiscountType     nvarchar(20)  NOT NULL, -- Percentage | FixedAmount
        DiscountValue    decimal(10,2) NOT NULL,
        MinimumAmount    decimal(10,2) NULL,
        MaximumDiscount  decimal(10,2) NULL,
        StartDate        datetime2     NOT NULL,
        EndDate          datetime2     NOT NULL,
        UsageLimit       int           NULL,
        UsedCount        int           NOT NULL DEFAULT 0,
        IsActive         bit           NOT NULL DEFAULT 1,
        CreatedBy        int           NULL,
        CreatedDate      datetime2     NOT NULL DEFAULT GETDATE()
    );
END
GO
IF COL_LENGTH(N'dbo.Promotions', N'Code') IS NULL
    ALTER TABLE dbo.Promotions ADD Code nvarchar(50) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Promotions_Code')
    CREATE UNIQUE INDEX UX_Promotions_Code ON dbo.Promotions (Code) WHERE Code IS NOT NULL;
GO

-- Kullanıcı başına bir kullanım
IF OBJECT_ID(N'dbo.PromotionUsages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PromotionUsages (
        PromotionUsageID  int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PromotionUsages PRIMARY KEY,
        PromotionID       int       NOT NULL,
        UserID            int       NOT NULL,
        MatchingRequestID int       NULL,
        DiscountAmount    decimal(10,2) NOT NULL,
        UsedAt            datetime2 NOT NULL CONSTRAINT DF_PromotionUsages_UsedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_PromotionUsages_Promotions FOREIGN KEY (PromotionID) REFERENCES dbo.Promotions (PromotionID)
    );
    CREATE UNIQUE INDEX UX_PromotionUsages_User ON dbo.PromotionUsages (PromotionID, UserID);
END
GO

-- İleri tarihli yolculuk
IF OBJECT_ID(N'dbo.ScheduledRides', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ScheduledRides (
        ScheduledRideID  int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScheduledRides PRIMARY KEY,
        UserID           int            NOT NULL,
        PickupAddress    nvarchar(500)  NOT NULL,
        PickupLatitude   float          NOT NULL,
        PickupLongitude  float          NOT NULL,
        DropoffAddress   nvarchar(500)  NOT NULL,
        DropoffLatitude  float          NOT NULL,
        DropoffLongitude float          NOT NULL,
        ScheduledTime    datetime2      NOT NULL,   -- UTC
        VehicleType      varchar(20)    NULL,
        PaymentMethod    varchar(10)    NULL,
        EstimatedCost    decimal(10,2)  NULL,
        Status           varchar(20)    NOT NULL CONSTRAINT DF_ScheduledRides_Status DEFAULT 'Scheduled', -- Scheduled | Reminded | Cancelled
        CreatedDate      datetime2      NOT NULL CONSTRAINT DF_ScheduledRides_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ScheduledRides_Users FOREIGN KEY (UserID) REFERENCES dbo.Users (UserID)
    );
    CREATE INDEX IX_ScheduledRides_Due ON dbo.ScheduledRides (Status, ScheduledTime);
    CREATE INDEX IX_ScheduledRides_User ON dbo.ScheduledRides (UserID);
END
GO

-- Sürücü belgesi onay durumu
IF COL_LENGTH(N'dbo.DriverDocuments', N'Status') IS NULL
    ALTER TABLE dbo.DriverDocuments ADD Status varchar(20) NOT NULL
        CONSTRAINT DF_DriverDocuments_Status DEFAULT 'Pending'; -- Pending | Approved | Rejected
IF COL_LENGTH(N'dbo.DriverDocuments', N'ReviewNote') IS NULL
    ALTER TABLE dbo.DriverDocuments ADD ReviewNote nvarchar(500) NULL;
GO
