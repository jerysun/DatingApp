using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        public DataContext Context { get; set; }
        public DatingRepository(DataContext context)
        {
            this.Context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            Context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            Context.Remove(entity);
        }

        public async Task<Photo> GetPhoto(int id) // id of Photo
        {
            var photo = await Context.Photos.FirstOrDefaultAsync( p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            // Include is just 'join' in SQL. Every time we return a user, 
            // it must include the associated photos
            var user = await Context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var source = Context.Users.Include(p => p.Photos);
            var users = await PagedList<User>.CreateAsync(source, userParams.PageNumber, userParams.PageSize);
            
            return users;
        }

        public async Task<bool> SaveAll()
        {
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            //return await Context.Photos.FirstOrDefaultAsync(p => p.UserId == userId && p.IsMain);
            return await Context.Photos.Where(p => p.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }
    }
}
