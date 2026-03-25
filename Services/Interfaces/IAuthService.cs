using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Services.Interfaces
{
    public interface IAuthService
    {
        AuthResponse Login(LoginRequest request);
        void Register(RegisterRequest request);

        Permission CreatePermission(PermissionRequest request);
        IEnumerable<Permission> GetAllPermissions();
        Permission? GetPermissionById(int id);
        Permission? UpdatePermission(int id, PermissionRequest request);
        void DeletePermission(int id);

        Role CreateRole(RoleRequest request);
        IEnumerable<Role> GetAllRoles();
        Role? GetRoleById(int id);
        Role? UpdateRole(int id, RoleRequest request);
        void DeleteRole(int id);

        void AssignRoleToUser(AssignRoleToUserRequest request);
        void RemoveRoleFromUser(AssignRoleToUserRequest request);
        void AssignPermissionToRole(AssignPermissionToRoleRequest request);
        void RemovePermissionFromRole(AssignPermissionToRoleRequest request);
        void AssignPermissionToUser(AssignPermissionToUserRequest request);
        void RemovePermissionFromUser(AssignPermissionToUserRequest request);

        bool UserHasPermission(int userId, string permissionCode);
    }
}