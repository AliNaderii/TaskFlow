using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TaskFlow.Application.Abstractions.Services;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Authentication;
using TaskFlow.Infrastructure.Persistence.Repositories;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Infrastructure.MultiTenancy;

namespace TaskFlow.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(
            options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"));
            });
        
        services.AddHttpContextAccessor();

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
                ?? throw new InvalidOperationException("Jwt configuration is missing.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
            });
            
        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskItemRepository, TaskItemRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();

        services.AddScoped<IUnitOfWork>(
            provider => provider.GetRequiredService<ApplicationDbContext>());

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(
                options =>
                {
                    options.User.RequireUniqueEmail = true;
                })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ITenantResolver, TenantResolver>();
        services.AddScoped<ITenantContextInitializer, TenantContextInitializer>();
        services.AddScoped<CurrentTenant>();
        services.AddScoped<ICurrentTenant>(
            provider => provider.GetRequiredService<CurrentTenant>());

        return services;
    }
}