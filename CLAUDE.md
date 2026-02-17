# LemonTodo - Project Context for Claude Code

## Environment

- **.NET SDK 9.0.311** installed at `~/.dotnet/` (via dotnet-install script, NOT apt)
- **Node.js 25.x** with npm 11.x
- **Ubuntu 22.04** — no system-level .NET; VS Code C# Dev Kit configured to use `~/.dotnet`
- Always set PATH: `export HOME=/home/fazzo && export DOTNET_ROOT="$HOME/.dotnet" && export PATH="$DOTNET_ROOT:$PATH"`

## Architecture

Clean Architecture with 4 layers + React frontend:

```
LemonTodo.Domain        → Entities, enums, interfaces, domain events (zero deps)
LemonTodo.Application   → Services, DTOs, FluentValidation, mapping (depends: Domain)
LemonTodo.Infrastructure → EF Core, repositories, channels, ID gen (depends: Domain, Application)
LemonTodo.Api           → Minimal API, SignalR hub, workers (depends: Application, Infrastructure)
```

### Dual-Store Pattern
- **ActiveDbContext** (EF Core InMemory) — active tasks, volatile on restart
- **ArchiveDbContext** (EF Core SQLite at `src/LemonTodo.Api/archive.db`) — closed/archived tasks, persistent
- On task close: event published to `System.Threading.Channels` → `TaskArchiveWorker` moves task from InMemory → SQLite

### Key Domain Rules
- `TodoTask` entity uses **factory pattern** (`TodoTask.Create(...)`) with private setters
- **State machine**: Open→Closed, Closed→Reopened, Reopened→Closed. All other transitions throw `InvalidTransitionException`
- ID: NanoID-style 21-char string (custom generator, no external package)
- Name max 200 chars, Description max 2000 chars

## Project Structure

```
src/LemonTodo.Domain/
  TodoTask.cs                    # Rich domain entity
  TodoTaskStatus.cs              # Enum: Open, Closed, Reopened
  Events/DomainEvent.cs          # TaskClosedEvent, TaskReopenedEvent
  Exceptions/InvalidTransitionException.cs
  Interfaces/                    # IActiveTaskRepository, IArchiveTaskRepository, ITaskEventChannel, IIdGenerator

src/LemonTodo.Application/
  DTOs/TaskDtos.cs               # CreateTaskRequest, UpdateTaskRequest, TaskResponse, PagedResponse<T>
  Interfaces/                    # ITaskService, IArchiveService
  Mapping/TaskMappingExtensions.cs
  Services/TaskService.cs        # CRUD + close (publishes event) + reopen
  Services/ArchiveService.cs     # Search + restore (moves archive→active)
  Validators/                    # FluentValidation: CreateTaskValidator, UpdateTaskValidator

src/LemonTodo.Infrastructure/
  Data/ActiveDbContext.cs         # InMemory
  Data/ArchiveDbContext.cs        # SQLite
  Data/TodoTaskConfiguration.cs   # Shared EF config
  Repositories/ActiveTaskRepository.cs
  Repositories/ArchiveTaskRepository.cs
  Channels/TaskEventChannel.cs    # System.Threading.Channels wrapper
  IdGeneration/NanoIdGenerator.cs

src/LemonTodo.Api/
  Program.cs                     # Composition root (DI, CORS, OpenAPI, SignalR)
  Endpoints/TaskEndpoints.cs     # /api/tasks CRUD + close/reopen
  Endpoints/ArchiveEndpoints.cs  # /api/archive search + restore
  Hubs/TaskHub.cs                # SignalR hub at /hubs/tasks
  Workers/TaskArchiveWorker.cs   # BackgroundService: channel→archive→SignalR notify

client/                          # React 19 + TypeScript + Vite
  src/api/client.ts              # Base fetch wrapper
  src/api/taskApi.ts             # taskApi + archiveApi
  src/api/signalr.ts             # SignalR connection + event handlers
  src/hooks/                     # useTasks, useArchive, useSignalR (TanStack Query)
  src/components/Board/          # BoardView, BoardColumn, DraggableCard (@dnd-kit)
  src/components/List/           # ListView
  src/components/Archive/        # ArchiveView with search + pagination
  src/components/Task/           # TaskCard, TaskModal (create/edit)
  src/components/Layout/         # Header with view toggle
```

## API Contract

| Method | Route | Status Codes |
|--------|-------|-------------|
| GET | `/api/tasks` | 200 |
| GET | `/api/tasks/{id}` | 200, 404 |
| POST | `/api/tasks` | 201 (Location header), 400 (validation) |
| PUT | `/api/tasks/{id}` | 200, 400, 404 |
| PATCH | `/api/tasks/{id}/close` | 200, 404, 409 (invalid transition) |
| PATCH | `/api/tasks/{id}/reopen` | 200, 404, 409 |
| GET | `/api/archive?q=&page=&pageSize=` | 200 |
| GET | `/api/archive/{id}` | 200, 404 |
| PATCH | `/api/archive/{id}/restore` | 200, 404, 409 |

SignalR hub: `/hubs/tasks` — events: `TaskClosed`, `TaskRestored`, `TaskUpdated`

## Tests (68 total)

- `tests/LemonTodo.Domain.Tests/` (25) — entity creation, state transitions, validation
- `tests/LemonTodo.Application.Tests/` (21) — services (NSubstitute mocks), validators, mapping
- `tests/LemonTodo.Infrastructure.Tests/` (14) — repo CRUD, search/pagination, channel pub/sub
- `tests/LemonTodo.Api.Tests/` (8) — integration tests via WebApplicationFactory

Run: `dotnet test` from repo root

## NuGet Packages (key ones)

- `Microsoft.EntityFrameworkCore.InMemory` 9.x — active store
- `Microsoft.EntityFrameworkCore.Sqlite` 9.x — archive store
- `FluentValidation` 12.x + `FluentValidation.DependencyInjectionExtensions` — request validation
- `Microsoft.AspNetCore.OpenApi` 9.x + `Scalar.AspNetCore` — API docs at `/scalar/v1`
- `FluentAssertions` 8.x, `NSubstitute` 5.x, `Microsoft.AspNetCore.Mvc.Testing` 9.x — testing

## npm Packages (key ones)

- `@tanstack/react-query` — server state
- `@microsoft/signalr` — real-time
- `@dnd-kit/core` + `@dnd-kit/sortable` + `@dnd-kit/utilities` — drag-and-drop
- `sonner` — toast notifications

## Ports

- API: `http://localhost:5062` (see `src/LemonTodo.Api/Properties/launchSettings.json`)
- Frontend: `http://localhost:5173` (Vite default)
- CORS configured for localhost:5173

## Iteration 2 Plans (not yet implemented)

- User accounts and authentication (JWT)
- AI agent MVC layer with gRPC
- Persistent active store (seed from SQLite on startup)
- Task priorities, labels, due date reminders
- Bulk operations

## Git History

Clean phase-based commits on `main`:
Phase 0 (solution) → Phase 1 (domain) → Phase 2 (infra) → Phase 3 (app) → Phase 4 (api) → Phase 5 (frontend) → Phase 6 (docs)
