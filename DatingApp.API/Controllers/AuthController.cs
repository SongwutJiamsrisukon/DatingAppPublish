using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")] //[controller] is replace by auth like http://localhost:5000/api/auth
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager; //userManager used to find User in DB
        
        private readonly SignInManager<User> _signInManager; //signInManager to check user password
        public AuthController(IConfiguration config, IMapper mapper, 
        UserManager<User> userManager, SignInManager<User> signInManager)//inject repo and configuration
        {
            _mapper = mapper;
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {//Register([FromBody]UserForRegisterDto userForRegisterDto)
         //[FromBody] generate "" to "" if don't had it generate "" to null
         //no need [FromBody] if you declare [ApiController] on this class

            /* if(!ModelState.IsValid)  //no need this if you declare [ApiController] on this class
                return BadRequest(ModelState);*/

            var userToCreate = _mapper.Map<User>(userForRegisterDto);
            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);
            var resultAddRole = await _userManager.AddToRoleAsync(userToCreate, "Member");

            var userToReturn = _mapper.Map<UserForDetailedDto>(userToCreate);

            if (result.Succeeded && resultAddRole.Succeeded)
            {
                return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userToReturn);
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var user = await _userManager.FindByNameAsync(userForLoginDto.Username);
            try{
                var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);
                    if (result.Succeeded)
                    {
                        var localUserData = _mapper.Map<LocalUserData>(user);
                        return Ok(new
                        {
                            token = GenerateJWTToken(user).Result,
                            localUserData
                        });//return token as object type
                    }
            } catch (Exception) {
                return Unauthorized();
            }

            return Unauthorized();
        }

        private async Task<string> GenerateJWTToken(User user)
        {
            var claims = new List<Claim>{
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), //convert int Id to String because ClaimTypes is string
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role)); //get token cotain a role for particular user
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}