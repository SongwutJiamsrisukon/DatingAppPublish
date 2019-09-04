using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    //depricated code(don't using anymore)
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext _context)//injection DB
        {
            this._context = _context;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.UserName == username);

            if(user == null)return null;
            //if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))return null;

            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)){
                var getPasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));//convert string to byte[] and the compute hash

                if(getPasswordHash.Length!=passwordHash.Length) return false;
                for(int i = 0; i < getPasswordHash.Length;i++){
                    if(getPasswordHash[i]!=passwordHash[i]) return false;
                }
                return true;
            }
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] setPasswordHash, passwordSalt;
            CreatePasswordHash(password, out setPasswordHash, out passwordSalt);

            //user.PasswordHash = setPasswordHash;
            //user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] setPasswordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512()){//after end {} it will use Dispose() method to release all Cryptography resource
                passwordSalt = hmac.Key;
                setPasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));//convert string to byte[] and the compute hash
            }
        }

        public async Task<bool> UserExists(string username)
        {
            if(await _context.Users.AnyAsync(u => u.UserName == username))
                return true;

            return false;
        }
    }
}