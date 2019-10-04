using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // The return value of next() gives the access to 
            // HttpContext for the action to be executed
            var resultContext = await next();
            // Get userId from jwt
            var userId = int.Parse(resultContext.HttpContext.User
                .FindFirst(ClaimTypes.NameIdentifier).Value);
            
            // Get it from the Dependency Injection Container, it's registered in
            // Startup.cs: services.AddScoped<IDatingRepository, DatingRepository>();
            // GetService is not recognized by Intellisense, so we need manually add
            // using Microsoft.Extensions.DependencyInjection;
            var repo = resultContext.HttpContext.RequestServices.GetService<IDatingRepository>();
            var user = await repo.GetUser(userId);
            user.LastActive = DateTime.Now;
            await repo.SaveAll();
        }
    }
}