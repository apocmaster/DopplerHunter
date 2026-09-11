# AGENTS.md

## Overview

- WPF desktop app (.NET 10, C#, MVVM) that scans folders and finds duplicate files by content hash. Windows-only (`net10.0-windows`).
- Root `DopplerHunter.slnx` is the **XML solution format** (not `.sln`); build/test against it:
  - `DopplerHunter/` — the WPF app.
  - `DopplerHunter.Tests/` — xUnit **v3** (`xunit.v3` package) + NSubstitute for fakes.

## Commands (Windows only)

- Build: `dotnet build DopplerHunter.slnx`
- Test all: `dotnet test DopplerHunter.slnx`
- Single test class: `dotnet test DopplerHunter.slnx --filter "FullyQualifiedName~DirectoryServicesTests"`
- Requires .NET 10 SDK. Build currently emits ~10 warnings (CS8625/CS8509); they are not treated as errors — don't rabbit-hole fixing pre-existing ones.
- CI (`.github/workflows/ci.yml`) triggers **only on PRs targeting `develop`**: `dotnet restore` → build → test on `windows-latest`.

## Architecture

- **No DI container.** Services (`IDriveService`, `IFileService`, `IDirectoryService`) are hand-wired in `MainViewModel`: a parameterless ctor constructs the real services (this is what `MainWindow.xaml`'s inline `<local:MainViewModel/>` DataContext uses) and an overload accepting all three exists for tests via NSubstitute. Follow this pattern when adding services.
- Duplicate detection pipeline lives in `FileService`, driven by `MainViewModel.OnScanForDuplicatesCommand`: group by size → hash → `MarkFilesDuplicates` → `GroupFilesDuplicated`. `DuplicateDetectionService.cs` is fully commented-out dead code — don't add to it.
- Hashing is size-based in `FileService.ComputeHashAsync`: MD5 < 50 MB, `XxHash64` (`System.IO.Hashing`) 50–500 MB, sampled MD5 > 500 MB.
- Results UI: `FilesFound` is wrapped in an `ICollectionView`, sorted by FileHash+FolderPath, grouped by FileHash, and filtered to duplicates-only after a scan (set in the `MainViewModel` ctor / scan command). Rows are appended with `ObservableCollectionExtensions.AddRange`.
- `FileMetadata.SelectionChanged` is a **static** event fired from the `IsSelected` setter to refresh the delete-button count label; be careful wiring instance handlers to it (lifetime/leak implications).

## Conventions

- Branch naming: `feature/N-slug`, `bug/N-slug`, `doc/N-slug` where N is the issue number. `develop` is the integration branch; `master` is the release line.
- Code uses classic classes with `#region` blocks (no file-scoped namespaces) and `Should_X_When_Y` test names. `DirectoryService` tests create and clean up real temp dirs.