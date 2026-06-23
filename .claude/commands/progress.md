# GiapTech.Ipages — Phase Progress

Xem thêm: [rules.md](rules.md) | [stack.md](stack.md) | [history.md](history.md)

---

## ✅ Phase 1 — Infrastructure + Backend Core + Frontend Skeleton

**Trạng thái**: HOÀN THÀNH (2026-06-23)  
**Kết quả**: `docker compose up -d` chạy được, login được, seed data sẵn.

**Đã làm**:
- Root: `.gitignore`, `.env.example`, `docker-compose.yml`, `docker-compose.override.yml`, `README.md`
- Docker: Traefik static + dynamic config
- Backend: 4-project Clean Architecture solution, 33 Domain entities, Application layer (interfaces, CQRS behaviors, Auth commands), Infrastructure layer (EF Core + migrations viết tay + seed data + all services + TenantMiddleware), API layer (8 controllers, ExceptionMiddleware, Program.cs đầy đủ)
- Frontend: 3 React apps với skeleton pages (host-admin, tenant-admin, client-website)
- Bug fix: EF Core query filter dùng `_tenantService` field thay vì captured local variable

Chi tiết từng file → xem [history.md](history.md)

---

## ✅ Phase 2 — Full CRUD Application Layer

**Trạng thái**: HOÀN THÀNH (2026-06-23)  
**Mục tiêu**: Implement đầy đủ Commands/Queries cho mọi module.

- [x] Tenants CRUD (GetTenantsQuery, CreateTenantCommand, UpdateTenantCommand, DeleteTenantCommand)
- [x] Users (RegisterCommand, ChangePasswordCommand, GetUsersQuery)
- [x] Products CRUD + UpdateInventoryCommand
- [x] Product Categories CRUD
- [x] Orders CRUD + UpdateOrderStatusCommand + CancelOrderCommand
- [x] Customers CRUD
- [x] Articles CRUD + PublishArticleCommand
- [x] Article Categories CRUD
- [x] Media (GetMediaFilesQuery, DeleteMediaFileCommand)
- [x] Dashboard (GetHostDashboardQuery, GetTenantDashboardQuery)
- [x] IPasswordHasher interface + BCrypt implementation
- [x] Tất cả controllers refactored sang MediatR

---

## ✅ Phase 3 — Extended Modules

**Trạng thái**: HOÀN THÀNH (2026-06-23)  
**Mục tiêu**: Implement các module nâng cao.

- [x] Cart & CartItem CRUD
- [x] Coupon: create, validate, apply
- [x] ApiKey: generate, revoke, list
- [x] LandingPage builder: CRUD sections
- [x] Form builder: create form, submit entry, list entries
- [x] Menu builder: create menu, manage items
- [x] SeoMetadata: upsert per entity

---

## ✅ Phase 4 — Frontend Polish + E2E Integration

**Trạng thái**: HOÀN THÀNH (2026-06-23)  
**Mục tiêu**: Frontend kết nối đầy đủ với API thực, không dùng mock data.

- [x] `frontend-host/src/pages/DashboardPage.tsx` — dùng GET /dashboard (HostDashboardDto)
- [x] `frontend-tenant/src/pages/DashboardPage.tsx` — dùng GET /dashboard (TenantDashboardDto)
- [x] `frontend-client/src/pages/ArticleDetailPage.tsx` — fix URL bug `/articles/slug/${slug}`
- [x] `frontend-tenant/src/pages/ProductCategoriesPage.tsx` — trang CRUD mới
- [x] `frontend-tenant/src/layouts/AdminLayout.tsx` — thêm menu Danh mục SP
- [x] `frontend-tenant/src/App.tsx` — thêm route product-categories
- [x] `frontend-tenant/src/lib/api.ts` — error handling: queue refresh, server error toast, network error toast
- [x] `frontend-host/src/lib/api.ts` — error handling: queue refresh, server error toast, network error toast

---

## ⏳ Phase 5 — Production Hardening

**Trạng thái**: CHƯA BẮT ĐẦU  
**Mục tiêu**: Sẵn sàng deploy production.

- [ ] HTTPS với Let's Encrypt trong Traefik
- [ ] Rate limiting per-tenant
- [ ] Redis caching cho public API endpoints
- [ ] Background jobs (Hangfire): cleanup expired sessions, email notifications
- [ ] Email service (SMTP) integration
- [ ] Prometheus metrics
- [ ] Production docker-compose (không expose internal ports)
