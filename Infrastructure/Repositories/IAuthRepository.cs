using System.Xml.Serialization;
using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Infrastructure.Repositories
{
    public interface IAuthRepository
    {
        User? GetUserByUsername(string username);
        User? GetUserById(int id);
        User? GetUserByEmail(string email);
        User? GetUserWithRoleByUsername(string username);
        User AddUser(User user);
        void DeleteUser(User user);

        Permission? GetPermissionById(int id);
        Permission? GetPermissionByCode(string code);
        ICollection<Permission> GetAllPermissions();
        Permission AddPermission(Permission permission);
        Permission UpdatePermission(Permission permission);
        void DeletePermission(Permission permission);

        Role? GetRoleByName(string roleName);

        bool HasPermission(int userId, string permissionCode);
    }
}