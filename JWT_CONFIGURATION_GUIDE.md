# JWT Configuration Troubleshooting Guide

## Current Configuration Status

Your JWT configuration in `appsettings.json` appears to be correctly formatted. Let's verify it's working properly.

## Configuration Location

**File**: `appsettings.json`

```json
{
  "Jwt": {
    "SecretKey": "P03dVL0pyQgmFsUMy1pSNvm2+Y9ELq93ZA3W/474TgMz+6yaTIYh2SOkp3covov3njrm6MIc8u7EkqBEnKKTYg==",
    "Issuer": "EyeHospitalPOS",
    "Audience": "EyeHospitalPOS-API",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

## What Was Fixed

### Issue 1: Missing Null Checks in Startup.cs ✅ FIXED
- **Problem**: The JWT configuration in `Startup.cs` didn't validate if values were loaded from `appsettings.json`
- **Fix**: Added null/empty checks with clear error messages
- **Impact**: Application will now fail fast at startup with clear error messages if JWT config is missing

### Issue 2: Security Validation ✅ ADDED
- **Problem**: No validation for SecretKey length
- **Fix**: Added check to ensure SecretKey is at least 32 bytes (256 bits) for HS256 algorithm
- **Impact**: Better security - prevents using weak keys

## Verification Steps

### 1. Check Configuration is Loaded

Run the application and check for startup errors. If JWT configuration is missing, you'll see a clear error message.

### 2. Test JWT Token Generation

1. **Login via API or UI**
   - Use Swagger UI: `/swagger`
   - Or login through the application UI
   - Check that a JWT token is generated

2. **Inspect the Token**
   - Copy the token from the response
   - Decode at https://jwt.io to verify:
     - `iss` (issuer) = "EyeHospitalPOS"
     - `aud` (audience) = "EyeHospitalPOS-API"
     - Token is properly signed

### 3. Test Token Validation

Try accessing a protected API endpoint:
```
GET /api/products
Authorization: Bearer <your-token>
```

Should return 200 OK if token is valid.

### 4. Check Application Logs

Look for any JWT-related errors in:
- Browser console (F12)
- Application logs
- Server output

## Common Issues & Solutions

### Issue: "JWT SecretKey is not configured"

**Symptoms:**
- Application fails to start
- Error message: "JWT SecretKey is not configured in appsettings.json"

**Solution:**
1. Verify `appsettings.json` exists in the project root
2. Check that the `Jwt:SecretKey` section exists
3. Ensure the JSON syntax is correct (no trailing commas, proper quotes)

### Issue: "JWT SecretKey must be at least 32 bytes"

**Symptoms:**
- Application fails to start
- Error message about key length

**Solution:**
1. Generate a new, secure key (see below)
2. Update `appsettings.json` with the new key
3. Restart the application

### Issue: Token Validation Fails

**Symptoms:**
- 401 Unauthorized errors
- "Token validation failed" messages

**Possible Causes:**
1. **Issuer/Audience Mismatch**
   - Check that token issuer matches `Jwt:Issuer` in config
   - Check that token audience matches `Jwt:Audience` in config

2. **Token Expired**
   - Check token expiration time
   - Default is 60 minutes (`AccessTokenExpirationMinutes`)

3. **Secret Key Mismatch**
   - Ensure the same SecretKey is used for generation and validation
   - Check for typos in `appsettings.json`

4. **Clock Skew**
   - Already set to `TimeSpan.Zero` in your config
   - Verify server time is correct

## Generating a Secure JWT Secret Key

If you need to generate a new secure key, use one of these methods:

### Method 1: Using PowerShell (Windows)
```powershell
$bytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
[Convert]::ToBase64String($bytes)
```

### Method 2: Using C# Code
```csharp
using System.Security.Cryptography;

var bytes = new byte[64]; // 512 bits
using var rng = RandomNumberGenerator.Create();
rng.GetBytes(bytes);
var key = Convert.ToBase64String(bytes);
Console.WriteLine(key);
```

### Method 3: Online Generator
- Visit: https://generate-secret.vercel.app/64
- Copy the generated base64 string
- Use it as your `SecretKey`

**Important**: Store the key securely! Never commit it to public repositories.

## Configuration Validation Checklist

- [ ] `Jwt:SecretKey` exists and is not empty
- [ ] `Jwt:SecretKey` is at least 32 bytes (64 Base64 characters = 48 bytes, your current key is 88 chars = 66 bytes ✅)
- [ ] `Jwt:Issuer` is set (current: "EyeHospitalPOS" ✅)
- [ ] `Jwt:Audience` is set (current: "EyeHospitalPOS-API" ✅)
- [ ] `Jwt:AccessTokenExpirationMinutes` is a valid number (current: 60 ✅)
- [ ] `Jwt:RefreshTokenExpirationDays` is a valid number (current: 7 ✅)
- [ ] JSON syntax is valid (no errors)
- [ ] Application starts without JWT configuration errors
- [ ] Tokens are generated successfully
- [ ] Tokens validate correctly

## Testing JWT Configuration

### Quick Test Script

Add this to test JWT configuration at startup (optional):

```csharp
// Add to Startup.cs ConfigureServices method (temporary for testing)
var secretKey = Configuration["Jwt:SecretKey"];
var issuer = Configuration["Jwt:Issuer"];
var audience = Configuration["Jwt:Audience"];

Console.WriteLine($"JWT Config Check:");
Console.WriteLine($"  SecretKey: {(string.IsNullOrEmpty(secretKey) ? "❌ MISSING" : $"✅ Found ({secretKey.Length} chars)")}");
Console.WriteLine($"  Issuer: {(string.IsNullOrEmpty(issuer) ? "❌ MISSING" : $"✅ {issuer}")}");
Console.WriteLine($"  Audience: {(string.IsNullOrEmpty(audience) ? "❌ MISSING" : $"✅ {audience}")}");
```

## Current Configuration Analysis

Your current configuration:

✅ **SecretKey**: 88 characters (66 bytes) - **SECURE** (exceeds 32-byte minimum)
✅ **Issuer**: "EyeHospitalPOS" - **VALID**
✅ **Audience**: "EyeHospitalPOS-API" - **VALID**
✅ **AccessTokenExpirationMinutes**: 60 - **REASONABLE**
✅ **RefreshTokenExpirationDays**: 7 - **REASONABLE**

## Next Steps

1. **Verify the fixes work:**
   - Restart your application
   - Check for any startup errors
   - Test login functionality

2. **Monitor for issues:**
   - Watch application logs
   - Test API endpoints with tokens
   - Verify token expiration works correctly

3. **Production considerations:**
   - Use User Secrets or Environment Variables for production
   - Never commit real secrets to source control
   - Use different keys for dev/staging/production

## User Secrets (Recommended for Development)

For better security, store secrets outside of `appsettings.json`:

```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key-here"
dotnet user-secrets set "Jwt:Issuer" "EyeHospitalPOS"
dotnet user-secrets set "Jwt:Audience" "EyeHospitalPOS-API"
```

Then remove the `Jwt` section from `appsettings.json` or use a placeholder.

## Summary

✅ **Your JWT configuration looks correct!**
✅ **Added validation to catch configuration errors early**
✅ **Added security checks for key length**

The configuration should work properly. If you're experiencing issues, check:
1. Application startup logs for errors
2. Token generation/validation in your auth flow
3. API endpoint authorization

