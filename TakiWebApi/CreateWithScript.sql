USE [master]
GO
/****** Object:  Database [TaxiDb]    Script Date: 26.11.2025 16:23:09 ******/
CREATE DATABASE [TaxiDb]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'TaxiDb', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\TaxiDb.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'TaxiDb_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\TaxiDb_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [TaxiDb] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [TaxiDb].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [TaxiDb] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [TaxiDb] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [TaxiDb] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [TaxiDb] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [TaxiDb] SET ARITHABORT OFF 
GO
ALTER DATABASE [TaxiDb] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [TaxiDb] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [TaxiDb] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [TaxiDb] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [TaxiDb] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [TaxiDb] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [TaxiDb] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [TaxiDb] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [TaxiDb] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [TaxiDb] SET  DISABLE_BROKER 
GO
ALTER DATABASE [TaxiDb] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [TaxiDb] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [TaxiDb] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [TaxiDb] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [TaxiDb] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [TaxiDb] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [TaxiDb] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [TaxiDb] SET RECOVERY FULL 
GO
ALTER DATABASE [TaxiDb] SET  MULTI_USER 
GO
ALTER DATABASE [TaxiDb] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [TaxiDb] SET DB_CHAINING OFF 
GO
ALTER DATABASE [TaxiDb] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [TaxiDb] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [TaxiDb] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [TaxiDb] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'TaxiDb', N'ON'
GO
ALTER DATABASE [TaxiDb] SET QUERY_STORE = ON
GO
ALTER DATABASE [TaxiDb] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [TaxiDb]
GO
/****** Object:  UserDefinedFunction [dbo].[CalculateDistance]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[CalculateDistance](
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
    
    SET @c = 2 * ATN2(SQRT(@a), SQRT(1 - @a));
    SET @distance = @earthRadius * @c;

    RETURN @distance;
END
GO
/****** Object:  Table [dbo].[Users]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](200) NOT NULL,
	[PhoneNumber] [nvarchar](20) NOT NULL,
	[Email] [nvarchar](200) NULL,
	[PasswordHash] [nvarchar](255) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Drivers]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Drivers](
	[DriverID] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](200) NOT NULL,
	[PhoneNumber] [nvarchar](20) NOT NULL,
	[Email] [nvarchar](200) NULL,
	[PasswordHash] [nvarchar](255) NULL,
	[VehiclePlate] [nvarchar](20) NULL,
	[VehicleModel] [nvarchar](100) NULL,
	[VehicleColor] [nvarchar](50) NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DriverID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MatchingRequests]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MatchingRequests](
	[MatchingRequestID] [int] IDENTITY(1,1) NOT NULL,
	[PassengerID] [int] NOT NULL,
	[DriverID] [int] NULL,
	[PickupAddress] [nvarchar](500) NOT NULL,
	[DropoffAddress] [nvarchar](500) NOT NULL,
	[PickupLatitude] [float] NOT NULL,
	[PickupLongitude] [float] NOT NULL,
	[DropoffLatitude] [float] NOT NULL,
	[DropoffLongitude] [float] NOT NULL,
	[EstimatedCost] [decimal](10, 2) NULL,
	[EstimatedDistance] [float] NULL,
	[EstimatedDuration] [int] NULL,
	[Status] [int] NOT NULL,
	[RequestTime] [datetime2](7) NOT NULL,
	[AcceptedTime] [datetime2](7) NULL,
	[ExpiryTime] [datetime2](7) NULL,
	[Notes] [nvarchar](1000) NULL,
	[MaxWaitTime] [float] NULL,
	[MinRating] [float] NULL,
	[MaxDistance] [float] NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MatchingRequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_ActiveMatchingRequests]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_ActiveMatchingRequests]
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
/****** Object:  Table [dbo].[DriverLocations]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DriverLocations](
	[LocationID] [int] IDENTITY(1,1) NOT NULL,
	[DriverID] [int] NOT NULL,
	[Latitude] [float] NOT NULL,
	[Longitude] [float] NOT NULL,
	[IsAvailable] [bit] NOT NULL,
	[CurrentAddress] [nvarchar](100) NULL,
	[LocationTime] [datetime2](7) NOT NULL,
	[Speed] [float] NULL,
	[Heading] [float] NULL,
	[IsOnline] [bit] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[LocationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_DriverLocations_DriverID] UNIQUE NONCLUSTERED 
(
	[DriverID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DriverRatings]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DriverRatings](
	[DriverRatingID] [int] IDENTITY(1,1) NOT NULL,
	[TripID] [int] NOT NULL,
	[DriverID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[Rating] [decimal](2, 1) NOT NULL,
	[Comment] [nvarchar](1000) NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DriverRatingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_AvailableDrivers]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- View for available drivers with their current location
CREATE VIEW [dbo].[vw_AvailableDrivers]
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
    COUNT(dr.DriverRatingID) as TotalRatings
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
/****** Object:  Table [dbo].[AdminRoles]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdminRoles](
	[AdminRoleID] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Permissions] [ntext] NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AdminRoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[RoleName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Admins]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Admins](
	[AdminID] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](200) NOT NULL,
	[Email] [nvarchar](200) NOT NULL,
	[PasswordHash] [nvarchar](255) NOT NULL,
	[Role] [nvarchar](50) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[LastLoginDate] [datetime2](7) NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AdminID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AnnouncementRead]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AnnouncementRead](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[userId] [int] NOT NULL,
	[announcementId] [int] NOT NULL,
	[createdDate] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Announcements]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Announcements](
	[AnnouncementID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](300) NOT NULL,
	[Content] [ntext] NULL,
	[PublishedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AnnouncementID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AppVersions]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppVersions](
	[VersionID] [int] IDENTITY(1,1) NOT NULL,
	[Platform] [nvarchar](50) NOT NULL,
	[VersionNumber] [nvarchar](20) NOT NULL,
	[IsMandatory] [bit] NULL,
	[ReleaseNotes] [nvarchar](2000) NULL,
	[ReleaseDate] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[VersionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BankInfo]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BankInfo](
	[BankInfoID] [int] IDENTITY(1,1) NOT NULL,
	[DriverID] [int] NOT NULL,
	[BankName] [nvarchar](100) NOT NULL,
	[IBAN] [nvarchar](34) NOT NULL,
	[AccountNumber] [nvarchar](30) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[BankInfoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Blogs]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Blogs](
	[BlogID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](300) NOT NULL,
	[Content] [ntext] NULL,
	[ImageUrl] [nvarchar](500) NULL,
	[PublishedAt] [datetime2](7) NULL,
	[IsPublished] [bit] NOT NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[BlogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CreditCards]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CreditCards](
	[CardID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[CardNumber] [nvarchar](20) NOT NULL,
	[CardHolder] [nvarchar](100) NOT NULL,
	[ExpiryMonth] [int] NOT NULL,
	[ExpiryYear] [int] NOT NULL,
	[CVC] [nvarchar](4) NOT NULL,
	[IsDefault] [bit] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CardID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DriverDetails]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DriverDetails](
	[DriverDetailID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[VehiclePlate] [nvarchar](20) NULL,
	[VehicleModel] [nvarchar](100) NULL,
	[VehicleColor] [nvarchar](50) NULL,
	[LicenseNumber] [nvarchar](50) NULL,
	[ExperienceYears] [int] NULL,
	[RatingAverage] [float] NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[DriverDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DriverDocuments]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DriverDocuments](
	[DocumentID] [int] IDENTITY(1,1) NOT NULL,
	[DriverID] [int] NOT NULL,
	[DocumentType] [nvarchar](50) NOT NULL,
	[DocumentUrl] [nvarchar](500) NOT NULL,
	[UploadedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DocumentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DriverEarnings]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DriverEarnings](
	[EarningID] [int] IDENTITY(1,1) NOT NULL,
	[DriverID] [int] NULL,
	[TripID] [int] NULL,
	[Amount] [decimal](10, 2) NULL,
	[EarnedAt] [datetime2](7) NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[EarningID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DriverReviews]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DriverReviews](
	[ReviewID] [int] IDENTITY(1,1) NOT NULL,
	[TripID] [int] NULL,
	[DriverID] [int] NULL,
	[UserID] [int] NULL,
	[ReviewText] [nvarchar](1000) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedBy] [int] NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[ReviewID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FAQs]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FAQs](
	[FAQID] [int] IDENTITY(1,1) NOT NULL,
	[Question] [nvarchar](500) NOT NULL,
	[Answer] [nvarchar](2000) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[SortOrder] [int] NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[FAQID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Feedbacks]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Feedbacks](
	[FeedbackID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[DriverID] [int] NULL,
	[TripID] [int] NULL,
	[Rating] [decimal](2, 1) NOT NULL,
	[Comment] [nvarchar](1000) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[FeedbackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Invoices]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Invoices](
	[InvoiceID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[TripID] [int] NOT NULL,
	[Amount] [decimal](10, 2) NOT NULL,
	[InvoiceDate] [datetime2](7) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[InvoiceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Messages]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Messages](
	[MessageID] [int] IDENTITY(1,1) NOT NULL,
	[SenderID] [int] NOT NULL,
	[ReceiverID] [int] NOT NULL,
	[Content] [nvarchar](2000) NOT NULL,
	[SentAt] [datetime2](7) NOT NULL,
	[IsRead] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MessageID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notifications]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notifications](
	[NotificationID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Message] [ntext] NOT NULL,
	[UserID] [int] NULL,
	[DriverID] [int] NULL,
	[NotificationType] [nvarchar](50) NOT NULL,
	[IsRead] [bit] NOT NULL,
	[ReadDate] [datetime2](7) NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[NotificationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OtpRequests]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OtpRequests](
	[OtpID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[PhoneNumber] [nvarchar](20) NOT NULL,
	[OTPCode] [nvarchar](10) NOT NULL,
	[ExpirationTime] [datetime2](7) NOT NULL,
	[IsUsed] [bit] NULL,
	[UsedAt] [datetime2](7) NULL,
	[RequestIP] [nvarchar](50) NULL,
	[RequestSource] [nvarchar](100) NULL,
	[CreatedDate] [datetime2](7) NULL,
	[CreatedBy] [int] NULL,
	[IsDeleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[OtpID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Promotions]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Promotions](
	[PromotionID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Description] [ntext] NULL,
	[DiscountType] [nvarchar](20) NOT NULL,
	[DiscountValue] [decimal](10, 2) NOT NULL,
	[MinimumAmount] [decimal](10, 2) NULL,
	[MaximumDiscount] [decimal](10, 2) NULL,
	[StartDate] [datetime2](7) NOT NULL,
	[EndDate] [datetime2](7) NOT NULL,
	[UsageLimit] [int] NULL,
	[UsedCount] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PromotionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SharedRides]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SharedRides](
	[SharedRideID] [int] IDENTITY(1,1) NOT NULL,
	[TripID] [int] NOT NULL,
	[PassengerCount] [int] NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[SharedRideID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SmsLogs]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SmsLogs](
	[SmsID] [int] IDENTITY(1,1) NOT NULL,
	[PhoneNumber] [nvarchar](20) NOT NULL,
	[MessageBody] [nvarchar](1000) NULL,
	[IsSuccess] [bit] NULL,
	[SentAt] [datetime2](7) NULL,
	[ProviderResponse] [nvarchar](1000) NULL,
	[OTPID] [int] NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[SmsID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SupportTickets]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SupportTickets](
	[SupportTicketID] [int] IDENTITY(1,1) NOT NULL,
	[Subject] [nvarchar](300) NOT NULL,
	[Description] [ntext] NOT NULL,
	[UserID] [int] NULL,
	[DriverID] [int] NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Priority] [nvarchar](20) NOT NULL,
	[AssignedTo] [int] NULL,
	[ResolvedDate] [datetime2](7) NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[SupportTicketID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TripRatings]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TripRatings](
	[TripRatingID] [int] IDENTITY(1,1) NOT NULL,
	[TripID] [int] NOT NULL,
	[RatedByUserID] [int] NOT NULL,
	[Rating] [decimal](2, 1) NOT NULL,
	[Comment] [nvarchar](1000) NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TripRatingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Trips]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Trips](
	[TripID] [int] IDENTITY(1,1) NOT NULL,
	[PassengerID] [int] NULL,
	[DriverID] [int] NULL,
	[StartAddress] [nvarchar](500) NULL,
	[EndAddress] [nvarchar](500) NULL,
	[StartLatitude] [float] NULL,
	[StartLongitude] [float] NULL,
	[EndLatitude] [float] NULL,
	[EndLongitude] [float] NULL,
	[StartTime] [datetime2](7) NULL,
	[EndTime] [datetime2](7) NULL,
	[Cost] [decimal](10, 2) NULL,
	[PaymentMethod] [nvarchar](50) NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[Distance] [float] NULL,
	[Status] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[TripID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserAddresses]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserAddresses](
	[UserAddressID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[AddressName] [nvarchar](100) NOT NULL,
	[FullAddress] [nvarchar](500) NOT NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
	[IsDefault] [bit] NOT NULL,
	[AddressType] [nvarchar](50) NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserAddressID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserCreditCards]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserCreditCards](
	[CardID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[CardHolderName] [nvarchar](200) NULL,
	[CardNumberMasked] [nvarchar](20) NULL,
	[ExpiryMonth] [int] NULL,
	[ExpiryYear] [int] NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NULL,
	[CardType] [nvarchar](30) NULL,
PRIMARY KEY CLUSTERED 
(
	[CardID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserNotificationSettings]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserNotificationSettings](
	[SettingID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[AllowPromotions] [bit] NULL,
	[AllowTripUpdates] [bit] NULL,
	[AllowNews] [bit] NULL,
	[CreatedDate] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[SettingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserRatings]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserRatings](
	[UserRatingID] [int] IDENTITY(1,1) NOT NULL,
	[TripID] [int] NOT NULL,
	[RatedUserID] [int] NOT NULL,
	[RatedByDriverID] [int] NOT NULL,
	[Rating] [decimal](2, 1) NOT NULL,
	[Comment] [nvarchar](1000) NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserRatingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Wallets]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Wallets](
	[WalletID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[Balance] [decimal](18, 2) NOT NULL,
	[TotalIn] [decimal](18, 2) NOT NULL,
	[TotalOut] [decimal](18, 2) NOT NULL,
	[LastUpdated] [datetime2](7) NOT NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedBy] [int] NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[WalletID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WalletTransactions]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WalletTransactions](
	[TransactionID] [int] IDENTITY(1,1) NOT NULL,
	[WalletID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[TransactionType] [nvarchar](50) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[TransactionDate] [datetime2](7) NOT NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TransactionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_Announcements_IsDeleted]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Announcements_IsDeleted] ON [dbo].[Announcements]
(
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Announcements_PublishedAt]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Announcements_PublishedAt] ON [dbo].[Announcements]
(
	[PublishedAt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Blogs_IsDeleted]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Blogs_IsDeleted] ON [dbo].[Blogs]
(
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Blogs_IsPublished]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Blogs_IsPublished] ON [dbo].[Blogs]
(
	[IsPublished] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Blogs_PublishedAt]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Blogs_PublishedAt] ON [dbo].[Blogs]
(
	[PublishedAt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DriverEarnings_DriverID]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_DriverEarnings_DriverID] ON [dbo].[DriverEarnings]
(
	[DriverID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DriverLocations_Available_Online]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_DriverLocations_Available_Online] ON [dbo].[DriverLocations]
(
	[IsAvailable] ASC,
	[IsOnline] ASC,
	[LocationTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DriverLocations_DriverID]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_DriverLocations_DriverID] ON [dbo].[DriverLocations]
(
	[DriverID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DriverLocations_IsAvailable]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_DriverLocations_IsAvailable] ON [dbo].[DriverLocations]
(
	[IsAvailable] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DriverLocations_IsOnline]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_DriverLocations_IsOnline] ON [dbo].[DriverLocations]
(
	[IsOnline] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DriverLocations_LocationTime]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_DriverLocations_LocationTime] ON [dbo].[DriverLocations]
(
	[LocationTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Drivers_IsDeleted]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Drivers_IsDeleted] ON [dbo].[Drivers]
(
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Drivers_PhoneNumber]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Drivers_PhoneNumber] ON [dbo].[Drivers]
(
	[PhoneNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Drivers_VehiclePlate]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Drivers_VehiclePlate] ON [dbo].[Drivers]
(
	[VehiclePlate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_FAQs_IsActive]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_FAQs_IsActive] ON [dbo].[FAQs]
(
	[IsActive] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_FAQs_IsDeleted]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_FAQs_IsDeleted] ON [dbo].[FAQs]
(
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_FAQs_SortOrder]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_FAQs_SortOrder] ON [dbo].[FAQs]
(
	[SortOrder] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MatchingRequests_DriverID]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_MatchingRequests_DriverID] ON [dbo].[MatchingRequests]
(
	[DriverID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MatchingRequests_IsDeleted]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_MatchingRequests_IsDeleted] ON [dbo].[MatchingRequests]
(
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MatchingRequests_PassengerID]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_MatchingRequests_PassengerID] ON [dbo].[MatchingRequests]
(
	[PassengerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MatchingRequests_RequestTime]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_MatchingRequests_RequestTime] ON [dbo].[MatchingRequests]
(
	[RequestTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MatchingRequests_Status]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_MatchingRequests_Status] ON [dbo].[MatchingRequests]
(
	[Status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MatchingRequests_Status_IsDeleted]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_MatchingRequests_Status_IsDeleted] ON [dbo].[MatchingRequests]
(
	[Status] ASC,
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_OtpRequests_PhoneNumber]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_OtpRequests_PhoneNumber] ON [dbo].[OtpRequests]
(
	[PhoneNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Trips_DriverID]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Trips_DriverID] ON [dbo].[Trips]
(
	[DriverID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Trips_IsDeleted]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Trips_IsDeleted] ON [dbo].[Trips]
(
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Trips_PassengerID]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Trips_PassengerID] ON [dbo].[Trips]
(
	[PassengerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Trips_StartTime]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Trips_StartTime] ON [dbo].[Trips]
(
	[StartTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_Email]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Users_IsDeleted]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Users_IsDeleted] ON [dbo].[Users]
(
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_PhoneNumber]    Script Date: 26.11.2025 16:23:09 ******/
CREATE NONCLUSTERED INDEX [IX_Users_PhoneNumber] ON [dbo].[Users]
(
	[PhoneNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AdminRoles] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[AdminRoles] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[AdminRoles] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Admins] ADD  DEFAULT ('Admin') FOR [Role]
