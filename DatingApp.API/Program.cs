using System;
using DatingApp.API.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DatingApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            // This CreateScope() comes from Microsoft.Extensions.DependencyInjection,
            // Extensions means Extension methods group, yes, DI exists in the form of
            // Extension methods, DI works at runtime operated by CLR and JRT making
            // use of Reflection technology. "Place extension methods in the 
            // Microsoft.Extensions.DependencyInjection namespace to encapsulate groups 
            // of service registrations". See also
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.2
            using (var scope = host.Services.CreateScope())
            {
                var service = scope.ServiceProvider;// Something like CreateFactory();
                try
                {
                    // something like factory.Create<DataContext>(), or simple new DataContext();
                    var context = service.GetRequiredService<DataContext>();
                    context.Database.Migrate();
                    Seed.SeedUsers(context);
                }
                catch (Exception ex)
                {
                    var logger = service.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred during migration");
                }
            }
            
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
