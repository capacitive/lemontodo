# Conditional Rendering via Default Prop Values in React

## Pattern Used

The `TaskCard` component uses an **optional boolean prop with a default value** to conditionally render the status badge.

```tsx
// In the props interface — optional with `?`
interface TaskCardProps {
  showStatus?: boolean;
  // ...
}

// In the function signature — default value via destructuring
export function TaskCard({ showStatus = true, ...rest }: TaskCardProps) {
```

Then in JSX, a **short-circuit `&&` expression** conditionally renders the element:

```tsx
{showStatus && (
  <span>...</span>
)}
```

## How It Works

1. **Optional prop (`?`)** — TypeScript allows callers to omit the prop entirely.
2. **Default parameter value (`= true`)** — JavaScript destructuring assigns `true` when the prop is `undefined` (i.e., not passed).
3. **Short-circuit rendering (`&&`)** — React skips rendering the `<span>` when `showStatus` is `false`.

## Where It's Used

- **BoardView** → `DraggableCard` → `TaskCard` with `showStatus={false}` — status badges are hidden because the column header already indicates the status.
- **ListView** → `TaskCard` with no `showStatus` prop — defaults to `true`, so badges are visible since tasks of mixed statuses appear in the same list section.

## Why This Over Other Approaches

| Approach | Trade-off |
|----------|-----------|
| **Default prop (chosen)** | Simple, no extra infrastructure. Caller opts out explicitly. |
| CSS class toggle | Requires a CSS file or class system; heavier for a single element. |
| React Context | Overkill for a single boolean flag flowing one level deep. |
| Separate components | Duplicates the entire card just to hide one element. |

The default prop pattern is idiomatic React for "show by default, hide when asked."
