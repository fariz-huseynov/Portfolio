# API Reference

All endpoints are versioned under `/api/v1/`. Protected endpoints require a JWT Bearer token via the `Authorization` header.

**Base URL**: `http://localhost:5001/api/v1`

---

## Authentication

### POST `/auth/login`

Login with email and password. Returns JWT access token and refresh token.

**Rate Limit**: Auth (10 req/min)

```json
// Request
{
  "email": "admin@portfolio.dev",
  "password": "Admin@123456"
}

// Response 200
{
  "accessToken": "eyJhbG...",
  "refreshToken": "a1b2c3...",
  "expiresAt": "2026-02-18T13:00:00Z"
}

// Response 200 (2FA required)
{
  "requiresTwoFactor": true,
  "twoFactorToken": "temp-token-uuid"
}
```

### POST `/auth/refresh`

Exchange a refresh token for a new access token.

```json
// Request
{
  "accessToken": "expired-jwt...",
  "refreshToken": "a1b2c3..."
}
```

### POST `/auth/logout`

Revoke a refresh token.

```json
{
  "refreshToken": "a1b2c3..."
}
```

### GET `/auth/me`

**Auth**: Any valid JWT

Returns the current user's profile with roles and permissions.

### POST `/auth/change-password`

**Auth**: Any valid JWT

```json
{
  "currentPassword": "OldPass@123",
  "newPassword": "NewPass@456"
}
```

### POST `/auth/forgot-password`

**Rate Limit**: ForgotPassword (3 req/15min)

```json
{
  "email": "admin@portfolio.dev"
}
```

### POST `/auth/reset-password`

**Rate Limit**: ForgotPassword

```json
{
  "email": "admin@portfolio.dev",
  "token": "reset-token-from-email",
  "newPassword": "NewPass@456"
}
```

### Two-Factor Authentication

| Endpoint | Auth | Description |
|----------|------|-------------|
| POST `/auth/2fa/setup` | JWT | Generate TOTP secret + QR code URI |
| POST `/auth/2fa/enable` | JWT | Verify code and enable 2FA |
| POST `/auth/2fa/disable` | JWT | Disable 2FA with verification code |
| POST `/auth/2fa/verify` | Anonymous | Verify 2FA code during login (rate limited: 5/min) |
| POST `/auth/2fa/recovery` | Anonymous | Use recovery code to bypass 2FA |

---

## Public Portfolio

All public endpoints are anonymous, rate limited (30 req/min), and cached for 5 minutes via output cache.

### Site Content

| Endpoint | Description |
|----------|-------------|
| GET `/site/settings` | Site-wide settings (title, description, SEO) |
| GET `/site/hero` | Hero section content |
| GET `/site/about` | About me section |
| GET `/site/skills` | Skills list |
| GET `/site/experiences` | Work experiences |
| GET `/site/services` | Services offered |
| GET `/site/testimonials` | Client testimonials |
| GET `/site/social-links` | Social media links |
| GET `/site/menu` | Navigation menu items |

### Blog Posts

| Endpoint | Description |
|----------|-------------|
| GET `/portfolio/blogs` | All published blog posts |
| GET `/portfolio/blogs/paged?pageNumber=1&pageSize=10` | Paginated published posts |
| GET `/portfolio/blogs/{slug}` | Single post by URL slug |

### Projects

| Endpoint | Description |
|----------|-------------|
| GET `/portfolio/projects` | All published projects |
| GET `/portfolio/projects/paged?pageNumber=1&pageSize=10` | Paginated published projects |
| GET `/portfolio/projects/{slug}` | Single project by URL slug |

---

## Admin: Blog Posts

**Permission**: `Blogs.View`, `Blogs.Create`, `Blogs.Edit`, `Blogs.Delete`

| Method | Endpoint | Permission |
|--------|----------|------------|
| GET | `/admin/blogs` | Blogs.View |
| GET | `/admin/blogs/paged?pageNumber=1&pageSize=10` | Blogs.View |
| GET | `/admin/blogs/{id}` | Blogs.View |
| POST | `/admin/blogs` | Blogs.Create |
| PUT | `/admin/blogs/{id}` | Blogs.Edit |
| DELETE | `/admin/blogs/{id}` | Blogs.Delete |
| POST | `/admin/blogs/{id}/publish` | Blogs.Edit |
| POST | `/admin/blogs/{id}/unpublish` | Blogs.Edit |

```json
// POST /admin/blogs
{
  "title": "Getting Started with .NET 10",
  "slug": "getting-started-dotnet-10",
  "content": "<p>Blog content with HTML...</p>",
  "excerpt": "A brief overview of .NET 10 features",
  "featuredImageUrl": "/uploads/2026/02/dotnet10.png",
  "tags": ["dotnet", "csharp"],
  "isPublished": false
}
```

