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
        Permission? UpdatePermission(PermissionRequest request);
        void DeletePermission(int id);

        void AssignPermissionToUser(int userId, int permissionId);
        void RemovePermissionFromUser(int userId, int permissionId);
        ICollection<Permission> GetPermissionsForUser(int userId);

        bool UserHasPermission(int userId, string permissionCode);
        bool UserHasPermission(int userId, string resource, string action);
    }
}