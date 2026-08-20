# Data estate discovery evidence

TASK 04 provides the read-only collector at `tools/data-estate-inventory`. Its output is evidence, not architecture authority: it must not be used by itself to infer ownership, select canonical MasterData, normalize legacy schemas, or authorize migration.

No production system was connected to while implementing or validating this tool. The committed fixtures contain synthetic metadata only.

Expected evidence fields include `found`, `store_type`, `database`, `schema_or_collection`, `object`, `object_type`, `evidence_source`, `owner`, `dependency`, `risk`, and `unknown`. Missing ownership or dependency evidence remains `UNKNOWN`.
