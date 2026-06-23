# GiapTech.Ipages — History: Files Đã Tạo Theo Phase

## Phase 5 — Production Hardening

**Ngày hoàn thành**: 2026-06-23

### Application — Common Interfaces
| File | Mô tả |
|------|-------|
| `Application/Common/Interfaces/ICacheable.cs` | `CacheKey` + `CacheDuration` — implement trên public queries để enable caching |
| `Application/Common/Interfaces/IEmailService.cs` | `SendAsync(to, subject, html)` — single + multi-recipient |

### Application — Behaviors
| File | Mô tả |
|------|-------|
| `Application/Common/Behaviors/CachingBehavior.cs` | MediatR pipeline: check Redis trước, set cache sau handler. Key = `{tenantId}:{CacheKey}` |

### Application — Updated Queries (ICacheable)
| File | Thay đổi |
|------|---------|
| `Application/Products/Queries/GetProductsQuery.cs` | `GetProductBySlugQuery` implements `ICacheable` — cache 15 phút |
| `Application/Articles/Queries/GetArticlesQuery.cs` | `GetArticleBySlugQuery` implements `ICacheable` — cache 30 phút |
| `Application/LandingPages/Queries/GetLandingPagesQuery.cs` | `GetLandingPageBySlugQuery` implements `ICacheable` — cache 60 phút |

### Application — DI Update
| File | Thay đổi |
|------|---------|
| `Application/DependencyInjection.cs` | Thêm `CachingBehavior<,>` vào MediatR pipeline (trước ValidationBehavior) |

### Infrastructure — Services
| File | Mô tả |
|------|-------|
| `Infrastructure/Services/SmtpEmailService.cs` | MailKit SMTP — connect + auth + send HTML email; no-op nếu SmtpSettings:Host trống |

### Infrastructure — Background Jobs
| File | Mô tả |
|------|-------|
| `Infrastructure/BackgroundJobs/CleanupExpiredTokensJob.cs` | Xóa expired refresh tokens (`RefreshTokenExpiresAt < now`) |
| `Infrastructure/BackgroundJobs/EmailNotificationJob.cs` | Gửi order confirmation + welcome email |

### Infrastructure — DI Update
| File | Thay đổi |
|------|---------|
| `Infrastructure/DependencyInjection.cs` | Thêm: IEmailService→SmtpEmailService, Hangfire (PostgreSQL storage), HangfireServer, jobs (Transient) |
| `Infrastructure/GiapTech.Ipages.Infrastructure.csproj` | Thêm: Hangfire.Core, Hangfire.PostgreSql, MailKit |

### API — Updates
| File | Thay đổi |
|------|---------|
| `Api/Program.cs` | Per-tenant rate limiter (keyed by subdomain, 300/min), Prometheus UseHttpMetrics + MapMetrics("/metrics"), Hangfire dashboard (/hangfire) + HangfireDashboardAuthFilter, RegisterRecurringJob CleanupExpiredTokens hourly |
| `Api/GiapTech.Ipages.Api.csproj` | Thêm: Hangfire.AspNetCore, prometheus-net.AspNetCore |

### Docker
| File | Mô tả |
|------|-------|
| `docker/traefik/traefik.prod.yml` | Production Traefik: websecure entrypoint :443, HTTP→HTTPS redirect, ACME Let's Encrypt httpChallenge |
| `docker-compose.prod.yml` | Production compose: không expose internal ports, HTTPS trên mọi router (tls.certresolver=letsencrypt), Redis maxmemory 256mb, Traefik basicauth, SMTP env vars |

---

## Phase 2 — Full CRUD Application Layer

**Ngày hoàn thành**: 2026-06-23

### New Interface
| File | Mô tả |
|------|-------|
| `Application/Common/Interfaces/IPasswordHasher.cs` | Hash() + Verify() — tránh BCrypt leak vào Application layer |

### Infrastructure — PasswordHasher
| File | Mô tả |
|------|-------|
| `Infrastructure/Services/PasswordHasher.cs` | BCrypt implementation của IPasswordHasher |

