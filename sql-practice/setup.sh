#!/bin/bash
set -e
DB="practice.db"
rm -f "$DB"
sqlite3 "$DB" < schema.sql
sqlite3 "$DB" < seed.sql
echo "OK: $DB を作成しました"
sqlite3 "$DB" "SELECT '顧客数', COUNT(*) FROM customers;"
