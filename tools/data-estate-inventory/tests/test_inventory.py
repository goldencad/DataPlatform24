import json
import tempfile
import unittest
from pathlib import Path

import inventory


class InventoryTests(unittest.TestCase):
    def test_no_connection_mode(self):
        self.assertEqual(0, inventory.main([]))

    def test_fixture_normalizes_unknown_owner(self):
        path = Path(__file__).parents[1] / "fixtures" / "mysql-metadata.json"
        record = inventory.fixture_records(path, "MariaDB/MySQL")[0]
        self.assertEqual("UNKNOWN", record["owner"])
        self.assertEqual("MariaDB/MySQL", record["store_type"])

    def test_config_scan_redacts_secrets(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "appsettings.json"
            path.write_text(json.dumps({"ConnectionStrings": {"Main": "Server=db;Database=legacy;User=app;Password=dont-print-me"}, "Mongo": "mongodb://alice:hunter2@db/legacy"}), encoding="utf-8")
            output = json.dumps(inventory.scan_config(path))
            self.assertNotIn("dont-print-me", output)
            self.assertNotIn("hunter2", output)
            self.assertIn("[REDACTED]", output)

    def test_all_mysql_queries_are_select_only(self):
        self.assertTrue(all(query.lstrip().upper().startswith("SELECT ") for query in inventory.MYSQL_QUERIES.values()))
        forbidden = ("INSERT", "UPDATE", "DELETE", "CREATE", "ALTER", "DROP")
        self.assertFalse(any(word in query.upper().split() for query in inventory.MYSQL_QUERIES.values() for word in forbidden))


if __name__ == "__main__":
    unittest.main()
