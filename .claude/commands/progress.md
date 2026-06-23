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
