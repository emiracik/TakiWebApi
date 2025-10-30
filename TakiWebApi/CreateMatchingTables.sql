-- ===============================================
-- MATCHING SYSTEM DATABASE TABLES
-- TakiWebApi - Driver-User Matching System yapılacak
-- ===============================================

USE TaxiDb;
GO

-- ===============================================
-- MATCHING REQUESTS TABLE
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
    EstimatedDuration int NULL, -- minutes
    Status int NOT NULL DEFAULT 0, -- 0:Pending, 1:Accepted, 2:Rejected, 3:Cancelled, 4:Expired
    RequestTime datetime2 NOT NULL DEFAULT GETUTCDATE(),
    AcceptedTime datetime2 NULL,
    ExpiryTime datetime2 NULL,
    Notes nvarchar(1000) NULL,
    MaxWaitTime float NULL, -- minutes
    MinRating float NULL,
    MaxDistance float NULL, -- km from pickup
    
    -- Audit fields
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

-- ===============================================
-- DRIVER LOCATIONS TABLE
-- ===============================================

CREATE TABLE DriverLocations (
    LocationID int IDENTITY(1,1) PRIMARY KEY,
    DriverID int NOT NULL,
    Latitude float NOT NULL,
    Longitude float NOT NULL,
    IsAvailable bit NOT NULL DEFAULT 1,
    CurrentAddress nvarchar(100) NULL,
    LocationTime datetime2 NOT NULL DEFAULT GETUTCDATE(),
    Speed float NULL, -- km/h
    Heading float NULL, -- degrees
    IsOnline bit NOT NULL DEFAULT 1,
    
    -- Audit fields
    CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate datetime2 NULL,
    
    -- Foreign Keys
    CONSTRAINT FK_DriverLocations_DriverID FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID),
    
    -- Unique constraint - one location per driver
    CONSTRAINT UQ_DriverLocations_DriverID UNIQUE (DriverID)
);

-- ===============================================
-- INDEXES FOR PERFORMANCE
-- ===============================================

-- MatchingRequests Indexes
CREATE INDEX IX_MatchingRequests_PassengerID ON MatchingRequests(PassengerID);
CREATE INDEX IX_MatchingRequests_DriverID ON MatchingRequests(DriverID);
CREATE INDEX IX_MatchingRequests_Status ON MatchingRequests(Status);
CREATE INDEX IX_MatchingRequests_RequestTime ON MatchingRequests(RequestTime);
CREATE INDEX IX_MatchingRequests_IsDeleted ON MatchingRequests(IsDeleted);

-- Composite index for pending requests
CREATE INDEX IX_MatchingRequests_Status_IsDeleted ON MatchingRequests(Status, IsDeleted);

-- DriverLocations Indexes
CREATE INDEX IX_DriverLocations_DriverID ON DriverLocations(DriverID);
CREATE INDEX IX_DriverLocations_IsAvailable ON DriverLocations(IsAvailable);
CREATE INDEX IX_DriverLocations_IsOnline ON DriverLocations(IsOnline);
CREATE INDEX IX_DriverLocations_LocationTime ON DriverLocations(LocationTime);

-- Spatial index for location-based queries (if using SQL Server with spatial support)
-- CREATE SPATIAL INDEX IX_DriverLocations_Spatial ON DriverLocations(geography_point);

-- Composite indexes for common queries
CREATE INDEX IX_DriverLocations_Available_Online ON DriverLocations(IsAvailable, IsOnline, LocationTime);

-- ===============================================
-- SAMPLE DATA FOR TESTING
-- ===============================================

-- Insert sample driver locations
INSERT INTO DriverLocations (DriverID, Latitude, Longitude, IsAvailable, CurrentAddress, LocationTime, IsOnline)
VALUES 
    (1, 41.0082, 28.9784, 1, 'Beşiktaş Merkez', GETUTCDATE(), 1),
    (2, 40.9833, 29.0167, 1, 'Kadıköy İskele', GETUTCDATE(), 1),
    (3, 41.0369, 28.9850, 0, 'Taksim Meydanı', GETUTCDATE(), 1);

