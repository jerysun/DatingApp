using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IMapper mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(IConfiguration config, IMapper mapper,
        UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            this.mapper = mapper;
            this.config = config;
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
        var userToCreate = this.mapper.Map<User>(userForRegisterDto);
        var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);

        if (result.Succeeded)
        {
            // we don't return userToCreate because we don't want the password to be revealed
            var userToReturn = this.mapper.Map<UserForDetailedDto>(userToCreate);

            // similar to php, route is redirected to xx/api/users/id, the GetUser method
            // in UsersController will be called, so the return value actually are 
            // "201 Created with the Location Header", and entity(Dto).
            return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userToReturn);
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
    {
        var user = await _userManager.FindByNameAsync(userForLoginDto.Username);

        // using false expresses we won't lock user due to his login failure
        var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);

        if (result.Succeeded)
        {
            var appUser = mapper.Map<UserForListDto>(user);

            return Ok(new
            {   //return value Task has lots of metadata we have no interest in, we only need Task.Result
                token = GenerateJwtToken(user).Result,
                user = appUser
            });
        }

        return Unauthorized();//401
    }

    private async Task<string> GenerateJwtToken(User user)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var roles = await _userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

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

        return tokenHandler.WriteToken(token);
    }
}
}