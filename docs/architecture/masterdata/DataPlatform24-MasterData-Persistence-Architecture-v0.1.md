# DataPlatform24 MasterData Persistence Architecture v0.1

**Status:** Proposed for architecture review; non-production  
**Classification:** PROPOSED NEW-FOUNDATION SCHEMA  
**Legacy migration compatibility:** DEFERRED  
**Execution baseline:** `ad578b9c6d388af1c97f492caa0cd98b75d5c65d`  
**Semantic authority:** `DataPlatform24-MasterData-Semantic-Architecture-v0.1.md`  
**Semantic authority SHA-256:** `441dc7d7e3b4ba670c1b7929f1fc4f0dbc06b6fb52b50ff736bf694af765be44`  
**Root architecture SHA-256:** `0fc43f232d22a13ec0e913cd3b57264c4650eeed921eee9fea02d1ce59732dcd`

## 1. Decision language and evidence boundary

- **PROPOSED** means suitable for architecture review, not approved for production.
- **DEFERRED** means evidence or a later review is required before the decision can
  become production-targeting.
- **REJECTED** means incompatible with the governing semantic or platform rules.

This document translates the approved semantic model into a proposed MariaDB
relational layout. It does not repeat or alter the semantic decisions. TASK 04
provides read-only inventory tooling and synthetic fixtures only. TASK 05 found no
accessible target-application source, configuration, datastore, or legacy schema.
Consequently no legacy key, table, relationship, lifecycle, or data conversion
mapping is inferred.

The root architecture identifies the current TS24 Application Platform Constitution
as its parent authority. Constitution v1.3 is not present in this worktree, so this
design preserves the authority, ownership, least-privilege, deployment-neutral,
contract-first, and no-cross-application-transaction rules already carried by the
root architecture. A direct v1.3 conformance review is **DEFERRED** until that
authority is available; this is not permission to override it.

## 2. Physical relational topology

The proposed logical MariaDB database name is `ts24_masterdata`. It is a deployment
configuration default, not a required server, catalog, or physical instance name.
Private, cloud, on-premises, and disconnected deployments may place it differently
without changing canonical identity or contracts.

The database contains only MasterData-owned identity, effective state, relationship,
assignment, concurrency, provenance, audit, and migration metadata. Licensing,
authorization, application business facts, payroll, declarations, documents, and
application snapshots are outside it. DigiDokument MongoDB collections are outside
this design.

Physical co-location with `taxonline`, `paycalc24`, `ibhxh`, or other databases does
not confer ownership or SQL access. There are no foreign keys from MasterData to
application-private tables or collections. Applications may persist canonical IDs
in their own data under their own ownership, but those outward references are not
enforced by cross-authority foreign keys.

All names in this document are **PROPOSED**. The `md_` prefix makes ownership visible
where a deployment cannot provide a distinct database.

## 3. Entity and state separation

Each stable semantic identity has an identity/header relation distinct from its
mutable, effective-dated state. A header is never recycled or physically deleted as
a lifecycle operation. Header `version` is the optimistic-concurrency token for its
defined mutable authority boundary.

| Header / aggregate | Effective relations | Stable anchors and meaning |
|---|---|---|
| `md_company` | `md_company_state` | One Company identity; state holds mutable names, descriptors, status, and approved attributes. |
| `md_company_group` | `md_company_group_state`, `md_company_group_membership` | A grouping concept independent of Company; membership is effective-dated. |
| `md_company_relationship` | `md_company_relationship_state` | One independently amendable typed, directed relationship occurrence between two Companies. |
| `md_branch` | `md_branch_state` | One Branch anchored to one Company; it is not an Organization Unit. |
| `md_organization_unit` | `md_organization_unit_state` | General enterprise unit. Department and Team are explicit effective classifications, not separate duplicate rows. |
| `md_organization_relationship` | `md_organization_relationship_state` | One typed structural relationship occurrence; graph-capable and not universally tree-only. |
| `md_position` | `md_position_state` | One organizational position/slot. Job title is a state classification/label until a shared catalog is approved. |
| `md_person` | `md_person_state` | One natural Person, independent of accounts and employment. Sensitive attribute scope remains deferred. |
| `md_employee` | `md_employee_state` | One employment relationship anchored to exactly one Person and one employing Company. |
| `md_employee_org_assignment` | `md_employee_org_assignment_state` | One organization-assignment occurrence for an Employee. |
| `md_employee_position_assignment` | `md_employee_position_assignment_state` | One position-assignment occurrence for an Employee. |
| `md_external_identity_mapping` | none | Optional mapping from a typed external authority/key to exactly one canonical semantic ID; not a canonical identity substitute. |

