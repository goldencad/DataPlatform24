# DataPlatform24 MasterData Semantic Architecture v0.1

**Status:** Proposed for architecture approval  
**Scope:** Canonical MasterData, identity mapping, effective time, concurrency,
provenance, and application snapshot boundaries  
**Architecture authority:** `TS24-Data-Platform-Architecture-Baseline-v0.1.md`  
**Evidence limit:** TASK 04 used synthetic metadata only; TASK 05 found no target
application source or configuration. Legacy behavior is therefore not inferred.

## 1. Decision language and scope

Every normative conclusion is classified as:

- **ACCEPTED** — established by the architecture baseline or required to make its
  invariants coherent.
- **DEFERRED** — intentionally left for later evidence or architecture work.
- **UNKNOWN** — legacy/application evidence is unavailable.
- **REJECTED** — explicitly incompatible with the governing invariants.

This document is a semantic model, not a persistence design. Table, database,
schema, column, index, foreign-key, ORM, physical identifier, temporal layout,
audit layout, cross-schema constraint, migration SQL, and Mongo document decisions
are **DEFERRED** to TASK 09 or later.

## 2. Governing decisions

| ID | Classification | Decision |
|---|---|---|
| MD-01 | **ACCEPTED** | MasterData is the single canonical authority for shared Company, enterprise-organization, Person, and Employee truth. |
| MD-02 | **ACCEPTED** | Application business artifacts and their revisions, calculations, workflows, submissions, documents, and evidence remain application-owned. |
| MD-03 | **ACCEPTED** | Stable semantic identity is immutable, is not derived from mutable labels or state, and is never reused for a different semantic entity. |
| MD-04 | **ACCEPTED** | `TS24 User != Person != Employee`; mappings between them are optional and do not transfer authority. |
| MD-05 | **ACCEPTED** | Access Membership and enterprise organization structure are separate models with separate authority. |
| MD-06 | **ACCEPTED** | Business effective time uses start-inclusive, end-exclusive intervals `[start, end)`; an absent end is open-ended. |
| MD-07 | **ACCEPTED** | Business valid time, recorded/system time, audit events, and mutation provenance are distinct semantics. |
| MD-08 | **ACCEPTED** | Shared authoritative mutation is optimistic-concurrency-aware and provenance-aware. |
| MD-09 | **ACCEPTED** | Historically reproducible application artifacts retain an immutable revision/snapshot sufficient for their purpose; they never depend only on current mutable MasterData. |
| MD-10 | **ACCEPTED** | Canonical identity and MasterData semantics do not require mandatory TS24 Cloud runtime transit. |
| MD-11 | **REJECTED** | Treating Company relationships as necessarily one tree, Company Group as a parent Company, Branch as Department, or Company as an access organization. |
| MD-12 | **REJECTED** | Platform-wide bitemporal persistence without a demonstrated domain requirement. |

## 3. Canonical semantic model

“Authority” below means authority for the stated shared semantic truth, not for an
application's business artifact. All listed identities are semantic; their physical
representations are **DEFERRED**.

### 3.1 Company

- **Purpose — ACCEPTED:** canonical enterprise, legal, or business entity in whose
  context shared business facts may exist.
- **Authority — ACCEPTED:** MasterData.
- **Stable identity — ACCEPTED:** `CompanyId` identifies the same company through
  name, address, status, ownership, organization, or other mutable state changes.
- **Lifecycle — ACCEPTED:** creation, effective-dated state changes, and inactive/
  ended state; identity is retained and not recycled.
- **Relationships — ACCEPTED:** may participate in typed, effective-dated
  relationships to other companies and may be classified in groups. It is not
  constrained to one parent.
- **Mutable state — ACCEPTED:** names, business descriptors, status, addresses and
  relationships, where accepted domain rules permit.
- **Stable state — ACCEPTED:** identity and semantic continuity of the company.
- **Consumers — ACCEPTED:** shared-data consumers including PayCalc24, TaxOnline,
  iBHXH, DigiDokument, ContractSigning, and access-context integrations when needed.
