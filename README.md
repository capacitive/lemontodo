# LemonTodo

A full-stack to-do task management application built with .NET 9 Minimal API and React + TypeScript.

## Architecture

```
┌─────────────┐     ┌───────────────────┐     ┌──────────────────┐
│  React SPA  │────▶│  .NET Minimal API │────▶│  EF Core Stores  │
│  (Vite+TS)  │◀────│  + SignalR Hub    │     │  InMemory+SQLite │
└─────────────┘     └───────────────────┘     └──────────────────┘
```

**Clean Architecture layers:**
- **Domain** — `TodoTask` entity with state machine, domain events, interfaces (zero dependencies)
- **Application** — Services, DTOs, FluentValidation, mapping
- **Infrastructure** — EF Core dual-context (InMemory for active, SQLite for archive), repositories, event channel
- **Api** — Minimal API endpoints, SignalR hub, background archive worker

**Frontend:**
- React 19 + TypeScript + Vite
- TanStack Query for server state management
- @dnd-kit for drag-and-drop board view
- @microsoft/signalr for real-time updates
- sonner for toast notifications

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/) with npm

## Getting Started

### Backend

```bash
# From repo root
dotnet restore
dotnet build
dotnet test     # Run all 80 backend tests
npm test        # Run all 74 UI tests

# Start the API server
dotnet run --project src/LemonTodo.Api
# API: http://localhost:5062
# OpenAPI docs: http://localhost:5062/scalar/v1
```

### Frontend

```bash
cd client
npm install
npm run dev
# UI: http://localhost:5173
```

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/tasks` | List all active tasks |
| `GET` | `/api/tasks/{id}` | Get task by ID |
| `POST` | `/api/tasks` | Create task (201 + Location) |
| `PUT` | `/api/tasks/{id}` | Update task name/description/date |
| `PATCH` | `/api/tasks/{id}/close` | Close task (triggers archival) |
| `PATCH` | `/api/tasks/{id}/reopen` | Reopen a closed task |
| `GET` | `/api/archive?q=&page=&pageSize=` | Search archived tasks |
| `GET` | `/api/archive/{id}` | Get archived task |
| `PATCH` | `/api/archive/{id}/restore` | Restore archived task to active |

**SignalR Hub:** `/hubs/tasks` — pushes `TaskClosed`, `TaskRestored`, `TaskUpdated` events.

## Data Model

**TodoTask** — rich domain entity with private setters and factory method:
- `Id` (NanoID-style, 21 chars)
- `Name` (string, max 200)
- `Description` (string?, max 2000)
- `CompletionDate` (DateOnly)
- `Status` (Open | Closed | Reopened)
- `CreatedAt`, `ClosedAt?`, `ReopenedAt?`

**State machine:** Open → Closed, Closed → Reopened, Reopened → Closed. Invalid transitions return 409.

## Event-Driven Archival

1. Close task → marks Closed in active InMemory store → publishes `TaskClosedEvent` to Channel
2. `TaskArchiveWorker` (BackgroundService) reads from Channel → moves task from InMemory to SQLite
3. SignalR notifies all connected clients → TanStack Query cache invalidated → UI updates

## Frontend Views

- **Board** (default) — Trello-style columns: Open, Reopened, Closed. Drag tasks between columns.
- **List** — Two-section layout: Active tasks + Recently closed
- **Archive** — Search input + paginated results + Restore button

## Tests

68 tests across 4 projects:
- **Domain** (25) — Entity creation, state transitions, validation rules
- **Application** (21) — Service methods with mocked repos, validators, mapping
- **Infrastructure** (14) — Repository CRUD, search/pagination, channel pub/sub
- **Api** (8) — Integration tests: endpoint contracts, status codes, validation

```bash
dotnet test --verbosity normal
```

## Project Structure

```
lemontodo/
├── LemonTodo.sln
├── src/
│   ├── LemonTodo.Domain/          # Entities, enums, interfaces
│   ├── LemonTodo.Application/     # Services, DTOs, validators
│   ├── LemonTodo.Infrastructure/  # EF Core, repositories, channels
│   └── LemonTodo.Api/             # Endpoints, SignalR, workers
├── tests/
│   ├── LemonTodo.Domain.Tests/
│   ├── LemonTodo.Application.Tests/
│   ├── LemonTodo.Infrastructure.Tests/
│   └── LemonTodo.Api.Tests/
├── client/                        # React + TypeScript + Vite
│   └── src/
│       ├── api/                   # HTTP client, SignalR
│       ├── hooks/                 # TanStack Query hooks
│       ├── components/            # Board/, List/, Archive/, Task/, Layout/
│       └── types/
└── docs/
```

## Assumptions & Trade-offs

- **InMemory = volatile** — Active tasks are lost on API restart. Acceptable for iteration 1; future: seed from SQLite on startup.
- **System.Threading.Channels over MediatR** — Simpler for single producer-consumer; would add MediatR if event taxonomy grows.
- **Manual DTO mapping** — Transparent, type-safe, no AutoMapper magic.
- **No authentication** — Planned for iteration 2 (current work in progress on the 'user-account' branch in the GitHub).
- **Inline styles** — Fast iteration; a real app would use CSS modules or Tailwind.

## Future Features

- User accounts with OAuth2 (Google and GitHub), 2FA, and API key authorization, with an account management dashboard.
- AI agent MVC layer with gRPC - agents can use this visual kanban to enable human-in-the-loop (HITL) interactions.
- Persistent active store (switch to scalable, performant and robust cloud data store)
- Task priorities, categories and labels - almost Jira-like features for a wide range of use cases.
- Due date reminders (email and/or text) - an essential feature included in most productivity apps.
