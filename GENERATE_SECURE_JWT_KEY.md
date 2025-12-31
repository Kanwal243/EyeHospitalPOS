# How to Generate Secure JWT Keys

This guide provides multiple methods to generate secure JWT secret keys for your application.

## Requirements for JWT Secret Keys

- **Minimum Length**: 32 bytes (256 bits) for HS256 algorithm
- **Recommended**: 64 bytes (512 bits) or more for better security
- **Format**: Base64 encoded string (recommended) or plain text
- **Character Set**: Should use cryptographically secure random generation

---

## Method 1: PowerShell (Windows) - Recommended ✅

### Quick One-Liner (64 bytes = 512 bits) - Compatible with all PowerShell versions
```powershell
$bytes = New-Object byte[] 64; $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create(); $rng.GetBytes($bytes); $rng.Dispose(); [Convert]::ToBase64String($bytes)
```

### Step-by-Step Version (Compatible)
```powershell
# Generate 64 random bytes (512 bits)
$bytes = New-Object byte[] 64
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$rng.Dispose()

# Convert to Base64 string
$key = [Convert]::ToBase64String($bytes)

# Display the key
Write-Host "Your JWT Secret Key:"
Write-Host $key
Write-Host ""
Write-Host "Length: $($key.Length) characters"
Write-Host "Byte Length: $([System.Text.Encoding]::UTF8.GetByteCount($key)) bytes"
```

### Copy to Clipboard (PowerShell)
```powershell
$bytes = New-Object byte[] 64
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$rng.Dispose()
$key = [Convert]::ToBase64String($bytes)
$key | Set-Clipboard
Write-Host "Key generated and copied to clipboard: $key"
```

---

## Method 2: C# Console Application

Create a simple C# program to generate keys:

```csharp
using System;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        // Generate 64 bytes (512 bits)
        var bytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        
        var key = Convert.ToBase64String(bytes);
        
        Console.WriteLine("JWT Secret Key:");
        Console.WriteLine(key);
        Console.WriteLine($"Length: {key.Length} characters");
        Console.WriteLine($"Byte Length: {bytes.Length} bytes");
    }
}
```

### Run in .NET Interactive (C# Script)
```csharp
#r "System.Security.Cryptography"

var bytes = new byte[64];
using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
rng.GetBytes(bytes);
var key = Convert.ToBase64String(bytes);
Console.WriteLine(key);
```

---

## Method 3: .NET CLI (dotnet-script)

If you have `dotnet-script` installed:

```bash
dotnet script -e "var bytes = new byte[64]; using var rng = System.Security.Cryptography.RandomNumberGenerator.Create(); rng.GetBytes(bytes); Console.WriteLine(Convert.ToBase64String(bytes));"
```

---

## Method 4: Online Generators (Use with Caution ⚠️)

### Option A: Generate Secret (Recommended)
- URL: https://generate-secret.vercel.app/64
- Generates 64-byte keys
- Runs locally in your browser (more secure)

### Option B: RandomKeygen
- URL: https://randomkeygen.com/
- Select "CodeIgniter Encryption Keys"
- Use the longest key (256-bit or 512-bit)

### ⚠️ Security Warning
- Only use online generators you trust
- Never use online generators for production keys
- Prefer local generation for production systems

---

## Method 5: OpenSSL (Linux/Mac/Windows)

### Linux/Mac
```bash
openssl rand -base64 64
```

### Windows (if OpenSSL is installed)
```bash
openssl rand -base64 64
```

### Generate 32-byte key (minimum)
```bash
openssl rand -base64 32
```

### Generate 64-byte key (recommended)
```bash
openssl rand -base64 64
```

---

## Method 6: Node.js

If you have Node.js installed:

```javascript
const crypto = require('crypto');
const key = crypto.randomBytes(64).toString('base64');
console.log(key);
```

Run with:
```bash
node -e "const crypto = require('crypto'); console.log(crypto.randomBytes(64).toString('base64'));"
```

---

## Method 7: Python

If you have Python installed:

```python
import secrets
import base64

# Generate 64 random bytes
key_bytes = secrets.token_bytes(64)
# Convert to Base64
key = base64.b64encode(key_bytes).decode('utf-8')
print(key)
```

One-liner:
```bash
python -c "import secrets, base64; print(base64.b64encode(secrets.token_bytes(64)).decode('utf-8'))"
```

---

## Quick Reference: Key Sizes

| Bytes | Bits | Base64 Length | Security Level | Use Case |
|-------|------|---------------|----------------|----------|
| 32    | 256  | ~44 chars     | Minimum        | Development |
| 64    | 512  | ~88 chars     | **Recommended** | Production |
| 128   | 1024 | ~172 chars    | High           | High Security |

---

## How to Update Your appsettings.json

### Step 1: Generate a New Key
Use one of the methods above to generate a secure key.