- **Does not own — ACCEPTED:** account membership, authorization, declarations,
  payroll, documents, signing workflows, or application evidence.
- **Legacy questions — UNKNOWN:** authoritative keys, lifecycle vocabulary,
  duplicate/merge rules, legal-entity distinctions, and existing parent semantics.

### 3.2 Company Group

- **Purpose — ACCEPTED:** a stable semantic grouping of companies for a stated
  business purpose; it is not itself presumed to be a Company.
- **Authority — ACCEPTED:** MasterData when the grouping is shared enterprise truth.
- **Stable identity — ACCEPTED:** a group identity denotes the same grouping concept
  while names and effective membership change.
- **Lifecycle — ACCEPTED:** create, rename, change effective membership, end; do not
  reuse identity.
- **Relationships — ACCEPTED:** membership relates companies to a group and may be
  effective-dated; group membership does not imply parent/subsidiary ownership.
- **Mutable state — ACCEPTED:** label, classification, status, and membership.
- **Stable state — ACCEPTED:** group identity and declared grouping purpose.
- **Consumers — ACCEPTED:** applications needing an approved shared grouping.
- **Does not own — ACCEPTED:** Company identity, legal ownership, access membership,
  or organization hierarchy.
- **Legacy questions — UNKNOWN:** whether legacy group concepts exist, their purposes,
  membership cardinality, and whether nested groups are required.

### 3.3 Company Relationship

- **Purpose — ACCEPTED:** a typed relationship between two Company identities, such
  as a parent/subsidiary or service-provider/customer relationship.
- **Authority — ACCEPTED:** MasterData for shared relationship truth.
- **Stable identity — ACCEPTED:** a business-significant relationship occurrence has
  stable identity where it must be independently referenced or amended; exact
  identity granularity is **DEFERRED** pending relationship taxonomy.
- **Lifecycle — ACCEPTED:** effective start, changes where semantically allowed, and
  end; direction and role are explicit.
- **Relationships — ACCEPTED:** graph-capable rather than universally tree-only;
  constraints are specific to relationship type.
- **Mutable state — ACCEPTED:** effective interval, status, descriptors, and permitted
  relationship attributes.
- **Stable state — ACCEPTED:** endpoint identities and relationship semantic type for
  an occurrence; changing either creates a different semantic relationship.
- **Consumers — ACCEPTED:** applications resolving subsidiaries, managed customers,
  or other approved cross-company contexts.
- **Does not own — ACCEPTED:** user authorization to either company or application
  business transactions between companies.
- **Legacy questions — UNKNOWN:** types, cardinality, cycle rules, ownership
  percentages, and source authorities.

### 3.4 Branch

- **Purpose — ACCEPTED:** a recognized operating subdivision of one Company where
  branch semantics matter independently from organization management structure.
- **Authority — ACCEPTED:** MasterData.
- **Stable identity — ACCEPTED:** `BranchId` identifies the branch despite mutable
  name, address, status, or internal assignment.
- **Lifecycle — ACCEPTED:** create, effective-dated change, close/end; no identity reuse.
- **Relationships — ACCEPTED:** belongs to a Company; may be referenced by enterprise
  organization assignments, but is not inherently a Department or access organization.
- **Mutable state — ACCEPTED:** label, operating details, status, and permitted
  company/organization associations.
- **Stable state — ACCEPTED:** branch identity and continuity within its semantic
  company context. Cross-company movement semantics are **DEFERRED**.
- **Consumers — ACCEPTED:** applications requiring a shared branch context.
- **Does not own — ACCEPTED:** department hierarchy, memberships, authorization, or
  application transactions.
- **Legacy questions — UNKNOWN:** legal/registration meaning, branch codes, whether
  branches can span companies, and closure/reopening practice.

### 3.5 Organization Structure

- **Purpose — ACCEPTED:** the effective-dated arrangement of organization units and
  their relationships within a Company; it is a structure, not an access container.
