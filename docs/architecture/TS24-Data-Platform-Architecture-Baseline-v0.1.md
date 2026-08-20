# TS24 Data Platform — Architecture Baseline v0.1

**Status:** IMPLEMENTATION BASELINE  
**Project:** TS24 Data Platform  
**Target:** .NET 9 / TS24 Application Platform  
**Authority:** TS24 Application Platform Constitution hiện hành  
**Purpose:** Kiến trúc nền tảng để bắt đầu xây dựng dự án trên Codex.

---

## 1. Architecture Mission

TS24 Data Platform là shared persistent-data foundation của hệ sinh thái TS24.

Nền tảng cung cấp:

- shared persistence architecture;
- canonical semantic identity cho shared enterprise data;
- authoritative MasterData;
- temporal/effective-dated state;
- versioning và optimistic concurrency;
- provenance/audit contracts;
- multi-store architecture;
- application data ownership governance;
- migration/version infrastructure;
- deployment-neutral persistence;
- Licensing/Entitlement foundation độc lập;
- contracts cho Human UI, application integration và AI Agent truy cập dữ liệu qua authorized capabilities.

TS24 Data Platform không phải:

- giant shared business database;
- ORM library dùng chung cho mọi Core;
- database bắt buộc cho pure computational cores;
- replacement cho application business domains;
- replacement cho Konect24;
- replacement cho TS24 Cloud Identity;
- replacement cho TS24 Link;
- authorization engine;
- mandatory TS24 Cloud runtime dependency.

---

## 2. Fundamental Architecture Law

```text
Share what represents the same enterprise truth.

Isolate what represents application business truth.
```

Và:

```text
Logical Authority
!=
Physical Storage
```

Và:

```text
Canonical Identity
!=
Central Cloud Dependency
```

Và:

```text
Physical DB Co-location
!=
Cross-Application Ownership
```

---

## 3. Platform Applicability

TS24 Data Platform áp dụng cho component:

```text
owns persistent data
OR
persists data
OR
queries persistent data
OR
synchronizes persistent data
OR
migrates persistent data
OR
exposes persistent data
OR
integrates persistent data
```

Không bắt buộc áp dụng cho pure Core:

```text
Input
  ↓
Pure Processing
  ↓
Output
```

Ví dụ:

- Calculation Engine;
- Formula Parser;
- AST Engine;
- pure XML transformation;
- pure validation;
- rendering core;
- cryptographic primitive/core;

không phụ thuộc Data Platform nếu không trực tiếp làm việc với persistence.

---

## 4. High-Level Platform Architecture

```text
┌────────────────────────────────────────────────────────────┐
│                    TS24 Applications                       │
│                                                            │
│ TaxOnline   iBHXH   PayCalc24   DigiDokument   Others      │
└───────────────┬────────────────────────────────────────────┘
                │
                │ Application API / Core
                │
        ┌───────▼──────────────────────┐
        │ Application Business Domain │
        │                              │
        │ Application-owned truth      │
        └───────┬──────────────────────┘
                │
                │ approved contracts
                ▼
┌────────────────────────────────────────────────────────────┐
│                  TS24 Data Platform                        │
│                                                            │
│  ┌───────────────────┐  ┌──────────────────────────────┐   │
│  │ Data Foundation   │  │ MasterData Authority         │   │
│  │                   │  │                              │   │
│  │ persistence       │  │ Company                      │   │
│  │ transactions      │  │ Person                       │   │
│  │ concurrency       │  │ Employee                     │   │
│  │ migrations        │  │ Organization                 │   │
│  │ provenance        │  │ Shared catalogs              │   │
│  └───────────────────┘  └──────────────────────────────┘   │
│                                                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Licensing / Entitlement Authority                   │  │
│  │ Product / Edition / Module / Capability / Service   │  │
│  └──────────────────────────────────────────────────────┘  │
└───────────────────┬────────────────────────────────────────┘
                    │
          ┌─────────┴───────────┐
          ▼                     ▼
       MariaDB                MongoDB
  relational foundation   application-specific
                           document workloads
```

Future approved stores có thể gồm:

```text
Object Storage
File Storage
Search Index
Bounded Cache
Specialized datastore
```

Không datastore application-specific nào tự trở thành MasterData authority.

---

## 5. Main Architecture Tracks

TS24 Data Platform chia thành ba authority tracks.

### TRACK A — Data Foundation

Chịu trách nhiệm về:

- persistence contracts;
- transaction contracts;
- concurrency contracts;
- migration/version contracts;
- datastore providers;
- provenance infrastructure;
- data access diagnostics;
- resilience;
- deployment-aware connection resolution;
- backup/restore contracts;
- multi-store architecture.

Track A không sở hữu Company, Employee hay business domain.

### TRACK B — MasterData

Authority cho shared enterprise truth.

Candidate semantic domains:

```text
Company
Company Group
Company Relationship
Branch

Organization Structure
Organization Unit
Department
Team
Position / Job Title

Person
Employee

Shared Catalogs
```

Danh sách trên là semantic candidate model.

Không được suy ra production tables từ danh sách này trước legacy inventory.

### TRACK C — Licensing / Entitlement

Authority riêng cho:

```text
Product
Edition
Module
Capability
Entitlement
Limit
Cloud Service Entitlement
Lifecycle
```

Luật:

```text
Licensing
!=
Authorization
!=
Business Domain
```

Licensing trả lời:

> Organization/customer được sử dụng capability nào?

Authorization trả lời:

> Actor nào được phép gọi capability đó?

Application trả lời:

> Capability đó thực hiện nghiệp vụ gì?

---

## 6. Authority Model

### 6.1 Shared MasterData authority

Shared MasterData sở hữu canonical enterprise truth.

Application không tạo MasterData authority riêng khi chỉ cần sử dụng cùng semantic entity.

Target:

```text
Application A ─┐
Application B ─┼──> MasterData Authority
Application C ─┘
```

Không:

```text
App A Employee
 ↕
App B Employee
 ↕
App C Employee
```

---

## 7. Application Business Ownership

Mỗi application tiếp tục sở hữu business truth của mình.

### PayCalc24

Sở hữu:

- payroll configuration;
- payroll inputs;
- payroll calculations;
- employee-level payroll facts/results;
- payroll periods;
- payroll approvals;
- payroll domain history.

References:

```text
CompanyId
EmployeeId
PersonId when applicable
```

### TaxOnline

Sở hữu:

- declaration;
- declaration period;
- revisions;
- tax position;
- working payload;
- form payload;
- XML;
- submission state;
- government response;
- evidence/snapshot cần để tái hiện hồ sơ.

TaxOnline không trở thành Employee Master.

### iBHXH

Tương tự:

- declaration;
- revision;
- submission workflow;
- employee data snapshot cần cho hồ sơ;
- statutory payload.

Không sở hữu permanent Employee Master chỉ vì hồ sơ chứa người lao động.

### DigiDokument

Sở hữu:

- document-domain truth;
- document metadata;
- document content reference;
- document lifecycle;
- document index/domain state.

MongoDB có thể được sử dụng cho workload này.

DigiDokument references canonical shared IDs nhưng không trở thành Company/Employee authority.

---

## 8. Semantic Identity Architecture

Các shared concepts phải có stable semantic identity.

Candidate IDs:

```text
CompanyId
PersonId
EmployeeId
OrganizationUnitId
DepartmentId
PositionId
BranchId
```

IDs:

- immutable về semantic identity;
- không phụ thuộc display text;
- không phụ thuộc localized name;
- không tái sử dụng cho entity khác;
- không đổi chỉ vì mutable state thay đổi.

---

## 9. User, Person và Employee

Hard invariant:

```text
TS24 User
!=
Person
!=
Employee
```

Conceptually:

```text
TS24UserId
     │
     │ optional mapping
     ▼
 PersonId
     │
     ├──────── EmployeeId @ Company A
     │
     └──────── EmployeeId @ Company B
```

Employee có thể tồn tại mà không có TS24 Account.

TS24 Account có thể là:

- employee;
- external representative;
- accountant/service operator;
- administrator;
- consultant;
- other actor.

Không nhập ba concepts thành một entity.