---

## Admin: Projects

**Permission**: `Projects.View`, `Projects.Create`, `Projects.Edit`, `Projects.Delete`

| Method | Endpoint | Permission |
|--------|----------|------------|
| GET | `/admin/projects` | Projects.View |
| GET | `/admin/projects/paged` | Projects.View |
| GET | `/admin/projects/{id}` | Projects.View |
| POST | `/admin/projects` | Projects.Create |
| PUT | `/admin/projects/{id}` | Projects.Edit |
| DELETE | `/admin/projects/{id}` | Projects.Delete |

---

## Admin: Site Content

**Permission**: `SiteContent.View`, `SiteContent.Edit`

Manages all site sections: hero, about, skills, experiences, services, testimonials, social links, and menu items.

| Method | Endpoint | Permission |
|--------|----------|------------|
| GET | `/admin/content/hero` | SiteContent.View |
| PUT | `/admin/content/hero` | SiteContent.Edit |
| GET | `/admin/content/about` | SiteContent.View |
| PUT | `/admin/content/about` | SiteContent.Edit |
| GET | `/admin/content/skills` | SiteContent.View |
| POST | `/admin/content/skills` | SiteContent.Edit |
| PUT | `/admin/content/skills/{id}` | SiteContent.Edit |
| DELETE | `/admin/content/skills/{id}` | SiteContent.Edit |
| GET | `/admin/content/experiences` | SiteContent.View |
| POST | `/admin/content/experiences` | SiteContent.Edit |
| PUT | `/admin/content/experiences/{id}` | SiteContent.Edit |
| DELETE | `/admin/content/experiences/{id}` | SiteContent.Edit |
| GET | `/admin/content/services` | SiteContent.View |
| POST | `/admin/content/services` | SiteContent.Edit |
| PUT | `/admin/content/services/{id}` | SiteContent.Edit |
| DELETE | `/admin/content/services/{id}` | SiteContent.Edit |
| GET | `/admin/content/testimonials` | SiteContent.View |
| POST | `/admin/content/testimonials` | SiteContent.Edit |
| PUT | `/admin/content/testimonials/{id}` | SiteContent.Edit |
| DELETE | `/admin/content/testimonials/{id}` | SiteContent.Edit |
| GET | `/admin/content/social-links` | SiteContent.View |
| POST | `/admin/content/social-links` | SiteContent.Edit |
| PUT | `/admin/content/social-links/{id}` | SiteContent.Edit |
| DELETE | `/admin/content/social-links/{id}` | SiteContent.Edit |
| GET | `/admin/content/menu-items` | SiteContent.View |
| POST | `/admin/content/menu-items` | SiteContent.Edit |
| PUT | `/admin/content/menu-items/{id}` | SiteContent.Edit |
| DELETE | `/admin/content/menu-items/{id}` | SiteContent.Edit |

---

## Admin: AI Content Generation

**Permission**: `AiContent.Generate`, `AiContent.View`
**Rate Limit**: AiGeneration (10 req/min)

See [AI Features](ai-features.md) for detailed usage guide.

| Method | Endpoint | Permission | Rate Limited |
|--------|----------|------------|--------------|
| POST | `/admin/ai/generate-text` | AiContent.Generate | Yes |
| POST | `/admin/ai/rewrite-text` | AiContent.Generate | Yes |
| POST | `/admin/ai/generate-image` | AiContent.Generate | Yes |
| GET | `/admin/ai/generations/{id}` | AiContent.View | No |
| GET | `/admin/ai/generations?pageNumber=1&pageSize=10` | AiContent.View | No |
| GET | `/admin/ai/providers` | AiContent.View | No |

```json
// POST /admin/ai/generate-text
{
  "operationType": "GenerateBlogPost",
  "prompt": "Write about .NET 10 performance improvements",
  "additionalContext": "Focus on AOT compilation",
  "preferredProvider": "OpenAi",
  "preferredModel": "gpt-4o"
}

// Response 200
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "provider": "OpenAi",
  "operationType": "GenerateBlogPost",
  "status": "Completed",
  "resultContent": "# .NET 10 Performance...\n\n...",
  "modelName": "gpt-4o",
  "inputTokens": 150,
  "outputTokens": 1200,
  "durationSeconds": 3.5,
  "completedAt": "2026-02-18T12:00:03Z"
}
```

---

## Admin: Users & Roles

**Permission**: `Users.View`, `Users.Create`, `Users.Edit`, `Users.Delete`, `Users.ResetPassword`

