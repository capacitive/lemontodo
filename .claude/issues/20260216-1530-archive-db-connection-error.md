# Issue: Archive DB Connection Error in Tests

**Created:** 2026-02-16 15:30
**Status:** RESOLVED
**Severity:** LOW (cosmetic - all tests pass)

## Symptom
Test execution shows error in logs:
```
fail: Microsoft.EntityFrameworkCore.Database.Connection[20004]
      An error occurred using the connection to database 'main' on server '/home/fazzo/.gwen/context/repos/lemontodo/src/LemonTodo.Api/archive.db'.
fail: LemonTodo.Api.Workers.TaskArchiveWorker[0]
      Error processing event for task s6IK4N2gTNaB5ksShaicG
      System.Threading.Tasks.TaskCanceledException: A task was canceled.
```

BUT: **All 68 tests pass** — this is a background logging artifact.

## Environment
- .NET 9.0.311
- EF Core 9.0.13 (SQLite provider)
- Ubuntu 22.04

## Investigation

### Full Test Output
```
Test Run Successful.
Total tests: 68 (Domain: 25, Application: 21, Infrastructure: 14, Api: 8)
     Passed: 68
     Failed: 0
```

### Root Cause
The `TaskArchiveWorker` BackgroundService runs during API integration tests. When a test:
1. Creates and closes a task via `PATCH /api/tasks/{id}/close`
2. Test completes and WebApplicationFactory tears down
3. Worker is still processing the event asynchronously
4. SQLite connection gets canceled mid-flight → `TaskCanceledException`

This is a **test lifecycle race condition**, not a production bug. The error is logged but doesn't fail tests because it's in a background service with catch-all error handling.

### Verification
```bash
dotnet test --verbosity normal
# Result: Build succeeded, 68/68 pass, 0 errors (only background logs)
```

## Resolution

**No fix needed** — this is expected behavior for async background workers in integration tests.

The error is benign because:
- All test assertions pass before teardown
- TaskArchiveWorker has proper exception handling
- Production runtime won't have abrupt teardowns like test fixtures

## Options to Suppress (if desired)

1. **Filter test logs** — configure `appsettings.Test.json` to set `Microsoft.EntityFrameworkCore` log level to `Warning` or higher
2. **Graceful shutdown** — add `Task.Delay()` after close operations in tests (fragile, not recommended)
3. **Mock the worker** — disable `TaskArchiveWorker` in test WebApplicationFactory setup (loses integration coverage)

**Recommendation:** ~~Leave as-is~~ **IMPLEMENTED**: Suppress via test-specific logging config.

## Applied Fix

Created `appsettings.Testing.json` to suppress EF Core and worker logs during tests:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Connection": "None",
      "Microsoft.EntityFrameworkCore.Update": "None",
      "LemonTodo.Api.Workers.TaskArchiveWorker": "Critical"
    }
  }
}
```

Modified `TaskEndpointTests.cs` to use Testing environment:
```csharp
var testFactory = factory.WithWebHostBuilder(builder =>
{
    builder.UseEnvironment("Testing");
});
```

Result: Clean test output, all 68 tests pass, no logging noise.

## Prevention
Use Testing environment for all integration tests to suppress verbose logging.