---

## 10. Organization Membership vs Enterprise Structure

Hard invariant:

```text
Access Organization / Membership
!=
Enterprise Organization Structure
```

Access membership:

```text
User
 ↓
Membership
 ↓
Company / Organization Access Context
```

Enterprise structure:

```text
Company
 ├─ Branch
 ├─ Department
 ├─ Team
 └─ Position
```

Hai models có thể reference cùng Company semantic identity nhưng có authority khác nhau.

---

## 11. Temporal MasterData Architecture

MasterData không chỉ giữ current state.

Phải hỗ trợ:

```text
Semantic Identity
+
Effective-dated State
```

Ví dụ:

```text
Employee E001

2025-01-01 → 2026-05-31
Department = Accounting
Status = Active

2026-06-01 → 2026-08-31
Department = Finance
Status = Active

2026-09-01 →
Status = Terminated
```

EmployeeId không thay đổi.

---

## 12. Temporal State != Audit History

Hai concern phải tách biệt.

### Temporal State

Trả lời:

> Business state nào có hiệu lực tại thời điểm T?

### Audit History

Trả lời:

> Ai thay đổi dữ liệu, từ application nào, khi nào?

Ví dụ:

```text
EffectiveFrom = 2026-06-01

RecordedAt = 2026-05-25 14:22
RecordedBy = User U001
SourceApplication = PayCalc24
```

Hai timestamp có semantic khác nhau.

---

## 13. Revision, Snapshot, Audit và Temporal State

Architecture phải phân biệt:

```text
Effective State
Revision
Snapshot
Audit Event
Calculated History
```

Không dùng một generic History table để biểu diễn mọi loại lịch sử.

### Effective State

Business state thay đổi theo effective date.

### Revision

Version nghiệp vụ của một business artifact.

Ví dụ declaration revision.

### Snapshot

Copy immutable của dữ liệu cần để tái hiện business artifact.

### Audit

Mutation provenance.

### Calculated History

Business result thuộc application domain.

Ví dụ payroll result.

---

## 14. MasterData Mutation Model

Shared MasterData hỗ trợ multi-application mutation nhưng tất cả mutation phải qua authority.

Conceptual flow:

```text
Application
    ↓
MasterData Command Contract
    ↓
Authorization / policy
    ↓
Concurrency validation
    ↓
Domain validation
    ↓
Mutation
    ↓
Version increment
    ↓
Audit / provenance
```

Không application nào được bypass authority bằng arbitrary SQL UPDATE.

---

## 15. Optimistic Concurrency

Mọi mutable authoritative entity/state cần concurrency semantics.

Concept:

```text
Read Version N
      ↓
Modify
      ↓
Submit ExpectedVersion = N
```

Authority kiểm tra:

```text
CurrentVersion == ExpectedVersion
```

Nếu không:

```text
CONFLICT
```

Không blind overwrite.

Concurrency conflict phải được surface cho caller.

Không tự động merge business-significant state nếu chưa có explicit merge policy.

---

## 16. Provenance

Mutation cần có provenance tối thiểu ở contract level:

```text
ActorId
SourceApplication
SourceModule
CompanyContext
Timestamp
CorrelationId
Reason/Operation
```

Không nhất thiết tất cả thành columns trên từng entity.

Physical representation quyết định sau.

---

## 17. Snapshot Law

Application artifact cần historical reproducibility phải giữ snapshot/revision đủ để tái hiện artifact.

Ví dụ:

```text
MasterData Employee
       ↓
Tax declaration creation
       ↓
Declaration Revision Snapshot
```

Sau đó Employee thay đổi:

```text
MasterData current state changes
```

không được làm thay đổi declaration revision lịch sử.

Hard invariant:

```text
Historical Business Evidence
must not depend exclusively
on mutable current MasterData.
```

---

## 18. MariaDB Architecture

MariaDB là relational foundation chính.

Target logical layout:

```text
MariaDB
├── shared platform relational data
├── MasterData relational data
├── Licensing relational data
└── application-owned relational data
```

Architecture không bắt buộc:

- một physical database duy nhất;
- một schema duy nhất;
- một MariaDB instance duy nhất.

