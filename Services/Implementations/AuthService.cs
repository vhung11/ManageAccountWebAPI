using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Services.Interfaces;

namespace ManageAccountWebAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IAuthRepository authRepository,
            ILogger<AuthService> logger,
            ITokenService tokenService)
        {
            _authRepository = authRepository;
            _logger = logger;
            _tokenService = tokenService;
        }

        // ── Auth ──────────────────────────────────────────────

        public AuthResponse Login(LoginRequest request)
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            var user = _authRepository.GetUserWithRoleByUsername(request.Username);
            if (user == null || user.PasswordHash != request.Password)
            {
                _logger.LogWarning("Login failed for username {Username}", request.Username);
                throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            var token = _tokenService.GenerateToken(user);
            _logger.LogInformation("Login successful for user {Username}", request.Username);

            return new AuthResponse { Token = token, Username = user.Username };
        }

        public void Register(RegisterRequest request)
        {
            _logger.LogInformation("Register attempt for user: {Username}", request.Username);

            if (_authRepository.GetUserByUsername(request.Username) != null)
            {
                _logger.LogWarning("Register failed: Username {Username} already exists", request.Username);
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");
            }

            if (_authRepository.GetUserByEmail(request.Email) != null)
            {
                _logger.LogWarning("Register failed: Email {Email} already exists", request.Email);
                throw new InvalidOperationException("Email đã tồn tại.");
            }

            // Assign default "User" role
            var defaultRole = _authRepository.GetRoleByName("User")
                ?? throw new InvalidOperationException("Default role 'User' not found. Run database seeder first.");

            _authRepository.AddUser(new User
            {
                Username = request.Username,
                PasswordHash = request.Password,
                Email = request.Email,
                RoleId = defaultRole.Id
            });

            _logger.LogInformation("Register successful for user {Username} with role 'User'", request.Username);
        }

        // ── Permission CRUD ──────────────────────────────────

        public Permission CreatePermission(PermissionRequest request)
        {
            _logger.LogInformation("Creating permission: {Code}", request.Code);
            var permission = _authRepository.AddPermission(new Permission
            {
                Code = request.Code,
                Resource = request.Resource,
                Action = request.Action
            });
            _logger.LogInformation("Created permission: {Code}", permission.Code);
            return permission;
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            return _authRepository.GetAllPermissions();
        }

        public Permission? GetPermissionById(int id)
        {
            return _authRepository.GetPermissionById(id);
        }

        public Permission? UpdatePermission(int id, PermissionRequest request)
        {
            _logger.LogInformation("Updating permission id={Id}", id);
            var existing = _authRepository.GetPermissionById(id);
            if (existing == null) return null;

            existing.Code = request.Code;
            existing.Resource = request.Resource;
            existing.Action = request.Action;
            return _authRepository.UpdatePermission(existing);
        }

        public void DeletePermission(int id)
        {
            var permission = _authRepository.GetPermissionById(id)
                ?? throw new KeyNotFoundException($"Permission id={id} not found.");
            _authRepository.DeletePermission(permission);
            _logger.LogInformation("Deleted permission id={Id}", id);
        }

        // ── Role CRUD ─────────────────────────────────────────

        public Role CreateRole(RoleRequest request)
        {
            _logger.LogInformation("Creating role: {Name}", request.Name);
            if (_authRepository.GetRoleByName(request.Name) != null)
                throw new InvalidOperationException($"Role '{request.Name}' already exists.");

            var role = _authRepository.AddRole(new Role
            {
                Name = request.Name,
                Description = request.Description
            });
            _logger.LogInformation("Created role: {Name}", role.Name);
            return role;
        }

        public IEnumerable<Role> GetAllRoles()
        {
            return _authRepository.GetAllRoles();
        }

        public Role? GetRoleById(int id)
        {
            return _authRepository.GetRoleById(id);
        }

        public Role? UpdateRole(int id, RoleRequest request)
        {
            _logger.LogInformation("Updating role id={Id}", id);
            var existing = _authRepository.GetRoleById(id);
            if (existing == null) return null;

            existing.Name = request.Name;
            existing.Description = request.Description;
            return _authRepository.UpdateRole(existing);
        }

        public void DeleteRole(int id)
        {
            var role = _authRepository.GetRoleById(id)
                ?? throw new KeyNotFoundException($"Role id={id} not found.");
            _authRepository.DeleteRole(role);
            _logger.LogInformation("Deleted role id={Id}", id);
        }

        // ── Role-Permission Assignment ─────────────────────────

        public void AssignPermissionToRole(int roleId, int permissionId)
        {
            _logger.LogInformation("Assigning permission {PermissionId} to role {RoleId}", permissionId, roleId);

            _ = _authRepository.GetRoleById(roleId)
                ?? throw new KeyNotFoundException($"Role id={roleId} not found.");
            _ = _authRepository.GetPermissionById(permissionId)
                ?? throw new KeyNotFoundException($"Permission id={permissionId} not found.");

            if (_authRepository.GetRolePermission(roleId, permissionId) != null)
                throw new InvalidOperationException("Role already has this permission.");

            _authRepository.AddRolePermission(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });
            _logger.LogInformation("Assigned permission {PermissionId} to role {RoleId}", permissionId, roleId);
        }

        public void RemovePermissionFromRole(int roleId, int permissionId)
        {
            _logger.LogInformation("Removing permission {PermissionId} from role {RoleId}", permissionId, roleId);

            var rp = _authRepository.GetRolePermission(roleId, permissionId)
                ?? throw new KeyNotFoundException("Role does not have this permission.");

            _authRepository.RemoveRolePermission(rp);
            _logger.LogInformation("Removed permission {PermissionId} from role {RoleId}", permissionId, roleId);
        }

        public ICollection<Permission> GetPermissionsForRole(int roleId)
        {
            _ = _authRepository.GetRoleById(roleId)
                ?? throw new KeyNotFoundException($"Role id={roleId} not found.");
            return _authRepository.GetPermissionsForRole(roleId);
        }

        // ── User-Role Assignment ──────────────────────────────

        public void AssignRoleToUser(int userId, int roleId)
        {
            _logger.LogInformation("Assigning role {RoleId} to user {UserId}", roleId, userId);

            var user = _authRepository.GetUserById(userId)
                ?? throw new KeyNotFoundException($"User id={userId} not found.");
            _ = _authRepository.GetRoleById(roleId)
                ?? throw new KeyNotFoundException($"Role id={roleId} not found.");

            user.RoleId = roleId;
            _authRepository.UpdateUser(user);
            _logger.LogInformation("Assigned role {RoleId} to user {UserId}", roleId, userId);
        }

        // ── Permission Checking (RBAC) ────────────────────────

        public bool UserHasPermission(int userId, string permissionCode)
        {
            _logger.LogInformation("Checking permission '{Code}' for user {UserId}", permissionCode, userId);

            var user = _authRepository.GetUserWithRole(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return false;
            }

            var hasPermission = user.Role.RolePermissions
                .Any(rp => rp.Permission.Code == permissionCode);

            _logger.LogInformation("User {UserId} {Result} permission '{Code}'",
                userId, hasPermission ? "has" : "lacks", permissionCode);
            return hasPermission;
        }

        public bool UserHasPermission(int userId, string resource, string action)
        {
            _logger.LogInformation("Checking permission {Resource}.{Action} for user {UserId}", resource, action, userId);

            var user = _authRepository.GetUserWithRole(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return false;
            }

            var hasPermission = user.Role.RolePermissions
                .Any(rp => rp.Permission.Resource == resource && rp.Permission.Action == action);

            _logger.LogInformation("User {UserId} {Result} permission {Resource}.{Action}",
                userId, hasPermission ? "has" : "lacks", resource, action);
            return hasPermission;
        }
    }
}