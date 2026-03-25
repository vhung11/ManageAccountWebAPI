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

        bool UserHasPermission(int userId, string permissionCode);
    }
}