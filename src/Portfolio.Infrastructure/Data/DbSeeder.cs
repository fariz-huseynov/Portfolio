using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Domain.Constants;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Identity;

namespace Portfolio.Infrastructure.Data;

public static class DbSeeder
{
    private const string SuperAdminRoleName = "SuperAdmin";
    private const string AdminRoleName = "Admin";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await dbContext.Database.MigrateAsync();

        await SeedPermissionsAsync(dbContext);
        await SeedRolesAsync(dbContext, roleManager);
        await EnsureSuperAdminHasAllPermissionsAsync(dbContext, roleManager);
        await SeedSuperAdminAsync(userManager, configuration);
        await SeedSingletonContentAsync(dbContext);
    }

    private static async Task SeedPermissionsAsync(AppDbContext dbContext)
    {
        var existingNames = await dbContext.Permissions.Select(p => p.Name).ToListAsync();

        var newPermissions = Permissions.All
            .Where(name => !existingNames.Contains(name))
            .Select(name => new Permission
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = name.Replace(".", " — ")
            });

        if (newPermissions.Any())
        {
            dbContext.Permissions.AddRange(newPermissions);
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedRolesAsync(AppDbContext dbContext, RoleManager<ApplicationRole> roleManager)
    {
        // SuperAdmin role — all permissions
        if (!await roleManager.RoleExistsAsync(SuperAdminRoleName))
        {
            var role = new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = SuperAdminRoleName,
                Description = "Full system access",
                CreatedAt = DateTime.UtcNow
            };
            await roleManager.CreateAsync(role);

            var allPermissions = await dbContext.Permissions.ToListAsync();
            var rolePermissions = allPermissions.Select(p => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = p.Id
            });
            dbContext.RolePermissions.AddRange(rolePermissions);
            await dbContext.SaveChangesAsync();
        }

        // Admin role — content + leads, no user management
        if (!await roleManager.RoleExistsAsync(AdminRoleName))
        {
            var role = new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = AdminRoleName,
                Description = "Content and leads management",
                CreatedAt = DateTime.UtcNow
            };
            await roleManager.CreateAsync(role);

            var adminPermissionNames = new[]
            {
                Permissions.DashboardView,
                Permissions.BlogsView, Permissions.BlogsCreate, Permissions.BlogsEdit, Permissions.BlogsDelete,
                Permissions.ProjectsView, Permissions.ProjectsCreate, Permissions.ProjectsEdit, Permissions.ProjectsDelete,
                Permissions.LeadsView, Permissions.LeadsMarkRead,
                Permissions.SiteContentView, Permissions.SiteContentEdit,
                Permissions.SettingsView
            };

            var permissions = await dbContext.Permissions
                .Where(p => adminPermissionNames.Contains(p.Name))
                .ToListAsync();

            var rolePermissions = permissions.Select(p => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = p.Id
            });
            dbContext.RolePermissions.AddRange(rolePermissions);
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task EnsureSuperAdminHasAllPermissionsAsync(AppDbContext dbContext, RoleManager<ApplicationRole> roleManager)
    {
        var superAdminRole = await roleManager.FindByNameAsync(SuperAdminRoleName);
        if (superAdminRole is null) return;

        var allPermissionIds = await dbContext.Permissions.Select(p => p.Id).ToListAsync();
        var existingPermissionIds = await dbContext.RolePermissions
            .Where(rp => rp.RoleId == superAdminRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var missingPermissionIds = allPermissionIds.Except(existingPermissionIds).ToList();

        if (missingPermissionIds.Count > 0)
        {
            var newRolePermissions = missingPermissionIds.Select(pid => new RolePermission
            {
                RoleId = superAdminRole.Id,
                PermissionId = pid
            });
            dbContext.RolePermissions.AddRange(newRolePermissions);
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        var email = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(
                "Admin credentials not configured. Please set Seed:AdminEmail (e.g. admin@portfolio.dev) and Seed:AdminPassword in user-secrets or environment variables.");
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null) return;

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FullName = "Super Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, SuperAdminRoleName);
    }

    private static async Task SeedSingletonContentAsync(AppDbContext dbContext)
    {
        // ─── Site Settings ───────────────────────────────────────
        if (!await dbContext.SiteSettings.AnyAsync())
        {
            dbContext.SiteSettings.Add(new SiteSettings
            {
                Id = Guid.NewGuid(),
                SiteName = "My Portfolio",
                SeoTitle = "Portfolio - Personal Branding & CMS",
                SeoDescription = "A personal portfolio website with content management",
                FooterText = "Built with Portfolio CMS",
                CreatedAt = DateTime.UtcNow
            });
        }

        // ─── Hero Section ────────────────────────────────────────
        if (!await dbContext.HeroSections.AnyAsync())
        {
            dbContext.HeroSections.Add(new HeroSection
            {
                Id = Guid.NewGuid(),
                Title = "Welcome to My Portfolio",
                Subtitle = "Showcasing my work and experience",
                CtaText = "View Projects",
                CtaUrl = "/projects",
                CreatedAt = DateTime.UtcNow
            });
        }

        // ─── About Section ───────────────────────────────────────
        if (!await dbContext.AboutSections.AnyAsync())
        {
            dbContext.AboutSections.Add(new AboutSection
            {
                Id = Guid.NewGuid(),
                Title = "About Me",
                Content = "Tell your story here. Edit this section from the admin panel.",
                CreatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
