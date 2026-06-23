# GiapTech.Ipages

SaaS Multi-Tenant Website Builder + Headless Commerce API

## Kiến trúc

```
GiapTech.Ipages
├── backend/                    # ASP.NET Core 9 — Clean Architecture, CQRS
│   └── src/
│       ├── GiapTech.Ipages.Domain          # Entities, Value Objects
│       ├── GiapTech.Ipages.Application     # CQRS Commands/Queries, Interfaces
│       ├── GiapTech.Ipages.Infrastructure  # EF Core, PostgreSQL, Redis, MinIO
│       └── GiapTech.Ipages.Api             # ASP.NET Core Web API
├── frontend-host/              # React + Vite — Host Admin Dashboard
├── frontend-tenant/            # React + Vite — Tenant Admin Dashboard
├── frontend-client/            # React + Vite — Client Public Website
├── docker/
│   └── traefik/                # Traefik reverse proxy config
├── docker-compose.yml
├── docker-compose.override.yml
└── .env.example
```

## Quick Start

```bash
git clone <repository>
cd GiapTech.Ipages

# 1. Copy env file
cp .env.example .env

# 2. Start all services
docker compose up -d
```

Toàn bộ hệ thống sẽ tự động:
1. Chờ PostgreSQL sẵn sàng
2. Chạy EF Core migrations
3. Seed dữ liệu mặc định
4. Khởi động API, Frontend, Traefik

## Default Accounts

| Role | Username | Password |
|------|----------|----------|
| Host Admin | `hostadmin` | `Host@123456` |
| Demo Tenant Admin | `admin` | `Admin@123456` |

## URLs (Development)

| Service | URL |
|---------|-----|
| Host Admin | http://host.localhost/admin |
| Demo Tenant Admin | http://demo.localhost/admin |
| Demo Client Site | http://demo.localhost |
| API | http://host.localhost/api/v1 |
| Swagger | http://host.localhost/swagger |
| Traefik Dashboard | http://traefik.localhost:8080 |
| MinIO Console | http://minio.localhost |

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 9, Clean Architecture, CQRS, MediatR |
| ORM | EF Core 9 (write), Dapper (read) |
| Database | PostgreSQL 16 |
| Cache | Redis 7 |
| Storage | MinIO |
| Auth | JWT + Refresh Token |
| Validation | FluentValidation |
| Logging | Serilog |
| Reverse Proxy | Traefik v3 |
| Frontend | React 18, TypeScript, Vite, Ant Design 5 |
| State | Zustand, TanStack Query |
| Containerization | Docker + Docker Compose |

## Environment Variables

See [.env.example](.env.example) for all required variables.

## API Documentation

Swagger UI available at: `http://host.localhost/swagger`

## Multi-Tenant Architecture

- **Subdomain routing**: `{tenant-slug}.localhost` → tenant context
- **Data isolation**: All business tables have `TenantId` column
- **Global query filter**: EF Core automatically filters by `TenantId`
- **Host admin**: `host.localhost` → full system access

## Default Ports (override mode)

| Service | Port |
|---------|------|
| Frontend Host | 3000 |
| Frontend Tenant | 3001 |
| Frontend Client | 3002 |
| Backend API | 5000 |
| PostgreSQL | 5432 |
| Redis | 6379 |
| MinIO | 9000/9001 |
| Traefik | 80/8080 |

## Production Deployment

1. Update `.env` with production values
2. Set strong passwords for all services
3. Configure domain in `BASE_DOMAIN`
4. Update Traefik rules for your domain
5. Run: `docker compose -f docker-compose.yml up -d`

## Backup Strategy

```bash
# Backup PostgreSQL
docker exec ipages-postgres pg_dump -U giaptech giaptech_ipages > backup.sql

# Restore
docker exec -i ipages-postgres psql -U giaptech giaptech_ipages < backup.sql
```
