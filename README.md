# DataPlatform24

DataPlatform24 provides the architectural foundation for shared enterprise data and licensing capabilities across TS24 products. It establishes explicit authority, dependency, provider, and deployment boundaries without coupling logical ownership to a physical database.

It is not an application business-domain library, authorization system, production database schema, or mandate for central-cloud runtime connectivity. Application-specific truth remains owned by its application.

The authoritative design is [TS24 Data Platform Architecture Baseline v0.1](docs/architecture/TS24-Data-Platform-Architecture-Baseline-v0.1.md). This repository implements only the initial skeleton; that document remains the authority.

## Project layout

- `Foundation` — technology-neutral abstractions and future transaction, concurrency, provenance, migration, and diagnostic contracts.
- `MasterData` — separate Contracts, Domain, Application, and Persistence assemblies.
- `Licensing` — separate Contracts, Domain, Application, and Persistence assemblies; licensing is not authorization.
- `Providers` — explicit MariaDB and MongoDB adapter boundaries, with no drivers selected yet.
- `Deployment` — configuration and connection-resolution boundary, without business semantics.
- `Tests` — dependency-graph architecture tests and general skeleton tests.

Each logical area currently contains marker types only. Production MasterData and Licensing models, database schema, persistence mappings, provider drivers, and ORM choices are intentionally absent and remain gated by later tasks and existing-data discovery.

## Build and test

Requires a compatible .NET 9 SDK.

```sh
dotnet restore
dotnet build
dotnet test
```
