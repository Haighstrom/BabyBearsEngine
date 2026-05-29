# BabyBearsEngine — Claude Instructions

Read and follow all rules in `C:\Users\simon\.claude\CLAUDE.md`. Any rules defined locally in this file take priority over the global rules if they conflict.

## Unit Tests

Write unit tests for everything you build. Tests live in `Tests/Unit/`. Follow the patterns established in existing test files (MSTest, `FakeMouse`/`FakeTarget` style fakes, `[TestInitialize]`/`[TestCleanup]` setup).

## Assembly Metadata

Assembly metadata (Product, Description, Authors, Copyright, Version) lives in the `<PropertyGroup>` of `BabyBearsEngine/BabyBearsEngine.csproj`. The `<Copyright>` year may need periodic updating — flag this when working on release-related changes or when the user mentions cutting a new version.

## Version Bump On Engine Changes Only

The `<Version>` in `BabyBearsEngine/BabyBearsEngine.csproj` describes the state of the **engine package**, not the state of the repo. Bump it only when the commit actually changes files inside `BabyBearsEngine/` (source code, shaders, assets shipped with the engine, the .csproj itself).

- Commit touches only `Demos/`, `Tests/`, `Tools/`, `Sandbox/`, `Benchmarks/`, or root-level docs → **do not bump the version**. These projects consume the engine; their changes don't change what someone integrating the engine package would receive.
- Commit touches anything under `BabyBearsEngine/` → bump the version (rules below).
- Commit touches both → bump the version once for the engine-side changes; the demo/test changes ride along without their own bump.

When bumping, the engine is pre-1.0:

- Default: increment the **patch** (third) digit — e.g. `0.1.0` → `0.1.1`.
- For a major feature shipping in the commit: increment the **minor** (middle) digit and reset patch to 0 — e.g. `0.1.7` → `0.2.0`.
- Do **not** touch the major (first) digit — the user will say when 1.0.0 is reached.

**Patch is the default, by a wide margin.** "Major feature" means a genuinely new capability the engine couldn't do before — a whole new subsystem (audio, particles, networking), a new public API surface, a new piece of game-facing infrastructure. The bar is high. The default for almost everything is patch:

- Tweaks, refinements, or extensions to existing systems → **patch**. Even if they touch many files or change a public type signature, if they're modifying something that already exists, it's a patch. Example: changing `StartSize` from `float` to `Point` so particle quads can be stretched is a *refinement* of the existing particle system — patch. Adding a new emitter shape, a new shader effect, a new UI control variant, or a new option to an existing class — all patch.
- Bug fixes, performance work, code cleanup, test additions inside `BabyBearsEngine/` → **patch**.
- Renames, refactors, file reorganisation inside `BabyBearsEngine/` → **patch**.
- New shader files, new texture loaders for an existing subsystem → **patch**.

A minor bump should feel like the kind of thing that would warrant a paragraph in release notes about a new chapter for the engine — not just a line item.

If multiple uncommitted changes are still being accumulated into one pending commit (i.e. the previous batch hasn't been committed yet), only bump once for the whole batch — do not bump again on each follow-up tweak.

**Before bumping, check `git status` to see which projects the pending diff actually touches. A diff confined to `Demos/` gets no bump even if the work is visually impressive — the engine is unchanged.**
