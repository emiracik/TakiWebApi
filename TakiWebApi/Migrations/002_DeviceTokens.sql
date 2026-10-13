-- 002: Anlık bildirim (FCM) için cihaz token'ları.
-- Yayından ÖNCE VeloxTaxiDb üzerinde bir kez çalıştırılmalı (tekrar
-- çalıştırmak güvenli).

IF OBJECT_ID(N'dbo.DeviceTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DeviceTokens (
        DeviceTokenID  int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DeviceTokens PRIMARY KEY,
        Token          nvarchar(512) NOT NULL,
        SubjectId      int           NOT NULL,   -- UserID veya DriverID
        SubjectType    varchar(10)   NOT NULL,   -- 'user' | 'driver'
        Platform       varchar(10)   NULL,       -- 'android' | 'ios'
        UpdatedAt      datetime2     NOT NULL CONSTRAINT DF_DeviceTokens_UpdatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE UNIQUE INDEX UX_DeviceTokens_Token ON dbo.DeviceTokens (Token);
    CREATE INDEX IX_DeviceTokens_Subject ON dbo.DeviceTokens (SubjectType, SubjectId);
END
GO
