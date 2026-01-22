using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ERPApi.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace ERPApi.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string,
        IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RoleMenu> RoleMenus { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Remove UserRole key configuration since it's inherited from IdentityUserRole<string>
            // The base IdentityDbContext already configures this key

            // Configure composite keys
            builder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Entity<RoleMenu>()
                .HasKey(rm => new { rm.RoleId, rm.MenuId });

            // Configure UserRole relationships
            builder.Entity<UserRole>(entity =>
            {
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure other relationships
            builder.Entity<ApplicationRole>()
                .HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Permission>()
                .HasMany(p => p.RolePermissions)
                .WithOne(rp => rp.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationRole>()
                .HasMany(r => r.RoleMenus)
                .WithOne(rm => rm.Role)
                .HasForeignKey(rm => rm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Menu>()
                .HasMany(m => m.RoleMenus)
                .WithOne(rm => rm.Menu)
                .HasForeignKey(rm => rm.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Menu>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.AuditLogs)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            // Seed permissions
            var permissions = new List<Permission>
            {
                new() { Id = 1, Name = "View Users", Code = "users.view", Group = "Users", Description = "Can view users" },
                new() { Id = 2, Name = "Create Users", Code = "users.create", Group = "Users", Description = "Can create users" },
                new() { Id = 3, Name = "Edit Users", Code = "users.edit", Group = "Users", Description = "Can edit users" },
                new() { Id = 4, Name = "Delete Users", Code = "users.delete", Group = "Users", Description = "Can delete users" },
                new() { Id = 5, Name = "View Roles", Code = "roles.view", Group = "Roles", Description = "Can view roles" },
                new() { Id = 6, Name = "Create Roles", Code = "roles.create", Group = "Roles", Description = "Can create roles" },
                new() { Id = 7, Name = "Edit Roles", Code = "roles.edit", Group = "Roles", Description = "Can edit roles" },
                new() { Id = 8, Name = "Delete Roles", Code = "roles.delete", Group = "Roles", Description = "Can delete roles" },
                new() { Id = 9, Name = "View Permissions", Code = "permissions.view", Group = "Permissions", Description = "Can view permissions" },
                new() { Id = 10, Name = "Manage Permissions", Code = "permissions.manage", Group = "Permissions", Description = "Can manage permissions" },
                new() { Id = 11, Name = "View Menus", Code = "menus.view", Group = "Menus", Description = "Can view menus" },
                new() { Id = 12, Name = "Manage Menus", Code = "menus.manage", Group = "Menus", Description = "Can manage menus" },
                new() { Id = 13, Name = "View Audit Logs", Code = "audit.view", Group = "Audit", Description = "Can view audit logs" }
            };

            builder.Entity<Permission>().HasData(permissions);

            // Seed roles
            var adminRole = new ApplicationRole
            {
                Id = "1", // Using simple string for easier reference
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "System Administrator",
                IsSystemRole = true,
                CreatedDate = DateTime.UtcNow
            };

            var userRole = new ApplicationRole
            {
                Id = "2",
                Name = "User",
                NormalizedName = "USER",
                Description = "Regular User",
                IsSystemRole = true,
                CreatedDate = DateTime.UtcNow
            };

            builder.Entity<ApplicationRole>().HasData(adminRole, userRole);

            // Seed role permissions (Admin gets all permissions)
            var rolePermissions = permissions.Select(p => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id
            }).ToList();

            builder.Entity<RolePermission>().HasData(rolePermissions);

            // Seed menus
            var menus = new List<Menu>
            {
                new() { Id = 1, Title = "Dashboard", Icon = "dashboard", Url = "/dashboard", Order = 1, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Id = 2, Title = "Administration", Icon = "admin_panel_settings", Url = "#", Order = 2, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Id = 3, Title = "Users", Icon = "people", Url = "/admin/users", Order = 1, ParentId = 2, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Id = 4, Title = "Roles", Icon = "assignment_ind", Url = "/admin/roles", Order = 2, ParentId = 2, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Id = 5, Title = "Permissions", Icon = "security", Url = "/admin/permissions", Order = 3, ParentId = 2, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Id = 6, Title = "Menus", Icon = "menu", Url = "/admin/menus", Order = 4, ParentId = 2, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Id = 7, Title = "Audit Logs", Icon = "history", Url = "/admin/audit-logs", Order = 5, ParentId = 2, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Id = 8, Title = "Profile", Icon = "person", Url = "/profile", Order = 3, IsActive = true, CreatedDate = DateTime.UtcNow },
                new() { Id = 9, Title = "Settings", Icon = "settings", Url = "/settings", Order = 4, IsActive = true, CreatedDate = DateTime.UtcNow }
            };

            builder.Entity<Menu>().HasData(menus);

            // Seed role menus (Admin gets all menus)
            var roleMenus = menus.Select(m => new RoleMenu
            {
                RoleId = adminRole.Id,
                MenuId = m.Id
            }).ToList();

            builder.Entity<RoleMenu>().HasData(roleMenus);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<AuditLog>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.Timestamp = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}