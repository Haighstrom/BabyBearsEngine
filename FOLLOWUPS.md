# Follow-ups

Open issues found during refactor / docs / tests work but deliberately deferred. Each entry lists where the bug lives, what's wrong, why it wasn't fixed in-line, and a suggested fix.

---

## `Matrix3` struct-copy shares the underlying `float[]`

- **File:** [BabyBearsEngine/Source/Geometry/Matrix3.cs](BabyBearsEngine/Source/Geometry/Matrix3.cs)
- **What's wrong:** `Matrix3` is a `struct` containing a single `private float[] _values` reference. Struct copy duplicates the reference, not the array, so two "copies" of the same matrix mutate each other:
  ```csharp
  var a = someMatrix;
  var b = a;       // b._values and a._values point to the same array
  b[0, 0] = 5;     // also mutates a
  ```
  This violates value-type semantics. Already triggered one cascade-bug: the `Identity`/`Zero`/`FlipXMatrix`/`FlipYMatrix` static fields were corrupted process-wide by any indexer-set on a "copy" — fixed by converting them to properties so each access yields a fresh array. The deeper aliasing problem remains for arbitrary copies.
- **Why deferred:** Bigger scope. The clean fix is to replace `float[] _values` with 9 individual `float` fields (m0..m8). Touches every method body in the file.
- **Suggested fix:** Switch to 9 named `float` fields. Keep `Values` as a getter that allocates a new array per call (or remove it — callers don't seem to need raw-array access). Update every method body to read/write the named fields directly. Same change should be considered for `Matrix2` and `Matrix4` if they have the same shape (likely).

## `World.Update` doesn't skip inactive children

- **File:** [BabyBearsEngine/Source/Worlds/World.cs](BabyBearsEngine/Source/Worlds/World.cs#L52)
- **What's wrong:** `ContainerEntity.Update` skips children where `Active == false`; `World.Update` doesn't. Inconsistent. Currently pinned by test `WorldTests.Update_DoesNotSkipInactiveChildren_CurrentBehaviour`.
- **Why deferred:** Behaviour change — could affect existing games that rely on the current semantics (unlikely but possible). Consistency call is the user's.
- **Suggested fix:** Mirror the `if (!entity.Active) continue;` check from `ContainerEntity.Update`. Update or remove the regression test pinning the current behaviour.

## `BMPTextGraphic` contains placeholder/stub vertex code

- **File:** [BabyBearsEngine/Source/Worlds/Graphics/Text/BMPTextGraphic.cs](BabyBearsEngine/Source/Worlds/Graphics/Text/BMPTextGraphic.cs#L142)
- **What's wrong:** `SetVerticesSimple` has a hardcoded test bear texture, hardcoded vertex coords (`new Vertex(0, 0, ...)`, `new Vertex(200, 0, ...)`), and a `break;` after the first character that makes the rest of the loop unreachable (CS0162). The `_colour` field is never used. The font/text rendering pipeline is half-done.
- **Why deferred:** Real work, not a quick fix. Scope wasn't part of the OpenTK isolation or docs/tests plans.
- **Suggested fix:** Treat as a feature — schedule a "finish text rendering" pass. In the meantime, the type's public API (`Colour`, `Text`, `X`/`Y`/`Width`/`Height` properties) is stable and OpenTK-clean, so consumers can interact with it even though rendering is broken.

## CA1812 warnings on Demo `*World` classes

- **Files:** all under [Demos/Source/Demos/](Demos/Source/Demos/)
- **What's wrong:** Build emits ~12 CA1812 warnings about internal classes never instantiated (e.g. `BearSpinnerWorld`, `CameraDemoWorld`, `KeyboardDemoWorld`). The classes ARE instantiated — but reflectively or via factories the analyzer can't see.
- **Why deferred:** Pre-existing; not introduced by recent work.
- **Suggested fix:** Either suppress CA1812 project-wide for `Demos.csproj`, or annotate each demo class with `[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by demo selector")]`. Project-wide suppression is the lighter touch.

## `Rect` round-trip via `ToString` / `Rect(string)` ctor

- **Status:** Resolved during Tier 1.2.
- *(Kept here as a note that the round-trip works now: `{X=1,Y=2,W=3,H=4}` format, `InvariantCulture`, no fixed decimal places.)*

## Brace style sweep paused

- **Files:** 12 source files still have single-line or missing-brace if/else/for/while bodies.
- **Status:** Paused mid-sweep. Files completed so far: `Colour.cs`, `Randomisation.cs`, `Rect.cs`, `Matrix3.cs`, `Matrix2.cs` (partly — verify before continuing).
- **Suggested fix:** Resume the mechanical sweep. The grep pattern from the original survey: `^\s+(if|while|for|foreach|else if) \(.+\)\s*$` followed by a non-`{` next line catches the multi-line variant; `^\s+(if|while|for|foreach|else if) \(.+\) [^{].*;\s*$` catches the single-line variant.