Database placement được resolve theo deployment configuration.

---

## 19. Physical Co-location Law

Ví dụ có thể tồn tại:

```text
ts24_platform
ts24_masterdata
taxonline
paycalc24
ibhxh
```

trên cùng MariaDB instance.

Nhưng:

```text
same MariaDB instance
!=
permission to query/write arbitrary tables
```

Application-private data phải được truy cập qua application-approved boundary.

Cross-schema SQL chỉ được phép nếu Architecture Spec sau này explicit approve một integration case.

Default:

```text
NO arbitrary cross-app write.
```

---

## 20. Multi-Store Architecture

MariaDB không phải datastore duy nhất.

Approved architecture:

```text
Shared MasterData
      ↓
MariaDB
```

Application-specific:

```text
DigiDokument
      ↓
MongoDB
```

Có thể:

```text
Document binary
      ↓
Object/File Store

Search projection
      ↓
Search Index
```

Nhưng:

```text
Projection
Cache
Search Index
Document Store
```

không được tự trở thành canonical enterprise authority.

---

## 21. Store Ownership

Mỗi persistent store phải có:

```text
Owning Module
Owning Domain
Approved Writers
Approved Readers
Migration Authority
Backup Responsibility
Restore Responsibility
```

Không có orphan dataset.

---

## 22. Data Access Architecture

Business code không làm việc trực tiếp với database-specific implementation.

Target:

```text
Application/Core
      ↓
Domain-facing contract
      ↓
Repository / Provider / Data Service
      ↓
Persistence implementation
      ↓
MariaDB / MongoDB / Other
```

Database technology không leak vào pure business contracts.

---

## 23. Application-to-Application Data Access

Default:

```text
App A
 ↓
App B Application API/Core
 ↓
App B Domain
 ↓
App B Persistence
```

Hoặc:

```text
App A
 ↓
Shared MasterData API/Core
 ↓
MasterData Persistence
```

Không:

```text
App A
 ↓
App B private tables
```

---

## 24. Konect24 Boundary

Konect24 là connectivity/access backbone.

Konect24 có thể:

- route requests;
- discover endpoints;
- manage internal connectivity;
- support private deployment;
- expose application capabilities;
- support AI Agent access;
- eventually participate in centralized internal authorization.

Konect24 không sở hữu application business truth.

```text
Transport / Routing
!=
Business Authority
```

---

## 25. TS24 Link Boundary

TS24 Link là Customer ↔ TS24 service application.

Responsibility gồm:

- customer relationship;
- Company registration/update flows;
- organization membership management;
- service registration;
- renewal;
- support interaction.

TS24 Link authorization không mặc định bằng internal application authorization.

Hard distinction:

```text
Customer ↔ TS24 Authorization
!=
Internal Application Authorization
```

---

## 26. Identity Boundary

Canonical account identity:

```text
TS24 Cloud Identity
→ TS24UserId
```

Application không được tạo một competing global TS24 identity.

Framework-local representations như Odoo user:

```text
OdooUser
   ↓ mapping
TS24UserId
```

được phép.

---

## 27. Authorization Direction

Candidate target:

```text
Applications
→ define semantic capabilities

Central Authorization
→ manages assignments / roles / policies

Applications
→ enforce capabilities
```

Konect24 là candidate host/authority cho centralized internal authorization.

Tuy nhiên implementation authorization migration là project riêng/bounded track và không được ngầm thực hiện trong Data Platform foundation.

---

## 28. Licensing Resolution

Candidate effective access:

```text
Valid Identity
      ∩
Organization Membership
      ∩
License Entitlement
      ∩
Application Assignment
      ∩
Authorization
      ∩
Company Context
      ∩
Application Policy
      =
Effective Capability
```

Data Platform Licensing chịu trách nhiệm entitlement component, không toàn bộ expression.

---

## 29. Human / AI Agent Parity

Human UI và AI Agent phải gọi cùng authorized business capabilities.

Target:

```text
Human UI ─────┐
              ├── Application API/Core
AI Agent ─────┘
                      ↓
                 Business Domain
                      ↓
                  Persistence
```

