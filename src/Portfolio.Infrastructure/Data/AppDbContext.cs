using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Identity;

namespace Portfolio.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Existing
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    // Permissions
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Identity
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // CMS
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<HeroSection> HeroSections => Set<HeroSection>();
    public DbSet<AboutSection> AboutSections => Set<AboutSection>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    // Security
    public DbSet<IpRule> IpRules => Set<IpRule>();

    // File Management
    public DbSet<FileMetadata> FileMetadata => Set<FileMetadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
