# BabyBearsEngine — Claude Instructions

Read and follow all rules in `C:\Users\simon\.claude\CLAUDE.md`. Any rules defined locally in this file take priority over the global rules if they conflict.

## Unit Tests

Write unit tests for everything you build. Tests live in `Tests/Unit/`. Follow the patterns established in existing test files (MSTest, `FakeMouse`/`FakeTarget` style fakes, `[TestInitialize]`/`[TestCleanup]` setup).

## Assembly Metadata

Assembly metadata (Product, Description, Authors, Copyright, Version) lives in the `<PropertyGroup>` of `BabyBearsEngine/BabyBearsEngine.csproj`. The `<Copyright>` year may need periodic updating — flag this when working on release-related changes or when the user mentions cutting a new version.

## Version Bump On Every Commit

Always bump the `<Version>` in `BabyBearsEngine/BabyBearsEngine.csproj` as part of every commit's changes, before suggesting the commit message. The version is currently pre-1.0, so:

- Default: increment the **patch** (third) digit — e.g. `0.1.0` → `0.1.1`.
- For a major feature shipping in the commit: increment the **minor** (middle) digit and reset patch to 0 — e.g. `0.1.7` → `0.2.0`.
- Do **not** touch the major (first) digit — the user will say when 1.0.0 is reached.

**Patch is the default, by a wide margin.** "Major feature" means a genuinely new capability the engine couldn't do before — a whole new subsystem (audio, particles, networking), a new public API surface, a new piece of game-facing infrastructure. The bar is high. The default for almost everything is patch:

- Tweaks, refinements, or extensions to existing systems → **patch**. Even if they touch many files or change a public type signature, if they're modifying something that already exists, it's a patch. Example: changing `StartSize` from `float` to `Point` so particle quads can be stretched is a *refinement* of the existing particle system — patch. Adding a new emitter shape, a new shader effect, a new UI control variant, or a new option to an existing class — all patch.
- New demos, demo improvements, asset additions → **patch**. Demos are consumers, not engine capabilities.
- Bug fixes, performance work, code cleanup, test additions → **patch**.
- Renames, refactors, file reorganisation → **patch**.
- New shader files, new texture loaders for an existing subsystem → **patch**.

A minor bump should feel like the kind of thing that would warrant a paragraph in release notes about a new chapter for the engine — not just a line item.

If multiple uncommitted changes are still being accumulated into one pending commit (i.e. the previous batch hasn't been committed yet), only bump once for the whole batch — do not bump again on each follow-up tweak.
