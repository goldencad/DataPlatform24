-- DataPlatform24 MasterData schema v0.1
-- IDs are authority-generated UUIDv7 bytes in RFC 9562/network byte order.
-- This migration contains no legacy conversion and is not production deployment authorization.

-- Apply to the deployment-resolved, MasterData-owned database; the logical database
-- name is a deployment default and is intentionally not created or selected here.

CREATE TABLE md_mutation (
    mutation_id BINARY(16) NOT NULL,
    recorded_at DATETIME(6) NOT NULL,
    actor_namespace VARCHAR(100) NOT NULL,
    actor_id VARCHAR(255) NOT NULL,
    source_application VARCHAR(100) NOT NULL,
    source_module VARCHAR(100) NOT NULL,
    company_context_id BINARY(16) NULL,
    correlation_id VARCHAR(255) NOT NULL,
    operation VARCHAR(100) NULL,
    reason VARCHAR(1000) NULL,
    source_timestamp DATETIME(6) NULL,
    deployment_id VARCHAR(255) NULL,
    transaction_id VARCHAR(255) NULL,
    PRIMARY KEY (mutation_id),
    KEY ix_md_mutation_correlation (correlation_id),
    KEY ix_md_mutation_recorded (recorded_at)
) ENGINE=InnoDB;

CREATE TABLE md_company (
    company_id BINARY(16) NOT NULL,
    version BIGINT UNSIGNED NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (company_id),
    CONSTRAINT fk_md_company_created_mutation FOREIGN KEY (created_mutation_id)
        REFERENCES md_mutation (mutation_id)
) ENGINE=InnoDB;

ALTER TABLE md_mutation
    ADD CONSTRAINT fk_md_mutation_company_context FOREIGN KEY (company_context_id)
        REFERENCES md_company (company_id);

CREATE TABLE md_company_state (
    company_state_id BINARY(16) NOT NULL,
    company_id BINARY(16) NOT NULL,
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    display_name VARCHAR(500) NOT NULL,
    status_code VARCHAR(64) NOT NULL,
    description VARCHAR(1000) NULL,
    created_at DATETIME(6) NOT NULL,
    last_recorded_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    last_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (company_state_id),
    KEY ix_md_company_state_timeline (company_id, effective_from, effective_to),
    KEY ix_md_company_state_open_end (company_id, effective_to, effective_from),
    CONSTRAINT fk_md_company_state_owner FOREIGN KEY (company_id) REFERENCES md_company (company_id),
    CONSTRAINT fk_md_company_state_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT fk_md_company_state_last_mutation FOREIGN KEY (last_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_company_state_interval CHECK (effective_to IS NULL OR effective_to > effective_from)
) ENGINE=InnoDB;

CREATE TABLE md_company_group (
    company_group_id BINARY(16) NOT NULL,
    grouping_purpose VARCHAR(255) NOT NULL,
    version BIGINT UNSIGNED NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (company_group_id),
    CONSTRAINT fk_md_company_group_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id)
) ENGINE=InnoDB;

