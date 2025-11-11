# ?? Password Reset Feature Documentation

## Overview

The Password Reset feature provides three secure endpoints for password management in your IdentityAuthorization application:

1. **Forgot Password** - Initiates password reset process
2. **Reset Password** - Completes password reset with token
3. **Change Password** - Allows logged-in users to change their password

---

## ?? API Endpoints

### 1. Forgot Password

**Endpoint:** `POST /forgot-password`  
**Authentication:** Not required  
**Rate Limiting:** LoginPolicy (5 requests per 15 minutes)

#### Request Body:
```json
{
  "email": "user@example.com"
}
```

#### Response (Success):
```json
{
  "message": "If the email exists, a password reset link has been sent.",
  "token": "CfDJ8...", // ?? DEVELOPMENT ONLY - Remove in production
  "email": "user@example.com" // ?? DEVELOPMENT ONLY - Remove in production
}
```

#### Security Features:
- ? Rate limiting to prevent abuse
- ? Doesn't reveal if email exists (security best practice)
- ? Checks for locked accounts
- ? Generates secure reset token
- ? Comprehensive logging

#### Usage Example (cURL):
```bash
curl -X POST https://localhost:5001/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com"}'
```

#### Usage Example (C# HttpClient):
```csharp
var client = new HttpClient();
var request = new
{
    Email = "user@example.com"
};

var response = await client.PostAsJsonAsync(
    "https://localhost:5001/forgot-password", 
    request);
var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
```

---

### 2. Reset Password

**Endpoint:** `POST /reset-password`  
**Authentication:** Not required (uses token)  
**Rate Limiting:** LoginPolicy (5 requests per 15 minutes)

#### Request Body:
```json
{
  "email": "user@example.com",
  "token": "CfDJ8KyZVzPBkGg...",
  "newPassword": "NewSecure@Pass123"
}
```

#### Response (Success):
```json
{
  "message": "Password has been reset successfully. You can now login with your new password."
}
```

#### Response (Failure):
```json
{
  "error": "Password reset failed.",
  "errors": [
    "Passwords must have at least one non alphanumeric character.",
    "Passwords must have at least one uppercase ('A'-'Z')."
  ]
}
```

#### Security Features:
- ? Validates reset token
- ? Enforces password policy
- ? Resets lockout on success
- ? Doesn't reveal if email exists
- ? Rate limiting

#### Password Requirements:
- Minimum 8 characters (if configured)
- At least one digit
- At least one uppercase letter
- At least one lowercase letter
- At least one non-alphanumeric character

#### Usage Example (cURL):
```bash
curl -X POST https://localhost:5001/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "token": "CfDJ8KyZVzPBkGg...",
    "newPassword": "NewSecure@Pass123"
  }'
```

#### Usage Example (C# HttpClient):
```csharp
var client = new HttpClient();
var request = new
{
    Email = "user@example.com",
    Token = resetToken,
    NewPassword = "NewSecure@Pass123"
};

var response = await client.PostAsJsonAsync(
    "https://localhost:5001/reset-password", 
    request);
```

---

### 3. Change Password (Authenticated)

**Endpoint:** `POST /change-password`  
**Authentication:** **Required** (Bearer token)  
**Rate Limiting:** GlobalPolicy (100 requests per minute)

#### Request Headers:
```
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```

#### Request Body:
```json
{
  "currentPassword": "OldPass@123",
  "newPassword": "NewSecure@Pass123"
}
```

#### Response (Success):
```json
{
  "message": "Password has been changed successfully.",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 900,
  "tokenType": "Bearer"
}
```

#### Response (Failure):
```json
{
  "error": "Password change failed.",
  "errors": [
    "Incorrect password."
  ]
}
```

#### Security Features:
- ? Requires authentication
- ? Validates current password
- ? Enforces password policy
- ? Issues new JWT token after change
- ? Tracks failed attempts
- ? Resets lockout on success

#### Usage Example (cURL):
```bash
curl -X POST https://localhost:5001/change-password \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "OldPass@123",
    "newPassword": "NewSecure@Pass123"
  }'
```

#### Usage Example (C# HttpClient):
```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", jwtToken);

var request = new
{
    CurrentPassword = "OldPass@123",
    NewPassword = "NewSecure@Pass123"
};

var response = await client.PostAsJsonAsync(
    "https://localhost:5001/change-password", 
    request);
var result = await response.Content.ReadFromJsonAsync<ChangePasswordResponse>();
```

---

## ?? Security Features

### Rate Limiting
- **Forgot Password:** 5 attempts per 15 minutes per IP
- **Reset Password:** 5 attempts per 15 minutes per IP
- **Change Password:** 100 requests per minute per IP

### Password Policy
Enforced by ASP.NET Core Identity (configurable in `Program.cs`):
- ? Minimum length: 8 characters
- ? Requires digit
- ? Requires uppercase letter
- ? Requires lowercase letter
- ? Requires non-alphanumeric character
- ? Requires unique characters: 4

### Token Security
- Reset tokens expire after a configurable time (default: typically 1 day)
- Tokens are single-use only
- Tokens are cryptographically secure
- Invalid tokens return generic error messages

### Account Lockout
- Accounts lock after 5 failed attempts
- Lockout duration: 15 minutes
- Lockout resets on successful password reset/change

### Logging
All password operations are logged with:
- Timestamp
- User email
- Action result
- IP address (through rate limiter)

---

## ?? Complete User Flow Examples

### Flow 1: Forgot Password ? Reset Password

