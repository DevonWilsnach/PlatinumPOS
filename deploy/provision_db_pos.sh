#!/usr/bin/env bash
# Provision the platinumpos Postgres role + database on the VPS (super69 superuser via a
# temporary pg_hba trust line). Preserves existing app rules (platinumauth/raalewaste). Idempotent.
# Usage: provision_db_pos.sh <pw>
set -euo pipefail

HBA="/etc/postgresql/16/main/pg_hba.conf"
BAK="/etc/postgresql/16/main/pg_hba.conf.platinumpos.bak"
APP_DB="platinumpos"
APP_ROLE="platinumpos"
APP_PASS="${1:?app password required}"

[ -f "$BAK" ] || cp "$HBA" "$BAK"
trap 'cp "$BAK" "$HBA"; systemctl reload postgresql || true' ERR

# 1) Temp trust for super69.
TMP="$(mktemp)"
{
  echo "# --- platinumpos temporary trust (auto-removed) ---"
  echo "local   all   super69   trust"
  cat "$BAK"
} > "$TMP"
cp "$TMP" "$HBA"
systemctl reload postgresql
sleep 1

# 2) Role + database (idempotent).
psql -U super69 -d postgres -v ON_ERROR_STOP=1 <<SQL
DO \$\$
BEGIN
   IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '${APP_ROLE}') THEN
      CREATE ROLE ${APP_ROLE} LOGIN PASSWORD '${APP_PASS}';
   ELSE
      ALTER ROLE ${APP_ROLE} LOGIN PASSWORD '${APP_PASS}';
   END IF;
END
\$\$;
SQL

if ! psql -U super69 -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='${APP_DB}'" | grep -q 1; then
  psql -U super69 -d postgres -v ON_ERROR_STOP=1 -c "CREATE DATABASE ${APP_DB} OWNER ${APP_ROLE};"
fi

# 3) Restore baseline hba + prepend platinumpos access rules.
TMP2="$(mktemp)"
{
  echo "# --- platinumpos app access (prepended; auto-managed) ---"
  echo "local   ${APP_DB}   ${APP_ROLE}                  scram-sha-256"
  echo "host    ${APP_DB}   ${APP_ROLE}   127.0.0.1/32   scram-sha-256"
  echo "host    ${APP_DB}   ${APP_ROLE}   ::1/128        scram-sha-256"
  echo "# --- end platinumpos ---"
  cat "$BAK"
} > "$TMP2"
cp "$TMP2" "$HBA"
rm -f "$TMP" "$TMP2"
systemctl reload postgresql
sleep 1
trap - ERR

echo "PROVISION_POS_OK"
PGPASSWORD="${APP_PASS}" psql -h 127.0.0.1 -U "${APP_ROLE}" -d "${APP_DB}" -tAc "SELECT 'app_login_ok', current_user, current_database();"