-- Insert sample matching requests
INSERT INTO MatchingRequests (PassengerID, PickupAddress, DropoffAddress, PickupLatitude, PickupLongitude, DropoffLatitude, DropoffLongitude, Status, EstimatedCost, EstimatedDistance, EstimatedDuration, MaxWaitTime, MinRating, MaxDistance)
VALUES 
    (1, 'Beşiktaş İskele', 'Kabataş Vapur İskelesi', 41.0428, 29.0094, 41.0611, 28.9847, 0, 25.50, 2.3, 8, 15.0, 4.0, 5.0),
    (2, 'Kadıköy Merkez', 'Üsküdar İskele', 40.9833, 29.0167, 41.0167, 29.0167, 1, 18.75, 1.8, 6, 20.0, 3.5, 8.0),
    (3, 'Taksim Meydanı', 'Galata Kulesi', 41.0369, 28.9850, 41.0256, 28.9744, 2, 15.25, 1.2, 5, 10.0, 4.5, 3.0);

-- ===============================================
-- UTILITY FUNCTIONS AND STORED PROCEDURES
-- ===============================================

-- Function to calculate distance between two points (Haversine formula)
GO
CREATE FUNCTION dbo.CalculateDistance(
    @lat1 FLOAT,
    @lng1 FLOAT,
    @lat2 FLOAT,
    @lng2 FLOAT
)
RETURNS FLOAT
AS
BEGIN
    DECLARE @earthRadius FLOAT = 6371; -- Earth radius in km
    DECLARE @dLat FLOAT = RADIANS(@lat2 - @lat1);
    DECLARE @dLng FLOAT = RADIANS(@lng2 - @lng1);
    DECLARE @a FLOAT;
    DECLARE @c FLOAT;
    DECLARE @distance FLOAT;

    SET @a = SIN(@dLat / 2) * SIN(@dLat / 2) + 
             COS(RADIANS(@lat1)) * COS(RADIANS(@lat2)) * 
             SIN(@dLng / 2) * SIN(@dLng / 2);
    
    SET @c = 2 * ATAN2(SQRT(@a), SQRT(1 - @a));
    SET @distance = @earthRadius * @c;

    RETURN @distance;
END
GO

-- Stored procedure to find nearby drivers
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
        COUNT(dr.RatingID) as TotalRatings
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

-- Stored procedure to get matching statistics
CREATE PROCEDURE dbo.GetMatchingStatistics
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        'ActiveRequests' AS StatType,
        COUNT(*) AS Value
    FROM MatchingRequests 
    WHERE Status IN (0, 1) AND IsDeleted = 0

    UNION ALL

    SELECT 
        'AvailableDrivers' AS StatType,
        COUNT(DISTINCT dl.DriverID) AS Value
    FROM DriverLocations dl
    INNER JOIN Drivers d ON dl.DriverID = d.DriverID
    WHERE dl.IsAvailable = 1 
        AND dl.IsOnline = 1 
        AND d.IsDeleted = 0
        AND dl.LocationTime > DATEADD(MINUTE, -30, GETUTCDATE())

    UNION ALL

    SELECT 
        'TotalRequestsToday' AS StatType,
        COUNT(*) AS Value
    FROM MatchingRequests 
    WHERE CAST(RequestTime AS DATE) = CAST(GETUTCDATE() AS DATE)
        AND IsDeleted = 0

    UNION ALL

    SELECT 
        'SuccessfulMatchesToday' AS StatType,
        COUNT(*) AS Value
    FROM MatchingRequests 
    WHERE CAST(RequestTime AS DATE) = CAST(GETUTCDATE() AS DATE)
        AND Status = 1 -- Accepted
        AND IsDeleted = 0;
END
GO

-- ===============================================
-- MAINTENANCE PROCEDURES
-- ===============================================

-- Procedure to clean up expired requests
CREATE PROCEDURE dbo.CleanupExpiredRequests
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE MatchingRequests 
    SET Status = 4, -- Expired
        UpdatedDate = GETUTCDATE()
    WHERE Status = 0 -- Pending
        AND ExpiryTime < GETUTCDATE()
        AND IsDeleted = 0;

    SELECT @@ROWCOUNT AS ExpiredRequestsCount;
END
GO