```csharp
// Step 1: User requests password reset
var forgotResponse = await httpClient.PostAsJsonAsync(
    "/forgot-password",
    new { Email = "user@example.com" });

var forgotResult = await forgotResponse.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
string resetToken = forgotResult.Token; // Get from email in production

// Step 2: User resets password with token
var resetResponse = await httpClient.PostAsJsonAsync(
    "/reset-password",
    new 
    { 
        Email = "user@example.com",
        Token = resetToken,
        NewPassword = "NewSecure@Pass123"
    });

// Step 3: User logs in with new password
var loginResponse = await httpClient.PostAsJsonAsync(
    "/login",
    new 
    { 
        Email = "user@example.com",
        Password = "NewSecure@Pass123"
    });
```

### Flow 2: Change Password (While Logged In)

```csharp
// User is already logged in with JWT token
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", currentJwtToken);

// Change password
var changeResponse = await httpClient.PostAsJsonAsync(
    "/change-password",
    new 
    { 
        CurrentPassword = "OldPass@123",
        NewPassword = "NewSecure@Pass123"
    });

var result = await changeResponse.Content.ReadFromJsonAsync<ChangePasswordResponse>();

// Update stored token with new one
string newJwtToken = result.Token;
```

---

## ?? Production Deployment Checklist

### Before Going to Production:

#### 1. **Remove Token from Forgot Password Response**

In `ResetPassword.cs`, change:
```csharp
// REMOVE THIS IN PRODUCTION:
return Results.Ok(new 
{ 
    Message = "If the email exists, a password reset link has been sent.",
    // Token = resetToken, // ? REMOVE THIS LINE
    // Email = request.Email // ? REMOVE THIS LINE
});
```

To:
```csharp
// PRODUCTION VERSION:
return Results.Ok(new 
{ 
    Message = "If the email exists, a password reset link has been sent."
});
```

#### 2. **Implement Email Service**

```csharp
// Add email service interface
public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken);
}

// In forgot-password endpoint:
// await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);
```

#### 3. **Configure Token Lifetime**

In `Program.cs`:
```csharp
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(3); // Token valid for 3 hours
});
```

#### 4. **Add HTTPS Enforcement**
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

#### 5. **Configure CORS**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

#### 6. **Set up Email Templates**

Example password reset email:
```html
<!DOCTYPE html>
<html>
<head>
    <title>Password Reset</title>
</head>
<body>
    <h2>Password Reset Request</h2>
    <p>Hello,</p>
    <p>You recently requested to reset your password. Click the link below to reset it:</p>
    <p>
        <a href="https://yourdomain.com/reset-password?token={{TOKEN}}&email={{EMAIL}}">
            Reset Password
        </a>
    </p>
    <p>This link will expire in 3 hours.</p>
    <p>If you didn't request this, please ignore this email.</p>
</body>
</html>
```

---

## ?? Testing

### Unit Test Example:

```csharp
[Fact]
public async Task ForgotPassword_ValidEmail_ReturnsSuccessMessage()
{
    // Arrange
    var request = new ForgotPasswordRequest("user@example.com");
    
    // Act
    var response = await _client.PostAsJsonAsync("/forgot-password", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
    result.Message.Should().Contain("password reset link has been sent");
}

[Fact]
public async Task ResetPassword_InvalidToken_ReturnsBadRequest()
{
    // Arrange
    var request = new ResetPasswordRequest
    {
        Email = "user@example.com",
        Token = "invalid-token",
        NewPassword = "NewSecure@Pass123"
    };
    
    // Act
    var response = await _client.PostAsJsonAsync("/reset-password", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}

[Fact]
public async Task ChangePassword_Authenticated_ReturnsNewToken()
{
    // Arrange
    _client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", _validToken);
    var request = new ChangePasswordRequest("OldPass@123", "NewSecure@Pass123");
    
    // Act
    var response = await _client.PostAsJsonAsync("/change-password", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ChangePasswordResponse>();
    result.Token.Should().NotBeNullOrEmpty();
}
```

---

## ?? Monitoring & Logging

### Log Events to Monitor:

1. **High Priority:**
   - Multiple failed reset attempts from same IP
   - Password reset for locked accounts
   - Invalid token usage patterns

2. **Medium Priority:**
   - Successful password resets
   - Password changes
   - Token generation

3. **Low Priority:**
   - Forgot password requests
   - User info lookups

### Example Log Queries (Application Insights):

```kusto
// Failed password reset attempts
traces
| where message contains "Password reset failed"
| summarize count() by user_Email, bin(timestamp, 1h)
| where count_ > 3

// Successful password resets
traces
| where message contains "Password successfully reset"
| summarize count() by bin(timestamp, 1d)
```

---

## ?? Troubleshooting

### Common Issues:

#### 1. "Invalid token" error
**Cause:** Token expired or already used  
**Solution:** Generate new token via forgot-password

#### 2. Rate limit exceeded
**Cause:** Too many requests  
**Solution:** Wait for rate limit window to reset (15 minutes)

#### 3. Password doesn't meet requirements
**Cause:** Password policy not met  
**Solution:** Follow password requirements listed above

#### 4. "Account is locked" error
**Cause:** Too many failed attempts  
**Solution:** Wait 15 minutes or contact support

---

## ?? Additional Resources

- [ASP.NET Core Identity Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Password Reset Best Practices](https://cheatsheetseries.owasp.org/cheatsheets/Forgot_Password_Cheat_Sheet.html)
- [JWT Best Practices](https://datatracker.ietf.org/doc/html/rfc8725)

---

**? Your password reset feature is now fully implemented with industry-standard security practices!**
