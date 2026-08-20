# DataPlatform24 Specification v0.1

## 1. Status and authority

| Item | Value |
|---|---|
| Specification | `DataPlatform24-Spec-v0.1` |
| Status | **PROPOSED IMPLEMENTATION AUTHORITY** |
| Approval | Requires explicit architecture-owner approval before becoming direct implementation authority |

This specification consolidates, but MUST NOT contradict, its higher authorities. It does not replace them.

| Order | Authority | SHA-256 |
|---:|---|---|
| 1 | TS24 Application Platform Constitution v1.3 — **AUTHORITATIVE** | `19da640ac8505664307b1f28e666708622c47d711b5e6ff53e8c1379050bb487` |
| 2 | `TS24-Data-Platform-Architecture-Baseline-v0.1.md` | `0fc43f232d22a13ec0e913cd3b57264c4650eeed921eee9fea02d1ce59732dcd` |
| 3 | `DataPlatform24-MasterData-Semantic-Architecture-v0.1.md` | `441dc7d7e3b4ba670c1b7929f1fc4f0dbc06b6fb52b50ff736bf694af765be44` |
| 4 | `DataPlatform24-MasterData-Persistence-Architecture-v0.1.md` | `dc7f0dc8aa365dd6d8a33770cf9170da63a0853dc1323fa164bf1d95d83154d1` |
| 5 | This specification | Calculated from the approved file revision |
| 6 | Subsequent implementation tasks | MUST conform to all preceding authorities |

Authority flows from Constitution v1.3 to the root architecture, semantic architecture, persistence architecture, this specification, and then implementation tasks. Constitution v1.3 is a platform-level authority and may reside outside this repository; its immutable SHA-256 is recorded here for verification.

## 2. Scope and applicability

DataPlatform24 applies to components that own, persist, query, synchronize, migrate, expose, or integrate persistent data. A pure/stateless Core need not depend on DataPlatform24.

```text
Application Uses Persistence
!=
Every Core Is Persistence-Aware
```

## 3. Platform boundary

DataPlatform24 owns the persistent-data foundation, shared MasterData authority, shared Licensing/Entitlement foundation, persistence contracts and infrastructure, migration/version infrastructure, multi-store governance, and deployment-aware persistence.

It MUST NOT take ownership of TS24 Cloud Identity, TS24 Link business domain, Konect24 connectivity authority, Azen24, Security24, Signing24, DocsView24, application authorization, or application business truth.

## 4. Application business ownership

| Application | Application-owned truth |
|---|---|
| PayCalc24 | Payroll inputs, configuration, calculations, periods, approvals, results, and payroll history |
| TaxOnline | Declarations, revisions, payloads/XML, submission state, government responses, and evidence |
| iBHXH | Declarations, revisions, statutory payloads, submission workflow, responses, and evidence |
| DigiDokument | Document metadata/content references, lifecycle, finalized artifacts, and document evidence |
| ContractSigning | Workflow/signing business truth, participants, approvals, signatures, versions, and finalized artifacts |

Shared MasterData MUST NOT become a giant application-business database. Applications MAY reference canonical IDs without transferring ownership.

## 5. Canonical semantic identity

MasterData is the canonical authority for Company, Company Group, Company Relationship, Branch, Organization Structure, Organization Unit, Department, Team, Position / Job Title, Person, and Employee shared truth.

```text
TS24 User != Person != Employee
Access Membership != Enterprise Organization Structure
```

Stable semantic identity MUST remain immutable, MUST remain separate from mutable state and display labels, and MUST NOT be reused for a different semantic entity. Company relationships MUST be graph-capable where their approved type requires it; Company Group MUST NOT be treated as a parent Company, and Branch MUST NOT be treated as Department.

## 6. Person and Employee semantics

Person identifies a natural person independently of accounts and employment. Employee identifies one employment relationship for one Person within one employing Company context.

- A Person MAY have multiple Employee identities across companies and MAY conceptually have concurrent employments; production concurrency policy remains deferred.
- Organization-unit, department, or position transfer MUST preserve Person and Employee identity and change effective assignments.
- Company transfer MUST create a distinct destination-company Employee identity while preserving Person identity.
- Termination MUST NOT delete or recycle Person or Employee identity.
- Rehire identity continuity is **DEFERRED** and MUST NOT be inferred.

