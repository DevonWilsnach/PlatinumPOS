#!/usr/bin/env bash
# Deploy PlatinumPOS to /opt/platinumpos as systemd service `platinumpos` on :1172.
# Auth pinned to auth.platinumbrink.com via appsettings.Production.json (config wins).
# Usage: deploy_pos.sh <db_password>   (tarball at /tmp/pos_publish.tar.gz)
set -euo pipefail

APP_DIR="/opt/platinumpos"
TARBALL="/tmp/pos_publish.tar.gz"
UNIT="/etc/systemd/system/platinumpos.service"
PORT="1172"
APP_PASS="${1:?db password required}"
CONN="Host=127.0.0.1;Port=5432;Database=platinumpos;Username=platinumpos;Password=${APP_PASS}"

echo "== extract PlatinumPOS =="
systemctl stop platinumpos 2>/dev/null || true
mkdir -p "$APP_DIR"
rm -rf "${APP_DIR:?}/"*
tar -xzf "$TARBALL" -C "$APP_DIR"
test -f "$APP_DIR/PlatinumPOS.dll"

echo "== write systemd unit =="
cat > "$UNIT" <<UNITEOF
[Unit]
Description=PlatinumPOS (point of sale)
After=network.target postgresql.service
Wants=postgresql.service

[Service]
Type=simple
WorkingDirectory=${APP_DIR}
ExecStart=/usr/bin/dotnet ${APP_DIR}/PlatinumPOS.dll
Restart=always
RestartSec=5
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:${PORT}
Environment=DOTNET_CLI_TELEMETRY_OPTOUT=1
Environment="ConnectionStrings__Default=${CONN}"
Environment=Licence__AuthBaseUrl=https://auth.platinumbrink.com

[Install]
WantedBy=multi-user.target
UNITEOF

echo "== start platinumpos =="
systemctl daemon-reload
systemctl enable platinumpos >/dev/null 2>&1
systemctl restart platinumpos
sleep 8
echo "== status =="; systemctl is-active platinumpos
echo "== listening? =="; ss -tlnp | grep ${PORT} || echo "NOT LISTENING"
echo "== local smoke =="; curl -s -o /dev/null -w 'local pos / -> HTTP %{http_code}\n' http://127.0.0.1:${PORT}/ || true
echo "== logs =="; journalctl -u platinumpos -n 20 --no-pager | grep -iE 'listening|exception|error|npgsql|database' || journalctl -u platinumpos -n 12 --no-pager
echo "DEPLOY_POS_OK"
