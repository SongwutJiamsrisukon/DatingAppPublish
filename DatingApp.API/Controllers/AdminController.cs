using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        public AdminController(DataContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            selectRoles = selectRoles ?? new string[] {}; // null coalescing operator

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectRoles.Except(userRoles));

            if (!result.Succeeded){
                return BadRequest("Failed to add to roles");
            }

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectRoles));
            if (!result.Succeeded){
                return BadRequest("Failed to remove this roles");
            }

            return Ok(await _userManager.GetRolesAsync(user));
        }
    }
}