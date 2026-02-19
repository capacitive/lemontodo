# InProgress State & Lemon Kanban - Implementation Plan

## Overview

Added an `InProgress` state to the task lifecycle, transforming the 3-column kanban board (Open/Reopened/Closed) into a 4-column board (Open/In Progress/Reopened/Closed). Renamed the application from "LemonTodo" to "Lemon Kanban".

## State Machine

### Before
```
Open ──[Close()]──> Closed ──[Reopen()]──> Reopened ──[Close()]──> Closed
```

### After
```
Open ──[Start()]──> InProgress ──[Close()]──> Closed ──[Reopen()]──> Reopened ──[Start()]──> InProgress
```

### Valid Transitions
| From | Action | To |
|------|--------|----|
| Open | Start() | InProgress |
| InProgress | Close() | Closed |
| Closed | Reopen() | Reopened |
| Reopened | Start() | InProgress |

### Button Labels
| Status | Button | Label |
|--------|--------|-------|
| Open | Start | "Start" |
| InProgress | Close | "Done" |
| Closed | Reopen | "Reopen" |
| Reopened | Start | "Start" |

## Changes Made

### Domain Layer
- `TodoTaskStatus.cs` - Added `InProgress` enum value
- `TodoTask.cs` - Added `Start()` method, `StartedAt` property; `Close()` now only valid from `InProgress`
- `Events/DomainEvent.cs` - Added `TaskStartedEvent` record

### Application Layer
- `DTOs/TaskDtos.cs` - Added `StartedAt` to `TaskResponse`
- `Mapping/TaskMappingExtensions.cs` - Maps `StartedAt`
- `Interfaces/ITaskService.cs` - Added `StartAsync()`
- `Services/TaskService.cs` - Implemented `StartAsync()`

### API Layer
- `Endpoints/TaskEndpoints.cs` - Added `PATCH /api/tasks/{id}/start` endpoint

### Frontend
- `types/index.ts` - Added `'InProgress'` to status union
- `api/taskApi.ts` - Added `start()` method
- `hooks/useTasks.ts` - Added `useStartTask()` hook
- `Layout/Header.tsx` - Title changed to "Lemon Kanban"
- `Board/BoardView.tsx` - 4 columns, updated drag logic
- `Board/BoardColumn.tsx` - Added `onStart` prop
- `Board/DraggableCard.tsx` - Passes `onStart` through
- `Task/TaskCard.tsx` - New button labels, InProgress color (amber)
- `List/ListView.tsx` - Added Start action, renamed "Recently Closed" to "Recently Done"

### Tests (80 total, up from 68)
- **Domain (33)**: Added `TodoTaskStartTests` (5 tests), `TodoTaskFullLifecycleTests` (1), updated Close/Reopen tests
- **Application (23)**: Added `Start_SetsInProgressStatus`, updated Close/Reopen/Archive tests
- **Infrastructure (14)**: Updated `CreateClosedTask` helper
- **API (10)**: Added `StartTask_Returns200`, `CloseOpenTask_Returns409`, updated `CloseTask_AfterStart_Returns200`

## API Contract Update

| Method | Route | Status Codes |
|--------|-------|-------------|
| PATCH | `/api/tasks/{id}/start` | 200, 404, 409 |

All other endpoints unchanged.
