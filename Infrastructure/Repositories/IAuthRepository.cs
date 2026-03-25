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

        Role? GetRoleByName(string roleName);
        ICollection<Role> GetAllRoles();
        Role? GetRoleById(int id);
        Role AddRole(Role role);
        Role UpdateRole(Role role);
        void DeleteRole(Role role);

        void AddUserRole(UserRole userRole);
        UserRole? GetUserRole(int userId, int roleId);
        void RemoveUserRole(UserRole userRole);

        void AddRolePermission(RolePermission rolePermission);
        RolePermission? GetRolePermission(int roleId, int permissionId);
        void RemoveRolePermission(RolePermission rolePermission);

        void AddUserPermission(UserPermission userPermission);
        UserPermission? GetUserPermission(int userId, int permissionId);
        void RemoveUserPermission(UserPermission userPermission);

        bool HasPermission(int userId, string permissionCode);
        User? GetUserWithRolesByUsername(string username);
    }
}