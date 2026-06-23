# GiapTech.Ipages — Quy Tắc Làm Việc

## Quy Tắc Bắt Buộc

1. **Không pseudo code, không TODO, không empty implementations.** Mọi file phải chạy được thực tế.
2. **Mỗi phase kết thúc → dừng lại, chờ user gõ "continue" rồi mới bắt đầu phase tiếp theo.**
3. **Không hỏi thêm câu hỏi** trừ khi thực sự bị chặn (thiếu thông tin bắt buộc, quyết định chỉ user mới biết).
4. **Docker first**: Mọi service chạy trong Docker. `docker compose up -d` phải bring up toàn bộ hệ thống.
5. **Không sửa code đã hoàn thành ở phase trước** trừ khi có bug được phát hiện.
6. **Không commit** trừ khi user yêu cầu rõ ràng.
7. **Không thêm feature, refactor, abstraction** ngoài scope của task hiện tại.
8. **Không thêm comment** trừ khi WHY không rõ ràng từ code.

## Quy Tắc Code

- **Migration**: Không dùng `dotnet ef migrations add`. Viết tay file migration mới khi cần.
- **EF Core Query Filter**: Luôn capture `_tenantService` field (không capture local variable) để tránh model caching bug.
- **Subdomain routing**: `TenantMiddleware` đọc `Host` header → parse subdomain → lookup DB.
- **JWT Claims**: `sub`, `name`, `isHostAdmin`, `tenantId`, `permission[]`.
- **Password**: BCrypt hash, không plain text.
- **Pagination**: Dùng `PaginatedList<T>` + `PagedQuery` base model.
- **Result pattern**: Mọi command/query trả về `Result<T>` hoặc `Result`.

## Lưu Ý Kỹ Thuật Quan Trọng

### EF Core Query Filter Bug (Đã fix ở Phase 1)
Primary constructor captures TenantId VALUE khi model được build lần đầu → tất cả requests sau
dùng cùng tenant ID. Fix: dùng field `_tenantService`, lambda capture `this` → evaluate per-request.

```csharp
// WRONG — captures value at model-build time
var tenantId = tenantService.TenantId;
.HasQueryFilter(e => e.TenantId == tenantId.Value);

// CORRECT — evaluates per-request
.HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
```
