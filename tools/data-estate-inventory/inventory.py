#!/usr/bin/env python3
"""Read-only data-estate evidence collector."""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any, Iterable


FIELDS = ("found", "store_type", "database", "schema_or_collection", "object", "object_type", "evidence_source", "owner", "dependency", "risk", "unknown")
ALLOWED_CONFIG_SUFFIXES = {".json", ".yaml", ".yml", ".config", ".xml", ".ini", ".properties", ".env", ".cs", ".csproj"}
SKIP_DIRS = {".git", "bin", "obj", "node_modules", ".idea", ".vs"}
SECRET_RE = re.compile(r"(?i)(password|pwd|token|secret|access[_-]?key)\s*([=:])\s*([^;\s,}\"']+|\"[^\"]*\"|'[^']*')")
URI_SECRET_RE = re.compile(r"(?i)(mongodb(?:\+srv)?|mysql|mariadb)://([^/@:\s]+):([^@/\s]+)@")
REFERENCE_RE = re.compile(r"(?i)\b(mariadb|mysql|mongodb|mongo|connectionstrings?|database|schema|collection|dbcontext|datasource|provider)\b")


def evidence(**values: Any) -> dict[str, Any]:
    item = {field: "UNKNOWN" for field in FIELDS}
    item.update(values)
    return item


def redact(value: str) -> str:
    value = SECRET_RE.sub(lambda m: f"{m.group(1)}{m.group(2)}[REDACTED]", value)
    return URI_SECRET_RE.sub(lambda m: f"{m.group(1)}://[REDACTED]@", value)


def load_json(path: Path) -> Any:
    return json.loads(path.read_text(encoding="utf-8"))


def fixture_records(path: Path, store_type: str) -> list[dict[str, Any]]:
    payload = load_json(path)
    records = payload.get("evidence", payload)
    if not isinstance(records, list):
        raise ValueError("fixture must contain an evidence array")
    normalized = []
    for record in records:
        item = evidence(store_type=store_type, evidence_source=str(path))
        item.update(record)
        normalized.append(item)
    return normalized


MYSQL_QUERIES = {
    "server": "SELECT VERSION() AS version",
    "tables_views": "SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM information_schema.TABLES WHERE TABLE_SCHEMA NOT IN ('information_schema','mysql','performance_schema','sys') ORDER BY TABLE_SCHEMA,TABLE_NAME",
    "columns": "SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, COLUMN_DEFAULT FROM information_schema.COLUMNS WHERE TABLE_SCHEMA NOT IN ('information_schema','mysql','performance_schema','sys') ORDER BY TABLE_SCHEMA,TABLE_NAME,ORDINAL_POSITION",
    "keys": "SELECT tc.CONSTRAINT_SCHEMA,tc.TABLE_NAME,tc.CONSTRAINT_NAME,tc.CONSTRAINT_TYPE,kcu.COLUMN_NAME,kcu.REFERENCED_TABLE_SCHEMA,kcu.REFERENCED_TABLE_NAME,kcu.REFERENCED_COLUMN_NAME FROM information_schema.TABLE_CONSTRAINTS tc JOIN information_schema.KEY_COLUMN_USAGE kcu ON tc.CONSTRAINT_SCHEMA=kcu.CONSTRAINT_SCHEMA AND tc.TABLE_NAME=kcu.TABLE_NAME AND tc.CONSTRAINT_NAME=kcu.CONSTRAINT_NAME WHERE tc.CONSTRAINT_SCHEMA NOT IN ('information_schema','mysql','performance_schema','sys') ORDER BY tc.CONSTRAINT_SCHEMA,tc.TABLE_NAME,tc.CONSTRAINT_NAME,kcu.ORDINAL_POSITION",
    "indexes": "SELECT TABLE_SCHEMA,TABLE_NAME,INDEX_NAME,NON_UNIQUE,COLUMN_NAME,SEQ_IN_INDEX FROM information_schema.STATISTICS WHERE TABLE_SCHEMA NOT IN ('information_schema','mysql','performance_schema','sys') ORDER BY TABLE_SCHEMA,TABLE_NAME,INDEX_NAME,SEQ_IN_INDEX",
    "triggers": "SELECT TRIGGER_SCHEMA,TRIGGER_NAME,EVENT_MANIPULATION,EVENT_OBJECT_SCHEMA,EVENT_OBJECT_TABLE,ACTION_TIMING FROM information_schema.TRIGGERS ORDER BY TRIGGER_SCHEMA,TRIGGER_NAME",
    "routines": "SELECT ROUTINE_SCHEMA,ROUTINE_NAME,ROUTINE_TYPE,DATA_TYPE FROM information_schema.ROUTINES ORDER BY ROUTINE_SCHEMA,ROUTINE_NAME",
}


