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

If multiple uncommitted changes are still being accumulated into one pending commit (i.e. the previous batch hasn't been committed yet), only bump once for the whole batch — do not bump again on each follow-up tweak.
