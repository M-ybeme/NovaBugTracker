using NovaBugTracker.Data;
using NovaBugTracker.Models.Enums;
using NovaBugTracker.Models;
using NovaBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace NovaBugTracker.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public BTRolesService(UserManager<BTUser> userManager,
                        RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task AddUserToRoleAsync(BTUser user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public Task<List<IdentityRole>> GetBTRolesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleNameByIdAsync(string roleId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserRoleAsync(BTUser user)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {

            try
            {
                List<BTUser> btUsers = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
                List<BTUser> results = btUsers.Where(b => b.CompanyId == companyId).ToList();

                return results;
            }
            catch (Exception)
            {

                throw;
            }
            //try
            //{
            //    return (await _userManager.GetUsersInRoleAsync(roleName)).Where(u => u.CompanyId == companyId).ToList();
            //}
            //catch { throw; }
        }

        public async Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
        {
            List<BTUser> usersInRole = await GetUsersInRoleAsync(roleName, companyId);
            return await _userManager.Users.Where(u => !usersInRole.Contains(u) && u.CompanyId == companyId).ToListAsync();
        }

        public Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles)
        {
            throw new NotImplementedException();
        }
    }
}