Every header has `id`, `version`, `created_at`, and `created_mutation_id`. Anchored
headers additionally carry immutable foreign keys required by semantics: Branch to
Company; Employee to Person and employing Company; relationship occurrences to
their endpoints; assignments to Employee and their target. Endpoint or employing-
Company changes create a new semantic occurrence rather than rewriting anchors.

Every state relation has its own row identity, owner identity, `effective_from`,
nullable `effective_to`, domain state columns, `created_at`, `last_recorded_at`,
`created_mutation_id`, and `last_mutation_id`. Domain columns must be added only from
approved semantics. This design does not use an entity-attribute-value table, an
untyped JSON property bag, or one generic history table.

Department and Team semantic identities are represented by the same physical
`OrganizationUnitId` plus an effective classification. This is representation
sharing, not semantic equivalence: typed contracts retain `DepartmentId`, `TeamId`,
and `OrganizationUnitId` meanings and must validate the effective classification.
If later evidence requires independent simultaneous identities, a reviewed
specialization relation and mapping will be added without recycling existing IDs.

TS24 account-to-Person mapping is not included until the account authority,
cardinality, assurance, and ownership rules are approved. The generic external
mapping relation must not be used to bypass that decision.

## 4. Key strategy

### 4.1 Proposed canonical representation

New canonical semantic IDs use UUID version 7 generated in the authority/application
layer and stored as `BINARY(16)` in RFC 9562 network-byte order. Contracts serialize
them as canonical lowercase UUID text and retain distinct semantic ID types. The
database representation is an implementation detail and does not leak into Domain
contracts.

UUIDv7 is proposed because it:

- can be generated without a database round trip in local, private, disconnected,
  and distributed deployments;
- provides 128-bit globally portable identity with negligible collision risk when
  generated correctly;
- has time-ordered high bits, improving B-tree locality over random UUIDv4;
- occupies 16 bytes in indexes rather than 36-character text;
- has standard UUID text/byte interoperability for APIs, messages, imports, and
  common languages; and
- avoids instance-specific integer ranges and coordination during later deployment
  consolidation.

The time component is not business time, audit time, authorization evidence, or a
guaranteed creation ordering. Security and correctness must not infer facts from it.
The writer validates UUID version/variant and uses a cryptographically suitable,
monotonic-within-generator implementation. MariaDB UUID conversion functions are
not part of the contract; providers bind the 16 bytes explicitly to avoid server-
version and byte-swap ambiguity.

Different semantic ID types may share this physical encoding but never share a
namespace in contracts or foreign-key targets. Primary keys remain the typed
relation's `BINARY(16)` ID. All foreign-key columns use the same byte order.

### 4.2 Legacy and external keys

Existing numeric, GUID, code, or composite keys are unknown and are not overwritten
or embedded into canonical IDs. After inventory and ownership review, mappings may
use `md_external_identity_mapping` with: mapping ID, canonical entity kind,
canonical ID, source authority/application, source namespace, normalized external
key, optional company scope, assurance/status, and provenance. A unique constraint
on the reviewed external-key scope prevents ambiguous mapping. Because MariaDB
cannot safely enforce a polymorphic foreign key, insertion is restricted to the
MasterData authority and must validate the typed canonical target transactionally.
Production use of this relation is **DEFERRED**.

## 5. Temporal persistence model

Business valid time is stored as DATE columns using `[effective_from,
effective_to)`. `effective_to IS NULL` means open-ended. A check constraint requires
`effective_to IS NULL OR effective_to > effective_from`. Dates are calendar dates;
recorded/audit timestamps use UTC `DATETIME(6)` and are not substituted for them.

State at date `T` is selected by:

```text
owner_id = ?
AND effective_from <= T
AND (effective_to IS NULL OR effective_to > T)
```

Single-valued timelines allow no overlap for one semantic scope. Standard MariaDB
does not provide a portable exclusion constraint, so enforcement is both
transactional and explicit:

1. lock the owning header using `SELECT ... FOR UPDATE`;
2. verify its expected version;
3. query for any interval in the same semantic scope where
   `existing_from < proposed_end` and
   `existing_end IS NULL OR existing_end > proposed_from`, treating an open proposed
   end as positive infinity;
4. reject overlap or invalid cardinality;
5. insert, split, close, or correct the necessary state rows;
6. record provenance/audit and atomically advance the header version; and
7. commit one MasterData transaction.

