-- Create TaxiDb database and Users table
USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TaxiDb')
BEGIN
    CREATE DATABASE TaxiDb;
END
GO

USE TaxiDb;
GO

-- Create Users table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        UserID int IDENTITY(1,1) PRIMARY KEY,
        FullName nvarchar(200) NOT NULL,
        PhoneNumber nvarchar(20) NOT NULL,
        Email nvarchar(200) NULL,
        IsActive bit NOT NULL DEFAULT 1,
        CreatedBy int NULL,
        CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedBy int NULL,
        UpdatedDate datetime2 NULL,
        DeletedBy int NULL,
        DeletedDate datetime2 NULL,
        IsDeleted bit NOT NULL DEFAULT 0
    );
END
GO

-- Insert some sample data
IF NOT EXISTS (SELECT * FROM Users)
BEGIN
    INSERT INTO Users (FullName, PhoneNumber, Email, IsActive, CreatedDate)
    VALUES 
        ('John Doe', '1234567890', 'john.doe@example.com', 1, GETUTCDATE()),
        ('Jane Smith', '0987654321', 'jane.smith@example.com', 1, GETUTCDATE()),
        ('Bob Johnson', '5555555555', 'bob.johnson@example.com', 1, GETUTCDATE()),
        ('Alice Brown', '7777777777', 'alice.brown@example.com', 0, GETUTCDATE()),
        ('Charlie Wilson', '9999999999', NULL, 1, GETUTCDATE());
END
GO

SELECT 'Database and Users table created successfully!' AS Message;
GO