## 7. Temporal MasterData

Business-effective intervals MUST use `[EffectiveFrom, EffectiveTo)`: `EffectiveFrom` is inclusive, `EffectiveTo` is exclusive, and a null end is open-ended. Business valid time MUST remain separate from recorded/system time.

Effective State, Revision, Snapshot, Audit Event, and Calculated History are distinct concepts and MUST NOT be collapsed into generic history. Platform-wide bitemporal persistence is NOT required. A domain-specific bitemporal model MAY be proposed only with an approved evidence/query requirement.

## 8. Historical snapshot law

Applications MUST preserve immutable revisions, snapshots, results, and evidence sufficient for historical reproducibility. Later MasterData changes MUST NOT rewrite finalized TaxOnline declarations, iBHXH declarations, PayCalc24 payroll results, DigiDokument artifacts, or ContractSigning workflows/artifacts. Exact snapshot payload and finalization rules remain application-owned and deferred.

## 9. Optimistic concurrency

Each mutable authoritative boundary MUST use logical `ExpectedVersion` and `CurrentVersion`. A successful authorized and validated mutation MUST match the expected version and advance the version; mismatch MUST return `Conflict` without the requested state change. Blind overwrite and automatic semantic merge are forbidden by default.

The approved MasterData persistence mapping is logical version to `BIGINT UNSIGNED`, initialized consistently with Foundation `EntityVersion.Initial` at zero. The conceptual mutation predicate is:

```sql
UPDATE owning_header
SET version = version + 1
WHERE id = :id AND version = :expected_version
```

Exactly one affected row is success. Zero rows MUST be resolved within the authority boundary as not-found or conflict with current version. Invalid or overflowed expectations MUST be rejected. MariaDB version mechanics MUST NOT leak into Domain/Foundation contracts.

## 10. Provenance and audit semantics

Accepted authoritative mutations MUST capture ActorId, SourceApplication, SourceModule, CompanyContext when applicable, recorded timestamp, CorrelationId, Operation, and Reason where applicable. `ActorId` is not necessarily `TS24UserId`. Recorded/system time MUST remain distinct from business-effective time.

Audit and diagnostic records MUST NOT contain secrets, raw business payloads, sensitive person data, document content, payroll data, declaration data, or authorization decisions by default. Rejected commands MAY be recorded in a separate operational security/diagnostic store under its own retention rules.

## 11. MasterData physical persistence baseline

The logical MariaDB namespace is proposed as `ts24_masterdata`; it is a deployment default, not a final physical server/catalog/database mandate. All names and the schema remain proposed until implementation and deployment review.

```text
Stable Identity Header != Effective-Dated State
```

The relational design MUST separate identity anchors, effective states, relationships, assignments, provenance/mutations, audit events, and migration metadata. It MUST NOT use an untyped property bag, entity-attribute-value model, or generic history blob as a substitute. Stable headers MUST NOT be recycled or physically deleted as normal lifecycle behavior.

## 12. MasterData ID persistence baseline

New canonical MasterData IDs MUST use UUIDv7 generated by the authority/application layer, stored in RFC 9562 network-byte order as `BINARY(16)`, and serialized externally as canonical lowercase UUID text. Providers MUST bind bytes explicitly and MUST NOT make MariaDB UUID conversion functions contractual.

```text
UUIDv7 + BINARY(16)
= DataPlatform24 MasterData persistence baseline
!= universal TS24 physical ID mandate
```

Semantic ID types MUST remain distinct even when physically encoded alike. Domain and application contracts MUST NOT depend on `BINARY(16)`. UUID time bits MUST NOT be treated as business time, audit evidence, authorization evidence, or guaranteed creation order.

## 13. Temporal persistence baseline

Approved MasterData effective dates MUST use `DATE effective_from` and nullable `DATE effective_to`, unless a specific domain obtains approval for higher resolution. Recorded timestamps use UTC timestamp semantics separately.