The mandatory owner lock serializes competing timeline changes even when no existing
row yet covers the proposed interval, eliminating the empty-range race. A database
constraint alone is not claimed to prevent overlaps.

Current, future, and past state use the same authoritative timeline. No mutable
`is_current` flag is stored. “Current” is evaluated against an explicit authority
date, avoiding midnight jobs and disagreement between flags and dates.

- A future change inserts or amends a future interval while retaining the current
  and past intervals.
- A termination closes active employment and assignment intervals at the exclusive
  termination boundary and creates the approved terminated employment state.
- Organization and position transfers close/split assignment intervals and create
  new assignment intervals; PersonId and EmployeeId do not change.
- Historical correction runs through the same concurrency/provenance boundary and
  amends/splits the asserted valid-time timeline. It is audited as a correction.
- Rehire identity behavior remains **DEFERRED**; storage can represent either a new
  interval on an approved continuing Employee or a new Employee identity, but the
  authority must not choose until policy/evidence resolves semantic continuity.

Assignment overlap and cardinality are keyed by an explicit `assignment_scope`
(for example, assignment type plus approved primary/secondary role). The actual
scope vocabulary and whether parallel assignments are allowed are **DEFERRED**.
No universal single-department or single-position rule is invented.

## 6. Version and concurrency model

`version` is an unsigned logical 64-bit value represented in MariaDB as `BIGINT
UNSIGNED`, initialized consistently with Foundation `EntityVersion.Initial` at 0.
It is not a timestamp or MariaDB row-version feature.

The proposed boundary is one version per mutable header aggregate. Company state is
guarded by Company version; Employee state and its organization/position assignment
timelines are guarded by Employee version. Independently referenced group
memberships, Company relationships, organization relationships, and assignments
have stable occurrence headers; mutations that affect an Employee assignment also
advance the owning Employee version so an Employee read/mutate cycle cannot silently
miss assignment changes. Final command granularity remains an application-contract
review concern.

An update uses one atomic predicate:

```text
UPDATE owning_header
SET version = version + 1
WHERE id = :id AND version = :expected_version
```

Exactly one affected row means success and the returned version is
`ExpectedVersion.Next()`. Zero rows triggers a read in the same authority boundary:
missing identity is not-found; existing identity returns `Conflict` and its current
version. Negative, overflowed, or otherwise invalid expectations return
`InvalidExpectedVersion`. All state writes, the compare/update, and audit/provenance
records occur in the same transaction and roll back together. No blind overwrite or
automatic merge is allowed. MariaDB mechanics remain inside the provider and are not
exposed through Domain contracts.

## 7. Provenance and audit model

Business valid time is physically separate from recorded time and operation evidence.
Two shared relations avoid duplicating provenance across every changed row:

### `md_mutation`

One immutable record per accepted authority mutation: `mutation_id`, `recorded_at`
(authority receipt/commit timestamp), `actor_id`, `actor_namespace`,
`source_application`, `source_module`, nullable `company_context_id`,
`correlation_id`, nullable `operation`, nullable bounded `reason`, and transaction/
deployment diagnostic identifiers where approved. The contract `Timestamp` is kept
as `source_timestamp` when it differs from authority `recorded_at`; it is not trusted
as the database recording clock.

### `md_audit_event`

One or more immutable operation outcomes linked to a mutation: event ID, event type,
typed aggregate kind and ID, prior/result version, affected state-row IDs, outcome,
and optional non-sensitive before/after digests. It does not store raw business
payloads, secrets, sensitive person data, document content, payroll data, declaration
data, or authorization decisions by default.

State rows reference their creating and most recent mutation. This attributes later
corrections without imposing universal bitemporal query semantics. Audit retention,
tamper evidence, actor namespace resolution, reason requirements, and whether a
specific regulated state requires full assertion history remain **DEFERRED**. If a
domain later proves a bitemporal evidence requirement, it receives a specific
reviewed assertion model—not a platform-wide generic history table.

Rejected commands may be recorded in an operational security/diagnostic store under
its own retention rules; they do not create authoritative `md_mutation` rows inside
a rolled-back MasterData transaction.

## 8. Constraints

Proposed database constraints reinforce, but do not replace, domain validation:

- primary key on every stable header and state-row ID;
- foreign keys only among MasterData-owned relations;
- `md_employee(person_id, employing_company_id)` are non-null and immutable;
- Branch has one immutable owning Company; cross-company movement is deferred;
- relationship endpoints are non-null and cannot be identical where the approved
  type disallows self-reference; type-specific cycles/cardinality are deferred;