- **Authority — ACCEPTED:** MasterData.
- **Stable identity — ACCEPTED:** a structure identity denotes a coherent organizing
  context if multiple structures must coexist; whether an explicit structure identity
  is required is **DEFERRED**.
- **Lifecycle — ACCEPTED:** structure relationships and assignments evolve over time.
- **Relationships — ACCEPTED:** may express hierarchy plus explicitly typed non-tree
  relationships when justified; a universal simple tree is not assumed.
- **Mutable state — ACCEPTED:** unit membership, reporting/parent relationships,
  labels, classifications, and effective status.
- **Stable state — ACCEPTED:** identities of participating semantic units survive
  rearrangement.
- **Consumers — ACCEPTED:** HR, payroll, declarations, documents, and other consumers
  needing shared enterprise placement.
- **Does not own — ACCEPTED:** access membership, authorization, or application-owned
  workflow/reporting snapshots.
- **Legacy questions — UNKNOWN:** hierarchy types, matrix reporting, parallel
  structures, and required integrity rules.

### 3.6 Organization Unit

- **Purpose — ACCEPTED:** general stable node representing a recognized enterprise
  organizational unit inside a Company structure.
- **Authority — ACCEPTED:** MasterData.
- **Stable identity — ACCEPTED:** `OrganizationUnitId` survives rename, re-parenting,
  and other state changes that preserve unit continuity.
- **Lifecycle — ACCEPTED:** create, effective change/re-parent, end; identity not reused.
- **Relationships — ACCEPTED:** participates in typed effective-dated structural
  relationships; Department and Team are semantic specializations/classifications,
  not automatically separate persistence kinds.
- **Mutable state — ACCEPTED:** label, type/classification, status, parent/reporting
  relationships, and approved company/branch associations.
- **Stable state — ACCEPTED:** unit identity. Whether reclassification preserves
  identity depends on semantic continuity and is **DEFERRED**.
- **Consumers — ACCEPTED:** applications needing enterprise assignments.
- **Does not own — ACCEPTED:** employees, memberships, permissions, or application
  artifacts; it is referenced by their assignments.
- **Legacy questions — UNKNOWN:** unit taxonomy, codes, nesting rules, and whether
  Department/Team have distinct legacy identities.

### 3.7 Department

- **Purpose — ACCEPTED:** an organization unit classified as a department for shared
  enterprise semantics.
- **Authority — ACCEPTED:** MasterData.
- **Stable identity — ACCEPTED:** `DepartmentId` denotes the department through rename
  or re-parenting. Its relationship to `OrganizationUnitId` is semantic; identifier
  aliasing or representation is **DEFERRED**.
- **Lifecycle/relationships — ACCEPTED:** effective-dated existence and placement in
  a Company organization structure; not equivalent to Branch.
- **Mutable state — ACCEPTED:** name, status, structural parent, approved attributes.
- **Stable state — ACCEPTED:** semantic department continuity.
- **Consumers — ACCEPTED:** Employee assignment and application contexts needing the
  effective department.
- **Does not own — ACCEPTED:** employee identity, access membership, payroll, or
  declaration history.
- **Legacy questions — UNKNOWN:** legacy identity, codes, hierarchy, and transfer rules.

### 3.8 Team

- **Purpose — ACCEPTED:** an organization unit classified as a team, potentially
  subordinate to or cross-cutting departments where rules permit.
- **Authority — ACCEPTED:** MasterData when shared; application-local temporary teams
  remain application-owned.
- **Stable identity — ACCEPTED:** team identity survives mutable state that preserves
  semantic continuity.
- **Lifecycle/relationships — ACCEPTED:** effective-dated existence and typed
  placement; no universal department-parent rule is assumed.
- **Mutable state — ACCEPTED:** name, status, membership/assignment relationships,
  and structure placement.
- **Stable state — ACCEPTED:** team identity.
- **Consumers — ACCEPTED:** applications needing canonical enterprise team context.
- **Does not own — ACCEPTED:** user access groups or application-local workflow teams.
- **Legacy questions — UNKNOWN:** whether canonical Team exists, matrix placement,
  and distinction from access groups.