### Application — Tenants
| File | Mô tả |
|------|-------|
| `Application/Tenants/Queries/GetTenantsQuery.cs` | TenantDto, GetTenantsQuery + Handler, GetTenantByIdQuery + Handler |
| `Application/Tenants/Commands/CreateTenantCommand.cs` | Command + Validator + Handler, slug uniqueness check |
| `Application/Tenants/Commands/UpdateTenantCommand.cs` | Command + Validator + Handler |
| `Application/Tenants/Commands/DeleteTenantCommand.cs` | Command + Handler |

### Application — Users
| File | Mô tả |
|------|-------|
| `Application/Users/Queries/GetUsersQuery.cs` | UserDto, GetUsersQuery + Handler, GetUserByIdQuery + Handler |
| `Application/Users/Commands/RegisterUserCommand.cs` | Command + Validator + Handler (dùng IPasswordHasher) |
| `Application/Users/Commands/ChangePasswordCommand.cs` | Command + Validator + Handler (verify + hash qua IPasswordHasher) |

### Application — Products
| File | Mô tả |
|------|-------|
| `Application/Products/Queries/GetProductsQuery.cs` | ProductListDto, ProductDetailDto, ProductVariantDto, ProductAttributeDto; GetProductsQuery + GetProductByIdQuery + GetProductBySlugQuery + Handlers; file-scoped ProductMapping |
| `Application/Products/Commands/CreateProductCommand.cs` | Command + Validator + Handler, slug uniqueness check |
| `Application/Products/Commands/UpdateProductCommand.cs` | Command + Validator + Handler |
| `Application/Products/Commands/DeleteProductCommand.cs` | Command + Handler |
| `Application/Products/Commands/UpdateInventoryCommand.cs` | Command + Validator + Handler (tạo Inventory entry) |

### Application — ProductCategories
| File | Mô tả |
|------|-------|
| `Application/ProductCategories/Queries/GetProductCategoriesQuery.cs` | ProductCategoryDto, GetProductCategoriesQuery + Handler |
| `Application/ProductCategories/Commands/CreateProductCategoryCommand.cs` | Create + Update + Delete commands + validators + handlers (3 classes trong 1 file) |

### Application — Orders
| File | Mô tả |
|------|-------|
| `Application/Orders/Queries/GetOrdersQuery.cs` | OrderListDto, OrderDetailDto, OrderItemDto; GetOrdersQuery + GetOrderByIdQuery + Handlers |
| `Application/Orders/Commands/CreateOrderCommand.cs` | Command + Validator + Handler (tự tính subTotal/total từ product prices) |
| `Application/Orders/Commands/UpdateOrderStatusCommand.cs` | UpdateOrderStatusCommand + CancelOrderCommand + Handlers (2 classes trong 1 file) |

### Application — Customers
| File | Mô tả |
|------|-------|
| `Application/Customers/Queries/GetCustomersQuery.cs` | CustomerDto, CustomerDetailDto, CustomerAddressDto; GetCustomersQuery + GetCustomerByIdQuery + Handlers |
| `Application/Customers/Commands/CreateCustomerCommand.cs` | Create + Update commands + validators + handlers (2 classes trong 1 file) |

### Application — Articles
| File | Mô tả |
|------|-------|
| `Application/Articles/Queries/GetArticlesQuery.cs` | ArticleListDto, ArticleDetailDto; GetArticlesQuery + GetArticleByIdQuery + GetArticleBySlugQuery + Handlers; file-scoped ArticleMapping |
| `Application/Articles/Commands/CreateArticleCommand.cs` | Command + Validator + Handler (set PublishedAt nếu status = Published) |
| `Application/Articles/Commands/UpdateArticleCommand.cs` | UpdateArticleCommand + DeleteArticleCommand + PublishArticleCommand + Handlers (3 classes trong 1 file) |