Single-valued scopes MUST prevent overlaps through one authority-controlled transaction that locks the owner header, verifies expected version, validates scoped overlap/cardinality, applies timeline changes, records provenance/audit, and advances the version atomically. UI validation or database constraints alone are insufficient. Current state MUST be evaluated from intervals at an explicit authority date; a mutable `is_current` source of truth is forbidden.

## 14. Constraints and indexing

Implementation MUST define primary keys, MasterData-internal foreign keys, effective interval validity, immutable semantic anchors, and reviewed type/status constraints. It MUST NOT create cross-authority foreign keys or cascade-delete stable identities.

Indexes MUST support stable identity/concurrency access, company-scoped Employee lookup, Person employments, effective-state and timeline/overlap lookup, organization and position assignment lookup, relationship traversal in both directions, group membership, and audit correlation/aggregate lookup. Speculative application-report indexes MUST NOT be introduced without workload evidence.

## 15. Transaction boundary

```text
one authoritative module = one transaction boundary
```

A MasterData mutation transaction MUST remain inside MasterData authority and include locked validation, timeline writes, version transition, provenance, and audit. DataPlatform24 MUST NOT introduce distributed transactions across applications or use MariaDB co-location to bypass authority boundaries. Cross-authority workflows SHOULD use explicit contracts, APIs, events, or orchestration with explicit partial-failure handling.

## 16. MariaDB role

MariaDB is the primary shared relational foundation. DataPlatform24 is not MariaDB-only, does not require one physical instance/schema/database, and MUST NOT require all application domains to migrate to MariaDB.

## 17. Multi-store governance

```text
Shared Semantic Identity is common
Application Persistence Technology may differ
```

Application-owned stores MAY include MongoDB, object/file storage, search indexes, bounded caches, and other approved stores. They MUST NOT independently become shared MasterData authority. DigiDokument MongoDB remains application-owned document-domain persistence where applicable; projections and replicas are non-authoritative and MUST expose freshness semantics.

## 18. Cross-application persistence access

Arbitrary cross-application write is forbidden. Application A MUST use Application B's approved Application API/Core or a shared DataPlatform24 capability/authority; it MUST NOT mutate Application B's private tables because schemas are co-located. Direct cross-schema access requires an explicit approved exception.

## 19. Data-access/provider boundary

```text
Application/Core
  -> Domain-facing contract
  -> Repository / Provider / Data Service
  -> Persistence provider
```

Domain/Foundation contracts MUST NOT expose MariaDB, MongoDB, ORM, or driver types. Provider implementations MUST remain replaceable and MUST NOT become domain authorities.

## 20. Licensing and entitlement boundary

```text
Licensing / Entitlement != Authorization != Business Domain
```

DataPlatform24 MAY own approved shared product/service entitlement semantics, but MUST NOT define application authorization or application business truth. Cloud Service entitlement remains distinct from product/application entitlement. Detailed Licensing semantics remain a separate approval track.

## 21. Deployment neutrality

DataPlatform24 MUST support `LOCAL`, `ON_PREMISE`, `CUSTOMER_CLOUD`, `TS24_CLOUD`, `CLOUD24_PRIVATE`, and `HYBRID`. Deployment is not connectivity. Store location, topology, credentials, and connection resolution are deployment configuration. Business semantics and canonical IDs MUST NOT change by hosting model.

## 22. Cloud24 Private and trust boundaries

Canonical identity and DataPlatform24 participation MUST NOT force private-capable business payloads through TS24 Cloud. Private deployment does not imply air-gapped operation. Runtime connectivity requirements and reader freshness MUST be explicit.

## 23. Migration framework

DataPlatform24 MAY implement an ordered, immutable, checksummed, observable, recoverable, deployment-aware, and backward-compatibility-conscious migration framework. It MUST detect checksum drift/order errors, use an owned ledger and deployment lock, expose outcome/progress, and define roll-forward/restore recovery before execution.

**Production Customer Migration is NOT YET AUTHORIZED.** Legacy conversion, destructive contraction, and runtime customer migration require separate evidence and approval.

## 24. Legacy compatibility status

