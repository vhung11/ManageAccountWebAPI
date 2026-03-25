using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Infrastructure.Repositories
{
    public interface IAuthRepository
    {
        // User
        User? GetUserByUsername(string username);
        User? GetUserById(int id);
        User? GetUserByEmail(string email);
        User? GetUserWithRole(int id);
        User? GetUserWithRoleByUsername(string username);
        User AddUser(User user);
        User UpdateUser(User user);
        void DeleteUser(User user);

        // Permission
        Permission? GetPermissionById(int id);
        Permission? GetPermissionByCode(string code);
        ICollection<Permission> GetAllPermissions();
        Permission AddPermission(Permission permission);
        Permission UpdatePermission(Permission permission);
        void DeletePermission(Permission permission);

        // Role
        Role? GetRoleById(int id);
        Role? GetRoleByName(string name);
        ICollection<Role> GetAllRoles();
        Role AddRole(Role role);
        Role UpdateRole(Role role);
        void DeleteRole(Role role);

        // RolePermission
        RolePermission? GetRolePermission(int roleId, int permissionId);
        RolePermission AddRolePermission(RolePermission rolePermission);
        void RemoveRolePermission(RolePermission rolePermission);
        ICollection<Permission> GetPermissionsForRole(int roleId);
    }
}