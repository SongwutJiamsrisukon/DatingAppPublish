using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Models;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        public static void SeedUsers(DataContext context){
            if(!context.Users.Any()){
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");//open to read text file and closed after read
                var users = JsonConvert.DeserializeObject<List<User>>(userData);//Deserialize Json to Object and then convert type to List<User> users

                foreach (var user in users)
                {
                    byte[] setPasswordHash, passwordSalt;
                    CreatePasswordHash("password", out setPasswordHash, out passwordSalt);

                    user.PasswordHash = setPasswordHash;
                    user.PasswordSalt = passwordSalt;
                    user.Username = user.Username.ToLower();
                    context.Users.Add(user);
                }
                context.SaveChanges();
            }
           
        }

        private static void CreatePasswordHash(string password, out byte[] setPasswordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512()){//after end {} it will use Dispose() method to release all Cryptography resource
                passwordSalt = hmac.Key;
                setPasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));//convert string to byte[] and the compute hash
            }
        }
    }
}