CREATE TABLE md_company_group_state (
    company_group_state_id BINARY(16) NOT NULL,
    company_group_id BINARY(16) NOT NULL,
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    display_name VARCHAR(500) NOT NULL,
    classification_code VARCHAR(64) NULL,
    status_code VARCHAR(64) NOT NULL,
    created_at DATETIME(6) NOT NULL,
    last_recorded_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    last_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (company_group_state_id),
    KEY ix_md_company_group_state_timeline (company_group_id, effective_from, effective_to),
    KEY ix_md_company_group_state_open_end (company_group_id, effective_to, effective_from),
    CONSTRAINT fk_md_company_group_state_owner FOREIGN KEY (company_group_id) REFERENCES md_company_group (company_group_id),
    CONSTRAINT fk_md_company_group_state_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT fk_md_company_group_state_last_mutation FOREIGN KEY (last_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_company_group_state_interval CHECK (effective_to IS NULL OR effective_to > effective_from)
) ENGINE=InnoDB;

CREATE TABLE md_company_group_membership (
    company_group_membership_id BINARY(16) NOT NULL,
    company_group_id BINARY(16) NOT NULL,
    company_id BINARY(16) NOT NULL,
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    created_at DATETIME(6) NOT NULL,
    last_recorded_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    last_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (company_group_membership_id),
    KEY ix_md_group_membership_group (company_group_id, company_id, effective_from, effective_to),
    KEY ix_md_group_membership_company (company_id, company_group_id, effective_from, effective_to),
    CONSTRAINT fk_md_group_membership_group FOREIGN KEY (company_group_id) REFERENCES md_company_group (company_group_id),
    CONSTRAINT fk_md_group_membership_company FOREIGN KEY (company_id) REFERENCES md_company (company_id),
    CONSTRAINT fk_md_group_membership_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT fk_md_group_membership_last_mutation FOREIGN KEY (last_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_group_membership_interval CHECK (effective_to IS NULL OR effective_to > effective_from)
) ENGINE=InnoDB;

CREATE TABLE md_company_relationship (
    company_relationship_id BINARY(16) NOT NULL,
    source_company_id BINARY(16) NOT NULL,
    target_company_id BINARY(16) NOT NULL,
    relationship_type VARCHAR(100) NOT NULL,
    version BIGINT UNSIGNED NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (company_relationship_id),
    KEY ix_md_company_relationship_forward (source_company_id, relationship_type, target_company_id),
    KEY ix_md_company_relationship_reverse (target_company_id, relationship_type, source_company_id),
    CONSTRAINT fk_md_company_relationship_source FOREIGN KEY (source_company_id) REFERENCES md_company (company_id),
    CONSTRAINT fk_md_company_relationship_target FOREIGN KEY (target_company_id) REFERENCES md_company (company_id),
    CONSTRAINT fk_md_company_relationship_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id)
) ENGINE=InnoDB;

CREATE TABLE md_company_relationship_state (
    company_relationship_state_id BINARY(16) NOT NULL,
    company_relationship_id BINARY(16) NOT NULL,
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    status_code VARCHAR(64) NOT NULL,
    descriptor VARCHAR(1000) NULL,
    created_at DATETIME(6) NOT NULL,
    last_recorded_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    last_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (company_relationship_state_id),
    KEY ix_md_company_relationship_state_timeline (company_relationship_id, effective_from, effective_to),
    KEY ix_md_company_relationship_state_open_end (company_relationship_id, effective_to, effective_from),
    CONSTRAINT fk_md_company_relationship_state_owner FOREIGN KEY (company_relationship_id) REFERENCES md_company_relationship (company_relationship_id),
    CONSTRAINT fk_md_company_relationship_state_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT fk_md_company_relationship_state_last_mutation FOREIGN KEY (last_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_company_relationship_state_interval CHECK (effective_to IS NULL OR effective_to > effective_from)
) ENGINE=InnoDB;

CREATE TABLE md_branch (
    branch_id BINARY(16) NOT NULL,
    company_id BINARY(16) NOT NULL,
    version BIGINT UNSIGNED NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (branch_id),
    KEY ix_md_branch_company (company_id, branch_id),
    CONSTRAINT fk_md_branch_company FOREIGN KEY (company_id) REFERENCES md_company (company_id),
    CONSTRAINT fk_md_branch_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id)
) ENGINE=InnoDB;

CREATE TABLE md_branch_state (
    branch_state_id BINARY(16) NOT NULL,
    branch_id BINARY(16) NOT NULL,
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    display_name VARCHAR(500) NOT NULL,
    status_code VARCHAR(64) NOT NULL,
    operating_details VARCHAR(1000) NULL,
    created_at DATETIME(6) NOT NULL,
    last_recorded_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    last_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (branch_state_id),
    KEY ix_md_branch_state_timeline (branch_id, effective_from, effective_to),
    KEY ix_md_branch_state_open_end (branch_id, effective_to, effective_from),
    CONSTRAINT fk_md_branch_state_owner FOREIGN KEY (branch_id) REFERENCES md_branch (branch_id),
    CONSTRAINT fk_md_branch_state_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT fk_md_branch_state_last_mutation FOREIGN KEY (last_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_branch_state_interval CHECK (effective_to IS NULL OR effective_to > effective_from)
) ENGINE=InnoDB;

CREATE TABLE md_organization_unit (
    organization_unit_id BINARY(16) NOT NULL,
    company_id BINARY(16) NOT NULL,
    version BIGINT UNSIGNED NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (organization_unit_id),
    KEY ix_md_organization_unit_company (company_id, organization_unit_id),
    CONSTRAINT fk_md_organization_unit_company FOREIGN KEY (company_id) REFERENCES md_company (company_id),
    CONSTRAINT fk_md_organization_unit_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id)
) ENGINE=InnoDB;

CREATE TABLE md_organization_unit_state (
    organization_unit_state_id BINARY(16) NOT NULL,
    organization_unit_id BINARY(16) NOT NULL,
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    unit_classification VARCHAR(32) NOT NULL,
    display_name VARCHAR(500) NOT NULL,
    status_code VARCHAR(64) NOT NULL,
    branch_id BINARY(16) NULL,
    created_at DATETIME(6) NOT NULL,
    last_recorded_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    last_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (organization_unit_state_id),
    KEY ix_md_organization_unit_state_timeline (organization_unit_id, effective_from, effective_to),
    KEY ix_md_organization_unit_state_open_end (organization_unit_id, effective_to, effective_from),
    KEY ix_md_organization_unit_state_branch (branch_id, effective_from, effective_to),
    CONSTRAINT fk_md_organization_unit_state_owner FOREIGN KEY (organization_unit_id) REFERENCES md_organization_unit (organization_unit_id),
    CONSTRAINT fk_md_organization_unit_state_branch FOREIGN KEY (branch_id) REFERENCES md_branch (branch_id),
    CONSTRAINT fk_md_organization_unit_state_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT fk_md_organization_unit_state_last_mutation FOREIGN KEY (last_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_organization_unit_state_interval CHECK (effective_to IS NULL OR effective_to > effective_from),
    CONSTRAINT ck_md_organization_unit_classification CHECK (unit_classification IN ('ORGANIZATION_UNIT', 'DEPARTMENT', 'TEAM'))
) ENGINE=InnoDB;

CREATE TABLE md_organization_relationship (
    organization_relationship_id BINARY(16) NOT NULL,
    source_organization_unit_id BINARY(16) NOT NULL,
    target_organization_unit_id BINARY(16) NOT NULL,
    relationship_type VARCHAR(100) NOT NULL,
    version BIGINT UNSIGNED NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (organization_relationship_id),
    KEY ix_md_organization_relationship_forward (source_organization_unit_id, relationship_type, target_organization_unit_id),
    KEY ix_md_organization_relationship_reverse (target_organization_unit_id, relationship_type, source_organization_unit_id),
    CONSTRAINT fk_md_organization_relationship_source FOREIGN KEY (source_organization_unit_id) REFERENCES md_organization_unit (organization_unit_id),
    CONSTRAINT fk_md_organization_relationship_target FOREIGN KEY (target_organization_unit_id) REFERENCES md_organization_unit (organization_unit_id),
    CONSTRAINT fk_md_organization_relationship_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id)
) ENGINE=InnoDB;

CREATE TABLE md_organization_relationship_state (
    organization_relationship_state_id BINARY(16) NOT NULL,
    organization_relationship_id BINARY(16) NOT NULL,
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    status_code VARCHAR(64) NOT NULL,
    created_at DATETIME(6) NOT NULL,
    last_recorded_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    last_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (organization_relationship_state_id),
    KEY ix_md_organization_relationship_state_timeline (organization_relationship_id, effective_from, effective_to),
    KEY ix_md_organization_relationship_state_open_end (organization_relationship_id, effective_to, effective_from),
    CONSTRAINT fk_md_organization_relationship_state_owner FOREIGN KEY (organization_relationship_id) REFERENCES md_organization_relationship (organization_relationship_id),
    CONSTRAINT fk_md_organization_relationship_state_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT fk_md_organization_relationship_state_last_mutation FOREIGN KEY (last_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_organization_relationship_state_interval CHECK (effective_to IS NULL OR effective_to > effective_from)
) ENGINE=InnoDB;

CREATE TABLE md_position (
    position_id BINARY(16) NOT NULL,
    company_id BINARY(16) NOT NULL,
    version BIGINT UNSIGNED NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (position_id),
    KEY ix_md_position_company (company_id, position_id),
    CONSTRAINT fk_md_position_company FOREIGN KEY (company_id) REFERENCES md_company (company_id),
    CONSTRAINT fk_md_position_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id)
) ENGINE=InnoDB;

CREATE TABLE md_position_state (
    position_state_id BINARY(16) NOT NULL,
    position_id BINARY(16) NOT NULL,
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    display_name VARCHAR(500) NOT NULL,
    job_title VARCHAR(500) NULL,
    status_code VARCHAR(64) NOT NULL,
    organization_unit_id BINARY(16) NULL,
    created_at DATETIME(6) NOT NULL,
    last_recorded_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    last_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (position_state_id),
    KEY ix_md_position_state_timeline (position_id, effective_from, effective_to),
    KEY ix_md_position_state_open_end (position_id, effective_to, effective_from),
    KEY ix_md_position_state_organization (organization_unit_id, effective_from, effective_to),
    CONSTRAINT fk_md_position_state_owner FOREIGN KEY (position_id) REFERENCES md_position (position_id),
    CONSTRAINT fk_md_position_state_organization FOREIGN KEY (organization_unit_id) REFERENCES md_organization_unit (organization_unit_id),
    CONSTRAINT fk_md_position_state_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT fk_md_position_state_last_mutation FOREIGN KEY (last_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_position_state_interval CHECK (effective_to IS NULL OR effective_to > effective_from)
) ENGINE=InnoDB;

CREATE TABLE md_person (
    person_id BINARY(16) NOT NULL,
    version BIGINT UNSIGNED NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (person_id),
    CONSTRAINT fk_md_person_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id)
) ENGINE=InnoDB;

CREATE TABLE md_person_state (
    person_state_id BINARY(16) NOT NULL,
    person_id BINARY(16) NOT NULL,
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    display_name VARCHAR(500) NOT NULL,
    status_code VARCHAR(64) NOT NULL,
    created_at DATETIME(6) NOT NULL,
    last_recorded_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    last_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (person_state_id),
    KEY ix_md_person_state_timeline (person_id, effective_from, effective_to),
    KEY ix_md_person_state_open_end (person_id, effective_to, effective_from),
    CONSTRAINT fk_md_person_state_owner FOREIGN KEY (person_id) REFERENCES md_person (person_id),
    CONSTRAINT fk_md_person_state_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT fk_md_person_state_last_mutation FOREIGN KEY (last_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_person_state_interval CHECK (effective_to IS NULL OR effective_to > effective_from)
) ENGINE=InnoDB;

CREATE TABLE md_employee (
    employee_id BINARY(16) NOT NULL,
    person_id BINARY(16) NOT NULL,
    employing_company_id BINARY(16) NOT NULL,
    version BIGINT UNSIGNED NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (employee_id),
    KEY ix_md_employee_company (employing_company_id, employee_id),
    KEY ix_md_employee_person (person_id, employing_company_id, employee_id),
    CONSTRAINT fk_md_employee_person FOREIGN KEY (person_id) REFERENCES md_person (person_id),
    CONSTRAINT fk_md_employee_company FOREIGN KEY (employing_company_id) REFERENCES md_company (company_id),
    CONSTRAINT fk_md_employee_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id)
) ENGINE=InnoDB;

CREATE TABLE md_employee_state (
    employee_state_id BINARY(16) NOT NULL,
    employee_id BINARY(16) NOT NULL,
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    employment_status_code VARCHAR(64) NOT NULL,
    terms_summary VARCHAR(1000) NULL,
    created_at DATETIME(6) NOT NULL,
    last_recorded_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    last_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (employee_state_id),
    KEY ix_md_employee_state_timeline (employee_id, effective_from, effective_to),
    KEY ix_md_employee_state_open_end (employee_id, effective_to, effective_from),
    CONSTRAINT fk_md_employee_state_owner FOREIGN KEY (employee_id) REFERENCES md_employee (employee_id),
    CONSTRAINT fk_md_employee_state_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT fk_md_employee_state_last_mutation FOREIGN KEY (last_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_employee_state_interval CHECK (effective_to IS NULL OR effective_to > effective_from)
) ENGINE=InnoDB;

CREATE TABLE md_employee_org_assignment (
    employee_org_assignment_id BINARY(16) NOT NULL,
    employee_id BINARY(16) NOT NULL,
    organization_unit_id BINARY(16) NOT NULL,
    version BIGINT UNSIGNED NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (employee_org_assignment_id),
    KEY ix_md_employee_org_assignment_employee (employee_id, organization_unit_id, employee_org_assignment_id),
    KEY ix_md_employee_org_assignment_unit (organization_unit_id, employee_id, employee_org_assignment_id),
    CONSTRAINT fk_md_employee_org_assignment_employee FOREIGN KEY (employee_id) REFERENCES md_employee (employee_id),
    CONSTRAINT fk_md_employee_org_assignment_unit FOREIGN KEY (organization_unit_id) REFERENCES md_organization_unit (organization_unit_id),
    CONSTRAINT fk_md_employee_org_assignment_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id)
) ENGINE=InnoDB;

CREATE TABLE md_employee_org_assignment_state (
    employee_org_assignment_state_id BINARY(16) NOT NULL,
    employee_org_assignment_id BINARY(16) NOT NULL,
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    status_code VARCHAR(64) NOT NULL,
    created_at DATETIME(6) NOT NULL,
    last_recorded_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    last_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (employee_org_assignment_state_id),
    KEY ix_md_employee_org_assignment_state_timeline (employee_org_assignment_id, effective_from, effective_to),
    KEY ix_md_employee_org_assignment_state_open_end (employee_org_assignment_id, effective_to, effective_from),
    CONSTRAINT fk_md_employee_org_assignment_state_owner FOREIGN KEY (employee_org_assignment_id) REFERENCES md_employee_org_assignment (employee_org_assignment_id),
    CONSTRAINT fk_md_employee_org_assignment_state_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT fk_md_employee_org_assignment_state_last_mutation FOREIGN KEY (last_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_employee_org_assignment_state_interval CHECK (effective_to IS NULL OR effective_to > effective_from)
) ENGINE=InnoDB;

CREATE TABLE md_employee_position_assignment (
    employee_position_assignment_id BINARY(16) NOT NULL,
    employee_id BINARY(16) NOT NULL,
    position_id BINARY(16) NOT NULL,
    version BIGINT UNSIGNED NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (employee_position_assignment_id),
    KEY ix_md_employee_position_assignment_employee (employee_id, position_id, employee_position_assignment_id),
    KEY ix_md_employee_position_assignment_position (position_id, employee_id, employee_position_assignment_id),
    CONSTRAINT fk_md_employee_position_assignment_employee FOREIGN KEY (employee_id) REFERENCES md_employee (employee_id),
    CONSTRAINT fk_md_employee_position_assignment_position FOREIGN KEY (position_id) REFERENCES md_position (position_id),
    CONSTRAINT fk_md_employee_position_assignment_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id)
) ENGINE=InnoDB;

CREATE TABLE md_employee_position_assignment_state (
    employee_position_assignment_state_id BINARY(16) NOT NULL,
    employee_position_assignment_id BINARY(16) NOT NULL,
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    status_code VARCHAR(64) NOT NULL,
    created_at DATETIME(6) NOT NULL,
    last_recorded_at DATETIME(6) NOT NULL,
    created_mutation_id BINARY(16) NOT NULL,
    last_mutation_id BINARY(16) NOT NULL,
    PRIMARY KEY (employee_position_assignment_state_id),
    KEY ix_md_employee_position_assignment_state_timeline (employee_position_assignment_id, effective_from, effective_to),
    KEY ix_md_employee_position_assignment_state_open_end (employee_position_assignment_id, effective_to, effective_from),
    CONSTRAINT fk_md_employee_position_assignment_state_owner FOREIGN KEY (employee_position_assignment_id) REFERENCES md_employee_position_assignment (employee_position_assignment_id),
    CONSTRAINT fk_md_employee_position_assignment_state_created_mutation FOREIGN KEY (created_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT fk_md_employee_position_assignment_state_last_mutation FOREIGN KEY (last_mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_employee_position_assignment_state_interval CHECK (effective_to IS NULL OR effective_to > effective_from)
) ENGINE=InnoDB;

CREATE TABLE md_audit_event (
    audit_event_id BINARY(16) NOT NULL,
    mutation_id BINARY(16) NOT NULL,
    recorded_at DATETIME(6) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    aggregate_kind VARCHAR(100) NOT NULL,
    aggregate_id BINARY(16) NOT NULL,
    prior_version BIGINT UNSIGNED NULL,
    result_version BIGINT UNSIGNED NOT NULL,
    outcome VARCHAR(64) NOT NULL,
    before_digest VARBINARY(64) NULL,
    after_digest VARBINARY(64) NULL,
    PRIMARY KEY (audit_event_id),
    KEY ix_md_audit_event_mutation (mutation_id),
    KEY ix_md_audit_event_aggregate (aggregate_kind, aggregate_id, recorded_at),
    CONSTRAINT fk_md_audit_event_mutation FOREIGN KEY (mutation_id) REFERENCES md_mutation (mutation_id),
    CONSTRAINT ck_md_audit_event_versions CHECK (prior_version IS NULL OR result_version = prior_version + 1)
) ENGINE=InnoDB;

CREATE TABLE md_audit_affected_state (
    audit_event_id BINARY(16) NOT NULL,
    state_kind VARCHAR(100) NOT NULL,
    state_id BINARY(16) NOT NULL,
    PRIMARY KEY (audit_event_id, state_kind, state_id),
    CONSTRAINT fk_md_audit_affected_state_event FOREIGN KEY (audit_event_id) REFERENCES md_audit_event (audit_event_id)
) ENGINE=InnoDB;

-- No external identity mapping is created: normalization, scope, assurance and use are deferred.
-- No Organization Structure or Job Title catalog identity is created: their identity semantics are deferred.
-- No uniqueness/cardinality constraint is inferred for relationship types, employment, rehire, or assignments.