### Application — ArticleCategories
| File | Mô tả |
|------|-------|
| `Application/ArticleCategories/Queries/GetArticleCategoriesQuery.cs` | ArticleCategoryDto, GetArticleCategoriesQuery + Handler |
| `Application/ArticleCategories/Commands/CreateArticleCategoryCommand.cs` | Create + Update commands + validators + handlers (2 classes trong 1 file) |

### Application — Media
| File | Mô tả |
|------|-------|
| `Application/Media/Queries/GetMediaFilesQuery.cs` | MediaFileDto, GetMediaFilesQuery + Handler |
| `Application/Media/Commands/DeleteMediaFileCommand.cs` | Command + Handler (xóa file trên storage trước) |

### Application — Dashboard
| File | Mô tả |
|------|-------|
| `Application/Dashboard/Queries/GetDashboardQuery.cs` | HostDashboardDto + GetHostDashboardQuery + Handler; TenantDashboardDto + GetTenantDashboardQuery + Handler (2 queries trong 1 file) |

### Updated Controllers (refactor từ direct DB → MediatR)
| File | Mô tả |
|------|-------|
| `Api/Controllers/TenantsController.cs` | Inject ISender, dùng Tenants Commands/Queries |
| `Api/Controllers/ProductsController.cs` | Inject ISender, dùng Products Commands/Queries, thêm slug + inventory endpoints |
| `Api/Controllers/OrdersController.cs` | Inject ISender, thêm create + cancel endpoints |
| `Api/Controllers/ArticlesController.cs` | Inject ISender + ICurrentUserService, thêm publish endpoint |
| `Api/Controllers/CustomersController.cs` | Inject ISender, thêm create + update |
| `Api/Controllers/MediaController.cs` | Inject ISender, giữ upload logic trực tiếp (cần IFormFile) |

### New Controllers
| File | Mô tả |
|------|-------|
| `Api/Controllers/ProductCategoriesController.cs` | CRUD product categories |
| `Api/Controllers/ArticleCategoriesController.cs` | CRUD article categories |
| `Api/Controllers/DashboardController.cs` | GET /dashboard — tự động chọn Host hoặc Tenant dashboard |
| `Api/Controllers/UsersController.cs` | GET list/detail, POST register, POST change-password |

---

## Phase 3 — Extended Modules

**Ngày hoàn thành**: 2026-06-23

### Application — Cart
| File | Mô tả |
|------|-------|
| `Application/Carts/Queries/GetCartQuery.cs` | CartDto, CartItemDto; GetCartQuery (by sessionId or customerId) + Handler |
| `Application/Carts/Commands/CartCommands.cs` | AddToCartCommand + UpdateCartItemCommand + RemoveCartItemCommand + ClearCartCommand + Handlers |

### Application — Coupons
| File | Mô tả |
|------|-------|
| `Application/Coupons/Queries/GetCouponsQuery.cs` | CouponDto; GetCouponsQuery + GetCouponByCodeQuery + Handlers |
| `Application/Coupons/Commands/CouponCommands.cs` | CreateCouponCommand + UpdateCouponCommand + DeleteCouponCommand + ValidateCouponCommand + Handlers |

### Application — ApiKeys
| File | Mô tả |
|------|-------|
| `Application/ApiKeys/Queries/GetApiKeysQuery.cs` | ApiKeyDto; GetApiKeysQuery + Handler |
| `Application/ApiKeys/Commands/ApiKeyCommands.cs` | GenerateApiKeyCommand + RevokeApiKeyCommand + Handlers |

### Application — LandingPages
| File | Mô tả |
|------|-------|
| `Application/LandingPages/Queries/GetLandingPagesQuery.cs` | LandingPageDto, LandingSectionDto; GetLandingPagesQuery + GetLandingPageBySlugQuery + Handlers |
| `Application/LandingPages/Commands/LandingPageCommands.cs` | CreateLandingPageCommand + UpdateLandingPageCommand + DeleteLandingPageCommand + UpsertSectionCommand + DeleteSectionCommand + Handlers |

