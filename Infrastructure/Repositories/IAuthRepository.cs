using System.Xml.Serialization;
using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Infrastructure.Repositories
{
    public interface IAuthRepository
    {
        User? GetUserByUsername(string username);
        User? GetUserById(int id);
        User? GetUserByEmail(string email);
        User AddUser(User user);
        void DeleteUser(User user);

        Permission? GetPermissionById(int id);
        Permission? GetPermissionByCode(string code);
        ICollection<Permission> GetAllPermissions();
        Permission AddPermission(Permission permission);
        Permission UpdatePermission(Permission permission);
        void DeletePermission(Permission permission);

        UserPermission? GetUserPermission(int userId, int permissionId);
        UserPermission AddUserPermission(UserPermission userPermission);
        void RemoveUserPermission(UserPermission userPermission);
        ICollection<Permission> GetPermissionsForUser(int userId);
    }
}