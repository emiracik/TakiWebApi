-- 001: Refresh token'lar bellek yerine veritabanında.
-- Yayından ÖNCE VeloxTaxiDb üzerinde bir kez çalıştırılmalı (tekrar
-- çalıştırmak güvenli). Token'ın kendisi değil SHA-256 hash'i saklanır.

IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshTokens (
        RefreshTokenID  int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
        TokenHash       char(64)    NOT NULL,
        SubjectId       int         NOT NULL,   -- UserID veya DriverID
        SubjectType     varchar(10) NOT NULL,   -- 'user' | 'driver'
        ExpiresAt       datetime2   NOT NULL,
        CreatedAt       datetime2   NOT NULL CONSTRAINT DF_RefreshTokens_CreatedAt DEFAULT SYSUTCDATETIME(),
        RevokedAt       datetime2   NULL,
        ReplacedByHash  char(64)    NULL
    );

    CREATE UNIQUE INDEX UX_RefreshTokens_TokenHash ON dbo.RefreshTokens (TokenHash);
    CREATE INDEX IX_RefreshTokens_Subject ON dbo.RefreshTokens (SubjectType, SubjectId);
END
GO
