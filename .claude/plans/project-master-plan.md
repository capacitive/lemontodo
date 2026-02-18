# LemonTodo — Master Project Plan

> Living document. Update as work progresses.
> Last updated: 2026-02-17

---

## Project Overview

Full-stack task management app built as a learning-focused exercise with TDD,
Clean Architecture, event-driven archival, and real-time updates.

**Stack:**
- Backend: .NET 9 Minimal API · EF Core 9 · SignalR · FluentValidation · xUnit
- Frontend: React 19 · TypeScript · Vite · TanStack Query · @dnd-kit · MSW

**Ports (current):**
- API: `http://localhost:5175` (launchSettings.json — http profile)
- Frontend: `http://localhost:5173` (Vite default)

---

## Current State

### Iteration 1 — Completed ✅

**Git history (main → dev):**

| Commit | Description |
|--------|-------------|
| Phase 0 | Solution structure, project refs, NuGet packages |
| Phase 1 | Domain layer TDD (25 tests) |
| Phase 2 | Infrastructure layer TDD (14 tests) |
| Phase 3 | Application layer TDD (21 tests) |
| Phase 4 | API layer + integration tests (8 tests) |
| Phase 5 | React frontend (Board, List, Archive, SignalR) |
| Phase 6 | README and architecture docs |
| Hotfix | Fix API port mismatch (5062 → 5175 in client) |
| Hotfix | Suppress verbose test logging (appsettings.Testing.json) |
| Feature | Add comprehensive React UI test suite (55 tests, Vitest + RTL + MSW) |
| Bugfix | Fix Edit button blocked by @dnd-kit listeners in BoardView |
| Chore | Downgrade FluentAssertions 8.8.0 → 6.12.0 (remove Xceed license warnings) |

### Test Coverage

| Suite | Tests | Tool |
|-------|-------|------|
| Domain | 25 | xUnit + FluentAssertions |
| Application | 21 | xUnit + NSubstitute + FluentAssertions |
| Infrastructure | 14 | xUnit + EF InMemory |
| API integration | 8 | WebApplicationFactory |
| Frontend | 55 | Vitest + React Testing Library + MSW |
| **Total** | **123** | |

---

## Architecture Reference

### Backend — Clean Architecture

```
Domain        → Entities, enums, interfaces, domain events  (zero deps)
Application   → Services, DTOs, FluentValidation, mapping
Infrastructure → EF Core repos, channels, ID generator
Api           → Minimal API endpoints, SignalR hub, TaskArchiveWorker
```

**Dual-store pattern:**
- `ActiveDbContext` (EF InMemory) — volatile, active tasks
- `ArchiveDbContext` (EF SQLite at `src/LemonTodo.Api/archive.db`) — persistent
- On `Close`: event → `System.Threading.Channels` → `TaskArchiveWorker` → SQLite + SignalR notify

**Domain rules:**
- `TodoTask.Create(...)` — factory, private setters
- State machine: `Open → Closed`, `Closed → Reopened`, `Reopened → Closed`
  All other transitions throw `InvalidTransitionException`
- IDs: custom 21-char NanoID-style generator (no external NuGet)
- Name ≤ 200 chars · Description ≤ 2000 chars

### Frontend — React Component Tree

```
App
├── Header (view toggle: board | list | archive)
├── BoardView (DndContext → BoardColumn × 3 → DraggableCard → TaskCard)
├── ListView (active tasks + recently closed)
└── ArchiveView (search + pagination + restore)

TaskModal — shared create/edit modal (controlled by parent state)
```

**Key patterns:**
- TanStack Query for server state; MSW mocks for tests
- `useSignalR` triggers query invalidation on `TaskClosed` / `TaskRestored` / `TaskUpdated`
- DraggableCard uses a **drag handle** (`⠿ drag` div) — listeners isolated from action buttons

### API Contract

| Method | Route | Status Codes |
|--------|-------|-------------|
| GET | `/api/tasks` | 200 |
| GET | `/api/tasks/{id}` | 200, 404 |
| POST | `/api/tasks` | 201 + Location, 400 |
| PUT | `/api/tasks/{id}` | 200, 400, 404 |
| PATCH | `/api/tasks/{id}/close` | 200, 404, 409 |
| PATCH | `/api/tasks/{id}/reopen` | 200, 404, 409 |
| GET | `/api/archive?q=&page=&pageSize=` | 200 |
| GET | `/api/archive/{id}` | 200, 404 |
| PATCH | `/api/archive/{id}/restore` | 200, 404, 409 |

SignalR: `/hubs/tasks` → events `TaskClosed`, `TaskRestored`, `TaskUpdated`

---

## Technical Debt