- state interval end is null or strictly greater than start;
- status/type/discriminator values use reviewed reference/check constraints, never
  undocumented ORM ordinals;
- unique relationship occurrence keys are introduced only after taxonomy defines
  semantic identity granularity;
- source/namespace/external-key uniqueness is added only after legacy mapping scope
  is approved; and
- no cascade delete crosses stable identities. Lifecycle closes state; administrative
  purge, if ever lawful, requires a separate retention design.

MariaDB check-constraint behavior and supported DDL features must be verified against
the minimum deployed server versions before production migration approval. Graph
cycles, interval overlap, polymorphic mapping targets, and policy-driven cardinality
require authority validation inside the locked transaction.

## 9. Index strategy

Indexes are limited to authoritative access paths:

| Access path | Proposed index |
|---|---|
| Stable identity and mutation compare/update | Header primary key `(id)`; the equality predicate reads `version` from the same row. |
| Effective state at date / entity timeline | State `(owner_id, effective_from DESC, effective_to)`; state-row ID remains the primary key. |
| Open-ended timeline candidates | State `(owner_id, effective_to, effective_from)` to support open-ended/current candidate filtering and overlap checks. |
| Company-scoped Employee lookup | `md_employee(employing_company_id, id)`; any employee-number index is deferred until scope/normalization is evidenced. |
| Person employments | `md_employee(person_id, employing_company_id, id)`. |
| Employee organization assignments | Assignment header `(employee_id, organization_unit_id, id)` plus state `(assignment_id, effective_from DESC, effective_to)`. |
| Organization occupancy/placement | Assignment header `(organization_unit_id, employee_id, id)` and relationship endpoint/type indexes. |
| Position assignments | Header `(position_id, employee_id, id)` plus its state timeline index. |
| Company relationship traversal | `(source_company_id, relationship_type, target_company_id)` and reverse `(target_company_id, relationship_type, source_company_id)`. |
| Group membership | `(group_id, company_id, effective_from, effective_to)` and reverse company/group order. |
| Audit correlation and aggregate timeline | Mutation `(correlation_id)`, `(recorded_at)`, and audit event `(aggregate_kind, aggregate_id, recorded_at)`. |

Large deployments should verify selectivity and plans using non-production,
representative metadata and workloads before adding or changing indexes. Hypothetical
reporting indexes for payroll, tax, social insurance, documents, licensing, or other
application domains are explicitly excluded. Read replicas or projections, if later
approved, are not authorities and must expose staleness semantics.

## 10. Transaction boundaries

One accepted MasterData command executes in one local transaction owned by the
MasterData authority. It includes validation requiring locked state, timeline writes,
the atomic version transition, mutation provenance, and audit events. It never spans
an application database, Licensing, Authorization, MongoDB, an external identity
provider, or another deployment.

Cross-authority workflows use explicit versioned contracts, events, APIs, or
orchestration and handle partial failure outside the MasterData transaction. Physical
co-location is not grounds for a distributed or cross-schema transaction.

## 11. Read/write authority and security

Least-privilege database identities are separate deployment roles:

| Identity | Permission boundary |
|---|---|
| MasterData writer | DML and required locking only on MasterData runtime relations; invoked only behind authority policy/contracts. No schema administration. |
| MasterData reader | SELECT only on approved MasterData relations or views; deployments may require contract/API-only access instead. |
| Migration identity | Time-bound DDL plus migration-ledger access for the target MasterData database; not used by runtime services. |
| Inventory identity | Read-only metadata access needed by approved inventory tooling; no business-row reads and no DDL/DML. |

Applications receive no arbitrary MasterData table-write permission. The expected
mutation path is the MasterData command boundary with authorization, validation,
ExpectedVersion, and provenance. Raw credentials are deployment secrets and never
enter Domain objects, source control, audit payloads, or UI clients.

## 12. Migration and version model

No executable migration or production SQL is created by TASK 09. A later approved
provider uses ordered, immutable, checksummed migration units and a MasterData-owned
migration ledger containing migration ID, semantic version, checksum, description,
deployment scope, started/completed timestamps, outcome, and tool/operator identity.
This maps to Foundation `MigrationMetadata` without making the Domain depend on DDL.

The deployment process must:

1. inventory the target and verify supported MariaDB version/features;
2. back up and prove restore for the owned dataset;
3. acquire a deployment lock and reject checksum drift or unexpected order;
4. expose progress, duration, outcome, correlation, and migration version;
5. use expand/migrate/contract phases where running-version compatibility is needed;
6. make each unit transaction-aware while acknowledging that MariaDB DDL atomicity
   varies by operation and version;