### 3.9 Position / Job Title

- **Purpose — ACCEPTED:** Position represents a shared organizational slot/role for
  assignment; Job Title is its human/business classification or label unless later
  evidence establishes a separate shared catalog concept.
- **Authority — ACCEPTED:** MasterData for shared Position and shared Job Title truth.
- **Stable identity — ACCEPTED:** `PositionId` survives incumbent and mutable label/
  placement changes that preserve the position's continuity.
- **Lifecycle — ACCEPTED:** create, effective changes, vacant/filled state where
  modeled, and end; assignment lifecycle is separate.
- **Relationships — ACCEPTED:** position may be associated with Company and effective
  organization placement; Person/Employee occupies it through an assignment.
- **Mutable state — ACCEPTED:** title/classification, organization placement, status,
  and incumbent assignments.
- **Stable state — ACCEPTED:** position identity; a materially different position is
  not represented by relabeling an old identity.
- **Consumers — ACCEPTED:** HR/payroll/document/workflow consumers needing shared
  position context.
- **Does not own — ACCEPTED:** Employee identity, employment, authorization role, or
  application workflow role.
- **Legacy questions — UNKNOWN:** whether Position and Job Title are distinct, whether
  positions are slots or labels, assignment cardinality, and vacancy semantics.

### 3.10 Person

- **Purpose — ACCEPTED:** canonical identity of a natural person independently of
  account access and any particular employment.
- **Authority — ACCEPTED:** MasterData for shared Person truth.
- **Stable identity — ACCEPTED:** `PersonId` identifies the person through account,
  company, employment, name, contact, and organization changes.
- **Lifecycle — ACCEPTED:** establish identity, amend effective state, and retain
  continuity; employment termination does not terminate Person identity.
- **Relationships — ACCEPTED:** may optionally map to TS24 accounts and may have zero,
  one, or many Employee identities at companies.
- **Mutable state — ACCEPTED:** name and other shared person attributes subject to
  domain and privacy rules.
- **Stable state — ACCEPTED:** semantic identity of the natural person.
- **Consumers — ACCEPTED:** applications needing cross-employment person continuity.
- **Does not own — ACCEPTED:** login account, membership, employment, payroll,
  declarations, documents, or authorization.
- **Legacy questions — UNKNOWN:** matching/deduplication, sensitive attribute scope,
  lawful sources, correction/merge policy, and identifiers used by legacy systems.

### 3.11 Employee (employment relationship at Company)

- **Purpose — ACCEPTED:** canonical shared identity and lifecycle of a Person's
  employment relationship in a Company; conceptually `Person -> Employment
  relationship -> Employee @ Company`.
- **Authority — ACCEPTED:** MasterData for shared employment identity and effective
  state; applications retain their domain-specific employee facts/results.
- **Stable identity — ACCEPTED:** `EmployeeId` identifies one employment relationship
  and survives department/unit/position transfers and mutable employment state.
- **Lifecycle — ACCEPTED:** establish, future/current/past state, assignments,
  termination, and potentially rehire. Rehire identity behavior is **DEFERRED**.
- **Relationships — ACCEPTED:** exactly one Person and one employing Company per
  Employee identity; may have effective-dated organization and position assignments.
  A Person may have multiple Employee identities across companies and may
  conceptually have concurrent employments.
- **Mutable state — ACCEPTED:** employment status and terms represented as shared
  MasterData, organization/position assignments, and effective dates.
- **Stable state — ACCEPTED:** Person and employing Company anchors for an Employee
  identity. Transfer to another Company is a different employment relationship and
  therefore a different Employee identity.
- **Consumers — ACCEPTED:** PayCalc24, TaxOnline, iBHXH, DigiDokument,
  ContractSigning, and other approved shared-data consumers.
- **Does not own — ACCEPTED:** Person/account identity, access membership, payroll
  results, declaration revisions, statutory payloads, or document/workflow truth.
- **Legacy questions — UNKNOWN:** employee-number scope, concurrent employment rules,
  termination vocabulary, rehire behavior, company-transfer practice, assignment
  cardinality, and source authority.

