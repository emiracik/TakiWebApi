# API'yi canlıya alır (apivelox.truvaburo.com).
# İç ağa (VPN) bağlıyken çalıştırın:
#   powershell -ExecutionPolicy Bypass -File deploy.ps1
#
# Adımlar: publish -> sunucudaki mevcut sürümü yedekle -> app_offline.htm ile
# uygulamayı durdur -> dosyaları kopyala -> başlat -> sağlık kontrolü.
# Geri almak için: backups\<tarih> klasörünü aynı şekilde sunucuya kopyalayın.

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$target = '\\10.200.0.3\e$\Websites\apivelox.truvaburo.com'

if (-not (Test-Path $target)) {
    throw "Sunucu klasörüne erişilemiyor: $target (VPN bağlı mı?)"
}

dotnet publish "$root\TakiWebApi\TakiWebApi.csproj" -c Release -o "$root\publish" --nologo
if ($LASTEXITCODE -ne 0) { throw 'Publish başarısız.' }

if (-not (Test-Path "$root\publish\appsettings.Production.json")) {
    throw 'appsettings.Production.json yok; canlı ayarlar olmadan deploy edilmez.'
}

$backup = Join-Path $root ('backups\' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
robocopy $target $backup /E /XD logs App_Data /NFL /NDL /NJH /NJS | Out-Null
if ($LASTEXITCODE -ge 8) { throw "Yedek alınamadı (robocopy $LASTEXITCODE)." }
Write-Host "Yedek: $backup"

Set-Content "$target\app_offline.htm" '<html><body>Bakim calismasi, birkac saniye icinde donuyoruz.</body></html>'
Start-Sleep -Seconds 3

robocopy "$root\publish" $target /E /NFL /NDL /NJH /NJS
if ($LASTEXITCODE -ge 8) {
    throw "Kopyalama hatası (robocopy $LASTEXITCODE). app_offline.htm sunucuda duruyor; yedek: $backup"
}

Remove-Item "$target\app_offline.htm"
Start-Sleep -Seconds 5

$r = Invoke-WebRequest 'http://apivelox.truvaburo.com/health/database' -UseBasicParsing -TimeoutSec 30
Write-Host "Sağlık kontrolü: $($r.StatusCode) $($r.Content)"
