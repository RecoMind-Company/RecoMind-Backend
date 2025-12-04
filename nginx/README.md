# Nginx Configuration for RecoMind Backend Services

This directory contains the nginx configuration for routing requests to RecoMind backend microservices.

## Configuration File

- `recomind-Backend-server`: Main nginx server configuration for `/etc/nginx/sites-available/`

## Issues Fixed

This configuration fixes the following issues:

### 1. URL Rewriting Problem (404 errors)

**Problem:** The original `proxy_pass` directive with a trailing `/` was stripping the location prefix:
```nginx
# WRONG - strips /api/Authentication/ prefix
location /api/Authentication/ {
    proxy_pass http://backend:8011/;  # Request to /api/Authentication/login becomes /login
}
```

**Fix:** Remove the trailing `/` to preserve the full request path:
```nginx
# CORRECT - preserves full path
location /api/Authentication/ {
    proxy_pass http://backend:8011;  # Request to /api/Authentication/login stays /api/Authentication/login
}
```

### 2. HTTP/2 gRPC Support

**Problem:** Backend services use HTTP/2 only for gRPC, but nginx was configured for HTTP/1.1.

**Fix:** Added separate server block with `http2` directive and `grpc_pass` for gRPC services:
```nginx
server {
    listen 50051 http2;
    
    location /authentication. {
        grpc_pass grpc://authentication_grpc;
    }
}
```

## Installation

1. Copy or symlink the configuration:
   ```bash
   sudo cp nginx/recomind-Backend-server /etc/nginx/sites-available/recomind-Backend-server
   # Or create symbolic link:
   sudo ln -sf $(pwd)/nginx/recomind-Backend-server /etc/nginx/sites-available/recomind-Backend-server
   ```

2. Enable the site:
   ```bash
   sudo ln -sf /etc/nginx/sites-available/recomind-Backend-server /etc/nginx/sites-enabled/
   ```

3. Test the configuration:
   ```bash
   sudo nginx -t
   ```

4. Reload nginx:
   ```bash
   sudo systemctl reload nginx
   ```

## Service Port Configuration

### HTTP (REST API) Endpoints

| Service | HTTP Port | API Route |
|---------|-----------|-----------|
| Authentication | 8011 | `/api/Authentication/` |
| Account (Auth Service) | 8011 | `/api/Account/` |
| Database Setting | 8003 | `/api/DbSetting/` |
| Subscription | 8004 | `/api/Subscription/` |
| Plan | 8005 | `/api/Planes/` |
| Invitation | 8006 | `/api/invitation/` |
| Company | 8007 | `/api/Companies/` |
| Report | 8008 | `/api/Report/` |
| Chatbot | 8009 | `/api/Chatbot/` |
| Team | 8010 | `/api/Team/` |

### gRPC Endpoints (HTTP/2)

| Service | gRPC Port | nginx Port |
|---------|-----------|------------|
| Authentication | 5011 | 50051 |
| Invitation | 5006 | 50051 |
| Plan | 5005 | 50051 |

## HTTP/2 Support

- **REST API endpoints**: Use HTTP/1.1 (`proxy_http_version 1.1`) for proxying to backend services
- **gRPC endpoints**: Require HTTP/2 and are exposed on nginx port 50051
- **Backend Kestrel configuration**:
  - HTTP/1 on HTTP ports (for REST API)
  - HTTP/2 on gRPC ports (for gRPC communication)

## URL Routing Examples

| Client Request | Backend Request |
|----------------|-----------------|
| `GET /api/Authentication/login` | `GET http://127.0.0.1:8011/api/Authentication/login` |
| `POST /api/Account/register` | `POST http://127.0.0.1:8011/api/Account/register` |
| `GET /api/Planes/` | `GET http://127.0.0.1:8005/api/Planes/` |

## Environment Variables

Backend services read their port configuration from environment variables:
- `HTTP_PORT`: HTTP port for REST API
- `GRPC_PORT`: gRPC port for gRPC services
- `Kestrel__Endpoints__Http__Port`: Alternative HTTP port configuration
- `Kestrel__Endpoints__Grpc__Port`: Alternative gRPC port configuration

## SSL/TLS Configuration

The configuration includes SSL setup for `api.recomind.site`:
- Certificate: `/etc/letsencrypt/live/api.recomind.site/fullchain.pem`
- Private key: `/etc/letsencrypt/live/api.recomind.site/privkey.pem`
- Protocols: TLSv1.2, TLSv1.3
- HTTP to HTTPS redirect enabled

## Customization

To customize the port mappings:
1. Update the `upstream` blocks at the top of the configuration file
2. Modify the corresponding environment variables for the backend services
3. Test and reload nginx:
   ```bash
   sudo nginx -t && sudo systemctl reload nginx
   ```
