# Package Management

The solution uses centralized NuGet package management. All package versions live in `Directory.Packages.props`, build-wide properties live in `Directory.Build.props`, and the SDK and test runner are pinned in `global.json`.

## Rules

* **Centralized versions.** Define every package version in `Directory.Packages.props`. Do not put `<Version>` on individual `PackageReference` items, and do not add per-project `<Version>` tags.
* **`GlobalPackageReference`** is reserved for packages that apply solution-wide, such as analyzers, polyfills, and annotation sources. The current set is the `<GlobalPackageReference>` entries in `Directory.Packages.props`.
* **Keep the shipped library dependency-light.** `LookupEngine.Abstractions` must stay pure and framework-agnostic. Prefer platform APIs and existing dependencies before adding a new one.
* **Multi-targeting.** The libraries multi-target for broad compatibility, and the framework list is declared in the project files and `Directory.Build.props`. Gate framework-specific code with `#if` directives (for example, `#if NET` for .NET-only APIs) rather than dropping a target.

## Add a Dependency

1. Add the package version to `Directory.Packages.props`.
2. Add a versionless `PackageReference` to the project that uses it.
3. Keep the scope narrow and prefer the abstractions layer to stay dependency-free.

## Update Dependencies

* Renovate (`renovate.json`) manages routine version bumps. Keep updates focused and separate from feature work.
* Run the build/tests after any dependency change.
* Versioning is driven by `GitVersion.yml`.
