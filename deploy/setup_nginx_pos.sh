#!/usr/bin/env bash
# nginx vhost + TLS for pos.platinumbrink.com -> 127.0.0.1:1172 (Blazor Server, needs WS upgrade).
set -euo pipefail

FQDN="pos.platinumbrink.com"
UPSTREAM="127.0.0.1:1172"
SITE="/etc/nginx/sites-available/platinumpos"
LINK="/etc/nginx/sites-enabled/platinumpos"
MAP="/etc/nginx/conf.d/platinumpos_ws.conf"

echo "== websocket upgrade map (unique var) =="
cat > "$MAP" <<'EOF'
# PlatinumPOS: map Upgrade header for Blazor Server (SignalR) websocket proxying
map $http_upgrade $pos_connection_upgrade {
    default upgrade;
    ''      close;
}
EOF

echo "== http vhost (certbot adds 443 after) =="
cat > "$SITE" <<EOF
server {
    listen 80;
    listen [::]:80;
    server_name ${FQDN};

    client_max_body_size 20m;

    location / {
        proxy_pass         http://${UPSTREAM};
        proxy_http_version 1.1;
        proxy_set_header   Upgrade            \$http_upgrade;
        proxy_set_header   Connection         \$pos_connection_upgrade;
        proxy_set_header   Host               \$host;
        proxy_set_header   X-Real-IP          \$remote_addr;
        proxy_set_header   X-Forwarded-For    \$proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto  \$scheme;
        proxy_buffering    off;
        proxy_read_timeout 100s;
    }
}
EOF

ln -sf "$SITE" "$LINK"
nginx -t
systemctl reload nginx

echo "== obtain TLS cert =="
certbot --nginx -d "${FQDN}" --non-interactive --agree-tos -m devonwilsnach@gmail.com --redirect || echo "CERTBOT_FAILED"
nginx -t && systemctl reload nginx

echo "== local vhost smoke =="
curl -s -o /dev/null -w 'pos vhost local http -> %{http_code}\n' -H "Host: ${FQDN}" http://127.0.0.1/ || true
echo "NGINX_POS_OK"
