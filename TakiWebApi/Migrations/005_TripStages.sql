-- 005: Yolculuk aşamaları. Sürücü "Kapıdayım" (ArrivedAt) ve "Yolcu bindi"
-- (PickedUpAt) dediğinde zaman yazılır; yolcu ekranı ve bekleme süresi
-- bunlara göre gösterilir. Tekrar çalıştırmak güvenli.

IF COL_LENGTH(N'dbo.Trips', N'ArrivedAt') IS NULL
    ALTER TABLE dbo.Trips ADD ArrivedAt datetime2 NULL;
IF COL_LENGTH(N'dbo.Trips', N'PickedUpAt') IS NULL
    ALTER TABLE dbo.Trips ADD PickedUpAt datetime2 NULL;
GO
