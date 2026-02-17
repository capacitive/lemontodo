# Fix: Board View Edit + Test Coverage

## Context

Manual testing revealed that clicking the **Edit** button on task cards in **BoardView** is unreliable — it works in ListView but not BoardView. The root cause is that `DraggableCard.tsx` spreads the entire `{...listeners}` object (containing `onPointerDown`) onto the outer `<div>` that wraps the full `TaskCard`, including its action buttons. This causes `@dnd-kit`'s pointer sensor to intercept events that should reach the Edit/Close/Reopen buttons.

Additionally, neither `BoardView.test.tsx` nor `ListView.test.tsx` have tests for the edit flow, so the bug was never caught by tests.

## Root Cause

**`client/src/components/Board/DraggableCard.tsx`** (lines 19-27):
```tsx
<div
  ref={setNodeRef}
  style={{ transform: CSS.Translate.toString(transform) }}
  {...listeners}    // ← intercepts ALL pointer events on the card
  {...attributes}
>
  <TaskCard task={task} onClose={onClose} onReopen={onReopen} onEdit={onEdit} draggable />
</div>
```

When `{...listeners}` is spread on the container div, `@dnd-kit`'s `onPointerDown` handler captures the initial pointer press. In some configurations, this prevents `onClick` from firing on buttons inside the card. The fix is to restrict drag listeners to a **dedicated drag handle** element, leaving button clicks unaffected.

## Changes Required

### 1. Fix `DraggableCard.tsx` — Use a Drag Handle

Move `{...listeners}` from the outer div to a drag handle `<div>` placed inside the card, above the `TaskCard`. The outer div keeps `ref={setNodeRef}`, `{...attributes}`, and the transform style. Only the handle div gets `{...listeners}`.

**File:** `client/src/components/Board/DraggableCard.tsx`

Implementation:
```tsx
export function DraggableCard({ task, onClose, onReopen, onEdit }: DraggableCardProps) {
  const { attributes, listeners, setNodeRef, transform } = useDraggable({ id: task.id });

  return (
    <div
      ref={setNodeRef}
      style={{ transform: CSS.Translate.toString(transform) }}
      {...attributes}
    >
      {/* Drag handle — only this area initiates drag */}
      <div
        {...listeners}
        style={{ cursor: 'grab', padding: '4px 0', color: '#d1d5db', fontSize: '0.75rem', userSelect: 'none' }}
        title="Drag to move"
      >
        ⠿ drag
      </div>
      <TaskCard task={task} onClose={onClose} onReopen={onReopen} onEdit={onEdit} draggable />
    </div>
  );
}
```

### 2. Add Edit Tests to `BoardView.test.tsx`

**File:** `client/src/components/Board/BoardView.test.tsx`

Add two tests:
- **"should open edit modal when Edit button clicked on a task"**
  Wait for tasks to load → click the first Edit button → verify "Edit Task" modal heading appears
- **"should pre-fill edit modal with task data"**
  Wait for tasks → click Edit on "Test Task 1" → verify input has value "Test Task 1"

### 3. Add Edit Tests to `ListView.test.tsx`

**File:** `client/src/components/List/ListView.test.tsx`

Add three tests:
- **"should open edit modal when Edit button clicked on active task"**
  Wait for tasks → click Edit button in active tasks section → verify "Edit Task" modal heading
- **"should pre-fill edit modal with task data"**
  Click Edit on "Test Task 1" → verify input has value "Test Task 1"
- **"should not show Edit button on recently closed tasks"**
  Wait for tasks → verify "Test Task 2" (Closed) card has no Edit button

## Files to Modify

| File | Change |
|------|--------|
| `client/src/components/Board/DraggableCard.tsx` | Move listeners to drag handle element |
| `client/src/components/Board/BoardView.test.tsx` | Add edit flow tests |
| `client/src/components/List/ListView.test.tsx` | Add edit flow tests |

## Verification

```bash
# Run all frontend tests — all should pass
cd client && npm test

# In browser: start API + frontend, verify:
# 1. Board cards can be dragged by grip handle
# 2. Edit button on board cards opens the modal with correct task data
# 3. Edit button on active tasks in list view opens modal
# 4. Edit button does NOT appear on Recently Closed tasks in list view
```

Expected outcome: `npm test` reports all tests passing, and Edit works in both views.