### Application — Forms
| File | Mô tả |
|------|-------|
| `Application/Forms/Queries/GetFormsQuery.cs` | FormDto, FormEntryDto; GetFormsQuery + GetFormEntriesQuery + Handlers |
| `Application/Forms/Commands/FormCommands.cs` | CreateFormCommand + UpdateFormCommand + DeleteFormCommand + SubmitFormEntryCommand + Handlers (IHttpContextAccessor cho IP/UserAgent) |

### Application — Menus
| File | Mô tả |
|------|-------|
| `Application/Menus/Queries/GetMenusQuery.cs` | MenuDto, MenuItemDto (recursive tree); GetMenusQuery + GetMenuByLocationQuery + Handlers |
| `Application/Menus/Commands/MenuCommands.cs` | CreateMenuCommand + UpdateMenuCommand + DeleteMenuCommand + UpsertMenuItemCommand + DeleteMenuItemCommand + Handlers |

### Application — Seo
| File | Mô tả |
|------|-------|
| `Application/Seo/Commands/SeoCommands.cs` | GetSeoMetadataQuery + UpsertSeoMetadataCommand + Handlers (cả query lẫn command trong 1 file) |

### New Controllers
| File | Mô tả |
|------|-------|
| `Api/Controllers/CartsController.cs` | GET cart, POST add, PUT update, DELETE item, DELETE clear |
| `Api/Controllers/CouponsController.cs` | CRUD + POST validate |
| `Api/Controllers/ApiKeysController.cs` | GET list, POST generate, DELETE revoke |
| `Api/Controllers/LandingPagesController.cs` | CRUD pages + sections |
| `Api/Controllers/FormsController.cs` | CRUD forms + list entries + public submit |
| `Api/Controllers/MenusController.cs` | CRUD menus + items |
| `Api/Controllers/SeoController.cs` | GET + PUT seo metadata |

---

## Phase 4 — Frontend Polish + E2E Integration

**Ngày hoàn thành**: 2026-06-23

### Frontend Host
| File | Mô tả |
|------|-------|
| `frontend-host/src/pages/DashboardPage.tsx` | **REWRITE**: GET /dashboard → HostDashboardDto; 4 stat cards (totalTenants, activeTenants, inactiveTenants, newTenantsThisMonth) |
| `frontend-host/src/lib/api.ts` | **ENHANCED**: queue-based refresh (tránh race condition), server error toast, network/timeout toast, redirect to /login on clearAuth |

### Frontend Tenant
| File | Mô tả |
|------|-------|
| `frontend-tenant/src/pages/DashboardPage.tsx` | **REWRITE**: GET /dashboard → TenantDashboardDto; 7 stat cards (todayOrders, todayRevenue, thisMonthRevenue, pendingOrders, activeProducts/total, customers, articles) |
| `frontend-tenant/src/pages/ProductCategoriesPage.tsx` | **NEW**: CRUD table + Modal; create/edit/delete, auto-slug, parent category select |
| `frontend-tenant/src/layouts/AdminLayout.tsx` | **UPDATE**: thêm menu item "Danh mục SP" (BarsOutlined, /admin/product-categories) |
| `frontend-tenant/src/App.tsx` | **UPDATE**: import ProductCategoriesPage, thêm route /admin/product-categories |
| `frontend-tenant/src/lib/api.ts` | **ENHANCED**: queue-based refresh, server error toast, network/timeout toast, redirect to /admin/login on clearAuth |

### Frontend Client
| File | Mô tả |
|------|-------|
| `frontend-client/src/pages/ArticleDetailPage.tsx` | **BUG FIX**: URL `/articles/${slug}` → `/articles/slug/${slug}` |

---

## Phase 1 — Infrastructure + Backend Core + Frontend Skeleton

**Ngày hoàn thành**: 2026-06-23

