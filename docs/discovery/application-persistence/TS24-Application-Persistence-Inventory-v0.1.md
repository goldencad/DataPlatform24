# TS24 Application Persistence Inventory v0.1

## Scope and method

This Level C inventory records only source and configuration present in the TASK 05
worktree at baseline `fe3eb5d6ec39f709a97c6af300af9716793f26ae`.
The architecture baseline was used as authority for task scope, not as evidence of
application persistence. Searches were limited to the named applications and
persistence terms in repository paths, source, and configuration.

**FOUND — accessible repository:** DataPlatform24 only. Its README identifies the
checkout as an initial skeleton and states that production models, database schema,
persistence mappings, provider drivers, and ORM choices are absent
([evidence](../../../README.md#L7-L18)). The repository tree contains no application
source/configuration for the target applications.

## Application accounting

| Application | Accessibility | Persistence inventory |
|---|---|---|
| TaxOnline | **NOT ACCESSIBLE** | **UNKNOWN** — no repository/source/configuration available |
| iBHXH | **NOT ACCESSIBLE** | **UNKNOWN** — no repository/source/configuration available |
| PayCalc24 | **NOT ACCESSIBLE** | **UNKNOWN** — no repository/source/configuration available |
| DigiDokument | **NOT ACCESSIBLE** | **UNKNOWN** — no repository/source/configuration available |
| ContractSigning | **NOT ACCESSIBLE** | **UNKNOWN** — no repository/source/configuration available |
| TS24 Link | **NOT ACCESSIBLE** | **UNKNOWN** — no repository/source/configuration available |
| Konect24 | **NOT ACCESSIBLE** | **UNKNOWN** — no repository/source/configuration available |
| HR24 | **NOT ACCESSIBLE** | **UNKNOWN** — no repository/source/configuration available |
| 2ez | **NOT ACCESSIBLE** | **UNKNOWN** — no repository/source/configuration available |
| Invoice applications | **NOT ACCESSIBLE** | **UNKNOWN** — no identifiable invoice application repository/source/configuration available |
| Customs applications | **NOT ACCESSIBLE** | **UNKNOWN** — no identifiable customs application repository/source/configuration available |
| Other identifiable TS24 persistent-data apps | **NOT FOUND** | No other application was identifiable in the accessible repository tree |

There are no accessible target applications. Consequently, datastore technology;
database/schema/collection; data-access path; repository/provider/data-service
patterns; direct or cross-application DB access; shared MariaDB access; MongoDB
usage; file/XML/JSON state; Konect24/API call paths; Company/Person/Employee
persistence; temporal history; destructive overwrite; revisions/snapshots; and
migration artifacts are all **UNKNOWN** for every named target.

## Accessible persistence-related context

- **NOT FOUND — CURRENT STORE:** no implemented DataPlatform24 production store.
  The MariaDB boundary is a marker that explicitly references no driver
  ([evidence](../../../src/TS24.DataPlatform.Provider.MariaDb/MariaDbProviderMarker.cs#L1-L4));
  its project references only Foundation
  ([evidence](../../../src/TS24.DataPlatform.Provider.MariaDb/TS24.DataPlatform.Provider.MariaDb.csproj#L1-L5)).
- **NOT FOUND — MongoDB usage:** the MongoDB boundary likewise explicitly
  references no driver
  ([evidence](../../../src/TS24.DataPlatform.Provider.MongoDb/MongoDbProviderMarker.cs#L1-L4));
  its project references only Foundation
  ([evidence](../../../src/TS24.DataPlatform.Provider.MongoDb/TS24.DataPlatform.Provider.MongoDb.csproj#L1-L5)).
- **NOT FOUND — CURRENT API / CALL PATH:** no target application or Konect24 code
  is present.
- **NOT FOUND — CROSS-APP DEPENDENCY:** none can be evidenced from accessible
  target application source because no target application source is available.
- **UNKNOWN — DUPLICATION CANDIDATE:** Company/Person/Employee data cannot be
  classified as live enterprise state, working payload, declaration revision,
  immutable snapshot, payroll/calculation history, or cache/projection without
  application evidence.
- **NOT FOUND — TEMPORAL EVIDENCE / SNAPSHOT EVIDENCE:** none in accessible target
  application source; no target application source is available.
- **NOT FOUND — migration artifacts:** none in the accessible repository tree.

## Risks and limits

- **RISK:** application persistence remains unverified because all named application
  repositories are outside the accessible workspace.
- **RISK:** absence in this checkout is not evidence that an unavailable application
  lacks persistent data or a dependency.
- **UNKNOWN:** all application-level ownership, stores, APIs, call paths, duplication,
  temporal semantics, overwrite behavior, snapshots, and migrations require evidence
  from the corresponding application repositories.

## Verification

- BUILD: **NOT RUN — documentation discovery**
- TESTS: **NOT RUN — documentation discovery**
- PRODUCTION DATA MODIFIED: **NO**