Forbidden:

```text
AI Agent
   ↓
arbitrary SQL/Mongo access
```

Agent action phải có:

- actor/delegation identity;
- Company Context;
- membership;
- entitlement;
- authorization;
- application policy;
- audit/provenance.

---

## 30. Deployment Neutrality

Supported deployment profiles:

```text
LOCAL
ON_PREMISE
CUSTOMER_CLOUD
TS24_CLOUD
CLOUD24_PRIVATE
HYBRID
```

Business domain semantics phải giống nhau giữa deployment models.

Forbidden:

```text
if Cloud24Private then different payroll rule
```

Deployment concern phải nằm infrastructure/configuration layer.

---

## 31. Connectivity Model

Connectivity là dimension riêng:

```text
TS24_MANAGED
PRIVATE
HYBRID
ISOLATED
```

Ví dụ:

```text
ON_PREMISE + TS24_MANAGED
```

khác:

```text
ON_PREMISE + PRIVATE
```

Deployment profile không quyết định tự động connectivity mode.

---

## 32. Cloud24 Private

Cloud24 Private cho phép business payload nằm trong trust boundary khách hàng.

Có thể deploy:

- applications;
- Konect24/private connectivity;
- MariaDB;
- MongoDB;
- document services;
- signing components;
- AI Agent;
- other approved services.

Canonical IDs và application semantics vẫn giữ nguyên.

Hard law:

```text
Shared Semantic Identity
does not require
central TS24 Cloud data transit.
```

---

## 33. Integration Classes

Ba integration classes là architecture-level distinction.

### Class A — Licensed Government Integration

```text
Application
    ↓
TS24 Licensed Service
    ↓
Government System
```

Ví dụ:

- Tax;
- BHXH;
- Invoice;
- Customs.

Application owns statutory business integration semantics.

Endpoint/configuration là system-managed.

### Class B — Private-Capable TS24 Service

Ví dụ ContractSigning.

Có thể chạy:

```text
TS24-hosted
customer-hosted
Cloud24 Private
```

### Class C — Internal Direct Application Integration

```text
App A
 ↓
Konect24 / private connection
 ↓
App B API/Core
```

Không yêu cầu TS24 Cloud transit.

---

## 34. Government Integration Rule

Licensed government integrations là:

```text
application-owned
+
system-managed
+
zero user endpoint configuration
```

User không cấu hình:

- service URL;
- host;
- protocol;
- port.

Tuy nhiên endpoint không được hard-code rải rác trong business source.

Platform integration configuration/service discovery có thể quản lý endpoint/version.

---

## 35. Transaction Boundary

Transactions thuộc owning authority.

Default:

```text
one authoritative module
=
one transactional boundary
```

Không xây distributed transaction xuyên arbitrary application databases.

Cross-domain operation dùng explicit orchestration nếu cần.

Không dựa vào việc các schema nằm cùng MariaDB để tạo implicit multi-domain transaction.

---

## 36. Consistency Model

Within authority:

```text
strong transactional consistency where required
```

Across authorities:

```text
explicit contract
+
versioned exchange
+
event/API/orchestration where applicable
```

Không mặc định shared transaction xuyên application domains.

---

## 37. Migration Architecture

Data Platform phải có migration framework nhưng production migration chỉ được tạo sau discovery.

Migration requirements:

```text
versioned
repeatable where appropriate
observable
recoverable
tenant/deployment aware
backward-compatibility conscious
```

Không destructive migration nếu chưa có:

- backup path;
- rollback/recovery plan;
- legacy mapping;
- validation.

---

## 38. Legacy Database Rule

Existing database là evidence, không phải architecture authority.

Nhưng:

```text
Architecture must understand legacy reality
before production schema replacement.
```

Codex không được:

- rename production tables;
- normalize existing schemas;
- migrate legacy Employee/Company;
- delete duplicated data;

trước inventory và mapping task.

---

## 39. Backup / Restore Architecture

Backup responsibility phải theo owned persistent dataset.

Platform phải hỗ trợ inventory và coordination cho:

```text
MariaDB
MongoDB
files/object store
other persistent stores
```

