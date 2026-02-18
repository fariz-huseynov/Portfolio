using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Common;
using Portfolio.Application.Interfaces;
using Portfolio.Application.Services;
using Portfolio.Domain.Interfaces;
using Portfolio.Infrastructure.Caching;
using Portfolio.Infrastructure.Data;
using Portfolio.Infrastructure.Identity;
using Portfolio.Infrastructure.Services;
using Portfolio.Infrastructure.Storage;

namespace Portfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IBlogPostRepository, BlogPostRepository>();
        services.AddScoped<IIpRuleRepository, IpRuleRepository>();

        // Application Services
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IBlogPostService, BlogPostService>();
        services.AddScoped<ISiteContentService, SiteContentService>();
        services.AddScoped<IIpRuleService, IpRuleService>();
        services.AddScoped<IFileManagementService, FileManagementService>();

        // Identity Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();

        // CAPTCHA (Lazy.Captcha.Core)
        services.AddCaptcha(configuration);
        services.AddScoped<ICaptchaService, CaptchaService>();

        // Email
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<FrontendSettings>(configuration.GetSection(FrontendSettings.SectionName));
        services.AddScoped<IEmailService, SmtpEmailService>();

        // HTML Sanitization
        services.AddSingleton<IHtmlSanitizerService, HtmlSanitizerService>();

        // Caching (L1 in-memory + L2 Redis)
        services.Configure<CachingOptions>(configuration.GetSection("Caching"));
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "Portfolio:";
        });
        services.AddScoped<IHybridCacheService, HybridCacheService>();

        // File Storage
        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(name: "database");

        return services;
    }
}
