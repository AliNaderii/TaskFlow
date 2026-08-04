using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using TaskFlow.Application.Abstractions.BackgroundJobs;
using TaskFlow.Application.DependencyInjection;
using TaskFlow.Infrastructure.DependencyInjection;
using TaskFlow.Api.Extensions;
using TaskFlow.Infrastructure.Authorization;
using TaskFlow.Infrastructure.MultiTenancy;
using TaskFlow.Infrastructure.BackgroundJobs;


namespace TaskFlow.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication();

            builder.Services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

                options.AddPolicy("OrganizationMember", policy =>
                    policy.Requirements.Add(new OrganizationMemberRequirement()));
                options.AddPolicy("OrganizationAdmin", policy =>
                    policy.Requirements.Add(new OrganizationAdminRequirement()));
                options.AddPolicy("ProjectManager", policy =>
                    policy.Requirements.Add(new ProjectManagerRequirement()));
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API v1");
                    options.RoutePrefix = "swagger";
                });
            }
            
            app.UseExceptionHandling();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseTenant();
            app.UseAuthorization();

            // Skip Hangfire dashboard and recurring job registration in integration tests
            // (tests use in-memory storage and do not need the SQL-backed server lifecycle).
            if (!app.Environment.IsEnvironment("Testing"))
            {
                // Hangfire Dashboard - secured with OrganizationAdmin policy
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = [new HangfireAuthorizationFilter("OrganizationAdmin")],
                    DashboardTitle = "TaskFlow Background Jobs"
                });

                // Configure recurring jobs
                using (var scope = app.Services.CreateScope())
                {
                    var recurringJobService = scope.ServiceProvider.GetRequiredService<IRecurringJobService>();
                    recurringJobService.ConfigureRecurringJobs();
                }
            }

            app.MapControllers();

            app.Run();
        }
    }
}