7. define roll-forward and restore recovery before execution; and
8. block destructive contraction until compatibility windows, validation, and owner
   approval are complete.

Repeatable objects, if ever used, are explicitly marked and checksummed. Editing an
applied migration is rejected. Customer production migration scripts, legacy data
conversion, and runtime migration execution are separately authorized work.

## 13. Deployment implications

- Connection/store resolution is deployment configuration, not embedded in Domain
  contracts or canonical IDs.
- The logical database may be isolated, co-located, replicated, or privately hosted;
  permissions and authority remain identical.
- Deployments must configure UTC recorded timestamps, consistent DATE interpretation,
  supported collation for normalized external keys, backup/restore ownership, and
  version compatibility.
- UUIDv7 generation does not require Cloud reachability or a central sequence.
- Reader replication may serve non-authoritative reads only with explicit freshness
  behavior; concurrency mutations always use the authoritative writer.
- Schema naming remains **PROPOSED** until deployment and migration review.

## 14. Legacy compatibility status

**LEGACY MIGRATION COMPATIBILITY: DEFERRED.** Existing Company, Company Group,
Branch, organization, Person, Employee, identity, temporal, relationship, assignment,
and audit structures are entirely unknown. This architecture is therefore not
production-migration-ready and makes no source-to-target mapping claim.

Before production-target schema approval, a read-only inventory must establish at
least: source authorities and keys; duplicate/merge rules; nullable and orphan data;
company/person/employee identity continuity; status/type vocabularies; hierarchy and
cycle behavior; employee-number scope; concurrent employment; termination/rehire;
assignment cardinality; effective-date precision; correction practice; sensitive
data classification; account links; application snapshots; volumes; collations; and
MariaDB versions/features. Owners must approve semantic mappings and reconciliation.

## 15. Deferred decisions

- Direct Constitution v1.3 text conformance review when the authority is accessible.
- Legacy source-to-canonical mappings and external-key normalization.
- Company relationship taxonomy, occurrence identity, cycles, and cardinality.
- Organization structure identity, unit classification changes, graph constraints,
  and Department/Team specialization if independent identities are evidenced.
- Position versus shared Job Title catalog semantics and vacancy rules.
- Person attribute scope, privacy/retention, matching, deduplication, and merge.
- Account-to-Person mapping authority, assurance, and cardinality.
- Employee number scope, concurrent employment, termination vocabulary, Company
  transfer process, rehire identity continuity, and assignment scopes/cardinality.
- Aggregate/command granularity beyond the proposed header boundaries; idempotency.
- Regulated bitemporal assertion requirements, audit retention, tamper evidence, and
  reason requirements.
- Minimum MariaDB version, checked DDL syntax, logical database name, collation,
  partitioning, capacity, and deployment-specific topology.
- Any read projections, event publication/outbox, or cross-authority integration.

## 16. Rejected alternatives

- A single mutable current-state row: cannot represent past/future valid state.
- One generic History table: collapses unrelated state, audit, revision, snapshot,
  and calculated-history semantics.
- Platform-wide bitemporal tables: no demonstrated universal requirement.
- Database-generated integer identity: complicates offline/distributed generation,
  deployment consolidation, and portable references.
- UUID text primary keys: unnecessary storage and index cost.
- UUIDv4 as the default: weaker insertion locality without a compensating requirement.
- IDs derived from names, codes, employee numbers, TS24 users, or legacy keys:
  mutable/reusable and semantically incorrect.
- Company Group as Company, universal Company/organization trees, Branch as
  Department, or access membership as enterprise structure: semantic collapse.
- An `is_current` source of truth: duplicates date semantics and becomes stale.
- Database trigger-only overlap/concurrency rules: obscures authority behavior and
  does not replace a locked semantic-scope validation transaction.
- Direct application writes and cross-application foreign keys/transactions:
  violate ownership, authority, and least privilege.
- Event sourcing as the default persistence model: no approved requirement and does
  not remove the need for valid-time constraints.
- ORM-dependent schema or version mechanics: leaks a technology choice not authorized
  by this task.
- Production migration or invented legacy mapping: blocked by absent evidence.

## 17. Review gate

This document may advance only as a proposed architecture. Production DDL, provider
repositories, ORM selection, customer migration, and data conversion require later
authorization. Architecture review must confirm semantic fidelity, Constitution v1.3
compatibility, deployed MariaDB capabilities, aggregate boundaries, security roles,
and sufficient legacy evidence. The semantic and root authority files are unchanged.
