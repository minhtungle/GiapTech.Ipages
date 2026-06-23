using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiapTech.Ipages.Infrastructure.Persistence.Migrations;

[Migration("20240101000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Slug = table.Column<string>(maxLength: 100, nullable: false),
                Email = table.Column<string>(maxLength: 200, nullable: true),
                Phone = table.Column<string>(maxLength: 50, nullable: true),
                Address = table.Column<string>(nullable: true),
                Description = table.Column<string>(nullable: true),
                LogoUrl = table.Column<string>(nullable: true),
                FaviconUrl = table.Column<string>(nullable: true),
                Status = table.Column<int>(nullable: false, defaultValue: 1),
                ExpiresAt = table.Column<DateTime>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Tenants", x => x.Id));

        migrationBuilder.CreateIndex("IX_Tenants_Slug", "Tenants", "Slug", unique: true);

        migrationBuilder.CreateTable(
            name: "Permissions",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 100, nullable: false),
                Module = table.Column<string>(maxLength: 100, nullable: false),
                Action = table.Column<string>(maxLength: 50, nullable: false),
                Description = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Permissions", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: true),
                Username = table.Column<string>(maxLength: 100, nullable: false),
                Email = table.Column<string>(maxLength: 200, nullable: false),
                PasswordHash = table.Column<string>(nullable: false),
                FullName = table.Column<string>(maxLength: 200, nullable: true),
                PhoneNumber = table.Column<string>(maxLength: 50, nullable: true),
                AvatarUrl = table.Column<string>(nullable: true),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                IsHostAdmin = table.Column<bool>(nullable: false, defaultValue: false),
                RefreshToken = table.Column<string>(nullable: true),
                RefreshTokenExpiresAt = table.Column<DateTime>(nullable: true),
                LastLoginAt = table.Column<DateTime>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
                table.ForeignKey("FK_Users_Tenants_TenantId", x => x.TenantId, "Tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_Users_TenantId_Username", "Users", new[] { "TenantId", "Username" }, unique: true);
        migrationBuilder.CreateIndex("IX_Users_TenantId_Email", "Users", new[] { "TenantId", "Email" }, unique: true);

        migrationBuilder.CreateTable(
            name: "Roles",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 100, nullable: false),
                Description = table.Column<string>(nullable: true),
                IsSystem = table.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roles", x => x.Id);
                table.ForeignKey("FK_Roles_Tenants_TenantId", x => x.TenantId, "Tenants", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserRoles",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                UserId = table.Column<Guid>(nullable: false),
                RoleId = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRoles", x => x.Id);
                table.ForeignKey("FK_UserRoles_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_UserRoles_Roles_RoleId", x => x.RoleId, "Roles", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RolePermissions",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                RoleId = table.Column<Guid>(nullable: false),
                PermissionId = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => x.Id);
                table.ForeignKey("FK_RolePermissions_Roles_RoleId", x => x.RoleId, "Roles", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_RolePermissions_Permissions_PermissionId", x => x.PermissionId, "Permissions", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Subscriptions",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                PlanName = table.Column<string>(maxLength: 100, nullable: false),
                Price = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                StartDate = table.Column<DateTime>(nullable: false),
                EndDate = table.Column<DateTime>(nullable: false),
                Status = table.Column<int>(nullable: false, defaultValue: 1),
                Notes = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Subscriptions", x => x.Id);
                table.ForeignKey("FK_Subscriptions_Tenants_TenantId", x => x.TenantId, "Tenants", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Amount = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(maxLength: 10, nullable: false, defaultValue: "VND"),
                Method = table.Column<int>(nullable: false),
                Status = table.Column<int>(nullable: false, defaultValue: 1),
                TransactionId = table.Column<string>(nullable: true),
                Notes = table.Column<string>(nullable: true),
                PaidAt = table.Column<DateTime>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payments", x => x.Id);
                table.ForeignKey("FK_Payments_Tenants_TenantId", x => x.TenantId, "Tenants", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ArticleCategories",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Slug = table.Column<string>(maxLength: 200, nullable: false),
                Description = table.Column<string>(nullable: true),
                ParentId = table.Column<Guid>(nullable: true),
                SortOrder = table.Column<int>(nullable: false, defaultValue: 0),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArticleCategories", x => x.Id);
                table.ForeignKey("FK_ArticleCategories_ArticleCategories_ParentId", x => x.ParentId, "ArticleCategories", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Tags",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 100, nullable: false),
                Slug = table.Column<string>(maxLength: 100, nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Tags", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Articles",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Title = table.Column<string>(maxLength: 500, nullable: false),
                Slug = table.Column<string>(maxLength: 500, nullable: false),
                Excerpt = table.Column<string>(nullable: true),
                Content = table.Column<string>(nullable: false),
                ThumbnailUrl = table.Column<string>(nullable: true),
                CategoryId = table.Column<Guid>(nullable: true),
                AuthorId = table.Column<Guid>(nullable: false),
                Status = table.Column<int>(nullable: false, defaultValue: 1),
                PublishedAt = table.Column<DateTime>(nullable: true),
                ScheduledAt = table.Column<DateTime>(nullable: true),
                ViewCount = table.Column<int>(nullable: false, defaultValue: 0),
                MetaTitle = table.Column<string>(maxLength: 300, nullable: true),
                MetaDescription = table.Column<string>(maxLength: 500, nullable: true),
                CanonicalUrl = table.Column<string>(nullable: true),
                OgImage = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Articles", x => x.Id);
                table.ForeignKey("FK_Articles_ArticleCategories_CategoryId", x => x.CategoryId, "ArticleCategories", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_Articles_TenantId_Slug", "Articles", new[] { "TenantId", "Slug" }, unique: true);

        migrationBuilder.CreateTable(
            name: "ProductCategories",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Slug = table.Column<string>(maxLength: 200, nullable: false),
                Description = table.Column<string>(nullable: true),
                ImageUrl = table.Column<string>(nullable: true),
                ParentId = table.Column<Guid>(nullable: true),
                SortOrder = table.Column<int>(nullable: false, defaultValue: 0),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductCategories", x => x.Id);
                table.ForeignKey("FK_ProductCategories_ProductCategories_ParentId", x => x.ParentId, "ProductCategories", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 300, nullable: false),
                Slug = table.Column<string>(maxLength: 300, nullable: false),
                Sku = table.Column<string>(maxLength: 100, nullable: true),
                Description = table.Column<string>(nullable: true),
                ShortDescription = table.Column<string>(nullable: true),
                Price = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                SalePrice = table.Column<decimal>(precision: 18, scale: 2, nullable: true),
                ThumbnailUrl = table.Column<string>(nullable: true),
                Images = table.Column<string>(nullable: true),
                CategoryId = table.Column<Guid>(nullable: true),
                StockQuantity = table.Column<int>(nullable: false, defaultValue: 0),
                TrackInventory = table.Column<bool>(nullable: false, defaultValue: true),
                Status = table.Column<int>(nullable: false, defaultValue: 1),
                SortOrder = table.Column<int>(nullable: false, defaultValue: 0),
                SoldCount = table.Column<int>(nullable: false, defaultValue: 0),
                MetaTitle = table.Column<string>(maxLength: 300, nullable: true),
                MetaDescription = table.Column<string>(maxLength: 500, nullable: true),
                CanonicalUrl = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
                table.ForeignKey("FK_Products_ProductCategories_CategoryId", x => x.CategoryId, "ProductCategories", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_Products_TenantId_Slug", "Products", new[] { "TenantId", "Slug" }, unique: true);

        migrationBuilder.CreateTable(
            name: "ProductVariants",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                ProductId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Sku = table.Column<string>(maxLength: 100, nullable: true),
                Price = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                SalePrice = table.Column<decimal>(precision: 18, scale: 2, nullable: true),
                StockQuantity = table.Column<int>(nullable: false, defaultValue: 0),
                ImageUrl = table.Column<string>(nullable: true),
                AttributeValues = table.Column<string>(nullable: true),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductVariants", x => x.Id);
                table.ForeignKey("FK_ProductVariants_Products_ProductId", x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductAttributes",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                ProductId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 100, nullable: false),
                Values = table.Column<string>(nullable: false),
                SortOrder = table.Column<int>(nullable: false, defaultValue: 0),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductAttributes", x => x.Id);
                table.ForeignKey("FK_ProductAttributes_Products_ProductId", x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Inventories",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                ProductId = table.Column<Guid>(nullable: false),
                VariantId = table.Column<Guid>(nullable: true),
                Type = table.Column<int>(nullable: false),
                Quantity = table.Column<int>(nullable: false),
                QuantityBefore = table.Column<int>(nullable: false),
                QuantityAfter = table.Column<int>(nullable: false),
                Reference = table.Column<string>(nullable: true),
                Notes = table.Column<string>(nullable: true),
                CreatedBy = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Inventories", x => x.Id);
                table.ForeignKey("FK_Inventories_Products_ProductId", x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Customers",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                FullName = table.Column<string>(maxLength: 200, nullable: false),
                Email = table.Column<string>(maxLength: 200, nullable: true),
                Phone = table.Column<string>(maxLength: 50, nullable: true),
                AvatarUrl = table.Column<string>(nullable: true),
                DateOfBirth = table.Column<DateTime>(nullable: true),
                Gender = table.Column<string>(maxLength: 10, nullable: true),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                LoyaltyPoints = table.Column<int>(nullable: false, defaultValue: 0),
                Notes = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Customers", x => x.Id));

        migrationBuilder.CreateTable(
            name: "CustomerAddresses",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                CustomerId = table.Column<Guid>(nullable: false),
                FullName = table.Column<string>(maxLength: 200, nullable: false),
                Phone = table.Column<string>(maxLength: 50, nullable: false),
                Address = table.Column<string>(nullable: false),
                Ward = table.Column<string>(nullable: true),
                District = table.Column<string>(nullable: true),
                Province = table.Column<string>(nullable: true),
                Country = table.Column<string>(nullable: true, defaultValue: "Vietnam"),
                IsDefault = table.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CustomerAddresses", x => x.Id);
                table.ForeignKey("FK_CustomerAddresses_Customers_CustomerId", x => x.CustomerId, "Customers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                OrderCode = table.Column<string>(maxLength: 50, nullable: false),
                CustomerId = table.Column<Guid>(nullable: true),
                CustomerName = table.Column<string>(maxLength: 200, nullable: false),
                CustomerEmail = table.Column<string>(maxLength: 200, nullable: true),
                CustomerPhone = table.Column<string>(maxLength: 50, nullable: false),
                ShippingAddress = table.Column<string>(nullable: false),
                ShippingWard = table.Column<string>(nullable: true),
                ShippingDistrict = table.Column<string>(nullable: true),
                ShippingProvince = table.Column<string>(nullable: true),
                SubTotal = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                ShippingFee = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                Discount = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                Total = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                Status = table.Column<int>(nullable: false, defaultValue: 1),
                PaymentMethod = table.Column<int>(nullable: false),
                PaymentStatus = table.Column<int>(nullable: false, defaultValue: 1),
                Notes = table.Column<string>(nullable: true),
                CouponCode = table.Column<string>(nullable: true),
                ConfirmedAt = table.Column<DateTime>(nullable: true),
                ShippedAt = table.Column<DateTime>(nullable: true),
                CompletedAt = table.Column<DateTime>(nullable: true),
                CancelledAt = table.Column<DateTime>(nullable: true),
                CancelReason = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
                table.ForeignKey("FK_Orders_Customers_CustomerId", x => x.CustomerId, "Customers", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_Orders_TenantId_OrderCode", "Orders", new[] { "TenantId", "OrderCode" }, unique: true);

        migrationBuilder.CreateTable(
            name: "OrderItems",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                OrderId = table.Column<Guid>(nullable: false),
                ProductId = table.Column<Guid>(nullable: false),
                VariantId = table.Column<Guid>(nullable: true),
                ProductName = table.Column<string>(maxLength: 300, nullable: false),
                VariantName = table.Column<string>(nullable: true),
                Sku = table.Column<string>(nullable: true),
                ImageUrl = table.Column<string>(nullable: true),
                UnitPrice = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                Quantity = table.Column<int>(nullable: false),
                Total = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderItems", x => x.Id);
                table.ForeignKey("FK_OrderItems_Orders_OrderId", x => x.OrderId, "Orders", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Carts",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                CustomerId = table.Column<Guid>(nullable: true),
                SessionId = table.Column<string>(nullable: true),
                CouponCode = table.Column<string>(nullable: true),
                Discount = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                ExpiresAt = table.Column<DateTime>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Carts", x => x.Id));

        migrationBuilder.CreateTable(
            name: "CartItems",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                CartId = table.Column<Guid>(nullable: false),
                ProductId = table.Column<Guid>(nullable: false),
                VariantId = table.Column<Guid>(nullable: true),
                ProductName = table.Column<string>(maxLength: 300, nullable: false),
                VariantName = table.Column<string>(nullable: true),
                ImageUrl = table.Column<string>(nullable: true),
                UnitPrice = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                Quantity = table.Column<int>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CartItems", x => x.Id);
                table.ForeignKey("FK_CartItems_Carts_CartId", x => x.CartId, "Carts", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Coupons",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Code = table.Column<string>(maxLength: 50, nullable: false),
                Name = table.Column<string>(nullable: true),
                Description = table.Column<string>(nullable: true),
                Type = table.Column<int>(nullable: false),
                Value = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                MinOrderAmount = table.Column<decimal>(precision: 18, scale: 2, nullable: true),
                MaxDiscount = table.Column<decimal>(precision: 18, scale: 2, nullable: true),
                UsageLimit = table.Column<int>(nullable: true),
                UsedCount = table.Column<int>(nullable: false, defaultValue: 0),
                StartsAt = table.Column<DateTime>(nullable: true),
                ExpiresAt = table.Column<DateTime>(nullable: true),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Coupons", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ApiKeys",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Key = table.Column<string>(maxLength: 100, nullable: false),
                Secret = table.Column<string>(nullable: false),
                AllowedOrigins = table.Column<string>(nullable: true),
                Permissions = table.Column<string>(nullable: true),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                ExpiresAt = table.Column<DateTime>(nullable: true),
                LastUsedAt = table.Column<DateTime>(nullable: true),
                RequestCount = table.Column<long>(nullable: false, defaultValue: 0L),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_ApiKeys", x => x.Id));

        migrationBuilder.CreateTable(
            name: "LandingPages",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Slug = table.Column<string>(maxLength: 200, nullable: false),
                Template = table.Column<string>(nullable: true),
                Status = table.Column<int>(nullable: false, defaultValue: 1),
                MetaTitle = table.Column<string>(nullable: true),
                MetaDescription = table.Column<string>(nullable: true),
                OgImage = table.Column<string>(nullable: true),
                PublishedAt = table.Column<DateTime>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_LandingPages", x => x.Id));

        migrationBuilder.CreateTable(
            name: "LandingSections",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                LandingPageId = table.Column<Guid>(nullable: false),
                Type = table.Column<string>(maxLength: 50, nullable: false),
                Title = table.Column<string>(nullable: true),
                Settings = table.Column<string>(nullable: true),
                SortOrder = table.Column<int>(nullable: false, defaultValue: 0),
                IsVisible = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LandingSections", x => x.Id);
                table.ForeignKey("FK_LandingSections_LandingPages_LandingPageId", x => x.LandingPageId, "LandingPages", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Forms",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Description = table.Column<string>(nullable: true),
                Type = table.Column<int>(nullable: false, defaultValue: 1),
                Fields = table.Column<string>(nullable: false, defaultValue: "[]"),
                SuccessMessage = table.Column<string>(nullable: true),
                NotifyEmails = table.Column<string>(nullable: true),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Forms", x => x.Id));

        migrationBuilder.CreateTable(
            name: "FormEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                FormId = table.Column<Guid>(nullable: false),
                Data = table.Column<string>(nullable: false, defaultValue: "{}"),
                IpAddress = table.Column<string>(nullable: true),
                UserAgent = table.Column<string>(nullable: true),
                IsRead = table.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FormEntries", x => x.Id);
                table.ForeignKey("FK_FormEntries_Forms_FormId", x => x.FormId, "Forms", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MediaFiles",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                FileName = table.Column<string>(maxLength: 300, nullable: false),
                OriginalName = table.Column<string>(maxLength: 300, nullable: false),
                ContentType = table.Column<string>(maxLength: 100, nullable: false),
                FileSize = table.Column<long>(nullable: false),
                Url = table.Column<string>(nullable: false),
                StoragePath = table.Column<string>(nullable: false),
                Type = table.Column<int>(nullable: false),
                Folder = table.Column<string>(nullable: true),
                Tags = table.Column<string>(nullable: true),
                AltText = table.Column<string>(nullable: true),
                Width = table.Column<int>(nullable: true),
                Height = table.Column<int>(nullable: true),
                UploadedBy = table.Column<Guid>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_MediaFiles", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Menus",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Location = table.Column<string>(maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Menus", x => x.Id));

        migrationBuilder.CreateTable(
            name: "MenuItems",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                MenuId = table.Column<Guid>(nullable: false),
                Label = table.Column<string>(maxLength: 200, nullable: false),
                Url = table.Column<string>(nullable: true),
                Target = table.Column<string>(nullable: true),
                Icon = table.Column<string>(nullable: true),
                ParentId = table.Column<Guid>(nullable: true),
                SortOrder = table.Column<int>(nullable: false, defaultValue: 0),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MenuItems", x => x.Id);
                table.ForeignKey("FK_MenuItems_Menus_MenuId", x => x.MenuId, "Menus", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_MenuItems_MenuItems_ParentId", x => x.ParentId, "MenuItems", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "SeoMetadata",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                EntityType = table.Column<string>(maxLength: 100, nullable: false),
                EntityId = table.Column<Guid>(nullable: false),
                MetaTitle = table.Column<string>(maxLength: 300, nullable: true),
                MetaDescription = table.Column<string>(maxLength: 500, nullable: true),
                CanonicalUrl = table.Column<string>(nullable: true),
                OgTitle = table.Column<string>(nullable: true),
                OgDescription = table.Column<string>(nullable: true),
                OgImage = table.Column<string>(nullable: true),
                JsonLd = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_SeoMetadata", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: true),
                UserId = table.Column<Guid>(nullable: true),
                Username = table.Column<string>(maxLength: 100, nullable: true),
                Action = table.Column<string>(maxLength: 100, nullable: false),
                EntityType = table.Column<string>(maxLength: 100, nullable: false),
                EntityId = table.Column<string>(nullable: true),
                OldValues = table.Column<string>(nullable: true),
                NewValues = table.Column<string>(nullable: true),
                IpAddress = table.Column<string>(maxLength: 50, nullable: true),
                UserAgent = table.Column<string>(nullable: true),
                IsSuccess = table.Column<bool>(nullable: false, defaultValue: true),
                ErrorMessage = table.Column<string>(nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_AuditLogs", x => x.Id));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("AuditLogs");
        migrationBuilder.DropTable("SeoMetadata");
        migrationBuilder.DropTable("MenuItems");
        migrationBuilder.DropTable("Menus");
        migrationBuilder.DropTable("MediaFiles");
        migrationBuilder.DropTable("FormEntries");
        migrationBuilder.DropTable("Forms");
        migrationBuilder.DropTable("LandingSections");
        migrationBuilder.DropTable("LandingPages");
        migrationBuilder.DropTable("ApiKeys");
        migrationBuilder.DropTable("Coupons");
        migrationBuilder.DropTable("CartItems");
        migrationBuilder.DropTable("Carts");
        migrationBuilder.DropTable("OrderItems");
        migrationBuilder.DropTable("Orders");
        migrationBuilder.DropTable("CustomerAddresses");
        migrationBuilder.DropTable("Customers");
        migrationBuilder.DropTable("Inventories");
        migrationBuilder.DropTable("ProductAttributes");
        migrationBuilder.DropTable("ProductVariants");
        migrationBuilder.DropTable("Products");
        migrationBuilder.DropTable("ProductCategories");
        migrationBuilder.DropTable("Articles");
        migrationBuilder.DropTable("ArticleCategories");
        migrationBuilder.DropTable("Tags");
        migrationBuilder.DropTable("Payments");
        migrationBuilder.DropTable("Subscriptions");
        migrationBuilder.DropTable("RolePermissions");
        migrationBuilder.DropTable("UserRoles");
        migrationBuilder.DropTable("Roles");
        migrationBuilder.DropTable("Permissions");
        migrationBuilder.DropTable("Users");
        migrationBuilder.DropTable("Tenants");
    }
}