**LEGACY COMPATIBILITY: DEFERRED / PARTIAL.** Inventory tooling exists, but TASK 05 lacked access to most target application source, configuration, stores, and schemas. Production migration MUST NOT be marked ready.

Unknown/deferred evidence includes legacy Company keys; Person matching/deduplication; Employee numbering, termination, rehire, transfer, and concurrent-employment policies; company/group/hierarchy and relationship semantics; assignment cardinalities; temporal/correction practices; snapshot implementations; cross-application database dependencies; and real TS24 Link/Konect24 identity, membership, authorization, and integration paths.

## 25. Backup and restore boundary

Every persistent store MUST name its Owning Module, Backup Responsibility, Restore Responsibility, and Recovery/Consistency Semantics. Where one business artifact spans stores, cross-store restore ordering, consistency point, and acceptable partial recovery MUST be explicit. Backup orchestration technology remains deferred.

## 26. Security and credentials

Deployments MUST use least-privilege datastore identities. Migration, runtime writer, runtime reader where appropriate, and inventory/read-only identities MUST be separated. Domain entities and source control MUST NOT contain database secrets; UI clients MUST NOT receive raw credentials; sensitive payload logging is forbidden by default. DataPlatform24 MUST NOT become Security24 or the Authorization authority.

## 27. Human and Azen24 access

Human UI and Azen24 Agent MUST invoke the same authorized Application API/Core business capability boundary. Azen24 MUST NOT receive arbitrary database access or an AI-only persistence path. An explicitly approved DataPlatform24 capability API MAY be invoked as another authorized capability with actor/delegation identity, Company Context, entitlement, authorization, policy, and provenance.

## 28. Architecture dependency rules

The following rules are mandatory and remain guarded by architecture tests:

```text
Foundation !-> MasterData business implementation
Foundation !-> Licensing business implementation
Foundation !-> MariaDB / MongoDB
Domain !-> datastore drivers
MasterData !-> application business domains
Licensing !-> application authorization implementation
Providers !-> domain authority
```

## 29. Implementation-ready decisions

Subsequent approved tasks MAY rely on:

- .NET 9 and the established Foundation boundaries/contracts;
- one canonical shared MasterData authority and application-owned business truth;
- stable semantic identity and `TS24 User != Person != Employee`;
- membership separate from enterprise organization structure;
- Person/Employee transfer semantics stated in section 6;
- `[start,end)` effective dates, separate recorded time, and no universal bitemporality;
- historical application snapshot/revision law;
- optimistic concurrency with no blind overwrite or default semantic merge;
- provenance semantics stated in section 10;
- stable-header/effective-state relational separation;
- UUIDv7/RFC 9562 `BINARY(16)` for new MasterData persistence only;
- MariaDB as primary relational foundation with approved multi-store use;
- authority-owned transactions and no arbitrary cross-application writes;
- replaceable providers, deployment neutrality, and least privilege;
- migration infrastructure may be built, but customer migration may not; and
- no ORM is currently selected.

## 30. Explicit deferred decisions

The following remain non-authoritative: final production DDL; minimum MariaDB version and checked feature syntax; ORM; legacy migration mappings and external-key normalization; rehire identity policy; Employee-number, concurrent-employment, termination, and transfer policies; Company relationship taxonomy, occurrence identity, cycles, and cardinalities; organization structure identity and assignment scope/cardinality; Position versus Job Title catalog details; Person matching/merge/privacy rules; membership authority and account-to-Person mapping rules; central authorization host; domain-specific bitemporal requirements; exact application snapshot payloads, retention, and finalization; audit retention/tamper evidence; backup orchestration technology; deployment database naming, collation, topology, and capacity; MongoDB application document schemas; projections/outbox/event publication; and production rollout sequence.

## 31. Implementation gate

After explicit approval of this specification, implementation MAY begin for the MariaDB MasterData provider, reviewed schema/DDL, migration infrastructure, repositories/providers, focused persistence tests, temporal/concurrency tests, and deployment connection resolution.

Production customer migration remains separately gated by legacy inventory, owner-approved mappings, reconciliation, supported-database verification, backup/restore proof, security review, and rollout approval.
