using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Services.Interfaces
{
    public interface IAuthService
    {
        // Auth
        AuthResponse Login(LoginRequest request);
        void Register(RegisterRequest request);

        // Permission CRUD
        Permission CreatePermission(PermissionRequest request);
        IEnumerable<Permission> GetAllPermissions();
        Permission? GetPermissionById(int id);
        Permission? UpdatePermission(int id, PermissionRequest request);
        void DeletePermission(int id);

        // Role CRUD
        Role CreateRole(RoleRequest request);
        IEnumerable<Role> GetAllRoles();
        Role? GetRoleById(int id);
        Role? UpdateRole(int id, RoleRequest request);
        void DeleteRole(int id);

        // Role-Permission assignment
        void AssignPermissionToRole(int roleId, int permissionId);
        void RemovePermissionFromRole(int roleId, int permissionId);
        ICollection<Permission> GetPermissionsForRole(int roleId);

        // User-Role assignment
        void AssignRoleToUser(int userId, int roleId);

        // Permission checking (RBAC)
        bool UserHasPermission(int userId, string permissionCode);
        bool UserHasPermission(int userId, string resource, string action);
    }
}