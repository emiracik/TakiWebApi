-- ===============================================
-- Add PasswordHash columns to existing tables
-- ===============================================

-- Add PasswordHash column to Users table if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'PasswordHash')
BEGIN
    ALTER TABLE Users ADD PasswordHash nvarchar(255) NULL;
    PRINT 'PasswordHash column added to Users table';
END
ELSE
BEGIN
    PRINT 'PasswordHash column already exists in Users table';
END

-- Add PasswordHash column to Drivers table if it doesn't exist  
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Drivers' AND COLUMN_NAME = 'PasswordHash')
BEGIN
    ALTER TABLE Drivers ADD PasswordHash nvarchar(255) NULL;
    PRINT 'PasswordHash column added to Drivers table';
END
ELSE
BEGIN
    PRINT 'PasswordHash column already exists in Drivers table';
END

-- Show table structures
SELECT 'Users Table Structure' as TableInfo;
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;

SELECT 'Drivers Table Structure' as TableInfo;  
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Drivers'
ORDER BY ORDINAL_POSITION;
