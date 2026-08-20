# MasterData MariaDB schema

`migrations/001_create_masterdata_schema.sql` is the proposed, non-production v0.1
physical schema. It contains only MasterData-owned relations and uses no ORM.
It applies to the deployment-resolved current database and deliberately does not
create or select a physical catalog. Ordered/checksummed ledger execution belongs
to the separately owned MariaDB migration infrastructure.

All canonical semantic IDs are authority-generated UUIDv7 values bound as 16 bytes
in RFC 9562 network-byte order. MariaDB does not generate or convert these IDs.
Header rows retain immutable anchors and a `BIGINT UNSIGNED` logical version starting
at zero. Effective state is stored separately with `[effective_from, effective_to)`
`DATE` intervals. There is deliberately no `is_current` column.

## Required authority transaction

DDL constraints do not claim to prevent all timeline overlap or deferred semantic
cardinality. Every accepted timeline mutation must execute in one MasterData-owned
transaction:

1. insert the immutable `md_mutation` provenance row;
2. lock the aggregate header with `SELECT ... FOR UPDATE`;
3. verify `version = expected_version` and reject invalid/overflow expectations;
4. query the relevant state/relationship/assignment scope for interval overlap;
5. validate only the taxonomy and cardinality approved by the authority layer;
6. write/split/close state rows and their mutation references;
7. update the owning header with `WHERE id = ? AND version = ?`;
8. insert `md_audit_event` and affected-state references; and
9. commit atomically.

Employee organization/position assignment changes must lock and advance the owning
Employee version as well as any occurrence version used by the command. Empty-range
races are serialized by locking the stable owner before overlap validation.

## Deliberately deferred

The DDL does not create an explicit Organization Structure identity, a separate Job
Title catalog, external/legacy identity mappings, employee numbers, account-to-Person
links, or taxonomy-driven uniqueness/cardinality. It also does not decide rehire,
concurrent employment, relationship cycles, primary assignments, or status/type
vocabularies. Code columns are bounded but their reviewed values remain authority
policy. These omissions prevent physical storage from deciding deferred semantics.

Run static validation with:

```sh
python3 -m unittest discover -s tests/schema -p 'test_*.py'
```

A disposable MariaDB apply is optional and was not added as a production pathway.
