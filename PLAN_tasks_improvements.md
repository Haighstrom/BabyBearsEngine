# Tasks system — improvements

The Tasks module was ported faithfully from BearsEngine. This plan
captures opportunities to tighten it up before HappyBlacksmith starts
leaning on it heavily (HB has 40+ Task subclasses; rough edges multiply).

## Status snapshot

- `ITask` / `Task` / `TaskGroup` / `ITaskController` / `TaskController`.
- `Task.Update` starts the task on first call; reports `IsComplete` when
  all `CompletionConditions` are true; `TaskController` is the thing that
  notices completion and calls `Complete()`.
- Tasks chain via `ITask.NextTask` (linked-list pattern).
- 18 unit tests; behaviour is solid for the happy path.

## Worth doing soon — small, reinforcing changes

### 1. Auto-complete in `Update`

Currently the lifecycle is two-step: a task's `IsComplete` flips true, and
later something else (the `TaskController`) calls `Complete()`. That's an
extra hop for every consumer and creates a "done but not finalised"
in-between state.

Fold the check into `Update`:

```csharp
public virtual void Update(double elapsed)
{
    if (!_isStarted) Start();
    if (IsComplete) Complete();
}
```

Side effect: `TaskController.Update` shrinks to "update current task,
advance the chain when `CurrentTask` is null" — no more
`if (task.IsComplete) task.Complete()` orchestration.

### 2. Drop the redundant `_isStarted = true` in `Update`

```csharp
if (!_isStarted)
{
    Start();
    _isStarted = true;   // Start() already did this
}
```

The double-assignment was an old "in case Start is overridden" guard. A
subclass that overrides `Start` without calling `base.Start()` is broken
either way (no `TaskStarted` event), so the belt-and-braces masks the bug
rather than fixing it. Drop the second assignment; trust `Start`.

### 3. Single bail-out at the top of `TaskController.Update`

Two near-identical guards exist today:

```csharp
if (Parent is IAddable parentAsAddable && parentAsAddable.Parent is null)
    return;
// ...
if (Parent is IAddable a && a.Parent is null)
    return;
```

They protect against "a task's `Complete` action removed our entity from
the world". Cleaner shape: check once at the top of `Update` and bail for
the rest of the frame.

```csharp
public virtual void Update(double elapsed)
{
    if (Parent is null) return;
    if (Parent is IAddable a && a.Parent is null) return;
    // ...rest of update logic
}
```

Worst case: a task's complete-action detaches the entity mid-frame and we
finish this frame's update on a detached entity. Next frame's bail-out
catches it; lost no real correctness.

### 4. Virtual `OnReset` hook

`Task.Reset()` only clears `_isStarted`. Real subclasses with state
(timers, counters, "waiting for X to arrive") need to reset their own
fields too — and right now have to override `Reset` and remember to call
`base.Reset()`.

Mirror the `AddableRectBase.OnSizeChanged` pattern: `Reset` does the
lifecycle bookkeeping and calls `OnReset` for subclass cleanup.

```csharp
public void Reset()
{
    _isStarted = false;
    OnReset();
}

protected virtual void OnReset() { }
```

Subclasses override `OnReset`, don't have to remember `base.Reset()`.

## Worth offering as a convenience

### 5. Inline factories for trivial tasks

HB will have 40+ Task subclasses. A lot of them are probably small —
"run this action", "wait for this condition", "wait N seconds". A static
helper class would let those be expressed inline:

```csharp
public static class Tasks
{
    public static ITask Run(Action action) => /* ... */;
    public static ITask WaitFor(Func<bool> condition) => /* ... */;
    public static ITask Delay(double seconds) => /* ... */;

    public static ITask Then(this ITask first, ITask second)
    {
        first.NextTask = second;
        return first;
    }
}

// usage:
Tasks.Run(OpenDoor).Then(Tasks.Delay(1)).Then(Tasks.Run(WalkIn));
```

Named subclasses still pay off when the behaviour is reused or genuinely
domain-specific (`CarryItemTask`, `CraftAtAnvilTask`). The factories just
remove the ceremony for the throwaway cases.

Build this when HB migration starts so the factory shapes are informed by
what HB's small tasks actually need.

## Worth a wider conversation, not now

### 6. List-owned chains instead of linked-list `NextTask`

The current model wires tasks together by mutating `task.NextTask`. Two
brittle modes:

- **Accidental sharing.** If task `A` is at the start of two chains, both
  chains mutate `A.NextTask` and corrupt each other. There's no way to
  reuse a task across groups today.
- **Mid-flight chain mutation.** Setting `NextTask` while the chain is
  running gives surprising results depending on timing.

Alternative: `TaskGroup` and `TaskController` own a `List<ITask>`
representing upcoming tasks. `NextTask` becomes a computed read-only
view. Tasks are naturally reusable.

Significant call-site change (every chain construction moves from "wire
pointers" to "build list"). Defer until something concrete bites.

### 7. First-class `Cancel` / `Abort`

Right now tasks complete (via conditions) or stop implicitly (their
entity got removed). There's no clean "user cancelled" or "skip this
task" verb. HB probably has this as an ad-hoc `CompletionCondition`
flip; making it first-class would tidy it up:

```csharp
public interface ITask
{
    ...
    void Cancel();   // run ActionsOnCancel (if any), don't run ActionsOnComplete
    event EventHandler? TaskCancelled;
}
```

Pairs with #6 (a cancelled task is removed from the group's list rather
than mutating itself). Defer.

## Skip

- **Async / coroutines (`await`-style).** Tempting, but a whole different
  mental model. The imperative per-frame-stepped pattern works well in a
  game loop where you want determinism, pausing, and visible state.
- **Behaviour trees / state machines.** Bigger redesign for a different
  feature set. The current model handles HB's sequential AI fine.

## Suggested order

One focused pass:

1. Tighten `Task.Update` (items 1 + 2 — they touch the same lines).
2. Tighten `TaskController.Update` (item 3 — same file, naturally
   follows once auto-complete moves into Task).
3. Add `OnReset` hook (item 4 — separate from the others, trivially safe).
4. Add unit tests for the new behaviours (especially that
   `Update`-driven auto-complete works without a controller, and that
   `OnReset` is called by `Reset`).

Then ship #5 (inline factories) when HB starts to reveal what shapes
matter. Items 6 and 7 stay in the freezer until needed.

## Risks / call-site impact

- **Item 1 changes when `TaskCompleted` fires.** Today it fires only when
  someone explicitly calls `Complete()`; after the change it fires
  automatically the first `Update` after conditions are met. Any HB code
  that depends on the explicit-call timing would need a look. Worth
  flagging in the migration notes.
- **Item 3 widens the bail-out window.** Currently the controller bails
  AT specific points; after the change it commits to the whole update
  once started. In practice the difference is one frame.
- **Items 2 and 4 are pure cleanups with no behaviour change.**
