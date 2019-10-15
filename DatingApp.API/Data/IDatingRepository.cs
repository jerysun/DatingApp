using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
         void Add<T>(T entity) where T: class;
         void Delete<T>(T entity) where T: class;
         Task<bool> SaveAll();
         Task<PagedList<User>> GetUsers(UserParams userParams);
         Task<User> GetUser(int id);

         Task<Photo> GetPhoto(int id); // id of Photo
         Task<Photo> GetMainPhotoForUser(int userId);

         //userId likes recipientId, the latter is passive, the 2 Ids are of UserId
         //scenario: In the likes list of an user of userId, he can click another
         //          member with the recipientID who is liked by him, then he will 
         //          be redirected to that member's homepage.
         Task<Like> GetLike(int userId, int recipientId);
         Task<Message> GetMessage(int id);//id is message id
         Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams);
         Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId);
    }
}