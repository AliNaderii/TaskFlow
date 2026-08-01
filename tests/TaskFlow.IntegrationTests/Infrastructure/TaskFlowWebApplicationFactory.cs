using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using TaskFlow.Application.Abstractions.Services;
using TaskFlow.Infrastructure.Authentication;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.IntegrationTests.Infrastructure;

public class TaskFlowWebApplicationFactory : WebApplicationFactory<TaskFlow.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "TestSecretKeyForIntegrationTestsThatIsLongEnoughForHS256Algorithm",
                ["Jwt:Issuer"] = "TaskFlow",
                ["Jwt:Audience"] = "TaskFlow.Client",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "7"
            }!);
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TaskFlowTestDb");
            });

            // Mock IEmailService for testing
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(x => x.SendInvitationAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(TaskFlow.Domain.Common.BaseResult.Success());
            
            emailServiceMock.Setup(x => x.SendEmailAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(TaskFlow.Domain.Common.BaseResult.Success());

            services.RemoveAll<IEmailService>();
            services.AddSingleton(emailServiceMock.Object);

            // Ensure the database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