GO
ALTER TABLE [dbo].[Admins] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Admins] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Admins] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[AnnouncementRead] ADD  DEFAULT (getdate()) FOR [createdDate]
GO
ALTER TABLE [dbo].[Announcements] ADD  DEFAULT (getdate()) FOR [PublishedAt]
GO
ALTER TABLE [dbo].[Announcements] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Announcements] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[AppVersions] ADD  DEFAULT ((0)) FOR [IsMandatory]
GO
ALTER TABLE [dbo].[AppVersions] ADD  DEFAULT (getdate()) FOR [ReleaseDate]
GO
ALTER TABLE [dbo].[BankInfo] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Blogs] ADD  DEFAULT ((0)) FOR [IsPublished]
GO
ALTER TABLE [dbo].[Blogs] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Blogs] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[CreditCards] ADD  DEFAULT ((0)) FOR [IsDefault]
GO
ALTER TABLE [dbo].[CreditCards] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[DriverDetails] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[DriverDetails] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[DriverDocuments] ADD  DEFAULT (getdate()) FOR [UploadedDate]
GO
ALTER TABLE [dbo].[DriverEarnings] ADD  DEFAULT (getdate()) FOR [EarnedAt]
GO
ALTER TABLE [dbo].[DriverEarnings] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[DriverEarnings] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[DriverLocations] ADD  DEFAULT ((1)) FOR [IsAvailable]
GO
ALTER TABLE [dbo].[DriverLocations] ADD  DEFAULT (getutcdate()) FOR [LocationTime]
GO
ALTER TABLE [dbo].[DriverLocations] ADD  DEFAULT ((1)) FOR [IsOnline]
GO
ALTER TABLE [dbo].[DriverLocations] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[DriverRatings] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[DriverRatings] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[DriverReviews] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[DriverReviews] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Drivers] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Drivers] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[FAQs] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[FAQs] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[FAQs] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Feedbacks] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Invoices] ADD  DEFAULT (getdate()) FOR [InvoiceDate]
GO
ALTER TABLE [dbo].[Invoices] ADD  DEFAULT ('Paid') FOR [Status]
GO
ALTER TABLE [dbo].[MatchingRequests] ADD  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[MatchingRequests] ADD  DEFAULT (getutcdate()) FOR [RequestTime]
GO
ALTER TABLE [dbo].[MatchingRequests] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[MatchingRequests] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Messages] ADD  DEFAULT (getdate()) FOR [SentAt]
GO
ALTER TABLE [dbo].[Messages] ADD  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[OtpRequests] ADD  DEFAULT ((0)) FOR [IsUsed]
GO
ALTER TABLE [dbo].[OtpRequests] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[OtpRequests] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Promotions] ADD  DEFAULT ((0)) FOR [UsedCount]
GO
ALTER TABLE [dbo].[Promotions] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Promotions] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Promotions] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[SharedRides] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[SmsLogs] ADD  DEFAULT ((0)) FOR [IsSuccess]
GO
ALTER TABLE [dbo].[SmsLogs] ADD  DEFAULT (getdate()) FOR [SentAt]
GO
ALTER TABLE [dbo].[SmsLogs] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[SmsLogs] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[SupportTickets] ADD  DEFAULT ('Open') FOR [Status]
GO
ALTER TABLE [dbo].[SupportTickets] ADD  DEFAULT ('Medium') FOR [Priority]
GO
ALTER TABLE [dbo].[SupportTickets] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[SupportTickets] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[TripRatings] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[TripRatings] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Trips] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Trips] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UserAddresses] ADD  DEFAULT ((0)) FOR [IsDefault]
GO
ALTER TABLE [dbo].[UserAddresses] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[UserAddresses] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UserCreditCards] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[UserCreditCards] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UserNotificationSettings] ADD  DEFAULT ((1)) FOR [AllowPromotions]
GO
ALTER TABLE [dbo].[UserNotificationSettings] ADD  DEFAULT ((1)) FOR [AllowTripUpdates]
GO
ALTER TABLE [dbo].[UserNotificationSettings] ADD  DEFAULT ((1)) FOR [AllowNews]
GO
ALTER TABLE [dbo].[UserNotificationSettings] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[UserRatings] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[UserRatings] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Wallets] ADD  DEFAULT ((0)) FOR [Balance]
GO
ALTER TABLE [dbo].[Wallets] ADD  DEFAULT ((0)) FOR [TotalIn]
GO
ALTER TABLE [dbo].[Wallets] ADD  DEFAULT ((0)) FOR [TotalOut]
GO
ALTER TABLE [dbo].[Wallets] ADD  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Wallets] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Wallets] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[WalletTransactions] ADD  DEFAULT (getdate()) FOR [TransactionDate]
GO
ALTER TABLE [dbo].[WalletTransactions] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[BankInfo]  WITH CHECK ADD FOREIGN KEY([DriverID])
REFERENCES [dbo].[Drivers] ([DriverID])
GO
ALTER TABLE [dbo].[CreditCards]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[DriverDocuments]  WITH CHECK ADD FOREIGN KEY([DriverID])
REFERENCES [dbo].[Drivers] ([DriverID])
GO
ALTER TABLE [dbo].[DriverLocations]  WITH CHECK ADD  CONSTRAINT [FK_DriverLocations_DriverID] FOREIGN KEY([DriverID])
REFERENCES [dbo].[Drivers] ([DriverID])
GO
ALTER TABLE [dbo].[DriverLocations] CHECK CONSTRAINT [FK_DriverLocations_DriverID]
GO
ALTER TABLE [dbo].[DriverRatings]  WITH CHECK ADD FOREIGN KEY([DriverID])
REFERENCES [dbo].[Drivers] ([DriverID])
GO
ALTER TABLE [dbo].[DriverRatings]  WITH CHECK ADD FOREIGN KEY([TripID])
REFERENCES [dbo].[Trips] ([TripID])
GO
ALTER TABLE [dbo].[DriverRatings]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Feedbacks]  WITH CHECK ADD FOREIGN KEY([DriverID])
REFERENCES [dbo].[Drivers] ([DriverID])
GO
ALTER TABLE [dbo].[Feedbacks]  WITH CHECK ADD FOREIGN KEY([TripID])
REFERENCES [dbo].[Trips] ([TripID])
GO
ALTER TABLE [dbo].[Feedbacks]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD FOREIGN KEY([TripID])
REFERENCES [dbo].[Trips] ([TripID])
GO
ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[MatchingRequests]  WITH CHECK ADD  CONSTRAINT [FK_MatchingRequests_DriverID] FOREIGN KEY([DriverID])
REFERENCES [dbo].[Drivers] ([DriverID])
GO
ALTER TABLE [dbo].[MatchingRequests] CHECK CONSTRAINT [FK_MatchingRequests_DriverID]
GO
ALTER TABLE [dbo].[MatchingRequests]  WITH CHECK ADD  CONSTRAINT [FK_MatchingRequests_PassengerID] FOREIGN KEY([PassengerID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[MatchingRequests] CHECK CONSTRAINT [FK_MatchingRequests_PassengerID]
GO
ALTER TABLE [dbo].[Messages]  WITH CHECK ADD FOREIGN KEY([ReceiverID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Messages]  WITH CHECK ADD FOREIGN KEY([SenderID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD FOREIGN KEY([DriverID])
REFERENCES [dbo].[Drivers] ([DriverID])
GO
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[SharedRides]  WITH CHECK ADD FOREIGN KEY([TripID])
REFERENCES [dbo].[Trips] ([TripID])
GO
ALTER TABLE [dbo].[SupportTickets]  WITH CHECK ADD FOREIGN KEY([AssignedTo])
REFERENCES [dbo].[Admins] ([AdminID])
GO
ALTER TABLE [dbo].[SupportTickets]  WITH CHECK ADD FOREIGN KEY([DriverID])
REFERENCES [dbo].[Drivers] ([DriverID])
GO
ALTER TABLE [dbo].[SupportTickets]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[TripRatings]  WITH CHECK ADD FOREIGN KEY([RatedByUserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[TripRatings]  WITH CHECK ADD FOREIGN KEY([TripID])
REFERENCES [dbo].[Trips] ([TripID])
GO
ALTER TABLE [dbo].[Trips]  WITH CHECK ADD FOREIGN KEY([DriverID])
REFERENCES [dbo].[Drivers] ([DriverID])
GO
ALTER TABLE [dbo].[Trips]  WITH CHECK ADD FOREIGN KEY([PassengerID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[UserAddresses]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[UserRatings]  WITH CHECK ADD FOREIGN KEY([RatedUserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[UserRatings]  WITH CHECK ADD FOREIGN KEY([RatedByDriverID])
REFERENCES [dbo].[Drivers] ([DriverID])
GO
ALTER TABLE [dbo].[UserRatings]  WITH CHECK ADD FOREIGN KEY([TripID])
REFERENCES [dbo].[Trips] ([TripID])
GO
ALTER TABLE [dbo].[Wallets]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[WalletTransactions]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[WalletTransactions]  WITH CHECK ADD FOREIGN KEY([WalletID])
REFERENCES [dbo].[Wallets] ([WalletID])
GO
ALTER TABLE [dbo].[DriverRatings]  WITH CHECK ADD CHECK  (([Rating]>=(1) AND [Rating]<=(5)))
GO
ALTER TABLE [dbo].[TripRatings]  WITH CHECK ADD CHECK  (([Rating]>=(1) AND [Rating]<=(5)))
GO
ALTER TABLE [dbo].[UserCreditCards]  WITH CHECK ADD CHECK  (([ExpiryMonth]>=(1) AND [ExpiryMonth]<=(12)))
GO
ALTER TABLE [dbo].[UserRatings]  WITH CHECK ADD CHECK  (([Rating]>=(1) AND [Rating]<=(5)))
GO
/****** Object:  StoredProcedure [dbo].[CleanupExpiredRequests]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ===============================================
-- MAINTENANCE PROCEDURES
-- ===============================================

-- Procedure to clean up expired requests
CREATE PROCEDURE [dbo].[CleanupExpiredRequests]
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
/****** Object:  StoredProcedure [dbo].[FindNearbyDrivers]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Stored procedure to find nearby drivers
CREATE PROCEDURE [dbo].[FindNearbyDrivers]
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
/****** Object:  StoredProcedure [dbo].[GetMatchingStatistics]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Stored procedure to get matching statistics
CREATE PROCEDURE [dbo].[GetMatchingStatistics]
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
/****** Object:  StoredProcedure [dbo].[UpdateOfflineDrivers]    Script Date: 26.11.2025 16:23:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Procedure to update offline drivers
CREATE PROCEDURE [dbo].[UpdateOfflineDrivers]
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
USE [master]
GO
ALTER DATABASE [TaxiDb] SET  READ_WRITE 
GO
