# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

WebServiceToolkit is a suite of NuGet packages providing ASP.NET Core utilities: query model binding, HTTP exception handling, attribute-based DI registration, and pagination/query model contracts. It is a **library**, not an application — there is no app to run; verification is done through unit tests.

## Commands

Targets **.NET 10** (`net10.0`). All commands run from the repo root.

```bash
dotnet build DevInstance.WebServiceToolkit.sln          # build all projects
dotnet test DevInstance.WebServiceToolkit.sln           # run all tests
dotnet pack -c Release                                  # produce .nupkg (also auto-generated on build)
```

Run a single test or a filtered subset (xUnit via `dotnet test --filter`):

```bash
dotnet test --filter "FullyQualifiedName~ModelListTests"
dotnet test --filter "DisplayName~IsEmpty"
```

Note: the Azure pipelines (`azure-pipeline-ci.yml`, `azure-pipeline-release.yml`) still pin `UseDotNet@2` to `9.x` even though the projects target `net10.0` — the SDK version there lags the project target framework.

## Architecture

Three layered packages (in `src/`) plus a test project (in `tests/`). The dependency direction is **Common ← Database ← main**; the main package depends on the other two **via published NuGet `PackageReference`** (see `DevInstance.WebServiceToolkit.csproj`), *not* project references. Consequently, a local change to Common or Database is **not** picked up by the main project until that dependency is published and the version bumped — keep this in mind when editing across package boundaries.

### `DevInstance.WebServiceToolkit.Common`
Pure POCOs and attributes with **no ASP.NET Core dependency** (so it can be shared with non-web/client code). Contains the `[QueryModel]` / `[QueryName]` marker attributes, `ModelItem`/`ModelList<T>` DTOs, and `IdGenerator`. This is the only package currently covered by tests.

### `DevInstance.WebServiceToolkit.Database`
Interface-only contracts (`IModelQuery<T,D>`, `IQPageable<T>`, `IQSearchable<T,K>`, `IQSortable<T>`) for building fluent, composable query objects, plus extension methods like `Paginate`. There is no EF Core or DB-engine dependency here — consumers implement these interfaces against their own data layer. The generic `D` parameter in `IModelQuery<T,D>` exists to let fluent methods (`Clone`, etc.) return the concrete query type for chaining.

### `DevInstance.WebServiceToolkit` (main)
The ASP.NET Core integration layer (`FrameworkReference` on `Microsoft.AspNetCore.App`). Three concern areas:

- **Query model binding** (`Http/Query/`): `QueryModelBinder` is the standalone reflection-based engine that parses an `HttpRequest` query string into a `[QueryModel]`-marked POCO — handles nullables, enums, GUIDs, dates, comma-separated collections/arrays, `[DefaultValue]`, and `[QueryName]` overrides, falling back to `TypeConverter`. The MVC plumbing wraps it: `QueryModelBinderProvider` → `QueryModelMvcBinder` (calls `TryBind<T>` via reflection and pushes errors into `ModelState`), wired up by `RegistrationExtensions.AddWebServiceToolkitQuery()`. The provider is **inserted at index 0** of `ModelBinderProviders` so it runs before the default `FromQuery` binder.

- **Exception handling** (`Controllers/`, `Exceptions/`): Domain exceptions (`RecordNotFoundException`→404, `RecordConflictException`→409, `UnauthorizedException`→401, `BadRequestException`→400) are mapped to HTTP responses by the `ControllerUtils.HandleWebRequestAsync<T>` / `HandleWebRequest<T>` extension methods. The intended controller pattern is to wrap action bodies in `this.HandleWebRequestAsync<T>(async () => { ... throw RecordNotFoundException(id); ... })` rather than writing per-action try/catch.

- **Service registration** (`Tools/`): `AddServerWebServices()` / `AddServerWebServicesMocks()` scan the **calling assembly** (via `Assembly.GetCallingAssembly()`) for classes marked `[WebService]` / `[WebServiceMock]` and register each as **scoped**, bound to every interface it implements (or to itself if it has none). `[WebServiceMock]` is the parallel mechanism for swapping in test/mock implementations.

## Conventions

- Versions are set **per-project** in each `.csproj` (`<Version>`), not centrally — they drift independently (e.g. Common/main at `10.2.0`, Database at `10.0.2`). When publishing, bump the changed package and, if the main package consumes it, update the corresponding `PackageReference` version in `DevInstance.WebServiceToolkit.csproj`.
- `GeneratePackageOnBuild` is `True`, so every build produces `.nupkg` artifacts.
- Public APIs carry thorough XML doc comments with `<example>` blocks — match this style when adding public members.
- `ModelList<T>` carries several `[Obsolete]` properties (`SortBy`, `IsAsc`, `Filter`, `Fields`); prefer the `SortOrder` string-array convention (`"+Name"` ascending, `"-Name"` descending, array order = sort priority) for new code.
- User-facing docs live in `docs/` (one markdown file per feature) and the root `README.md` — update these when changing public behavior.