## 4. Company contexts and relationship semantics

- **ACCEPTED:** Parent/subsidiary is one typed Company Relationship, not the
  definition of Company Group and not proof of a universal hierarchy.
- **ACCEPTED:** A service provider and each managed customer are distinct Companies.
  Their service-provider/customer relationship does not grant a user access.
- **ACCEPTED:** One user can participate in multiple Company Contexts through
  separate memberships/policy decisions; this does not merge the companies or
  create Employee identities.
- **ACCEPTED:** Branch is a subdivision of a Company with branch semantics;
  Organization Unit is a node in enterprise structure. A relationship may exist,
  but equivalence is **REJECTED**.
- **DEFERRED:** exact Company relationship taxonomy and type-specific graph
  constraints until legacy/business evidence is available.

## 5. Person, Employee, and TS24 User identity

```text
TS24 account (TS24UserId) -- optional mapping --> Person (PersonId)
                                                |
                                                +--> EmployeeId @ Company A
                                                +--> EmployeeId @ Company B
```

- **ACCEPTED:** A Person can exist without a TS24 account; an Employee can exist
  without a TS24 account; a TS24 User can exist without a Person mapping or Employee.
- **ACCEPTED:** An account-to-Person mapping is optional. Where mapping policy allows,
  a Person may need mappings to more than one account; exact account-link cardinality
  and assurance rules are **DEFERRED** to identity authority evidence.
- **ACCEPTED:** A Person may be employed by multiple Companies and may conceptually
  have multiple concurrent employment relationships. Whether production policy
  permits particular combinations is **DEFERRED**.
- **ACCEPTED:** Department, unit, or position transfer changes effective assignments,
  not PersonId or EmployeeId.
- **ACCEPTED:** Company transfer ends or changes the source employment according to
  policy and establishes a distinct Employee identity anchored to the destination
  Company; PersonId remains stable.
- **ACCEPTED:** Termination ends active employment state/intervals; it does not delete
  or recycle PersonId or EmployeeId.
- **DEFERRED:** Rehire using the prior EmployeeId versus a new EmployeeId. This hinges
  on whether the business recognizes continuity of the same employment relationship;
  legacy evidence is unavailable.
- **REJECTED:** using `TS24UserId` as `PersonId` or `EmployeeId`. Account lifecycle,
  authentication and access participation do not represent natural-person or
  employment continuity.
- **ACCEPTED:** Application business records reference `CompanyId`, `EmployeeId`, and
  `PersonId` only according to their meaning; they use `TS24UserId` for an account/
  actor reference, never as a substitute.
- **ACCEPTED:** DataPlatform24 does not redesign TS24 Cloud Identity and does not take
  account authority. Canonical IDs remain usable without mandatory Cloud transit.

## 6. Membership versus enterprise structure

```text
Access plane                              Enterprise plane
TS24 Account                              Company
  -> Membership                             -> Branch
  -> Company Context                        -> Organization Unit
                                               -> Department / Team
                                               -> Position
```

- **ACCEPTED:** Membership answers which account participates in an access context;
  enterprise structure answers how a Company organizes people and work.
- **ACCEPTED:** The planes may reference the same `CompanyId`, and policy may use
  canonical Employee/assignment facts, but neither plane becomes authority for the
  other.
- **ACCEPTED:** Company Context carried with a request identifies relevant operating
  context; by itself it is neither proof of membership nor authorization.
- **REJECTED:** deriving membership automatically from Department, Team, Position, or
  Employee assignment, or treating access groups as enterprise units.
- **DEFERRED:** final authorization host, membership authority, and any movement of
  TS24 Link/Konect24 responsibilities. No legacy evidence supports such a move.

## 7. Effective-time semantics

- **ACCEPTED:** Effective intervals follow `[EffectiveFrom, EffectiveTo)`. From is
  inclusive; To is exclusive; absent To means open-ended current/future state.
- **ACCEPTED:** State effective at date `T` is the applicable interval containing
  `T`, not necessarily the most recently recorded mutation.