-- Procedure to update offline drivers
CREATE PROCEDURE dbo.UpdateOfflineDrivers
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE DriverLocations 
    SET IsOnline = 0,
        IsAvailable = 0,
        UpdatedDate = GETUTCDATE()
    WHERE LocationTime < DATEADD(MINUTE, -60, GETUTCDATE())
        AND IsOnline = 1;

    SELECT @@ROWCOUNT AS OfflineDriversCount;
END
GO

-- ===============================================
-- TRIGGERS FOR DATA CONSISTENCY
-- ===============================================

-- Trigger to automatically set driver availability to false when request is accepted
GO
CREATE TRIGGER TR_MatchingRequests_AcceptRequest
ON MatchingRequests
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- When a request is accepted, set driver as unavailable
    UPDATE dl
    SET IsAvailable = 0,
        UpdatedDate = GETUTCDATE()
    FROM DriverLocations dl
    INNER JOIN inserted i ON dl.DriverID = i.DriverID
    WHERE i.Status = 1 -- Accepted
        AND EXISTS (SELECT 1 FROM deleted d WHERE d.MatchingRequestID = i.MatchingRequestID AND d.Status != 1);
END
GO

-- ===============================================
-- VIEWS FOR REPORTING
-- ===============================================

-- View for active matching requests with driver and passenger details
GO
CREATE VIEW vw_ActiveMatchingRequests
AS
SELECT 
    mr.MatchingRequestID,
    mr.PassengerID,
    u.FullName AS PassengerName,
    u.PhoneNumber AS PassengerPhone,
    mr.DriverID,
    d.FullName AS DriverName,
    d.PhoneNumber AS DriverPhone,
    d.VehiclePlate,
    mr.PickupAddress,
    mr.DropoffAddress,
    mr.EstimatedCost,
    mr.EstimatedDistance,
    mr.Status,
    CASE mr.Status 
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Accepted'
        WHEN 2 THEN 'Rejected'
        WHEN 3 THEN 'Cancelled'
        WHEN 4 THEN 'Expired'
        ELSE 'Unknown'
    END AS StatusText,
    mr.RequestTime,
    mr.AcceptedTime,
    mr.ExpiryTime
FROM MatchingRequests mr
INNER JOIN Users u ON mr.PassengerID = u.UserID
LEFT JOIN Drivers d ON mr.DriverID = d.DriverID
WHERE mr.Status IN (0, 1) -- Pending or Accepted
    AND mr.IsDeleted = 0;
GO

-- View for available drivers with their current location
CREATE VIEW vw_AvailableDrivers
AS
SELECT 
    d.DriverID,
    d.FullName,
    d.PhoneNumber,
    d.VehiclePlate,
    d.VehicleModel,
    d.VehicleColor,
    dl.Latitude,
    dl.Longitude,
    dl.CurrentAddress,
    dl.LocationTime,
    COALESCE(AVG(CAST(dr.Rating as FLOAT)), 0) as AverageRating,
    COUNT(dr.RatingID) as TotalRatings
FROM Drivers d
INNER JOIN DriverLocations dl ON d.DriverID = dl.DriverID
LEFT JOIN DriverRatings dr ON d.DriverID = dr.DriverID AND dr.IsDeleted = 0
WHERE d.IsDeleted = 0 
    AND dl.IsAvailable = 1 
    AND dl.IsOnline = 1
    AND dl.LocationTime > DATEADD(MINUTE, -30, GETUTCDATE())
GROUP BY d.DriverID, d.FullName, d.PhoneNumber, d.VehiclePlate, d.VehicleModel, 
         d.VehicleColor, dl.Latitude, dl.Longitude, dl.CurrentAddress, dl.LocationTime;
GO

-- ===============================================
-- SUCCESS MESSAGE
-- ===============================================

PRINT '✅ Matching system database tables created successfully!';
PRINT '📊 Created tables: MatchingRequests, DriverLocations';
PRINT '🔧 Created functions: CalculateDistance';
PRINT '⚡ Created procedures: FindNearbyDrivers, GetMatchingStatistics, CleanupExpiredRequests, UpdateOfflineDrivers';
PRINT '👁️ Created views: vw_ActiveMatchingRequests, vw_AvailableDrivers';
PRINT '🚀 Matching system is ready to use!';

-- Sample query to test the system
SELECT 'System Check' AS Test, COUNT(*) AS TableCount 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('MatchingRequests', 'DriverLocations');

EXEC dbo.GetMatchingStatistics;