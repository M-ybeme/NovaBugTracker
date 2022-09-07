using Microsoft.AspNetCore.Identity;
using NovaBugTracker.Models;

namespace NovaBugTracker.Services.Interfaces
{
    public interface IBTRolesService
    {
        //New
        public Task AddUserToRoleAsync(BTUser user, string roleName);
        //New
        public Task<List<IdentityRole>> GetBTRolesAsync();
        public Task<bool> IsUserInRoleAsync(BTUser member, string roleName);
        //New
        public Task<string> GetRoleNameByIdAsync(string roleId);
        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);
        //New
        public Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId);
        //New
        public Task<string> GetUserRoleAsync(BTUser user);
        //New
        public Task<IEnumerable<string>> GetUserRolesAsync(BTUser user);
        //New
        public Task RemoveUserFromRoleAsync(BTUser user, string roleName);
        //New
        public Task RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles);
    }
}
