# PowerShell script to remove virtual keywords from navigation properties

$modelFiles = Get-ChildItem "c:\Users\emira\Documents\GitHub\TakiWebApi\TakiWebApi\Models\*.cs"

foreach ($file in $modelFiles) {
    $content = Get-Content $file.FullName -Raw
    $content = $content -replace "public virtual ", "public "
    $content = $content -replace "// Navigation properties", "// Navigation properties - removed virtual keyword since we're using ADO.NET"
    Set-Content $file.FullName $content
}

Write-Host "Removed virtual keywords from all model files"
