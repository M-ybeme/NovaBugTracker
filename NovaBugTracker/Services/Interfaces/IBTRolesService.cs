using Microsoft.AspNetCore.Identity;
using NovaBugTracker.Models;

namespace NovaBugTracker.Services.Interfaces
{
    public interface IBTRolesService
    {
        //New
        public Task<bool> AddUserToRoleAsync(BTUser user, string roleName);
        //New
        public Task<List<IdentityRole>> GetRolesAsync();
        public Task<bool> IsUserInRoleAsync(BTUser member, string roleName);
        //New
        public Task<string> GetRoleNameByIdAsync(string roleId);
        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);
        //New
        public Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId);
        //New
        public Task<IEnumerable<string>> GetUserRolesAsync(BTUser user);
        //New
        public Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName);
        //New
        public Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles);
    }
}