### Step 2: Update appsettings.json
```json
{
  "Jwt": {
    "SecretKey": "YOUR_NEW_GENERATED_KEY_HERE",
    "Issuer": "EyeHospitalPOS",
    "Audience": "EyeHospitalPOS-API",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Step 3: Restart Application
After updating the key, restart your application.

### ⚠️ Important Notes:
- **All existing tokens will become invalid** when you change the key
- Users will need to log in again
- Use the same key across all instances of your application
- Never commit keys to source control (use User Secrets or Environment Variables)

---

## Using User Secrets (Recommended for Development)

Instead of storing keys in `appsettings.json`, use User Secrets:

### Initialize User Secrets
```bash
dotnet user-secrets init
```

### Set JWT Secret Key
```bash
dotnet user-secrets set "Jwt:SecretKey" "your-generated-key-here"
dotnet user-secrets set "Jwt:Issuer" "EyeHospitalPOS"
dotnet user-secrets set "Jwt:Audience" "EyeHospitalPOS-API"
```

### List User Secrets
```bash
dotnet user-secrets list
```

### Remove from appsettings.json
Remove or comment out the `Jwt` section in `appsettings.json`:
```json
{
  "Jwt": {
    // "SecretKey": "REMOVED - Using User Secrets",
    // "Issuer": "EyeHospitalPOS",
    // ...
  }
}
```

---

## Using Environment Variables (Recommended for Production)

### Windows (PowerShell)
```powershell
$env:Jwt__SecretKey = "your-generated-key-here"
$env:Jwt__Issuer = "EyeHospitalPOS"
$env:Jwt__Audience = "EyeHospitalPOS-API"
```

### Linux/Mac (Bash)
```bash
export Jwt__SecretKey="your-generated-key-here"
export Jwt__Issuer="EyeHospitalPOS"
export Jwt__Audience="EyeHospitalPOS-API"
```

### Docker
```yaml
environment:
  - Jwt__SecretKey=your-generated-key-here
  - Jwt__Issuer=EyeHospitalPOS
  - Jwt__Audience=EyeHospitalPOS-API
```

### Azure App Service
Set in Configuration → Application Settings

### AWS
Use AWS Secrets Manager or Parameter Store

---

## Security Best Practices

1. **Never Commit Keys to Git**
   - Add `appsettings.json` to `.gitignore` (if it contains secrets)
   - Use `appsettings.Development.json` for local overrides
   - Use User Secrets for development
   - Use Environment Variables or Secret Managers for production

2. **Key Rotation**
   - Generate new keys periodically
   - Rotate keys in production during maintenance windows
   - Plan for users to re-authenticate after rotation

3. **Key Storage**
   - Development: User Secrets
   - Staging: Environment Variables
   - Production: Secret Management Services (Azure Key Vault, AWS Secrets Manager, etc.)

4. **Key Length**
   - Minimum: 32 bytes (256 bits)
   - Recommended: 64 bytes (512 bits)
   - High Security: 128 bytes (1024 bits)

5. **Key Generation**
   - Always use cryptographically secure random generators
   - Never use predictable patterns or timestamps
   - Never reuse keys across applications/environments

---

## Verification Script

Use this PowerShell script to verify your key:

```powershell
$key = Read-Host "Enter your JWT Secret Key"
$byteLength = [System.Text.Encoding]::UTF8.GetByteCount($key)

Write-Host "Key Analysis:"
Write-Host "  Length: $($key.Length) characters"
Write-Host "  Byte Length: $byteLength bytes"

if ($byteLength -ge 32) {
    Write-Host "  Status: ✅ SECURE (>= 32 bytes)" -ForegroundColor Green
} else {
    Write-Host "  Status: ❌ INSECURE (< 32 bytes)" -ForegroundColor Red
    Write-Host "  Recommendation: Generate a new key with at least 32 bytes"
}
```

---

## Example: Complete Key Generation Workflow

### For Development:
```powershell
# 1. Generate key
$bytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
$key = [Convert]::ToBase64String($bytes)

# 2. Set as User Secret
dotnet user-secrets set "Jwt:SecretKey" $key

# 3. Verify
dotnet user-secrets list
```

### For Production:
```powershell
# 1. Generate key
$bytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
$key = [Convert]::ToBase64String($bytes)

# 2. Store securely (example: Azure Key Vault, AWS Secrets Manager, etc.)
# 3. Set as Environment Variable in hosting platform
# 4. Never commit to source control
```

---

## Quick Copy-Paste Scripts

### PowerShell - Generate and Display (Compatible)
```powershell
$bytes = New-Object byte[] 64; $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create(); $rng.GetBytes($bytes); $rng.Dispose(); $key = [Convert]::ToBase64String($bytes); Write-Host "JWT Secret Key:" -ForegroundColor Green; Write-Host $key -ForegroundColor Yellow; Write-Host ""; Write-Host "Length: $($key.Length) characters, $([System.Text.Encoding]::UTF8.GetByteCount($key)) bytes"
```

### PowerShell - Generate and Copy to Clipboard (Compatible)
```powershell
$bytes = New-Object byte[] 64; $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create(); $rng.GetBytes($bytes); $rng.Dispose(); $key = [Convert]::ToBase64String($bytes); $key | Set-Clipboard; Write-Host "✅ Key generated and copied to clipboard!" -ForegroundColor Green; Write-Host $key -ForegroundColor Yellow
```

---

## Summary

**Easiest Method (Windows - Compatible with all PowerShell versions):**
```powershell
$bytes = New-Object byte[] 64; $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create(); $rng.GetBytes($bytes); $rng.Dispose(); [Convert]::ToBase64String($bytes)
```

**Or use the provided script:**
```powershell
.\generate-jwt-key.ps1
```

**Recommended Key Size:** 64 bytes (512 bits)
**Your Current Key:** 88 bytes ✅ (Already secure!)

Generate a new key whenever you need to rotate credentials or set up a new environment.

