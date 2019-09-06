using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<CloudinarySettings> _cloundinaryConfig;
        private Cloudinary _cloundinary;
        
        public AdminController(DataContext context, UserManager<User> userManager, IOptions<CloudinarySettings> cloundinaryConfig)
        {
            _context = context;
            _userManager = userManager;
            _cloundinaryConfig = cloundinaryConfig;

            Account acc = new Account(
                _cloundinaryConfig.Value.CloudName,
                _cloundinaryConfig.Value.ApiKey,
                _cloundinaryConfig.Value.ApiSecret
            );

            _cloundinary = new Cloudinary(acc);
        }

        [Authorize(Policy = "AdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var userList = await _context.Users.OrderBy(u => u.Id).Select(u => new
            {
                Id = u.Id,
                UserName = u.UserName,
                Roles = (from userRole in u.UserRoles
                         join role in _context.Roles
                         on userRole.RoleId equals role.Id
                         select role.Name).ToList()
            }).ToListAsync();
            return Ok(userList);
        }

        [Authorize(Policy = "AdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);

            var selectRoles = roleEditDto.RoleNames;

            //selectRoles = selectRoles != null ? selectRoles : new string[] {};
            selectRoles = selectRoles ?? new string[] { }; // null coalescing operator

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectRoles.Except(userRoles));

            if (!result.Succeeded)
            {
                return BadRequest("Failed to add to roles");
            }

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectRoles));
            if (!result.Succeeded)
            {
                return BadRequest("Failed to remove this roles");
            }

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratorRole")]
        [HttpGet("photosForModeration")]
        public async Task<IActionResult> GetPhotosForModeration()
        {
            var photos = await _context.Photos.Include(u => u.User)
            .IgnoreQueryFilters().Where(p => p.IsApproved == false) //get all photo and show only not approved
            .Select(u => new
            {
                Id = u.Id,
                UserName = u.User.UserName,
                Url = u.Url,
                IsApproved = u.IsApproved
            }).ToListAsync();
            return Ok(photos);
        }

        [Authorize(Policy = "ModeratorRole")]
        [HttpPost("approvePhoto/{photoId}")]
        public async Task<IActionResult> ApprovePhoto(int photoId){
            var photo = await _context.Photos.IgnoreQueryFilters().SingleOrDefaultAsync(p => p.Id == photoId);
            photo.IsApproved = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [Authorize(Policy = "ModeratorRole")]
        [HttpPost("rejectPhoto/{photoId}")]
        public async Task<IActionResult> RejectPhoto(int photoId){
            var photo = await _context.Photos.IgnoreQueryFilters().SingleOrDefaultAsync(p => p.Id == photoId);
            if (photo.IsMain)
                return BadRequest("You cannot reject the main photo");

            if (photo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photo.PublicId);
                var result = _cloundinary.Destroy(deleteParams);

                if(result.Result == "ok"){
                    _context.Photos.Remove(photo);
                }
            }
            if (photo.PublicId == null)
            {
                _context.Photos.Remove(photo);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}