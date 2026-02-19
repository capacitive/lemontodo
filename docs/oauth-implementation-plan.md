# OAuth Implementation Plan (Google & GitHub)

## Current State

The OAuth endpoints in `src/LemonTodo.Api/Endpoints/AuthEndpoints.cs` (lines 63-88) are **stubs returning 501 Not Implemented**. The frontend buttons in `LoginView.tsx` redirect to these endpoints, which immediately fail. Email/password auth is fully functional.

The domain already supports external logins — `ExternalLogin.cs` exists and `OAuthLoginAsync` is defined on `IAuthService`. The remaining work is implementing the actual OAuth redirect + callback flow.

---

## Prerequisites: Provider Registration

### Google OAuth

1. Go to [Google Cloud Console](https://console.cloud.google.com/) → APIs & Services → Credentials
2. Create an **OAuth 2.0 Client ID** (Web application type)
3. Add authorized redirect URIs:
   - Local: `http://localhost:5062/api/auth/google/callback`
   - Production: `https://<your-domain>/api/auth/google/callback`
4. Note the **Client ID** and **Client Secret**

### GitHub OAuth

1. Go to GitHub → Settings → Developer settings → OAuth Apps → New OAuth App
2. Set the Authorization callback URL:
   - Local: `http://localhost:5062/api/auth/github/callback`
   - Production: `https://<your-domain>/api/auth/github/callback`
3. Note the **Client ID** and **Client Secret**

---

## Implementation Steps

### Step 1: Configuration

Add OAuth settings to `appsettings.json` (and `appsettings.Development.json` for local secrets):

```json
{
  "OAuth": {
    "Google": {
      "ClientId": "",
      "ClientSecret": "",
      "AuthorizationEndpoint": "https://accounts.google.com/o/oauth2/v2/auth",
      "TokenEndpoint": "https://oauth2.googleapis.com/token",
      "UserInfoEndpoint": "https://www.googleapis.com/oauth2/v3/userinfo",
      "Scopes": "openid email profile"
    },
    "GitHub": {
      "ClientId": "",
      "ClientSecret": "",
      "AuthorizationEndpoint": "https://github.com/login/oauth/authorize",
      "TokenEndpoint": "https://github.com/login/oauth/access_token",
      "UserInfoEndpoint": "https://api.github.com/user",
      "Scopes": "read:user user:email"
    }
  }
}
```

Create a strongly-typed config class:

```
src/LemonTodo.Infrastructure/Auth/OAuthSettings.cs
```

Register in DI via `builder.Configuration.GetSection("OAuth").Get<OAuthSettings>()`.

### Step 2: OAuth Service

Create an `IOAuthService` / `OAuthService` that handles the provider-agnostic parts:

- `GetAuthorizationUrl(provider, state)` — builds the redirect URL with client ID, scopes, redirect URI, and a CSRF `state` parameter
- `ExchangeCodeForTokenAsync(provider, code)` — POST to the provider's token endpoint with the authorization code
- `GetUserInfoAsync(provider, accessToken)` — GET the user's profile (email, name, provider user ID)

Location: `src/LemonTodo.Application/Interfaces/IOAuthService.cs` and `src/LemonTodo.Infrastructure/Auth/OAuthService.cs`

### Step 3: Implement the Endpoints

Replace the stubs in `AuthEndpoints.cs`:

#### `/api/auth/google` (and `/api/auth/github`)

1. Generate a random `state` token (CSRF protection)
2. Store `state` in a short-lived HTTP-only cookie (or in-memory cache)
3. Redirect (302) to the provider's authorization URL

#### `/api/auth/google/callback` (and `/api/auth/github/callback`)

1. Validate the `state` parameter against the stored value
2. Call `OAuthService.ExchangeCodeForTokenAsync(provider, code)` to get an access token
3. Call `OAuthService.GetUserInfoAsync(provider, accessToken)` to get email/name/provider ID
4. Call the existing `IAuthService.OAuthLoginAsync(...)` to create or link the user account
5. Issue JWT + refresh token (same as regular login)
6. Redirect to the frontend with the tokens (e.g., `http://localhost:5173/auth/callback?token=...`)

### Step 4: Frontend Callback Page

Create a new route/component at `/auth/callback` that:

1. Reads the token from the URL query parameters
2. Stores it (same as the regular login flow)
3. Redirects to the main app

### Step 5: Security Considerations

- **CSRF protection**: The `state` parameter must be validated on callback
- **HTTPS**: Google allows `http://localhost` for development; production must be HTTPS
- **Secret storage**: Never commit client secrets to git — use `dotnet user-secrets` locally, environment variables or a secrets manager in production
- **Token in URL**: The redirect from callback to frontend briefly exposes the JWT in the URL; consider using a short-lived authorization code that the frontend exchanges, or set the token in an HTTP-only cookie instead

---

## Local vs. Cloud Deployment

| Concern | Local Development | Cloud Deployment |
|---------|-------------------|------------------|
| OAuth redirect URIs | `http://localhost:5062/...` | `https://<domain>/...` |
| HTTPS requirement | Not required (localhost exempt) | Required |
| Secret storage | `appsettings.Development.json` or `dotnet user-secrets` | Environment variables / secrets manager (e.g., AWS SSM, Azure Key Vault) |
| CORS | `localhost:5173` | Frontend's production domain |
| Code changes | None | None — same code, different config |

When deploying, you just need to:

1. Register new redirect URIs with Google/GitHub pointing to your production domain
2. Set the client ID/secret via environment variables
3. Update CORS and any hardcoded localhost references to use config-driven URLs

---

## Files to Create/Modify

| File | Action |
|------|--------|
| `src/LemonTodo.Infrastructure/Auth/OAuthSettings.cs` | Create — config POCO |
| `src/LemonTodo.Application/Interfaces/IOAuthService.cs` | Create — interface |
| `src/LemonTodo.Infrastructure/Auth/OAuthService.cs` | Create — implementation |
| `src/LemonTodo.Api/Endpoints/AuthEndpoints.cs` | Modify — replace stubs |
| `src/LemonTodo.Api/Program.cs` | Modify — register OAuth config + service |
| `src/LemonTodo.Api/appsettings.json` | Modify — add OAuth section |
| `client/src/components/Auth/OAuthCallback.tsx` | Create — token capture page |
| `client/src/App.tsx` (or router config) | Modify — add `/auth/callback` route |

---

## Testing Plan

- **Unit tests**: `OAuthService` with mocked HTTP client — verify URL construction, token exchange, user info parsing
- **Integration tests**: Mock provider responses via `WebApplicationFactory` with a test HTTP handler — verify full redirect → callback → JWT flow
- **Manual testing**: Register real OAuth apps and test end-to-end locally

---

## Open Questions

1. **Token delivery to frontend**: Redirect with token in URL query param (simple but briefly exposed) vs. HTTP-only cookie (more secure) vs. short-lived code exchange?
2. **Account linking**: If a user registers with email, then later signs in with Google using the same email — auto-link or prompt?
3. **Provider-only accounts**: Allow users who sign up via OAuth to set a password later?
