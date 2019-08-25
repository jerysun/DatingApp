using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository repository;
        public AuthController(IAuthRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string password)
        {
            // TODO: validate request

            username = username.ToLower();

            if (await repository.UserExists(username))
                return BadRequest("Username already exists");

            var userToCreate = new User
            {
                Username = username,
            };

            var createdUser = await repository.Register(userToCreate, password);

            return StatusCode(201);
        }
    }
}