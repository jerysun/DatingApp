using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository repository;
        private readonly IConfiguration config;
        private readonly IMapper mapper;
        public AuthController(IAuthRepository repository, IConfiguration config, IMapper mapper)
        {
            this.mapper = mapper;
            this.config = config;
            this.repository = repository;
        }

        [HttpPost("register")]
        //We need [FromBody] if we don't use [ApiController]
        public async Task<IActionResult> Register(/*[FromBody]*/UserForRegisterDto userForRegisterDto)
        {
            /*
            //validate request without [ApiController], instead we use ModelState(traditional MVC style)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            */
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await repository.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists");

            var userToCreate = this.mapper.Map<User>(userForRegisterDto);
            var createdUser = await repository.Register(userToCreate, userForRegisterDto.Password);
            
            // we don't return createdUser because we don't want the password to be revealed
            var userToReturn = this.mapper.Map<UserForDetailedDto>(createdUser);

            // similar to php, route is redirected to xx/api/users/id, the GetUser method
            // in UsersController will be called, so the return value actually are 
            // "201 Created with the Location Header", and entity(Dto).
            return CreatedAtRoute("GetUser", new {controller = "Users", id = createdUser.Id }, userToReturn);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await repository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();//401

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var user = mapper.Map<UserForListDto>(userFromRepo);

            //Write the token into the response that will shortly be sent to our client
            //This operation makes token "stringlized", the token as a parameter in WriteToken
            //is an object of type SecurityToken, whereas the return value is a string
            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            });
        }
    }
}