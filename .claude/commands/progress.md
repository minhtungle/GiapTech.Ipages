# GiapTech.Ipages — Phase Progress

Xem thêm: [rules.md](rules.md) | [stack.md](stack.md) | [history.md](history.md)

---

## ✅ Phase 1 — Infrastructure + Backend Core + Frontend Skeleton

**Trạng thái**: HOÀN THÀNH (2026-06-23)

- Root, Docker, Backend (Clean Arch + 33 entities + migrations + seed), 3 Frontend apps skeleton
- Bug fix: EF Core query filter dùng `_tenantService` field thay vì captured local variable

---

## ✅ Phase 2 — Full CRUD Application Layer

**Trạng thái**: HOÀN THÀNH (2026-06-23)

- IPasswordHasher, tất cả Commands/Queries (Tenants, Users, Products, Orders, Customers, Articles, Media, Dashboard)
- Tất cả controllers refactored sang MediatR ISender

---

## ✅ Phase 3 — Extended Modules

**Trạng thái**: HOÀN THÀNH (2026-06-23)

- Cart, Coupon, ApiKey, LandingPage, Form, Menu, SeoMetadata — commands + queries + controllers

---

## ✅ Phase 4 — Frontend Polish + E2E Integration

**Trạng thái**: HOÀN THÀNH (2026-06-23)

- Dashboard pages dùng real `/dashboard` endpoint
- Fix ArticleDetailPage URL bug
- ProductCategoriesPage mới
- API interceptors: queue-based 401 refresh, error toasts, redirect on clearAuth

---

## ✅ Phase 5 — Production Hardening

**Trạng thái**: HOÀN THÀNH (2026-06-23)

- [x] HTTPS với Let's Encrypt trong Traefik (`traefik.prod.yml`)
- [x] Rate limiting per-tenant (keyed FixedWindow 300/min per subdomain)
- [x] Redis caching cho public endpoints qua `ICacheable` + `CachingBehavior` MediatR pipeline
  - Products, Articles, LandingPages slug endpoints cached 15/30/60 phút
- [x] Background jobs (Hangfire + PostgreSQL storage)
  - `CleanupExpiredTokensJob` chạy hourly
  - `EmailNotificationJob` (order confirmation, welcome email)
- [x] Email service (MailKit SMTP) — `IEmailService` + `SmtpEmailService`
- [x] Prometheus metrics — `prometheus-net.AspNetCore` + `/metrics` endpoint + HTTP metrics
- [x] Production docker-compose (`docker-compose.prod.yml`)
  - Không expose internal ports
  - HTTPS với certresolver=letsencrypt trên mọi router
  - Redis maxmemory 256mb + allkeys-lru
  - Traefik dashboard bảo vệ bằng basicauth

---

## ✅ Phase 5 — Post-Deploy Bug Fixes (Compile + Runtime)

**Trạng thái**: HOÀN THÀNH (2026-06-24)

### Compile Errors (18 files fixed)

| File | Lỗi | Fix |
|---|---|---|
| `Common/Models/PaginatedList.cs` | Không có constructor 4 tham số | Thêm constructor `(items, totalCount, page, pageSize)`; cập nhật `Create()` dùng constructor mới |
| `Products/Queries/GetProductsQuery.cs` | `file static class ProductMapping` không truy cập được từ file khác | Đổi thành `internal static class`; fix `return MapToDetail(p)` → `return ProductMapping.MapToDetail(p)` |
| `Articles/Queries/GetArticlesQuery.cs` | `file static class ArticleMapping` không truy cập được | Đổi thành `internal static class`; fix `return MapToDetail(a)` → `return ArticleMapping.MapToDetail(a)` |
| `ProductCategories/Queries/GetProductCategoriesQuery.cs` | `var query` inferred sai type (`IIncludableQueryable` không có `.Where()`) | Khai báo rõ `IQueryable<ProductCategory> query` |
| `ArticleCategories/Queries/GetArticleCategoriesQuery.cs` | Cùng vấn đề | Khai báo rõ `IQueryable<ArticleCategory> query` |
| `Auth/Commands/LoginCommand.cs` | Dùng `BCrypt.Net.BCrypt.Verify()` trực tiếp trong Application layer | Inject `IPasswordHasher`, dùng `passwordHasher.Verify()` |
| 10 command files | `ValidationException` ambiguous (FluentValidation vs Application.Common.Exceptions) | Thêm `using ValidationException = GiapTech.Ipages.Application.Common.Exceptions.ValidationException;` |
| `Infrastructure/Services/AuditService.cs` | `ApplicationDbContext` not found | Thêm `using GiapTech.Ipages.Infrastructure.Persistence;` |
| `Api/Controllers/MediaController.cs` | `UploadAsync(storagePath, stream, contentType)` sai thứ tự | Đổi thành `UploadAsync(stream, storagePath, contentType)` |

