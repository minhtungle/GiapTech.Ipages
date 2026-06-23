using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Common;
using GiapTech.Ipages.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentTenantService _tenantService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenantService tenantService) : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleCategory> ArticleCategories => Set<ArticleCategory>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<LandingPage> LandingPages => Set<LandingPage>();
    public DbSet<LandingSection> LandingSections => Set<LandingSection>();
    public DbSet<Form> Forms => Set<Form>();
    public DbSet<FormEntry> FormEntries => Set<FormEntry>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<SeoMetadata> SeoMetadata => Set<SeoMetadata>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global tenant filter — lambdas capture `this` (_tenantService) so the filter
        // is evaluated per-request using the scoped service value, not a cached value.
        modelBuilder.Entity<Role>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<Article>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<ArticleCategory>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<Tag>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<Product>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<ProductCategory>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<ProductVariant>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<ProductAttribute>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<Inventory>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<Customer>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<CustomerAddress>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<Order>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<Cart>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<CartItem>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<Coupon>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<ApiKey>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<LandingPage>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<LandingSection>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<Form>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<FormEntry>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<MediaFile>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<Menu>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<MenuItem>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
        modelBuilder.Entity<SeoMetadata>().HasQueryFilter(e => _tenantService.TenantId == null || e.TenantId == _tenantService.TenantId.Value);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
