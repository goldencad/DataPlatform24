# Data estate inventory

This is a small, read-only evidence collector for MariaDB/MySQL metadata, MongoDB metadata, and narrow configuration references. It does not make a connection when run without a subcommand. Connections require an explicit command and an explicit local JSON config file.

## Safety boundary

- MariaDB/MySQL uses fixed `SELECT` statements against `information_schema` plus `SELECT VERSION()`.
- MongoDB uses metadata operations only: `listCollections` and `listIndexes`. It never calls `find`, samples documents, or exports business data.
- Config evidence is redacted for common secret keys and credential-bearing database URIs.
- No credentials are included in this repository. Keep local connection files outside source control and use least-privilege metadata credentials.
- Evidence reports `UNKNOWN` for ownership and dependencies unless the source explicitly provides them.

## Usage

```text
python3 tools/data-estate-inventory/inventory.py
python3 tools/data-estate-inventory/inventory.py fixture --store mysql --input tools/data-estate-inventory/fixtures/mysql-metadata.json
python3 tools/data-estate-inventory/inventory.py fixture --store mongo --input tools/data-estate-inventory/fixtures/mongo-metadata.json
python3 tools/data-estate-inventory/inventory.py scan-config --path path/to/config-or-repository
python3 tools/data-estate-inventory/inventory.py mysql --config /outside/repository/mysql.json
python3 tools/data-estate-inventory/inventory.py mongo --config /outside/repository/mongo.json
```

The optional live commands require `mysql-connector-python` or `pymongo`, respectively. A MySQL config contains connector keyword arguments. A MongoDB config contains `uri`, `database`, and optionally `serverSelectionTimeoutMS`. Connection errors are redacted before printing.

Run focused validation from the tooling directory:

```text
python3 -m unittest discover -s tests -v
python3 -m compileall -q inventory.py tests
```
