using GiapTech.Ipages.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GiapTech.Ipages.Infrastructure.Persistence.Seed;

public class DataSeeder(ApplicationDbContext db, ILogger<DataSeeder> logger)
{
    public async Task SeedAsync()
    {
        await SeedPermissionsAsync();
        await SeedHostAdminAsync();
        await SeedDemoTenantAsync();
    }

    private async Task SeedPermissionsAsync()
    {
        if (await db.Permissions.AnyAsync()) return;

        var permissions = new List<Permission>
        {
            // Tenants
            new() { Id = Guid.NewGuid(), Name = "Tenants.Read", Module = "Tenants", Action = "Read" },
            new() { Id = Guid.NewGuid(), Name = "Tenants.Write", Module = "Tenants", Action = "Write" },
            new() { Id = Guid.NewGuid(), Name = "Tenants.Delete", Module = "Tenants", Action = "Delete" },
            // Products
            new() { Id = Guid.NewGuid(), Name = "Products.Read", Module = "Products", Action = "Read" },
            new() { Id = Guid.NewGuid(), Name = "Products.Write", Module = "Products", Action = "Write" },
            new() { Id = Guid.NewGuid(), Name = "Products.Delete", Module = "Products", Action = "Delete" },
            // Orders
            new() { Id = Guid.NewGuid(), Name = "Orders.Read", Module = "Orders", Action = "Read" },
            new() { Id = Guid.NewGuid(), Name = "Orders.Write", Module = "Orders", Action = "Write" },
            new() { Id = Guid.NewGuid(), Name = "Orders.Delete", Module = "Orders", Action = "Delete" },
            // Articles
            new() { Id = Guid.NewGuid(), Name = "Articles.Read", Module = "Articles", Action = "Read" },
            new() { Id = Guid.NewGuid(), Name = "Articles.Write", Module = "Articles", Action = "Write" },
            new() { Id = Guid.NewGuid(), Name = "Articles.Delete", Module = "Articles", Action = "Delete" },
            // Customers
            new() { Id = Guid.NewGuid(), Name = "Customers.Read", Module = "Customers", Action = "Read" },
            new() { Id = Guid.NewGuid(), Name = "Customers.Write", Module = "Customers", Action = "Write" },
            // Media
            new() { Id = Guid.NewGuid(), Name = "Media.Read", Module = "Media", Action = "Read" },
            new() { Id = Guid.NewGuid(), Name = "Media.Write", Module = "Media", Action = "Write" },
            new() { Id = Guid.NewGuid(), Name = "Media.Delete", Module = "Media", Action = "Delete" },
            // Users
            new() { Id = Guid.NewGuid(), Name = "Users.Read", Module = "Users", Action = "Read" },
            new() { Id = Guid.NewGuid(), Name = "Users.Write", Module = "Users", Action = "Write" },
            // Settings
            new() { Id = Guid.NewGuid(), Name = "Settings.Read", Module = "Settings", Action = "Read" },
            new() { Id = Guid.NewGuid(), Name = "Settings.Write", Module = "Settings", Action = "Write" },
        };

        await db.Permissions.AddRangeAsync(permissions);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} permissions", permissions.Count);
    }

    private async Task SeedHostAdminAsync()
    {
        if (await db.Users.AnyAsync(u => u.IsHostAdmin)) return;

        var hostAdminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "hostadmin",
            Email = "hostadmin@ipages.io.vn",
            FullName = "Host Administrator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Host@123456"),
            IsHostAdmin = true,
            IsActive = true,
            TenantId = null
        };

        await db.Users.AddAsync(hostAdminUser);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded host admin user");
    }

    private async Task SeedDemoTenantAsync()
    {
        if (await db.Tenants.AnyAsync(t => t.Slug == "demo")) return;

        var tenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "Demo Store",
            Slug = "demo",
            Email = "demo@ipages.io.vn",
            Description = "Demo tenant for testing",
            Status = TenantStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddYears(1)
        };
        await db.Tenants.AddAsync(tenant);

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanName = "Professional",
            Price = 0,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            Status = SubscriptionStatus.Active
        };
        await db.Subscriptions.AddAsync(subscription);

        // Seed admin role for demo tenant
        var allPermissions = await db.Permissions.ToListAsync();
        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Admin",
            Description = "Full access",
            IsSystem = true
        };
        await db.Roles.AddAsync(adminRole);
        await db.SaveChangesAsync();

        var rolePermissions = allPermissions
            .Where(p => !p.Module.Equals("Tenants", StringComparison.OrdinalIgnoreCase))
            .Select(p => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = adminRole.Id,
                PermissionId = p.Id
            });
        await db.RolePermissions.AddRangeAsync(rolePermissions);

        // Seed demo admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Username = "admin",
            Email = "admin@demo.ipages.io.vn",
            FullName = "Demo Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
            IsActive = true
        };
        await db.Users.AddAsync(adminUser);
        await db.SaveChangesAsync();

        await db.UserRoles.AddAsync(new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = adminUser.Id,
            RoleId = adminRole.Id,
            TenantId = tenantId
        });

        // Seed sample product categories
        var catId = Guid.NewGuid();
        await db.ProductCategories.AddAsync(new ProductCategory
        {
            Id = catId,
            TenantId = tenantId,
            Name = "Điện tử",
            Slug = "dien-tu",
            Description = "Sản phẩm điện tử",
            IsActive = true,
            SortOrder = 1
        });

        // Seed sample products
        await db.Products.AddRangeAsync(
            new Product
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Laptop Demo Pro",
                Slug = "laptop-demo-pro",
                Sku = "LDP-001",
                Description = "Laptop demo chất lượng cao",
                Price = 25000000,
                SalePrice = 22000000,
                StockQuantity = 50,
                CategoryId = catId,
                Status = ProductStatus.Active
            },
            new Product
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Điện thoại Demo X",
                Slug = "dien-thoai-demo-x",
                Sku = "DTX-001",
                Description = "Điện thoại demo flagship",
                Price = 15000000,
                SalePrice = 13500000,
                StockQuantity = 100,
                CategoryId = catId,
                Status = ProductStatus.Active
            }
        );

        // Seed sample article category
        var artCatId = Guid.NewGuid();
        await db.ArticleCategories.AddAsync(new ArticleCategory
        {
            Id = artCatId,
            TenantId = tenantId,
            Name = "Tin tức",
            Slug = "tin-tuc",
            IsActive = true
        });

        // Seed sample article
        await db.Articles.AddAsync(new Article
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = "Chào mừng đến với Demo Store",
            Slug = "chao-mung-den-voi-demo-store",
            Content = "<p>Đây là bài viết mẫu cho Demo Store.</p>",
            CategoryId = artCatId,
            AuthorId = adminUser.Id,
            Status = ArticleStatus.Published,
            PublishedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded demo tenant with sample data");
    }
}
