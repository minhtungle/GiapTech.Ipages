# GiapTech.Ipages — Stack & Kiến Trúc

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 9, Clean Architecture, CQRS + MediatR |
| ORM | EF Core 9 (write), Dapper (read) |
| Database | PostgreSQL 16 |
| Cache | Redis 7 |
| Storage | MinIO |
| Auth | JWT HS256 + Refresh Token (stored in DB) |
| Validation | FluentValidation |
| Logging | Serilog |
| Reverse Proxy | Traefik v3 (wildcard subdomain) |
| Frontend | React 18 + TypeScript + Vite + Ant Design 5 |
| State | Zustand 5 + TanStack Query 5 |
| Container | Docker + Docker Compose |

## Cấu Trúc Thư Mục

```
GiapTech.Ipages/
├── backend/
│   ├── GiapTech.Ipages.sln
│   ├── Dockerfile
│   └── src/
│       ├── GiapTech.Ipages.Domain          # Entities, Enums
│       ├── GiapTech.Ipages.Application     # CQRS, Interfaces, Behaviors
│       ├── GiapTech.Ipages.Infrastructure  # EF Core, PostgreSQL, Redis, MinIO, JWT
│       └── GiapTech.Ipages.Api             # Controllers, Middleware, Program.cs
├── frontend-host/     # Host Admin — quản lý toàn hệ thống (port 3000)
├── frontend-tenant/   # Tenant Admin — quản lý shop tenant (port 3001)
├── frontend-client/   # Client Website — trang công khai (port 3002)
├── docker/traefik/    # traefik.yml + config/dynamic.yml
├── docker-compose.yml
├── docker-compose.override.yml
└── .env.example
```

## Multi-Tenant Architecture

- **Subdomain routing**: `{slug}.localhost` → `TenantMiddleware` → `SetTenant()`
- **Host admin**: `host.localhost` → `SetHostAdmin()`
- **Data isolation**: `TenantId` (Guid) trên mọi bảng business
- **Global query filter**: EF Core filter per-request qua `_tenantService.TenantId`
- **Frontend**: Đọc subdomain từ `window.location.hostname`

## Default Accounts

| Role | Username | Password |
|------|----------|----------|
| Host Admin | `hostadmin` | `Host@123456` |
| Demo Tenant Admin | `admin` | `Admin@123456` |

## URLs (Development)

| Service | URL |
|---------|-----|
| Host Admin | http://host.localhost/admin |
| Tenant Admin | http://demo.localhost/admin |
| Client Site | http://demo.localhost |
| API | http://host.localhost/api/v1 |
| Swagger | http://host.localhost/swagger |
| Traefik Dashboard | http://traefik.localhost:8080 |
| MinIO Console | http://minio.localhost |

## Direct Ports (override mode)

| Service | Port |
|---------|------|
| Frontend Host | 3000 |
| Frontend Tenant | 3001 |
| Frontend Client | 3002 |
| Backend API | 5000 |
| PostgreSQL | 5432 |
| Redis | 6379 |
| MinIO API | 9000 |
| MinIO Console | 9001 |

## Docker Services

| Service | Image |
|---------|-------|
| traefik | traefik:v3 |
| postgres | postgres:16-alpine |
| redis | redis:7-alpine |
| minio | minio/minio:latest |
| backend-api | build từ `backend/Dockerfile` |
| frontend-host | build từ `frontend-host/Dockerfile` |
| frontend-tenant | build từ `frontend-tenant/Dockerfile` |
| frontend-client | build từ `frontend-client/Dockerfile` |
