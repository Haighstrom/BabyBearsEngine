# BabyBearsEngine — Claude Instructions

Read and follow all rules in `C:\Users\simon\.claude\CLAUDE.md`. Any rules defined locally in this file take priority over the global rules if they conflict.

## Unit Tests

Write unit tests for everything you build. Tests live in `Tests/Unit/`. Follow the patterns established in existing test files (MSTest, `FakeMouse`/`FakeTarget` style fakes, `[TestInitialize]`/`[TestCleanup]` setup).

## Assembly Metadata

Assembly metadata (Product, Description, Authors, Copyright, Version) lives in the `<PropertyGroup>` of `BabyBearsEngine/BabyBearsEngine.csproj`. The `<Version>` and `<Copyright>` year may need periodic updating — flag this when working on release-related changes or when the user mentions cutting a new version.