- **ACCEPTED:** Future-dated state is recorded before it becomes current. Current
  state is the state containing the evaluation date. Past state is retained as
  historical business truth.
- **ACCEPTED:** For a single-valued state dimension, accepted intervals for the same
  semantic scope do not overlap. Concurrent relationships/assignments require
  separately identified scopes or explicit cardinality rules.
- **ACCEPTED:** A business-significant change creates/amends effective state while
  preserving semantic identity. Cosmetic/non-semantic change classification is a
  domain rule, not a storage rule.
- **ACCEPTED:** Termination closes active employment/assignment state at the exclusive
  boundary and establishes the appropriate terminated state; it does not erase history.
- **ACCEPTED:** Organization and position assignments have their own effective
  intervals and do not mutate Employee identity.
- **ACCEPTED:** Historical correction changes the asserted valid-time timeline through
  an authority command with concurrency and provenance. It does not masquerade as a
  new business event when the intent is correction.
- **DEFERRED:** overlap/cardinality rules per assignment type, retroactive correction
  policy, closed-period controls, and rehire interval/identity behavior.

## 8. System time, audit, and provenance

| Concern | Meaning | Classification |
|---|---|---|
| Business valid time | When a state is true in the business domain. | **ACCEPTED** |
| Recorded/system time | When the authority recorded a representation/change. | **ACCEPTED** |
| Audit event | Evidence that an operation occurred and its audit-relevant outcome. | **ACCEPTED** |
| Mutation provenance | Actor/source/context attached to a mutation request/record. | **ACCEPTED** |

- **ACCEPTED:** v0.1 requires effective-dated semantics and sufficient mutation
  provenance/audit evidence; it does not require universal bitemporal domain state.
- **ACCEPTED:** Historical corrections must remain attributable. Recorded timestamps
  and audit evidence may support this without making every domain entity bitemporal.
- **DEFERRED:** A domain may adopt bitemporal semantics only after demonstrating a
  query/evidence requirement to answer both “valid when?” and “known/recorded when?”
  for prior assertions. Its persistence design remains later work.
- **UNKNOWN:** which legacy domains have statutory or evidentiary bitemporal needs.

## 9. Optimistic concurrency

- **ACCEPTED:** `EntityVersion` is the technology-neutral logical version of one
  authoritative mutable aggregate/state boundary. The exact aggregate boundaries
  are domain decisions; a version is not a timestamp or physical row-version promise.
- **ACCEPTED:** A caller submits `ExpectedVersion` obtained from its read. The authority
  compares it with `CurrentVersion` for the same mutable authority boundary.
- **ACCEPTED:** equality plus successful authorization and validation permits the
  mutation and advances the version exactly as the authority contract defines; the
  resulting current version is returned.
- **ACCEPTED:** mismatch yields `Conflict` with the current version, makes no requested
  state change, and is surfaced to the caller. An invalid expectation yields
  `InvalidExpectedVersion`.
- **REJECTED:** blind overwrite and automatic merge of business-significant state by
  default. A future explicit domain merge policy may be proposed separately.
- **DEFERRED:** aggregate granularity, command idempotency, and physical concurrency
  mechanism.

## 10. Mutation provenance

Every accepted authoritative mutation carries the Foundation `ProvenanceContext`:

- **ACCEPTED — `ActorId`:** stable identity of the acting human, service, or agent in
  the relevant actor namespace; it is not necessarily `TS24UserId`.
- **ACCEPTED — `SourceApplication` / `SourceModule`:** logical origin of the command,
  not proof of authority.
- **ACCEPTED — `CompanyContext`:** optional operating context when applicable; it is
  not an ownership or authorization assertion by itself.
- **ACCEPTED — `Timestamp`:** mutation/provenance timestamp, distinct from business
  effective time.
- **ACCEPTED — `CorrelationId`:** links the operation across processing boundaries.
- **ACCEPTED — `Operation` / `Reason`:** supplied when applicable to describe intent,
  especially correction or administratively significant change.
