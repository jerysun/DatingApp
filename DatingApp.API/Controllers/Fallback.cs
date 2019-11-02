using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [AllowAnonymous]
    public class Fallback : Controller // used for publishing
    {
        public IActionResult Index()
        {
            return PhysicalFile(Path.Combine
            (Directory.GetCurrentDirectory(),
                "wwwroot", "index.html"), "text/html");
        }
    }
}