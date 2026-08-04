using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Hangfire;
using Hangfire.SqlServer;
using TaskFlow.Application.Abstractions.Services;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Abstractions.Authorization;
using TaskFlow.Application.Abstractions.BackgroundJobs;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Authentication;
using TaskFlow.Infrastructure.Persistence.Repositories;
using TaskFlow.Infrastructure.Services;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Infrastructure.Messaging;
using TaskFlow.Infrastructure.MultiTenancy;
using TaskFlow.Infrastructure.Authorization;
using TaskFlow.Infrastructure.BackgroundJobs;
using TaskFlow.Application.Events.Handlers;
using TaskFlow.Domain.Events;

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

        services.Configure<EmailOptions>(
            configuration.GetSection(EmailOptions.SectionName));

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
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        Console.WriteLine($"[DEBUG] JWT OnMessageReceived token present: {context.Token is not null}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"[DEBUG] JWT OnTokenValidated: IsAuthenticated={context.Principal?.Identity?.IsAuthenticated}");
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"[DEBUG] JWT OnAuthenticationFailed: {context.Exception}");
                        return Task.CompletedTask;
                    }
                };

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
                    {
                        KeyId = "TaskFlow"
                    }
                };
            });
            
        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskItemRepository, TaskItemRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddScoped<IUnitOfWork>(
            provider => provider.GetRequiredService<ApplicationDbContext>());

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.PostConfigure<AuthenticationOptions>(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ITenantResolver, TenantResolver>();
        services.AddScoped<ITenantContextInitializer, TenantContextInitializer>();
        services.AddScoped<CurrentTenant>();
        services.AddScoped<ICurrentTenant>(
            provider => provider.GetRequiredService<CurrentTenant>());

        services.AddScoped<IAppAuthorizationService, AuthorizationService>();

        services.AddScoped<IAuthorizationHandler, OrganizationMemberHandler>();
        services.AddScoped<IAuthorizationHandler, OrganizationAdminHandler>();
        services.AddScoped<IAuthorizationHandler, ProjectManagerHandler>();

        services.AddScoped<IDomainEventHandler<TaskAssignedEvent>, TaskAssignedEventHandler>();
        services.AddScoped<IDomainEventHandler<CommentCreatedEvent>, CommentCreatedEventHandler>();
        services.AddScoped<IDomainEventHandler<MembershipAddedEvent>, MembershipAddedEventHandler>();
        services.AddScoped<IDomainEventHandler<ProjectArchivedEvent>, ProjectArchivedEventHandler>();
        services.AddScoped<IDomainEventHandler<TaskCompletedEvent>, TaskCompletedEventHandler>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped<IEmailService, EmailService>();

        // Hangfire
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));
            config.UseSimpleAssemblyNameTypeSerializer();
            config.UseRecommendedSerializerSettings();
        });
        services.AddHangfireServer();

        // Background Jobs
        services.AddScoped<IReminderJobService, ReminderJobService>();
        services.AddScoped<ICleanupJobService, CleanupJobService>();
        services.AddScoped<IRecurringJobService, RecurringJobService>();

        return services;
    }
}