def mysql_inventory(config_path: Path) -> list[dict[str, Any]]:
    try:
        import mysql.connector  # type: ignore[import-not-found]
    except ImportError as exc:
        raise RuntimeError("mysql-connector-python is required only for an explicit MySQL/MariaDB connection") from exc
    config = load_json(config_path)
    connection = mysql.connector.connect(**config)
    try:
        records: list[dict[str, Any]] = []
        cursor = connection.cursor(dictionary=True)
        try:
            for category, query in MYSQL_QUERIES.items():
                if not query.lstrip().upper().startswith("SELECT "):
                    raise RuntimeError("non-read-only query rejected")
                cursor.execute(query)
                for row in cursor.fetchall():
                    schema = row.get("TABLE_SCHEMA") or row.get("CONSTRAINT_SCHEMA") or row.get("TRIGGER_SCHEMA") or row.get("ROUTINE_SCHEMA") or config.get("database", "UNKNOWN")
                    name = row.get("TABLE_NAME") or row.get("TRIGGER_NAME") or row.get("ROUTINE_NAME") or row.get("version", "server")
                    dependency = "UNKNOWN"
                    if row.get("REFERENCED_TABLE_NAME"):
                        dependency = f"{row.get('REFERENCED_TABLE_SCHEMA')}.{row['REFERENCED_TABLE_NAME']}.{row.get('REFERENCED_COLUMN_NAME')}"
                    records.append(evidence(found=True, store_type="MariaDB/MySQL", database=config.get("database", "UNKNOWN"), schema_or_collection=schema, object=name, object_type=category, evidence_source=f"information_schema:{category}", dependency=dependency, risk="cross-schema reference" if row.get("REFERENCED_TABLE_SCHEMA") and row.get("REFERENCED_TABLE_SCHEMA") != schema else "UNKNOWN", details=row))
        finally:
            cursor.close()
        return records
    finally:
        connection.close()


def mongo_inventory(config_path: Path) -> list[dict[str, Any]]:
    try:
        from pymongo import MongoClient  # type: ignore[import-not-found]
    except ImportError as exc:
        raise RuntimeError("pymongo is required only for an explicit MongoDB connection") from exc
    config = load_json(config_path)
    uri = config.get("uri")
    database_name = config.get("database")
    if not uri or not database_name:
        raise ValueError("MongoDB config requires explicit uri and database")
    client = MongoClient(uri, serverSelectionTimeoutMS=config.get("serverSelectionTimeoutMS", 5000))
    try:
        database = client[database_name]
        records: list[dict[str, Any]] = []
        for metadata in database.list_collections(filter={"type": "collection"}):
            name = metadata["name"]
            safe_options = metadata.get("options", {})
            records.append(evidence(found=True, store_type="MongoDB", database=database_name, schema_or_collection=name, object=name, object_type="collection", evidence_source="listCollections", details={"type": metadata.get("type"), "validator": safe_options.get("validator", "UNKNOWN"), "validationLevel": safe_options.get("validationLevel", "UNKNOWN"), "validationAction": safe_options.get("validationAction", "UNKNOWN")}))
            for index in database[name].list_indexes():
                records.append(evidence(found=True, store_type="MongoDB", database=database_name, schema_or_collection=name, object=index.get("name", "UNKNOWN"), object_type="index", evidence_source="listIndexes", details={"key": dict(index.get("key", {})), "unique": index.get("unique", False), "sparse": index.get("sparse", False), "expireAfterSeconds": index.get("expireAfterSeconds", "UNKNOWN")}))
        return records
    finally:
        client.close()


def iter_config_files(root: Path) -> Iterable[Path]:
    if root.is_file():
        if root.suffix.lower() in ALLOWED_CONFIG_SUFFIXES:
            yield root
        return
    for path in root.rglob("*"):
        if path.is_file() and path.suffix.lower() in ALLOWED_CONFIG_SUFFIXES and not any(part in SKIP_DIRS for part in path.parts):
            yield path


def scan_config(root: Path) -> list[dict[str, Any]]:
    records: list[dict[str, Any]] = []
    for path in iter_config_files(root):
        try:
            lines = path.read_text(encoding="utf-8", errors="replace").splitlines()
        except OSError:
            continue
        for number, line in enumerate(lines, 1):
            if REFERENCE_RE.search(line):
                snippet = redact(line.strip())[:500]
                store = "MongoDB" if re.search(r"(?i)mongo", line) else "MariaDB/MySQL" if re.search(r"(?i)maria|mysql", line) else "UNKNOWN"
                records.append(evidence(found=True, store_type=store, object=snippet, object_type="configuration reference", evidence_source=f"{path}:{number}", risk="secret-like value redacted" if "[REDACTED]" in snippet else "UNKNOWN"))
    return records


def parser() -> argparse.ArgumentParser:
    result = argparse.ArgumentParser(description="Collect read-only data-estate metadata evidence")
    commands = result.add_subparsers(dest="command")
    for name in ("mysql", "mongo"):
        command = commands.add_parser(name, help=f"explicit {name} metadata connection")
        command.add_argument("--config", required=True, type=Path, help="local JSON connection configuration")
    fixture = commands.add_parser("fixture", help="render fixture metadata without a connection")
    fixture.add_argument("--store", required=True, choices=("mysql", "mongo"))
    fixture.add_argument("--input", required=True, type=Path)
    scan = commands.add_parser("scan-config", help="narrow repository/config reference scan")
    scan.add_argument("--path", required=True, type=Path)
    return result


def main(argv: list[str] | None = None) -> int:
    args = parser().parse_args(argv)
    try:
        if args.command is None:
            records = [evidence(found=False, store_type="UNKNOWN", object_type="safe no-connection mode", evidence_source="command line", risk="UNKNOWN", unknown="No inventory source selected; no connection attempted")]
        elif args.command == "mysql":
            records = mysql_inventory(args.config)
        elif args.command == "mongo":
            records = mongo_inventory(args.config)
        elif args.command == "fixture":
            records = fixture_records(args.input, "MariaDB/MySQL" if args.store == "mysql" else "MongoDB")
        else:
            records = scan_config(args.path)
        print(json.dumps({"mode": args.command or "no-connection", "read_only": True, "evidence": records}, indent=2, default=str))
        return 0
    except (OSError, ValueError, RuntimeError, json.JSONDecodeError) as exc:
        print(json.dumps({"read_only": True, "error": redact(str(exc))}), file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
