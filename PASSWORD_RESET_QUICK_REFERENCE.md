# ?? Password Reset - Quick Reference

## Three Endpoints Added:

### 1. ?? Forgot Password (No Auth Required)
```http
POST /forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

**Returns:** Generic success message (doesn't reveal if email exists)

---

### 2. ?? Reset Password (Uses Token)
```http
POST /reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "CfDJ8...",
  "newPassword": "NewSecure@Pass123"
}
```

**Returns:** Success message or validation errors

---

### 3. ?? Change Password (Requires Auth)
```http
POST /change-password
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "currentPassword": "OldPass@123",
  "newPassword": "NewSecure@Pass123"
}
```

**Returns:** New JWT token + success message

---

## Security Features ?

- ? Rate limiting (prevents brute force)
- ? Account lockout protection
- ? Secure token generation
- ? Password policy enforcement
- ? Comprehensive logging
- ? New JWT after password change
- ? Generic error messages (security)

---

## Password Requirements

- Minimum 8 characters
- At least one digit (0-9)
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one special character (!@#$%^&*)

---

## Rate Limits

- **Forgot/Reset Password:** 5 requests per 15 minutes
- **Change Password:** 100 requests per minute

---

## ?? Before Production

1. **Remove token from response** in forgot-password endpoint
2. **Add email service** to send reset links
3. **Configure token lifetime** (default: 1 day)
4. **Set up email templates**
5. **Test all flows thoroughly**

---

## Testing Workflow

```bash
# 1. Request reset
curl -X POST http://localhost:5000/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com"}'

# 2. Reset password (use token from step 1)
curl -X POST http://localhost:5000/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "email":"user@example.com",
    "token":"<token-from-response>",
    "newPassword":"NewSecure@Pass123"
  }'

# 3. Login with new password
curl -X POST http://localhost:5000/login \
  -H "Content-Type: application/json" \
  -d '{
    "email":"user@example.com",
    "password":"NewSecure@Pass123"
  }'
```

---

## Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| "Invalid reset attempt" | Invalid token | Request new token |
| "Account is locked" | Too many failed attempts | Wait 15 minutes |
| Password validation errors | Doesn't meet requirements | Use stronger password |
| 429 Too Many Requests | Rate limit exceeded | Wait before retrying |
| 401 Unauthorized | Missing/invalid JWT | Login again |

---

## Files Modified/Created

? **Created:** `IdentityAuthorization/Features/ResetPassword.cs`  
? **Modified:** `IdentityAuthorization/Program.cs` (added `.AddDefaultTokenProviders()` and endpoint registration)  
? **Build Status:** Successful ?

---

**Your password reset feature is ready to use! ??**
