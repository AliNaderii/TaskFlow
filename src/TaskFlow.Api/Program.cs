using TaskFlow.Application.DependencyInjection;
using TaskFlow.Infrastructure.DependencyInjection;
using TaskFlow.Api.Extensions;
using TaskFlow.Infrastructure.MultiTenancy;


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
            app.MapControllers();

            app.Run();
        }
    }
}
