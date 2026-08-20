from pathlib import Path
import re
import unittest


ROOT = Path(__file__).resolve().parents[2]
DDL_PATH = ROOT / "database/masterdata/migrations/001_create_masterdata_schema.sql"
DDL = DDL_PATH.read_text(encoding="utf-8")


def table_body(table: str) -> str:
    match = re.search(
        rf"CREATE TABLE {re.escape(table)} \((.*?)\n\) ENGINE=InnoDB;",
        DDL,
        flags=re.DOTALL,
    )
    if match is None:
        raise AssertionError(f"missing table {table}")
    return match.group(1)


class MasterDataSchemaTests(unittest.TestCase):
    headers = (
        "md_company",
        "md_company_group",
        "md_company_relationship",
        "md_branch",
        "md_organization_unit",
        "md_organization_relationship",
        "md_position",
        "md_person",
        "md_employee",
        "md_employee_org_assignment",
        "md_employee_position_assignment",
    )
    states = (
        "md_company_state",
        "md_company_group_state",
        "md_company_relationship_state",
        "md_branch_state",
        "md_organization_unit_state",
        "md_organization_relationship_state",
        "md_position_state",
        "md_person_state",
        "md_employee_state",
        "md_employee_org_assignment_state",
        "md_employee_position_assignment_state",
    )

    def test_expected_schema_relations_exist(self):
        for table in self.headers + self.states + (
            "md_company_group_membership",
            "md_mutation",
            "md_audit_event",
            "md_audit_affected_state",
        ):
            table_body(table)

    def test_headers_have_binary_identity_and_unsigned_version(self):
        for table in self.headers:
            body = table_body(table)
            identifier = table.removeprefix("md_") + "_id"
            self.assertRegex(body, rf"\b{identifier} BINARY\(16\) NOT NULL")
            self.assertIn("version BIGINT UNSIGNED NOT NULL DEFAULT 0", body)
            self.assertIn("created_mutation_id BINARY(16) NOT NULL", body)

    def test_states_are_effective_dated_and_audited(self):
        for table in self.states:
            body = table_body(table)
            self.assertIn("effective_from DATE NOT NULL", body)
            self.assertIn("effective_to DATE NULL", body)
            self.assertIn("effective_to IS NULL OR effective_to > effective_from", body)
            self.assertIn("created_at DATETIME(6) NOT NULL", body)
            self.assertIn("last_recorded_at DATETIME(6) NOT NULL", body)
            self.assertIn("created_mutation_id BINARY(16) NOT NULL", body)
            self.assertIn("last_mutation_id BINARY(16) NOT NULL", body)
            self.assertRegex(body, r"KEY ix_\w+_timeline \(\w+_id, effective_from, effective_to\)")
            self.assertRegex(body, r"KEY ix_\w+_open_end \(\w+_id, effective_to, effective_from\)")

    def test_semantic_anchors_are_explicit(self):
        employee = table_body("md_employee")
        self.assertIn("person_id BINARY(16) NOT NULL", employee)
        self.assertIn("employing_company_id BINARY(16) NOT NULL", employee)
        self.assertIn("FOREIGN KEY (person_id) REFERENCES md_person", employee)
        self.assertIn("FOREIGN KEY (employing_company_id) REFERENCES md_company", employee)
        self.assertIn("KEY ix_md_employee_company (employing_company_id, employee_id)", employee)
        self.assertIn("KEY ix_md_employee_person (person_id, employing_company_id, employee_id)", employee)

    def test_relationship_and_assignment_access_paths_exist(self):
        self.assertIn(
            "(source_company_id, relationship_type, target_company_id)",
            table_body("md_company_relationship"),
        )
        self.assertIn(
            "(target_company_id, relationship_type, source_company_id)",
            table_body("md_company_relationship"),
        )
        self.assertIn(
            "(organization_unit_id, employee_id, employee_org_assignment_id)",
            table_body("md_employee_org_assignment"),
        )
        self.assertIn(
            "(position_id, employee_id, employee_position_assignment_id)",
            table_body("md_employee_position_assignment"),
        )

    def test_provenance_and_audit_are_separate_from_business_time(self):
        mutation = table_body("md_mutation")
        for field in (
            "actor_namespace",
            "actor_id",
            "source_application",
            "source_module",
            "company_context_id",
            "correlation_id",
            "operation",
            "reason",
            "recorded_at",
        ):
            self.assertRegex(mutation, rf"\b{field}\b")
        self.assertNotIn("effective_from", mutation)
        audit = table_body("md_audit_event")
        self.assertIn("(aggregate_kind, aggregate_id, recorded_at)", audit)
        self.assertNotRegex(audit.lower(), r"\b(payload|document_content|payroll|declaration)\b")

    def test_no_cross_authority_or_cascade_foreign_keys(self):
        references = re.findall(r"REFERENCES\s+([a-zA-Z0-9_]+)", DDL)
        self.assertTrue(references)
        self.assertTrue(all(name.startswith("md_") for name in references))
        self.assertNotIn("ON DELETE CASCADE", DDL.upper())
        self.assertNotRegex(DDL.upper(), r"\b(CREATE DATABASE|USE TS24_MASTERDATA)\b")

    def test_deferred_models_are_not_invented(self):
        lowered = "\n".join(
            line for line in DDL.lower().splitlines() if not line.lstrip().startswith("--")
        )
        self.assertNotIn("is_current", lowered)
        self.assertNotIn("employee_number", lowered)
        self.assertNotIn("rehire", lowered)
        self.assertNotRegex(lowered, r"create table md_(organization_structure|job_title|external_identity_mapping)\b")
        self.assertNotRegex(lowered, r"\b(json|longblob|mediumblob)\b")


if __name__ == "__main__":
    unittest.main()
