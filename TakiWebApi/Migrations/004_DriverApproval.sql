-- 004: Sürücü onay süreci. Yeni kayıt olan sürücü belgeleri incelenip
-- onaylanana kadar müsait olamaz / talep kabul edemez.
-- Mevcut sürücüler 'Approved' kabul edilir. Tekrar çalıştırmak güvenli.

IF COL_LENGTH(N'dbo.Drivers', N'ApprovalStatus') IS NULL
BEGIN
    ALTER TABLE dbo.Drivers ADD ApprovalStatus varchar(20) NOT NULL
        CONSTRAINT DF_Drivers_ApprovalStatus DEFAULT 'Approved'; -- Pending | Approved | Rejected
    ALTER TABLE dbo.Drivers ADD ApprovalNote nvarchar(500) NULL;
END
GO
