# GitHub OAuth — Implementation Summary & Setup Guide

## What Was Implemented

### Commit 1: GitHubOAuthService (Infrastructure Layer)

**Files created:**
- `src/LemonTodo.Infrastructure/Auth/OAuthSettings.cs` — strongly-typed config POCO for GitHub OAuth settings (ClientId, ClientSecret, endpoints, scopes)
- `src/LemonTodo.Application/Interfaces/IGitHubOAuthService.cs` — interface with `GetAuthorizationUrl(state)` and `ExchangeCodeAsync(code)`
- `src/LemonTodo.Infrastructure/Auth/GitHubOAuthService.cs` — full implementation:
  - Builds GitHub authorization URL with client ID, scopes, redirect URI, and CSRF state
  - Exchanges authorization code for access token via GitHub's token endpoint
  - Fetches user profile from GitHub's user API
  - Falls back to the emails endpoint when the user's email is not public (picks primary verified email)
- `tests/LemonTodo.Infrastructure.Tests/GitHubOAuthServiceTests.cs` — 5 tests with mocked HTTP:
  - URL building with correct parameters
  - Valid code exchange returning user info
  - Private email fallback to emails endpoint
  - Error response handling (bad verification code)
  - No verified email throws

**Files modified:**
- `src/LemonTodo.Api/appsettings.json` — added full GitHub OAuth config section with endpoint URLs and scopes

---

### Commit 2: GitHub OAuth Endpoints (API Layer)

**Files created:**
- `tests/LemonTodo.Api.Tests/AuthEndpointTests.cs` — 6 integration tests:
  - `GET /api/auth/github` returns 302 redirect to GitHub
  - Redirect sets `oauth_state` cookie
  - Valid callback with matching state redirects to frontend with tokens
  - Mismatched state returns 400
  - Missing state cookie returns 400
  - Service error returns 502

**Files modified:**
- `src/LemonTodo.Api/Endpoints/AuthEndpoints.cs` — replaced GitHub stubs with:
  - `GET /api/auth/github`: generates random state, sets it as HttpOnly cookie, redirects to GitHub
  - `GET /api/auth/github/callback`: validates state cookie against query param (CSRF protection), exchanges code via `IGitHubOAuthService`, calls `IAuthService.OAuthLoginAsync`, redirects to frontend with tokens in URL fragment
- `src/LemonTodo.Api/Program.cs` — registered `OAuthSettings`, `IGitHubOAuthService`, and `HttpClient` in DI

---

### Commit 3: Frontend OAuth Callback

**Files created:**
- `client/src/components/Auth/OAuthCallback.tsx` — reads `access_token` and `refresh_token` from URL fragment, clears hash from browser history, calls `loginWithTokens`, shows error state if tokens missing

**Files modified:**
- `client/src/contexts/AuthContext.tsx` — added `loginWithTokens(accessToken, refreshToken)` method that stores tokens and fetches user profile via `accountApi.getProfile()`
- `client/src/App.tsx` — routes `/auth/callback` path to `OAuthCallback` component

---

## End-to-End Testing Setup

### Step 1: Register a GitHub OAuth App

1. Go to **GitHub → Settings → Developer settings → OAuth Apps → New OAuth App**
   - Direct link: https://github.com/settings/applications/new
2. Fill in:
   - **Application name**: `LemonTodo (dev)` (or anything you like)
   - **Homepage URL**: `http://localhost:5173`
   - **Authorization callback URL**: `http://localhost:5175/api/auth/github/callback`
3. Click **Register application**
4. On the next page, note your **Client ID**
5. Click **Generate a new client secret** and copy the **Client Secret** immediately (you won't see it again)

### Step 2: Configure the App

Edit `src/LemonTodo.Api/appsettings.json` and fill in your credentials:

```json
{
  "OAuth": {
    "GitHub": {
      "ClientId": "your-client-id-here",
      "ClientSecret": "your-client-secret-here",
      "AuthorizationEndpoint": "https://github.com/login/oauth/authorize",
      "TokenEndpoint": "https://github.com/login/oauth/access_token",
      "UserInfoEndpoint": "https://api.github.com/user",
      "UserEmailsEndpoint": "https://api.github.com/user/emails",
      "Scopes": "read:user user:email"
    }
  },
  "OAuth:CallbackBaseUrl": "http://localhost:5175",
  "OAuth:FrontendUrl": "http://localhost:5173"
}
```

> **Security note**: Don't commit real secrets to git. For production, use environment variables or `dotnet user-secrets`:
> ```bash
> cd src/LemonTodo.Api
> dotnet user-secrets set "OAuth:GitHub:ClientId" "your-client-id"
> dotnet user-secrets set "OAuth:GitHub:ClientSecret" "your-client-secret"
> ```

### Step 3: Start the App

Terminal 1 — API:
```bash
cd src/LemonTodo.Api
dotnet run
```

Terminal 2 — Frontend:
```bash
cd client
npm run dev
```

### Step 4: Test the Flow

1. Open `http://localhost:5173` in your browser
2. On the login page, click the **GitHub** button
3. You'll be redirected to GitHub's authorization page
4. Authorize the app — GitHub redirects back to `http://localhost:5175/api/auth/github/callback`
5. The backend validates the state cookie, exchanges the code for a token, fetches your GitHub profile, creates/links a user account, and redirects to `http://localhost:5173/auth/callback#access_token=...&refresh_token=...`
6. The frontend `OAuthCallback` component picks up the tokens, fetches your profile, and you're logged in

### What to Verify

- **New user**: First GitHub login creates a new account with your GitHub email and display name
- **Account linking**: If you already registered with the same email via password, the GitHub login links to the existing account
- **Profile**: Check the profile view — your `linkedProviders` should include `"GitHub"`
- **Token refresh**: The access token auto-refreshes every 13 minutes via the existing refresh mechanism
- **CSRF protection**: The state cookie is validated on callback — tampering returns 400

### Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| "redirect_uri mismatch" from GitHub | Callback URL doesn't match what's registered | Ensure GitHub app callback is exactly `http://localhost:5175/api/auth/github/callback` |
| 400 "Invalid OAuth state" | State cookie not sent or expired | Check browser allows cookies from localhost; try clearing cookies |
| 502 from callback | GitHub rejected the code | Code may have expired (10 min TTL) or been used already; try again |
| "No verified email found" | GitHub account has no verified email | Verify an email address in GitHub settings |
| Frontend shows "Missing authentication tokens" | Redirect didn't include tokens in fragment | Check API logs for errors in the callback endpoint |

---

## Test Suite

Run all tests (135 total):
```bash
dotnet test
```

GitHub OAuth-specific tests:
```bash
# Infrastructure: GitHubOAuthService (5 tests)
dotnet test tests/LemonTodo.Infrastructure.Tests/ --filter "GitHubOAuthService"

# API: Auth endpoints (6 tests)
dotnet test tests/LemonTodo.Api.Tests/ --filter "AuthEndpoint"
```
