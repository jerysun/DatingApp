using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository repo;
        private readonly IMapper mapper;
        private readonly IOptions<CloudinarySettings> cloudinaryConfig;
        public Cloudinary cloudinary;


        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            this.cloudinaryConfig = cloudinaryConfig;
            this.mapper = mapper;
            this.repo = repo;

            Account acc = new Account(
                this.cloudinaryConfig.Value.CloudName,
                this.cloudinaryConfig.Value.ApiKey,
                this.cloudinaryConfig.Value.ApiSecret
            );

            this.cloudinary = new Cloudinary(acc);
        }

        [HttpPost]
        // The param userId comes from the route parameters
        public async Task<IActionResult> AddPhotoForUser(int userId,
            PhotoForCreationDto photoForCreationDto)
        {
            //check if the passed userId = the one from the decodedToken, and if the guy posting
            //the upload is the current user(associated with the executing action). PUT and POST
            //are extremely important for the security of our running app.
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await this.repo.GetUser(userId);

            var file = photoForCreationDto.File;
            var uploadResult = new ImageUploadResult();//store the returned value from Cloudinary

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()
                            .Width(500).Height(500).Crop("fill").Gravity("face") // this is optional, in our case we need it
                    };

                    uploadResult = this.cloudinary.Upload(uploadParams);
                }

                photoForCreationDto.Url = uploadResult.Uri.ToString();
                photoForCreationDto.PublicId = uploadResult.PublicId;

                var photo = this.mapper.Map<Photo>(photoForCreationDto);

                if (!userFromRepo.Photos.Any(u => u.IsMain))
                    photo.IsMain = true;

                userFromRepo.Photos.Add(photo);
                
                if (await this.repo.SaveAll()) {
                    return Ok();
                }
            }

            return BadRequest("Could not add  the photo!");
        }
    }
}