### Root
| File | Mô tả |
|------|-------|
| `.gitignore` | .NET bin/obj, node_modules/dist, Docker volumes, secrets |
| `.env.example` | Tất cả env vars: POSTGRES, REDIS, JWT, MINIO, TRAEFIK, ports, BASE_DOMAIN |
| `docker-compose.yml` | 8 services với healthchecks và depends_on |
| `docker-compose.override.yml` | Expose direct ports cho dev |
| `README.md` | Docs đầy đủ: quick start, URLs, accounts, backup |

### Docker
| File | Mô tả |
|------|-------|
| `docker/traefik/traefik.yml` | Static config: dashboard, entryPoint web:80, Docker provider |
| `docker/traefik/config/dynamic.yml` | CORS middleware, strip-api-prefix middleware |

### Backend — Solution
| File | Mô tả |
|------|-------|
| `backend/GiapTech.Ipages.sln` | 4-project solution với fixed GUIDs |
| `backend/Dockerfile` | Multi-stage: sdk:9.0 build → aspnet:9.0 runtime, port 5000 |

### Backend — Domain
| File | Mô tả |
|------|-------|
| `Domain/Common/BaseEntity.cs` | Id (Guid), CreatedAt, UpdatedAt |
| `Domain/Common/TenantEntity.cs` | Extends BaseEntity với TenantId (Guid) |
| `Domain/Entities/Tenant.cs` + `TenantStatus` enum | |
| `Domain/Entities/Subscription.cs` | |
| `Domain/Entities/Payment.cs` + `PaymentMethod`, `PaymentStatus` enums | |
| `Domain/Entities/User.cs` | TenantId nullable, IsHostAdmin, RefreshToken fields |
| `Domain/Entities/Role.cs` | |
| `Domain/Entities/Permission.cs` | |
| `Domain/Entities/UserRole.cs` | |
| `Domain/Entities/RolePermission.cs` | |
| `Domain/Entities/Article.cs` + `ArticleStatus` enum | |
| `Domain/Entities/ArticleCategory.cs` | |
| `Domain/Entities/Tag.cs` | |
| `Domain/Entities/Product.cs` + `ProductStatus` enum | |
| `Domain/Entities/ProductCategory.cs` | |
| `Domain/Entities/ProductVariant.cs` | |
| `Domain/Entities/ProductAttribute.cs` | |
| `Domain/Entities/Inventory.cs` + `InventoryType` enum | |
| `Domain/Entities/Customer.cs` | |
| `Domain/Entities/CustomerAddress.cs` | |
| `Domain/Entities/Order.cs` + `OrderStatus`, `PaymentMethod`, `PaymentStatus` enums | |
| `Domain/Entities/OrderItem.cs` | |
| `Domain/Entities/Cart.cs` | |
| `Domain/Entities/CartItem.cs` | |
| `Domain/Entities/Coupon.cs` + `CouponType` enum | |
| `Domain/Entities/ApiKey.cs` | |
| `Domain/Entities/LandingPage.cs` + `LandingPageStatus` enum | |
| `Domain/Entities/LandingSection.cs` | |
| `Domain/Entities/Form.cs` + `FormType` enum | |
| `Domain/Entities/FormEntry.cs` | |
| `Domain/Entities/MediaFile.cs` + `MediaFileType` enum | |
| `Domain/Entities/Menu.cs` | |
| `Domain/Entities/MenuItem.cs` | |
| `Domain/Entities/SeoMetadata.cs` | |
| `Domain/Entities/AuditLog.cs` | |

