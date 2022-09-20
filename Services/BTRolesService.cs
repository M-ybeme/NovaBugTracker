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
        private readonly ApplicationDbContext _context;

        public BTRolesService(UserManager<BTUser> userManager,
                        RoleManager<IdentityRole> roleManager,
                        ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            try
            {
                bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<IdentityRole>> GetRolesAsync()
        {
            try
            {
                List<IdentityRole> result = new ();
                result = await _context.Roles.ToListAsync();
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<string> GetRoleNameByIdAsync(string roleId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserRoleAsync(BTUser user)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
        {
            try
            {
                IEnumerable<string> result = await _userManager.GetRolesAsync(user);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
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

        public async Task<bool> IsUserInRoleAsync(BTUser member, string roleName)
        {
            try
            {
                bool result = await _userManager.IsInRoleAsync(member, roleName);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            try
            {
                bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles)
        {
            try
            {
                bool result = (await _userManager.RemoveFromRolesAsync(user, roles)).Succeeded;
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
