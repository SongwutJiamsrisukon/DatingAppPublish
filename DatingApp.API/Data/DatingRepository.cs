using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using static DatingApp.API.Helpers.EnumWarehouse;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
             _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            //var like = await _context.Likes.Where(l => l.LikerId == userId).Where(l => l.LikeeId == recipientId).FirstOrDefaultAsync();
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.LikerId == userId && l.LikeeId == recipientId);
            return like;
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(p => p.UserId == userId).FirstOrDefaultAsync(p => p.IsMain == true);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id, bool isCurrentUser)
        {
            var queryUser = _context.Users.Include(u => u.Photos).AsQueryable();
            if (isCurrentUser) {
                queryUser = queryUser.IgnoreQueryFilters();
            }
            return await queryUser.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.AsQueryable();
            users = users.Where(u => u.Id != userParams.UserId);

            switch (userParams.TypeOfLike) {
                case TypeOfLike.None: users = users.Where(u => u.Gender == userParams.Gender);
                                        var minDob = DateTime.Today.AddYears(-userParams.MaxAge-1);
                                        var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                                        users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
                                        break;
                case TypeOfLike.Likers: var userLikers = await GetUserLikes(userParams.UserId, userParams.TypeOfLike);
                                        users = users.Where(u => userLikers.Contains(u.Id));
                                        break;
                case TypeOfLike.Likees: var userLikees = await GetUserLikes(userParams.UserId, userParams.TypeOfLike);
                                        users = users.Where(u => userLikees.Contains(u.Id));
                                        break;
            }

            var count = await users.CountAsync();

            users = users.Include(u => u.Photos);
            if(!string.IsNullOrEmpty(userParams.OrderBy)) {
                switch (userParams.OrderBy){
                    case "created": users = users.OrderByDescending(u => u.Created);break;
                    default: users = users.OrderByDescending(u => u.LastActive);break;
                }
            } else {
                users = users.OrderByDescending(u => u.LastActive); 
            }

            
            return await PagedList<User>.CreateAsync(count ,users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, TypeOfLike typeOfLike){
            var user = await _context.Users.Include(u => u.Likers).Include(u => u.Likees).FirstOrDefaultAsync(u => u.Id == id);

            if (typeOfLike == TypeOfLike.Likers){
                return user.Likers.Where(l => l.LikeeId == id).Select(l => l.LikerId); //select is return only list of likerId
            } else if (typeOfLike == TypeOfLike.Likees) {
                return user.Likees.Where(l => l.LikerId == id).Select(l => l.LikeeId); //select is return only list of likeeId
            }
            return Enumerable.Empty<int>();
            
        }
        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0; //if > 0 return true
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages.AsQueryable();

            switch (messageParams.MessageContainer) {
                case "Inbox": messages = messages.Where(m => m.RecipientId == messageParams.UserId && m.RecipientDeleted == false);
                break;
                case "Outbox": messages = messages.Where(m => m.SenderId == messageParams.UserId && m.SenderDeleted == false);
                break;
                default: messages = messages.Where(m => m.RecipientId == messageParams.UserId && m.IsRead == false && m.RecipientDeleted == false);
                break; // Unread
            }
            var count = await messages.CountAsync();

            messages = messages.Include(m => m.Sender).ThenInclude(u => u.Photos);
            messages = messages.Include(m => m.Recipient).ThenInclude(u => u.Photos);
            messages = messages.OrderByDescending(m => m.SentTime);

            return await PagedList<Message>.CreateAsync(count, messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessagesThread(int userId, int recipientId)
        {
            var messages = _context.Messages.AsQueryable();
            messages = messages.Include(m => m.Sender).ThenInclude(u => u.Photos);
            messages = messages.Include(m => m.Recipient).ThenInclude(u => u.Photos);

            messages = messages.Where(m => (m.RecipientId == userId && m.SenderId == recipientId && m.RecipientDeleted == false)
            || (m.RecipientId == recipientId && m.SenderId == userId && m.SenderDeleted == false)); // get all conversation between 2 users

            messages = messages.OrderByDescending(m => m.SentTime);

            return await messages.ToListAsync();
            
        }
    }
}