- **ACCEPTED:** provenance is validated and captured at the authority boundary and may
  feed audit evidence. It need not be embedded physically in every entity.
- **DEFERRED:** actor namespace resolution, retention, audit event shape, physical
  storage, and reason requirements per operation.

## 11. Revision and snapshot boundary

```text
Current/effective MasterData -> application working reference
                              -> application artifact revision/snapshot
                              -> immutable, historically reproducible evidence
```

- **ACCEPTED:** A snapshot is a purpose-shaped immutable capture, not a competing
  MasterData record and not a mechanism for updating MasterData.
- **ACCEPTED:** A revision is an application-owned business version; a snapshot is
  captured evidence; effective state is MasterData valid-time truth; an audit event
  is operation evidence; calculated history is an application-owned result. These
  concepts are not interchangeable.
- **ACCEPTED — TaxOnline:** declaration and each business-significant declaration
  revision, payload, submission state, government response, and sufficient employee/
  company evidence remain TaxOnline-owned.
- **ACCEPTED — iBHXH:** declaration revision, statutory payload, submission workflow,
  response, and sufficient employee/company evidence remain iBHXH-owned.
- **ACCEPTED — PayCalc24:** payroll configuration/inputs, calculation, approvals,
  period, employee-level result, and reproducibility evidence remain PayCalc24-owned
  calculated/business history.
- **ACCEPTED — DigiDokument:** document metadata/content reference, lifecycle, signed
  or finalized artifact, and evidence remain DigiDokument-owned document truth.
- **ACCEPTED — ContractSigning:** workflow instance, participants as represented for
  the workflow, approvals/signatures, versions, finalized artifact, and evidence
  remain ContractSigning-owned.
- **ACCEPTED:** Applications may retain canonical IDs alongside captured values for
  traceability, but later MasterData changes cannot rewrite finalized historical
  artifacts.
- **DEFERRED:** exact snapshot contents, capture/finalization point, retention, and
  immutability rules are owned by each application and require domain evidence.
- **UNKNOWN:** current legacy revision/snapshot implementations for all named apps.

## 12. Mutation authority and multi-application use

```text
Application
  -> MasterData command/API contract
  -> authorization/policy boundary
  -> ExpectedVersion concurrency check
  -> domain validation
  -> authoritative mutation
  -> version advance
  -> provenance/audit capture
```

- **ACCEPTED:** Multiple applications may propose commands; only the MasterData
  authority accepts or rejects and commits shared semantic changes.
- **ACCEPTED:** Source application identifies provenance, not data ownership. Calling
  the command/API does not make an application a parallel authority.
- **ACCEPTED:** Consumers resolve by stable ID and, when business meaning requires,
  by effective date; finalized application artifacts use their own snapshot/revision.
- **REJECTED:** arbitrary direct writes that bypass policy, concurrency, validation,
  versioning, or provenance.
- **DEFERRED:** final authorization engine/host, transport, deployment topology,
  command granularity, and policy ownership.

## 13. Open evidence register

The following are **UNKNOWN** because TASK 05 had no accessible application source:

- legacy Company/Group/Branch/organization definitions and identifiers;
- relationship taxonomy, hierarchy, cycles, and service-provider/customer behavior;
- Person matching, duplicates, merges, privacy sources, and account-link rules;
- Employee numbering, concurrent employments, termination, rehire, and transfer rules;
- organization/position assignment cardinality and retroactive correction practice;
- TS24 Link, Konect24, identity membership, and authorization integration behavior;
- application revision, snapshot, audit, calculated-history, and bitemporal behavior.

These gaps are not permission to invent defaults. They require evidence from the
corresponding application owners/repositories before application-specific policy,
migration mapping, or physical persistence architecture is approved.

## 14. Explicitly deferred physical decisions

**DEFERRED:** table/database/schema/column/index/foreign-key names and layouts; ORM;
GUID, UUID, ULID, integer, long, or string representation; temporal/audit table
layout; cross-schema foreign-key policy; migration SQL; Mongo document layout;
event sourcing; provider details; and all production storage mappings.

