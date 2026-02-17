# Issue: UI Shows "Error loading tasks" and No Create Button

**Created:** 2026-02-16 15:45
**Status:** RESOLVED
**Severity:** CRITICAL (app non-functional)

## Symptom
- UI displays "Error loading tasks" on load
- No "New Task" button visible
- Clicking "Board" button shows brief spinner, then same error
- Screenshot: `.claude/issues/lemontoto-ui-no-create.png`

## Environment
- Backend: .NET 9 API (assumed running)
- Frontend: React 19 + Vite + TanStack Query
- Node 25.x, npm 11.x

## Investigation

### Step 1: Verify API is running
```bash
ps aux | grep LemonTodo
# Output: PID 394218 running /home/fazzo/.gwen/context/repos/lemontodo/src/LemonTodo.Api/bin/Debug/net9.0/LemonTodo.Api
```
✅ API process is running

### Step 2: Test connection
```bash
curl http://localhost:5062/api/tasks
# Output: curl: (7) Failed to connect to localhost port 5062 after 0 ms: Connection refused
```
❌ Connection refused on port 5062

### Step 3: Check actual listening port
```bash
ss -tlnp | grep -i lemontodo
# Output: LISTEN 127.0.0.1:5175 (LemonTodo.Api, pid=394218)
```

**ROOT CAUSE FOUND**: API is listening on port **5175**, but frontend client.ts is configured for port **5062**.

Verified in `src/LemonTodo.Api/Properties/launchSettings.json`:
- http profile: `applicationUrl: "http://localhost:5175"`
- Frontend `client/src/api/client.ts`: `BASE_URL = 'http://localhost:5062/api'` ❌ WRONG PORT

### Step 4: Test API on correct port
```bash
curl http://localhost:5175/api/tasks
# Output: []
```
✅ API responds correctly on port 5175

## Resolution

**Fixed port mismatch** between API and frontend:

1. Updated `client/src/api/client.ts`:
   - Changed `BASE_URL` from `http://localhost:5062/api` to `http://localhost:5175/api`

2. Updated `client/src/api/signalr.ts`:
   - Changed SignalR hub URL from `http://localhost:5062/hubs/tasks` to `http://localhost:5175/hubs/tasks`

The "New Task" button was always present in the code (BoardView.tsx) - it was just hidden because the UI couldn't load due to the connection error.

## Prevention

- Document the correct API port in README.md
- Consider using environment variables for API URL configuration
- Add frontend dev setup instructions referencing launchSettings.json
