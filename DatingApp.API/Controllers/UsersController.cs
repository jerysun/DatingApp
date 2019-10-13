using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository repo;
        private readonly IMapper mapper;
        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            this.mapper = mapper;
            this.repo = repo;
        }

        [HttpGet]
        //public async Task<IActionResult> GetUsers([FromQuery(Name = "pageNumber")] int pageNumber, [FromQuery(Name = "pageSize")] int pageSize) // ASP.NET way
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams) // ASP.NET Core way
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await this.repo.GetUser(currentUserId);
            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender)) // kinda default
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await this.repo.GetUsers(userParams);
            var usersToReturn = this.mapper.Map<IEnumerable<UserForListDto>>(users);// <dst>(src), Dto is always dst

            // Add pagination to the HttpResponse Headers. Because we're in Controller
            // so we have access to Response, furhtermore we added an extension method
            // to HttpResponse - AddPagination() in DatingApp.API\Helpers\Extensions.cs
            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")] // query string parameter
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await this.repo.GetUser(id);
            var userToReturn = this.mapper.Map<UserForDetailedDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            //check if the passed id = the one from the decodedToken, and if the guy putting
            //the update is the current user(associated with the executing action). PUT and POST
            //are extremely important for the security of our running app.
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await this.repo.GetUser(id);
            this.mapper.Map(userForUpdateDto, userFromRepo);//PUT operation is different from GET operation above
            
            if (await this.repo.SaveAll())
                return NoContent();

            throw new Exception($"Updating user {id} failed on save.");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var like = await this.repo.GetLike(id, recipientId);
            if (like != null)
                return BadRequest("You already like this user.");
            
            if (await this.repo.GetUser(recipientId) == null)
                return NotFound();

            var user = await this.repo.GetUser(id);
            var recipient = await this.repo.GetUser(recipientId);

            like = new Like {
                LikerId = id,
                LikeeId = recipientId //complete here already, shouldn't add two user instances
            };

            this.repo.Add<Like>(like);
            if (await this.repo.SaveAll())
                return Ok();

            return BadRequest("Failed to like user.");
        }
    }
}