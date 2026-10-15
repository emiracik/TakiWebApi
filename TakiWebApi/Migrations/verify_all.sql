-- 001-005 migration'larının VeloxTaxiDb üzerinde başarıyla uygulandığını
-- doğrular. Çalıştırmadan önce sol üstteki veritabanı dropdown'ından
-- VeloxTaxiDb'nin seçili olduğundan emin ol (ya da en başa `USE VeloxTaxiDb;`
-- ekle). Sadece okur, hiçbir şeyi değiştirmez.

SELECT * FROM (
    -- 001_RefreshTokens
    SELECT '001' AS Migration, 'Tablo: dbo.RefreshTokens' AS Kontrol,
        CASE WHEN OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END AS Durum
    UNION ALL
    SELECT '001', 'Index: UX_RefreshTokens_TokenHash',
        CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_RefreshTokens_TokenHash') THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '001', 'Index: IX_RefreshTokens_Subject',
        CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RefreshTokens_Subject') THEN 'OK' ELSE 'EKSIK' END

    -- 002_DeviceTokens
    UNION ALL
    SELECT '002', 'Tablo: dbo.DeviceTokens',
        CASE WHEN OBJECT_ID(N'dbo.DeviceTokens', N'U') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '002', 'Index: UX_DeviceTokens_Token',
        CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_DeviceTokens_Token') THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '002', 'Index: IX_DeviceTokens_Subject',
        CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DeviceTokens_Subject') THEN 'OK' ELSE 'EKSIK' END

    -- 003_NewFeatures
    UNION ALL
    SELECT '003', 'Kolon: MatchingRequests.CancelReason',
        CASE WHEN COL_LENGTH(N'dbo.MatchingRequests', N'CancelReason') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '003', 'Kolon: MatchingRequests.CancellationFee',
        CASE WHEN COL_LENGTH(N'dbo.MatchingRequests', N'CancellationFee') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '003', 'Kolon: Trips.TipAmount',
        CASE WHEN COL_LENGTH(N'dbo.Trips', N'TipAmount') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '003', 'Tablo: dbo.Promotions',
        CASE WHEN OBJECT_ID(N'dbo.Promotions', N'U') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '003', 'Kolon: Promotions.Code',
        CASE WHEN COL_LENGTH(N'dbo.Promotions', N'Code') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '003', 'Index: UX_Promotions_Code',
        CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Promotions_Code') THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '003', 'Tablo: dbo.PromotionUsages',
        CASE WHEN OBJECT_ID(N'dbo.PromotionUsages', N'U') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '003', 'Index: UX_PromotionUsages_User',
        CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_PromotionUsages_User') THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '003', 'Tablo: dbo.ScheduledRides',
        CASE WHEN OBJECT_ID(N'dbo.ScheduledRides', N'U') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '003', 'Index: IX_ScheduledRides_Due',
        CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ScheduledRides_Due') THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '003', 'Index: IX_ScheduledRides_User',
        CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ScheduledRides_User') THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '003', 'Kolon: DriverDocuments.Status',
        CASE WHEN COL_LENGTH(N'dbo.DriverDocuments', N'Status') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '003', 'Kolon: DriverDocuments.ReviewNote',
        CASE WHEN COL_LENGTH(N'dbo.DriverDocuments', N'ReviewNote') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END

    -- 004_DriverApproval
    UNION ALL
    SELECT '004', 'Kolon: Drivers.ApprovalStatus',
        CASE WHEN COL_LENGTH(N'dbo.Drivers', N'ApprovalStatus') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '004', 'Kolon: Drivers.ApprovalNote',
        CASE WHEN COL_LENGTH(N'dbo.Drivers', N'ApprovalNote') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END

    -- 005_TripStages
    UNION ALL
    SELECT '005', 'Kolon: Trips.ArrivedAt',
        CASE WHEN COL_LENGTH(N'dbo.Trips', N'ArrivedAt') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END
    UNION ALL
    SELECT '005', 'Kolon: Trips.PickedUpAt',
        CASE WHEN COL_LENGTH(N'dbo.Trips', N'PickedUpAt') IS NOT NULL THEN 'OK' ELSE 'EKSIK' END
) AS Sonuclar
ORDER BY Migration, Kontrol;

-- Özet: hiç EKSIK satır yoksa tüm migration'lar tam uygulanmış demektir.
SELECT
    CASE WHEN COUNT(*) = 0 THEN N'✅ Tüm migration''lar eksiksiz uygulanmış.'
         ELSE CONCAT(N'❌ ', COUNT(*), N' kontrol eksik — yukarıdaki tabloya bak.')
    END AS Ozet
FROM (
    SELECT 1 AS Eksik WHERE OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
    UNION ALL SELECT 1 WHERE NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_RefreshTokens_TokenHash')
    UNION ALL SELECT 1 WHERE NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RefreshTokens_Subject')
    UNION ALL SELECT 1 WHERE OBJECT_ID(N'dbo.DeviceTokens', N'U') IS NULL
    UNION ALL SELECT 1 WHERE NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_DeviceTokens_Token')
    UNION ALL SELECT 1 WHERE NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DeviceTokens_Subject')
    UNION ALL SELECT 1 WHERE COL_LENGTH(N'dbo.MatchingRequests', N'CancelReason') IS NULL
    UNION ALL SELECT 1 WHERE COL_LENGTH(N'dbo.MatchingRequests', N'CancellationFee') IS NULL
    UNION ALL SELECT 1 WHERE COL_LENGTH(N'dbo.Trips', N'TipAmount') IS NULL
    UNION ALL SELECT 1 WHERE OBJECT_ID(N'dbo.Promotions', N'U') IS NULL
    UNION ALL SELECT 1 WHERE COL_LENGTH(N'dbo.Promotions', N'Code') IS NULL
    UNION ALL SELECT 1 WHERE NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Promotions_Code')
    UNION ALL SELECT 1 WHERE OBJECT_ID(N'dbo.PromotionUsages', N'U') IS NULL
    UNION ALL SELECT 1 WHERE NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_PromotionUsages_User')
    UNION ALL SELECT 1 WHERE OBJECT_ID(N'dbo.ScheduledRides', N'U') IS NULL
    UNION ALL SELECT 1 WHERE NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ScheduledRides_Due')
    UNION ALL SELECT 1 WHERE NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ScheduledRides_User')
    UNION ALL SELECT 1 WHERE COL_LENGTH(N'dbo.DriverDocuments', N'Status') IS NULL
    UNION ALL SELECT 1 WHERE COL_LENGTH(N'dbo.DriverDocuments', N'ReviewNote') IS NULL
    UNION ALL SELECT 1 WHERE COL_LENGTH(N'dbo.Drivers', N'ApprovalStatus') IS NULL
    UNION ALL SELECT 1 WHERE COL_LENGTH(N'dbo.Drivers', N'ApprovalNote') IS NULL
    UNION ALL SELECT 1 WHERE COL_LENGTH(N'dbo.Trips', N'ArrivedAt') IS NULL
    UNION ALL SELECT 1 WHERE COL_LENGTH(N'dbo.Trips', N'PickedUpAt') IS NULL
) AS Eksikler;