| Item | Priority | Notes |
|------|----------|-------|
| CLAUDE.md still references port 5062 | Low | Update to 5175 |
| CLAUDE.md says FluentAssertions 8.x | Low | Now pinned to 6.12.0 |
| Active store resets on API restart | Medium | Planned in Iteration 2 |
| Closed tasks editable in Recently Closed (ListView) | Low | No Edit button — intentional, but worth confirming UX intent |
| BoardView edit tests use `getByLabelText('Edit')` | Medium | Labels not formally associated to inputs; tests work but fragile |
| No error boundary in React app | Low | Unhandled fetch failures show raw error text |
| SignalR reconnect loop logs to console | Low | Cosmetic; could add exponential backoff |

---

## Resolved Issues

| ID | Issue | Resolution |
|----|-------|------------|
| 20260216-1530 | SQLite connection errors during test runs | appsettings.Testing.json + Testing env in WebApplicationFactory |
| 20260216-1545 | UI "Error loading tasks" + no create button | Port mismatch: client hardcoded 5062, API on 5175 |
| — | FluentAssertions 8.x Xceed license warnings | Downgraded to 6.12.0 (Apache-2.0) across all 4 test projects |
| — | Edit button unresponsive in BoardView | @dnd-kit listeners on outer div intercepted pointer events; moved to dedicated drag handle |

---

## Iteration 2 Roadmap

### Definite (from original design)

1. **Persistent active store** — seed `ActiveDbContext` from SQLite on startup
   - Removes the "restart wipes active tasks" limitation
   - Requires migration strategy for the InMemory→SQLite seeding

2. **User accounts + JWT auth**
   - Registration / login endpoints
   - `[Authorize]` on all task endpoints
   - Per-user task isolation at the repository layer
   - Frontend: login page, token storage, auth headers in `client.ts`

3. **Task priorities** — Low / Medium / High enum field
   - Schema change, migration needed for SQLite archive
   - Board columns could be colour-coded by priority

4. **Labels / tags** — many-to-many with `TaskLabel` table

5. **Due date reminders** — background worker checks overdue tasks,
   emits SignalR `TaskOverdue` event → toast in frontend

6. **Bulk operations** — select multiple, close/reopen/delete in one action

### Planned by User (UX changes)

> User indicated UX changes are coming — to be detailed when work starts.
> Likely candidates based on context:
> - Drag handle UX refinement (current `⠿ drag` text label is functional but rough)
> - Recently Closed section edit capability (currently no Edit button)
> - Visual improvements to board columns and card styling

### AI / Integration Layer

7. **AI agent MVC layer with gRPC**
   - Separate service that can create/update/query tasks via a gRPC interface
   - Integrates with Gwen orchestration system

---

## Known Gotchas

| Problem | Solution |
|---------|----------|
| `dotnet` not on default PATH | Always: `export HOME=/home/fazzo && export DOTNET_ROOT="$HOME/.dotnet" && export PATH="$DOTNET_ROOT:$PATH"` |
| EF Core / OpenAPI / Mvc.Testing auto-install .NET 10 package | Pin versions to `9.*` in csproj |
| FluentValidation DI | Requires separate `FluentValidation.DependencyInjectionExtensions` package |
| `WebApplicationFactory<Program>` .NET 9 | Use `WithWebHostBuilder`, not `WithWebApplicationBuilder` |
| @dnd-kit v6 `DragEndEvent` type | Not cleanly exported; use inline type `{ active: { id: string\|number }; over: ... }` |
| Nanoid NuGet v3 | API incompatible with v2; replaced with custom `NanoIdGenerator` |
| TaskArchiveWorker during tests | Logs SQLite errors on test teardown; suppressed via appsettings.Testing.json |

---

## Workflow Protocols

### Git
- **Branch**: all work happens on `dev`; user merges to `main`
- **No commits without explicit user authorization**
- **Stash**: always `git stash push --message "..." -- file1 file2` (never bare `git stash`)
- **Stash lifecycle**: use `apply` not `pop`; user deletes stashes after push

### Troubleshooting
1. Create `.claude/issues/{timestamp}-{slug}.md` BEFORE starting any investigation
2. Show all command output — never suppress during troubleshooting
3. Update issue file with findings and mark RESOLVED / WORKAROUND when done

### Testing
- Run backend: `dotnet test` from repo root
- Run frontend: `cd client && npm test`
- Frontend watch mode: `npm run test:watch`

---

## Files Quick Reference

```
src/LemonTodo.Api/Properties/launchSettings.json  ← ports (API on 5175)
src/LemonTodo.Api/appsettings.Testing.json        ← suppressed logging for tests
src/LemonTodo.Api/archive.db                      ← SQLite archive store
client/src/api/client.ts                          ← BASE_URL (must match launchSettings)
client/src/api/signalr.ts                         ← SignalR hub URL (must match launchSettings)
client/src/test/mocks/handlers.ts                 ← MSW mock data + reset helper
client/src/test/setup.ts                          ← Vitest global setup (MSW lifecycle)
```