| Method | Endpoint | Permission |
|--------|----------|------------|
| GET | `/admin/users` | Users.View |
| GET | `/admin/users/paged` | Users.View |
| GET | `/admin/users/{id}` | Users.View |
| POST | `/admin/users` | Users.Create |
| PUT | `/admin/users/{id}` | Users.Edit |
| DELETE | `/admin/users/{id}` | Users.Delete |
| PUT | `/admin/users/{id}/reset-password` | Users.ResetPassword |
| GET | `/admin/roles` | Users.View |
| GET | `/admin/roles/{id}` | Users.View |
| POST | `/admin/roles` | Users.Create |
| PUT | `/admin/roles/{id}` | Users.Edit |
| DELETE | `/admin/roles/{id}` | Users.Delete |
| GET | `/admin/roles/permissions` | Users.View |

---

## Admin: Leads

**Permission**: `Leads.View`, `Leads.MarkRead`

| Method | Endpoint | Permission |
|--------|----------|------------|
| GET | `/admin/leads` | Leads.View |
| GET | `/admin/leads/paged` | Leads.View |
| PUT | `/admin/leads/{id}/read` | Leads.MarkRead |

### Lead Submission (Public)

**Rate Limit**: LeadSubmit (5 req/min)

```json
// POST /leads/submit
{
  "name": "John Doe",
  "email": "john@example.com",
  "subject": "Project Inquiry",
  "message": "I'd like to discuss a project...",
  "captchaId": "captcha-uuid",
  "captchaCode": "A3X9"
}
```

---

## Admin: Files

**Permission**: `Files.Manage`
**Auth**: Any valid JWT (controller-level `[Authorize]`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/admin/files/upload` | Upload file (multipart/form-data, max 10 MB) |
| GET | `/admin/files/{id}` | Get file metadata |
| GET | `/admin/files?pageNumber=1&pageSize=20` | List files (paginated) |
| DELETE | `/admin/files/{id}` | Delete file |

---

## Admin: IP Rules

**Permission**: `Security.View`, `Security.Manage`

| Method | Endpoint | Permission |
|--------|----------|------------|
| GET | `/admin/ip-rules` | Security.View |
| GET | `/admin/ip-rules/{id}` | Security.View |
| POST | `/admin/ip-rules` | Security.Manage |
| PUT | `/admin/ip-rules/{id}` | Security.Manage |
| DELETE | `/admin/ip-rules/{id}` | Security.Manage |

---

## Admin: Site Settings

**Permission**: `Settings.View`, `Settings.Edit`

| Method | Endpoint | Permission |
|--------|----------|------------|
| GET | `/admin/settings` | Settings.View |
| PUT | `/admin/settings` | Settings.Edit |

---

## Admin: Profile

**Auth**: Any valid JWT (no specific permission)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/admin/profile` | Get own profile |
| PUT | `/admin/profile` | Update own profile |
| POST | `/admin/profile/avatar` | Upload avatar (max 5 MB, images only) |
| DELETE | `/admin/profile/avatar` | Remove avatar |

---

## CAPTCHA

| Endpoint | Description |
|----------|-------------|
| GET `/captcha/generate` | Returns `{ id, image }` (base64 data URI) |
| GET `/captcha/image` | Returns raw PNG bytes |

---

## Health Check

| Endpoint | Description |
|----------|-------------|
| GET `/health` | Returns health status of all dependencies |

```json
// Response 200
{
  "status": "Healthy",
  "totalDuration": 45.2,
  "timestamp": "2026-02-18T12:00:00Z",
  "checks": [
    { "name": "sqlserver", "status": "Healthy", "duration": 12.3 },
    { "name": "redis", "status": "Healthy", "duration": 2.1 }
  ]
}
```

---

## Error Responses

All errors follow a consistent format:

```json
// 400 Bad Request
{
  "errors": {
    "Title": ["The Title field is required."]
  }
}

// 401 Unauthorized
// (empty body — JWT missing or expired)

// 403 Forbidden
// (empty body — insufficient permissions)

// 404 Not Found
{
  "error": "Blog post not found"
}

// 429 Too Many Requests
{
  "error": "Rate limit exceeded",
  "message": "Too many requests. Please wait a moment and try again.",
  "retryAfter": "60 seconds"
}
```

---

## Rate Limiting Summary

| Policy | Limit (Production) | Window | Applies To |
|--------|-------------------|--------|------------|
| Auth | 10 requests | 1 min | Login, refresh, password change |
| ForgotPassword | 3 requests | 15 min | Password reset |
| TwoFactorVerify | 5 requests | 1 min | 2FA verification |
| LeadSubmit | 5 requests | 1 min | Contact form |
| AiGeneration | 10 requests | 1 min | AI content generation |
| PublicApi | 30 requests | 1 min | Public portfolio/site endpoints |
| Global | 100 requests | 1 min | All endpoints (per IP) |

Whitelisted IPs bypass the global rate limiter.