Restore semantics phải xem xét consistency giữa stores khi business artifact trải qua nhiều storage technologies.

Chi tiết implementation deferred đến dedicated architecture task.

---

## 40. Observability

Persistence operations cần support:

```text
CorrelationId
Application
Module
CompanyContext
Operation
Duration
Success/Failure
Concurrency Conflict
Migration Version
Store
```

Không log sensitive/raw business payload mặc định.

---

## 41. Security Boundary

Data Platform phải:

- không chứa secrets trong domain entities;
- không expose raw DB credential tới UI;
- support least-privilege datastore credentials;
- enforce authority boundaries;
- provide audit/provenance hooks;
- support private deployment;
- support application-level authorization enforcement.

Data Platform không tự trở thành Security24 hoặc Authorization authority.

---

## 42. Initial Logical Modules

Repository có thể bắt đầu với logical modules sau.

Tên package/project cuối cùng có thể điều chỉnh, nhưng responsibility boundaries được khóa.

```text
TS24.DataPlatform
│
├── Foundation
│   ├── Abstractions
│   ├── Transactions
│   ├── Concurrency
│   ├── Provenance
│   ├── Migrations
│   └── Diagnostics
│
├── MasterData
│   ├── Contracts
│   ├── Domain
│   ├── Application
│   └── Persistence
│
├── Licensing
│   ├── Contracts
│   ├── Domain
│   ├── Application
│   └── Persistence
│
├── Providers
│   ├── MariaDb
│   └── MongoDb
│
├── Deployment
│   ├── Configuration
│   └── ConnectionResolution
│
└── Tests
```

Đây là logical source architecture, không phải physical database schema.

---

## 43. Dependency Direction

Hard rule:

```text
Domain
  ↓
Abstractions

Application
  ↓
Domain + Abstractions

Persistence Provider
  ↓
Abstractions + owning Domain contracts

UI/API Host
  ↓
Application
```

Forbidden:

```text
Domain
 ↓
MariaDB driver
```

Forbidden:

```text
MasterData Domain
 ↓
TaxOnline
```

Forbidden:

```text
Foundation
 ↓
Application business module
```

---

## 44. Foundation Contracts That May Be Implemented Before DB Inventory

Codex được phép bắt đầu các technology-neutral contracts:

```text
Semantic identity abstractions
Entity/version abstractions
Effective date primitives
Concurrency result model
Provenance context
Transaction abstraction
Persistence capability abstraction
Migration metadata abstraction
Data access diagnostic contracts
Store registration
Deployment connection descriptors
```

Các contracts không được assume Company table structure.

---

## 45. MasterData Contracts That May Be Started

Có thể xây semantic interfaces/use-case contracts cho:

```text
Get by stable ID
Resolve state effective at date
Create semantic entity
Amend effective state
Query timeline
Concurrency-aware mutation
Provenance-aware mutation
```

Không được chốt database columns/indexes trước inventory.

---

## 46. Explicit Deferred Decisions

Các vấn đề sau chưa được Codex tự quyết:

```text
exact Company table schema
exact Employee table schema
UUID vs alternative physical ID format
schema/database naming
bi-temporal implementation
effective-state physical representation
event sourcing
ORM choice
repository implementation details
cross-schema FK policy
authorization host
organization membership authority
legacy migration mapping
Mongo document schemas
backup orchestration technology
```

Mỗi decision cần evidence/task riêng.

---

## 47. Initial Project Invariants

Tất cả Codex tasks phải bảo vệ các invariants:

1. Shared enterprise truth có một canonical authority.
2. Application business truth thuộc application.
3. User != Person != Employee.
4. Membership != enterprise organization structure.
5. Stable semantic identity không đổi theo display/current state.
6. Temporal business history != audit history.
7. Revision != snapshot != audit != effective state.
8. Shared MasterData mutation phải concurrency-aware.
9. Application-specific store không trở thành MasterData authority.
10. Physical DB co-location không tạo cross-app ownership.
11. Application không arbitrary write private tables của application khác.
12. AI không bypass Application API/Core.
13. Licensing != Authorization != Business Domain.
14. Deployment != Connectivity.
15. Private deployment không đồng nghĩa air-gapped.
16. Canonical identity không tạo mandatory TS24 Cloud runtime dependency.
17. Government integrations là application-owned/system-managed.
18. Business semantics không đổi theo hosting model.
19. Pure Core không phụ thuộc persistence foundation nếu không cần persistence.
20. Production schema không được thiết kế trước legacy inventory phù hợp.

