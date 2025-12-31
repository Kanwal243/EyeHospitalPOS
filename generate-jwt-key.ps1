# JWT Secret Key Generator Script
# Usage: .\generate-jwt-key.ps1 [key-size-in-bytes]
# Default: 64 bytes (512 bits)

param(
    [int]$Size = 64
)

Write-Host "=== JWT Secret Key Generator ===" -ForegroundColor Cyan
Write-Host ""

# Validate size
if ($Size -lt 32) {
    Write-Host "⚠️  Warning: Key size is less than 32 bytes (minimum recommended)" -ForegroundColor Yellow
    Write-Host "   Generating anyway, but consider using at least 32 bytes for security" -ForegroundColor Yellow
    Write-Host ""
}

# Generate key
Write-Host "Generating $Size-byte ($($Size * 8)-bit) secure key..." -ForegroundColor Gray
$bytes = New-Object byte[] $Size
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$rng.Dispose()
$key = [Convert]::ToBase64String($bytes)

# Display results
Write-Host ""
Write-Host "Your JWT Secret Key:" -ForegroundColor Green
Write-Host $key -ForegroundColor Yellow
Write-Host ""

# Key analysis
$byteLength = [System.Text.Encoding]::UTF8.GetByteCount($key)
Write-Host "Key Details:" -ForegroundColor Cyan
Write-Host "  Length: $($key.Length) characters"
Write-Host "  Byte Length: $byteLength bytes"
Write-Host "  Bit Length: $($byteLength * 8) bits"

if ($byteLength -ge 64) {
    Write-Host "  Security: ✅ EXCELLENT (>= 64 bytes)" -ForegroundColor Green
} elseif ($byteLength -ge 32) {
    Write-Host "  Security: ✅ SECURE (>= 32 bytes)" -ForegroundColor Green
} else {
    Write-Host "  Security: ❌ INSECURE (< 32 bytes)" -ForegroundColor Red
    Write-Host "  Recommendation: Use at least 32 bytes" -ForegroundColor Yellow
}

Write-Host ""

# Usage instructions
Write-Host "Usage Options:" -ForegroundColor Cyan
Write-Host ""
Write-Host "Option 1: Update appsettings.json"
Write-Host '  Replace "Jwt:SecretKey" value with the key above'
Write-Host ""
Write-Host "Option 2: Use User Secrets (Recommended for Development)"
Write-Host "  dotnet user-secrets set 'Jwt:SecretKey' '$key'"
Write-Host ""
Write-Host "Option 3: Copy to Clipboard"
$copy = Read-Host "Copy key to clipboard? (Y/N)"
if ($copy -eq "Y" -or $copy -eq "y") {
    $key | Set-Clipboard
    Write-Host "✅ Key copied to clipboard!" -ForegroundColor Green
}

Write-Host ""
Write-Host "⚠️  Important Reminders:" -ForegroundColor Yellow
Write-Host "  - Changing the key will invalidate all existing tokens"
Write-Host "  - Users will need to log in again after key change"
Write-Host "  - Never commit keys to source control"
Write-Host "  - Use User Secrets for development"
Write-Host "  - Use Environment Variables or Secret Managers for production"
Write-Host ""

