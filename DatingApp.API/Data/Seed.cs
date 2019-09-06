using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        public static void SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager){
            if(!userManager.Users.Any()){
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");//open to read text file and closed after read
                var users = JsonConvert.DeserializeObject<List<User>>(userData);//Deserialize Json to Object and then convert type to List<User> users

                var roles = new List<Role>{
                    new Role{Name = "Member"},
                    new Role{Name = "Admin"},
                    new Role{Name = "Moderator"},
                    new Role{Name = "VIP"},
                };

                foreach (var role in roles)
                {
                    roleManager.CreateAsync(role).Wait();
                }
               
                var adminUser = new User{
                    UserName = "admin"
                };

                var result = userManager.CreateAsync(adminUser, "$3ReichS").Result;

                if(result.Succeeded)
                {
                    var admin = userManager.FindByNameAsync("admin").Result;
                    userManager.AddToRolesAsync(admin, new[] {"Admin","Moderator"});
                }
                
                foreach (var user in users)
                {
                    user.Photos.SingleOrDefault().IsApproved = true;
                    userManager.CreateAsync(user, "password").Wait();//this is not async method use Wait() instead
                    userManager.AddToRoleAsync(user, "Member").Wait();
                }
                //context.SaveChanges(); UserManager automatic SaveChanges
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