---

## 48. Codex Workflow

### Level A — Architecture

Dùng strong model cho:

- MasterData semantic design;
- identity;
- temporal model;
- authority;
- concurrency;
- Licensing;
- deployment/trust;
- authorization boundary;
- migration architecture;
- difficult legacy conflict.

### Level B — Technical Implementation

Dùng mid-tier cho:

- approved contracts;
- providers;
- migration infrastructure;
- adapters;
- test harness;
- application integration.

### Level C — Mechanical

Dùng lightweight cho:

- DB inventory;
- grep;
- schema listing;
- dependency mapping;
- build;
- tests;
- git;
- docs;
- CI.

---

## 49. Mandatory Task Handoff

Mọi task:

```text
BASELINE:
AUTHORITY SHA:
TASK LEVEL:

SCOPE:

OWNERSHIP:

FILES / AREAS:

INVARIANTS:

NON-GOALS:

ACCEPTANCE:

RETURN FORMAT:

STOP CONDITIONS:
```

Task discovery thêm:

```text
EVIDENCE REQUIRED:
CURRENT STORE:
CURRENT OWNER:
UNKNOWN:
```

Không paste toàn bộ Constitution hay toàn bộ architecture vào mỗi task.

Chỉ đưa clauses liên quan.

---

## 50. Initial Codex Build Roadmap

### TASK 01 — Repository & Architecture Skeleton

**Level:** B/C

Mục tiêu:

- tạo solution .NET 9;
- dựng logical module boundaries;
- architecture tests;
- no production DB schema;
- no MariaDB dependency trong Domain.

Deliverables:

```text
Foundation
MasterData
Licensing
Providers
Deployment
Tests
docs
```

### TASK 02 — Foundation Contracts

**Level:** A → B

Thiết kế và implement:

- semantic ID base contracts;
- version/concurrency primitives;
- provenance context;
- effective-date primitives;
- transaction abstraction;
- datastore abstraction;
- diagnostics contracts.

Không entity schema.

### TASK 03 — Architecture Boundary Tests

**Level:** B

Tests enforce:

```text
Domain !-> MariaDB
Domain !-> MongoDB
Foundation !-> Apps
MasterData !-> app business domains
Licensing independent from Authorization
```

### TASK 04 — Existing Data Estate Inventory Tooling

**Level:** C

Xây tooling/script read-only để inventory:

- MariaDB/MySQL;
- MongoDB;
- configuration references;
- schema metadata;
- collections;
- FK;
- views;
- procedures;
- cross-schema references.

Không mutation.

### TASK 05 — Existing Application Persistence Inventory

**Level:** C

Inventory source:

```text
TaxOnline
iBHXH
PayCalc24
DigiDokument
TS24 Link
Konect24
Odoo integrations
others
```

Return factual evidence only.

### TASK 06 — MasterData Semantic Model

**Level:** A

Dựa trên Task 04/05 evidence.

Chốt semantic boundaries:

```text
Company
Person
Employee
Organization
Branch
Department
Position
```

Không implementation trước approval.

### TASK 07 — Identity Mapping Architecture

**Level:** A

Thiết kế:

```text
TS24UserId
PersonId
EmployeeId
CompanyId
Organization Membership
Enterprise Organization
Odoo mapping
```

### TASK 08 — Temporal & Concurrency Architecture

**Level:** A

Chốt:

- effective-dating;
- correction;
- termination;
- rehire;
- assignment history;
- concurrency;
- provenance;
- snapshot boundary.

### TASK 09 — MasterData Persistence Model

**Level:** A

Chỉ sau Tasks 04–08.

Lúc này mới được propose relational schema.

Schema vẫn phải qua architecture review trước migration.

### TASK 10 — MariaDB Provider Foundation