### Docker Build Errors

| Lỗi | Fix |
|---|---|
| `AddHangfire`/`AddHangfireServer` not found | Thêm `Hangfire.NetCore 1.8.20` vào Infrastructure.csproj |
| `Microsoft.EntityFrameworkCore.Relational` version mismatch (9.0.4 vs 9.0.5) | Thêm explicit `Microsoft.EntityFrameworkCore.Relational 9.0.5` |
| `ExceptionMiddleware` not found trong Program.cs | Thêm `using GiapTech.Ipages.Api;` |
| `IPAddress.IsLoopback` dùng như property | Sửa thành `IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress)` |

### Runtime Startup Errors

| Lỗi | Fix |
|---|---|
| `MigrateAsync()` finds 0 migrations | Đổi thành `EnsureCreatedAsync()` — migration discovery fail (thiếu designer file) |
| `RecurringJob.AddOrUpdate` static API fail — "JobStorage not initialized" | Đổi sang `IRecurringJobManager` (DI-based) |

### Login 500 Error (quan trọng nhất)

**Root cause 1 — `ApplicationDbContext.cs`**: Tất cả 25 global query filters dùng `_tenantService.TenantId.Value`. EF Core parameterize expression tree nên gọi `.Value` trên `Guid?` null trước khi đến null-check → `InvalidOperationException: Nullable object must have a value`.

**Fix**: Đổi toàn bộ `.TenantId.Value` → `.TenantId.GetValueOrDefault()`.

**Root cause 2 — `frontend-host/nginx.conf`**: Không có proxy block cho `/api/`. Frontend build với `VITE_API_BASE_URL=/api`, axios combineURLs(`/api`, `/auth/login`) = `/api/auth/login` → nginx serve static file → **405**.

**Fix**: Thêm nginx proxy:
```nginx
location /api/ {
    proxy_pass http://backend-api:5000/api/v1/;
    ...
}
```
(Rewrite `/api/` → `/api/v1/` vì backend routes dùng prefix `/api/v1/`)

### Trạng thái sau fix

- `docker compose up -d` → 8/8 services healthy ✅
- `POST localhost:3000/api/auth/login` → 200 + JWT ✅
- Host admin login: `hostadmin` / `Host@123456` ✅

---

## Deploy Commands

```bash
# Development
docker compose up -d

# Production
cp .env.example .env.prod
# Edit .env.prod: set BASE_DOMAIN, ACME_EMAIL, SMTP_*, TRAEFIK_DASHBOARD_AUTH, etc.
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d

# URLs (production)
# https://host.{BASE_DOMAIN}         — Host Admin
# https://{slug}.{BASE_DOMAIN}       — Tenant Admin / Client
# https://host.{BASE_DOMAIN}/swagger — API Docs
# https://host.{BASE_DOMAIN}/hangfire — Job Dashboard
# https://host.{BASE_DOMAIN}/metrics  — Prometheus Metrics
# https://traefik.{BASE_DOMAIN}      — Traefik Dashboard
# https://minio.{BASE_DOMAIN}        — MinIO Console
```
