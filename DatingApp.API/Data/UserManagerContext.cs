
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Data
{
    public class UserManagerInclude : UserManager<User>
    {
        private readonly DataContext _context;
        public UserManagerInclude(DataContext context, IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _context = context;
        }

        public override async Task<User> FindByNameAsync(string username)
        {
            return await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(x => x.UserName.ToLower() == username.ToLower());
        }
    }
}