**Level:** B

Implement approved persistence architecture.

### TASK 11 — Licensing Semantic Architecture

**Level:** A

Chốt:

```text
Product
Edition
Module
Capability
Limit
Entitlement
Cloud Service Entitlement
```

Không nhập Authorization.

### TASK 12 — Licensing Persistence

**Level:** B

Implement sau Task 11 approval.

### TASK 13 — Multi-Store Architecture

**Level:** A/B

Validate DigiDokument split:

```text
shared identity → Data Platform
document truth → DigiDokument/MongoDB
```

### TASK 14 — Deployment / Connection Resolution

**Level:** A/B

Support:

```text
LOCAL
ON_PREMISE
CUSTOMER_CLOUD
TS24_CLOUD
CLOUD24_PRIVATE
HYBRID
```

Không business branching.

### TASK 15 — Data Migration Framework

**Level:** A/B

Versioned migration infrastructure.

Không migrate production customers yet.

### TASK 16 — Backup / Restore Architecture

**Level:** A

Cross-store recovery semantics.

### TASK 17 — Konect24 Integration Contracts

**Level:** A/B

Define access boundary, routing contracts and Data Platform capability exposure.

Không move authorization yet.

### TASK 18 — Human / AI Capability Access

**Level:** A

Ensure AI uses approved API/Core capability paths.

### TASK 19 — First Application Adoption Pilot

Chỉ sau foundation ổn định.

Chọn một bounded use case có rủi ro thấp để validate:

```text
canonical Company reference
+
MasterData read
+
concurrency-aware update
```

Không migrate toàn bộ TS24 ecosystem một lần.

### TASK 20 — Constitution v1.3 Proposal

Sau khi architecture đã được chứng minh bằng implementation/pilot.

Promote platform laws, không đưa schema detail vào Constitution.

---

## 51. First Implementation Boundary

Codex được phép bắt đầu ngay:

```text
Task 01
Task 02 architecture
Task 03
Task 04
Task 05
```

Có thể chạy song song:

```text
01 ──> 02 ──> 03

04 ──────────┐
05 ──────────┴──> 06/07/08
```

Không được bắt đầu:

```text
09 MasterData physical schema
10 production MariaDB implementation
production migration
legacy data conversion
```

cho đến khi inventory evidence đủ.

---

## 52. Definition of Architecture Ready

Data Platform foundation được xem là architecture-ready khi:

- module boundaries được enforce;
- shared/domain authority rõ;
- persistence abstraction không leak vào pure domain;
- existing data estate được inventory;
- canonical identity semantics được chốt;
- temporal model được chốt;
- concurrency semantics được chốt;
- MasterData physical model được review;
- Licensing boundary được chốt;
- multi-store rules được chốt;
- deployment model được kiểm chứng;
- migration path có evidence;
- architecture tests GREEN.

---

## 53. Architecture North Star

```text
                  CANONICAL IDENTITY
                         │
                         ▼
               AUTHORITATIVE MASTERDATA
                         │
           ┌─────────────┼─────────────┐
           ▼             ▼             ▼
       PayCalc24      TaxOnline      iBHXH
       Payroll        Declaration    Declaration
        Truth            Truth          Truth

                         │
                         ▼
                    DigiDokument
                    Document Truth
                       MongoDB
```

Surrounding authorities:

```text
TS24 Cloud Identity
        │
        ▼
    TS24UserId

TS24 Link
        │
        ▼
Customer / Membership interaction

Konect24
        │
        ▼
Connectivity / Access / API routing

Licensing
        │
        ▼
Entitlement

Applications
        │
        ▼
Business Capability Authority
```

---

## 54. Final Architecture Rule

TS24 Data Platform phải làm cho application TS24:

```text
share identity without sharing business ownership;

share MasterData without sharing arbitrary tables;

share persistence foundation without forcing one datastore;

share deployment semantics without forcing TS24 Cloud;

share authorized capabilities between Human and AI
without granting direct database authority.
```

Đó là foundation architecture của TS24 Data Platform.

**Implementation may begin from Tasks 01–05.**

**Production MasterData schema remains gated by current-database discovery.**