### Backend — Application
| File | Mô tả |
|------|-------|
| `Application/Common/Interfaces/IApplicationDbContext.cs` | 35 DbSet + SaveChangesAsync |
| `Application/Common/Interfaces/ICurrentTenantService.cs` | TenantId?, TenantSlug?, IsHostAdmin, SetTenant(), SetHostAdmin() |
| `Application/Common/Interfaces/ICurrentUserService.cs` | UserId?, Username?, IsAuthenticated, Permissions |
| `Application/Common/Interfaces/IJwtService.cs` | GenerateAccessToken, GenerateRefreshToken, ValidateAccessToken |
| `Application/Common/Interfaces/IStorageService.cs` | UploadAsync, DeleteAsync, GetPublicUrl |
| `Application/Common/Interfaces/ICacheService.cs` | GetAsync, SetAsync, RemoveAsync, RemoveByPatternAsync |
| `Application/Common/Interfaces/IAuditService.cs` | LogAsync |
| `Application/Common/Models/Result.cs` | Result + Result<T> với Success/Failure |
| `Application/Common/Models/PaginatedList.cs` | Items, Page, PageSize, TotalCount, HasPreviousPage, HasNextPage |
| `Application/Common/Models/PagedQuery.cs` | Page, PageSize, Search, SortBy, SortDir |
| `Application/Common/Exceptions/ValidationException.cs` | FluentValidation failures dict |
| `Application/Common/Exceptions/NotFoundException.cs` | |
| `Application/Common/Exceptions/ForbiddenException.cs` | |
| `Application/Common/Exceptions/UnauthorizedException.cs` | |
| `Application/Common/Behaviors/ValidationBehavior.cs` | MediatR pipeline + FluentValidation |
| `Application/Common/Behaviors/LoggingBehavior.cs` | MediatR logging pipeline |
| `Application/Auth/Commands/LoginCommand.cs` | Full handler: resolve tenant, BCrypt verify, load permissions, generate JWT+refresh |
| `Application/Auth/Commands/LoginCommandValidator.cs` | FluentValidation |
| `Application/Auth/Commands/RefreshTokenCommand.cs` | Validate refresh token, generate new pair |
| `Application/DependencyInjection.cs` | Register MediatR, FluentValidation, AutoMapper |

### Backend — Infrastructure
| File | Mô tả |
|------|-------|
| `Infrastructure/Persistence/ApplicationDbContext.cs` | **BUG FIX**: field `_tenantService` thay primary constructor; query filters evaluate per-request |
| `Infrastructure/Persistence/Configurations/TenantConfiguration.cs` | Unique index on Slug |
| `Infrastructure/Persistence/Configurations/UserConfiguration.cs` | Composite unique index (TenantId, Username) và (TenantId, Email) |
| `Infrastructure/Persistence/Configurations/ProductConfiguration.cs` | Unique index (TenantId, Slug), decimal precision |
| `Infrastructure/Persistence/Configurations/OrderConfiguration.cs` | Unique index (TenantId, OrderCode), cascade delete Items |
| `Infrastructure/Persistence/Migrations/20240101000000_InitialCreate.cs` | Viết tay — 33+ tables với FK, indexes, defaults |
| `Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs` | Minimal snapshot |
| `Infrastructure/Persistence/Seed/DataSeeder.cs` | 21 permissions, hostadmin, demo tenant, admin, roles, products, articles |
| `Infrastructure/Services/CurrentTenantService.cs` | Scoped, in-memory state per request |
| `Infrastructure/Services/CurrentUserService.cs` | Đọc từ HttpContext claims |
| `Infrastructure/Services/JwtService.cs` | HS256, claims: sub, name, isHostAdmin, tenantId, permission[] |
| `Infrastructure/Services/RedisCacheService.cs` | JSON serialize/deserialize, StackExchange.Redis |
| `Infrastructure/Services/MinioStorageService.cs` | Minio SDK, auto-create bucket, build public URL |
| `Infrastructure/Services/AuditService.cs` | Ghi AuditLogs với HTTP context |
| `Infrastructure/MultiTenant/TenantMiddleware.cs` | Parse subdomain, lookup DB, SetTenant() hoặc SetHostAdmin() |
| `Infrastructure/DependencyInjection.cs` | Register tất cả services |

