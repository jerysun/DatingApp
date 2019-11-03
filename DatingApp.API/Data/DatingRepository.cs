using System;
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
            var photo = await Context.Photos.IgnoreQueryFilters()
                .FirstOrDefaultAsync( p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id, bool isCurrentUser)
        {
            var query = Context.Users.AsQueryable();
            if (isCurrentUser)
                query = query.IgnoreQueryFilters();

            var user = await query.FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var source = Context.Users.OrderByDescending(u => u.LastActive).AsQueryable();

            source = source.Where(u => u.Id != userParams.UserId && u.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                source = source.Where(u => userLikers.Contains(u.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                source = source.Where(u => userLikees.Contains(u.Id));
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                source = source.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch(userParams.OrderBy)
                {
                    case "created":
                        source = source.OrderByDescending(u => u.Created);
                        break;
                    default:
                        source = source.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

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

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await this.Context.Users
                //.Include(x => x.Likers) // populate the collection of Likers in User class
                //.Include(x => x.Likees) // populate the collection of Likees in User class
                .FirstOrDefaultAsync(u => u.Id == id);//Comment 2 Include due to using LazyLoading
            
            if (likers)
            {   // returns a list of integers - i.e, IDs
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await Context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await Context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = Context.Messages
                //.Include(m => m.Sender).ThenInclude(u => u.Photos) //"embedded" relationship
                //.Include(m => m.Recipient).ThenInclude(u => u.Photos)
                .AsQueryable();//Comment 2 Include due to using LazyLoading
            
            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(m => m.RecipientId == messageParams.UserId
                        && !m.RecipientDeleted);
                    break;
                case "Outbox":
                    messages = messages.Where(m => m.SenderId == messageParams.UserId
                        && !m.SenderDeleted);
                    break;
                default: // "Unread" message
                    messages = messages.Where(m => m.RecipientId == messageParams.UserId 
                        && !m.RecipientDeleted && !m.IsRead);
                    break;
            }

            messages = messages.OrderByDescending(m => m.MessageSent);

            var pagedMessages = await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);

            return pagedMessages;
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = Context.Messages.AsQueryable();
            
            //conversation between two guys
            messages = messages.Where(m => m.RecipientId == userId && !m.RecipientDeleted
                && m.SenderId == recipientId /*interlaced*/
                || m.RecipientId == recipientId
                && m.SenderId == userId && !m.SenderDeleted /*separate*/);
            return await messages.OrderByDescending(m => m.MessageSent).ToListAsync();
        }
    }
}