### Backend — API
| File | Mô tả |
|------|-------|
| `Api/Program.cs` | Serilog, DI, Swagger+Bearer, JWT auth, CORS, HealthChecks, RateLimiter (300/min), Migrate+Seed on startup |
| `Api/appsettings.json` | Connection strings và settings |
| `Api/appsettings.Development.json` | Debug logging, EF SQL logging |
| `Api/Middleware/ExceptionMiddleware.cs` | Map exceptions → HTTP status codes |
| `Api/Controllers/AuthController.cs` | POST /auth/login, POST /auth/refresh |
| `Api/Controllers/HealthController.cs` | GET /health/ping |
| `Api/Controllers/TenantsController.cs` | CRUD — host-admin only |
| `Api/Controllers/ProductsController.cs` | CRUD với tenant filter |
| `Api/Controllers/OrdersController.cs` | List, get, patch status |
| `Api/Controllers/ArticlesController.cs` | Public list/detail + authenticated CRUD |
| `Api/Controllers/CustomersController.cs` | CRUD |
| `Api/Controllers/MediaController.cs` | Upload to MinIO, delete |

### Frontend Host (`frontend-host/`)
| File | Mô tả |
|------|-------|
| `package.json` | React 18, Ant Design 5, TanStack Query 5, React Router 6, Zustand 5, Axios |
| `vite.config.ts` | Port 3000, proxy /api → backend |
| `nginx.conf` | SPA routing, /health, gzip |
| `Dockerfile` | node:20-alpine build → nginx:alpine |
| `src/store/auth.ts` | Zustand persist: token, refreshToken, user |
| `src/lib/api.ts` | Axios + JWT interceptor + auto-refresh on 401 |
| `src/App.tsx` | BrowserRouter, ConfigProvider viVN, protected routes |
| `src/pages/LoginPage.tsx` | Login form (no tenantSlug) |
| `src/layouts/AdminLayout.tsx` | Sider collapsible + Header + Content |
| `src/pages/DashboardPage.tsx` | Stats: Total Tenants, Active, API Calls, Storage |
| `src/pages/TenantsPage.tsx` | Table + search + pagination + edit/delete |
| `src/pages/TenantFormPage.tsx` | Create/edit form với DatePicker, status select |

### Frontend Tenant (`frontend-tenant/`)
| File | Mô tả |
|------|-------|
| `package.json`, `vite.config.ts`, `nginx.conf`, `Dockerfile` | Same stack as frontend-host, port 3001 |
| `src/store/auth.ts` | Includes tenantSlug field |
| `src/pages/LoginPage.tsx` | Reads subdomain từ window.location |
| `src/layouts/AdminLayout.tsx` | Menu: Dashboard, Products, Orders, Customers, Articles, Media |
| `src/pages/ProductsPage.tsx` | Table + Modal CRUD |
| `src/pages/OrdersPage.tsx` | Table + status progression buttons |
| `src/pages/CustomersPage.tsx` | Read-only table + search |
| `src/pages/ArticlesPage.tsx` | Table + Modal CRUD |
| `src/pages/MediaPage.tsx` | Grid view + Upload + delete |

### Frontend Client (`frontend-client/`)
| File | Mô tả |
|------|-------|
| `package.json` | React 18, TanStack Query, Axios (no Ant Design) |
| `vite.config.ts` | Port 3002 |
| `nginx.conf`, `Dockerfile` | SPA routing |
| `src/index.css` | Pure CSS: container, product-grid, product-card, header, footer |
| `src/App.tsx` | Routes: /, /san-pham, /san-pham/:slug, /tin-tuc, /tin-tuc/:slug |
| `src/components/Header.tsx` | Reads subdomain as site name |
| `src/components/Footer.tsx` | Static footer |
| `src/pages/HomePage.tsx` | Hero banner + featured products grid |
| `src/pages/ProductsPage.tsx` | Product grid + pagination |
| `src/pages/ProductDetailPage.tsx` | Product detail + buy button |
| `src/pages/ArticlesPage.tsx` | Article list + thumbnail |
| `src/pages/ArticleDetailPage.tsx` | Article content với dangerouslySetInnerHTML |
