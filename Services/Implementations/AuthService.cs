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

        public AuthResponse Login(LoginRequest request)
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            var user = _authRepository.GetUserByUsername(request.Username);
            if (user == null || user.PasswordHash != request.Password)
            {
                _logger.LogWarning("Login failed: User not found for username {Username}", request.Username);
                throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            var token = _tokenService.GenerateToken(user);
            _logger.LogInformation("Login successful for user {Username}", request.Username);

            return new AuthResponse { Token = token, Username = user.Username };
        }

        public void Register(RegisterRequest request)
        {
            _logger.LogInformation("Register attempt for user: {Username}", request.Username);

            var userByUsername = _authRepository.GetUserByUsername(request.Username);
            if (userByUsername != null)
            {
                _logger.LogWarning("Register failed: User already exists for username {Username}", request.Username);
                throw new UnauthorizedAccessException("Tên đăng nhập đã tồn tại.");
            }

            var userByEmail = _authRepository.GetUserByEmail(request.Email);
            if (userByEmail != null)
            {
                _logger.LogWarning("Register failed: User already exists for email {Email}", request.Email);
                throw new UnauthorizedAccessException("Email đã tồn tại.");
            }

            _authRepository.AddUser(new User
            {
                Username = request.Username,
                PasswordHash = request.Password,
                Email = request.Email
            });
            _logger.LogInformation("Register successful for user {Username}", request.Username);
        }

        public Permission CreatePermission(PermissionRequest request)
        {
            _logger.LogInformation("Create permission attempt for user: {Username}", request.Code);
            var permission = _authRepository.AddPermission(new Permission
            {
                Code = request.Code,
                Resource = request.Resource,
                Action = request.Action
            });
            _logger.LogInformation("Create permission successful for user {Username}", permission.Code);
            return permission;
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            _logger.LogInformation("Get all permissions attempt");
            var permissions = _authRepository.GetAllPermissions();
            _logger.LogInformation("Get all permissions successful");
            return permissions;
        }

        public Permission? GetPermissionById(int id)
        {
            _logger.LogInformation("Get permission by id attempt for user: {Id}", id);
            var permission = _authRepository.GetPermissionById(id);
            _logger.LogInformation("Get permission by id successful for user: {Id}", id);
            return permission;
        }

        public Permission? UpdatePermission(PermissionRequest request)
        {
            _logger.LogInformation("Update permission attempt for user: {Code}", request.Code);
            var permission = _authRepository.UpdatePermission(new Permission
            {
                Code = request.Code,
                Resource = request.Resource,
                Action = request.Action
            });
            _logger.LogInformation("Update permission successful for user: {Code}", request.Code);
            return permission;
        }

        public void DeletePermission(int id)
        {
            _logger.LogInformation("Delete permission attempt for user: {Id}", id);
            var permission = _authRepository.GetPermissionById(id);
            if (permission == null)
            {
                _logger.LogWarning("Delete permission failed: Permission not found for id {Id}", id);
                throw new UnauthorizedAccessException("Không tìm thấy quyền.");
            }
            _authRepository.DeletePermission(permission);
            _logger.LogInformation("Delete permission successful for user: {Id}", id);
        }

        public void AssignPermissionToUser(int userId, int permissionId)
        {
            _logger.LogInformation("Assign permission attempt for user: {UserId}", userId);
            var user = _authRepository.GetUserById(userId);
            if (user == null)
            {
                _logger.LogWarning("Assign permission failed: User not found for id {UserId}", userId);
                throw new UnauthorizedAccessException("Không tìm thấy người dùng.");
            }
            var permission = _authRepository.GetPermissionById(permissionId);
            if (permission == null)
            {
                _logger.LogWarning("Assign permission failed: Permission not found for id {PermissionId}", permissionId);
                throw new UnauthorizedAccessException("Không tìm thấy quyền.");
            }
            if (_authRepository.GetUserPermission(userId, permissionId) != null)
            {
                _logger.LogWarning("Assign permission failed: User permission already exists for id {UserId} and {PermissionId}", userId, permissionId);
                throw new UnauthorizedAccessException("Người dùng đã có quyền này.");
            }
            _authRepository.AddUserPermission(new UserPermission
            {
                UserId = userId,
                PermissionId = permissionId
            });
            _logger.LogInformation("Assign permission successful for user: {UserId}", userId);
        }

        public void RemovePermissionFromUser(int userId, int permissionId)
        {
            _logger.LogInformation("Remove permission attempt for user: {UserId}", userId);
            var user = _authRepository.GetUserById(userId);
            if (user == null)
            {
                _logger.LogWarning("Remove permission failed: User not found for id {UserId}", userId);
                throw new UnauthorizedAccessException("Không tìm thấy người dùng.");
            }
            var permission = _authRepository.GetPermissionById(permissionId);
            if (permission == null)
            {
                _logger.LogWarning("Remove permission failed: Permission not found for id {PermissionId}", permissionId);
                throw new UnauthorizedAccessException("Không tìm thấy quyền.");
            }
            if (_authRepository.GetUserPermission(userId, permissionId) == null)
            {
                _logger.LogWarning("Remove permission failed: User permission not found for id {UserId} and {PermissionId}", userId, permissionId);
                throw new UnauthorizedAccessException("Không tìm thấy quyền của người dùng.");
            }
            _authRepository.RemoveUserPermission(new UserPermission
            {
                UserId = userId,
                PermissionId = permissionId
            });
            _logger.LogInformation("Remove permission successful for user: {UserId}", userId);
        }

        public ICollection<Permission> GetPermissionsForUser(int userId)
        {
            _logger.LogInformation("Get permissions for user attempt for user: {UserId}", userId);
            var permissions = _authRepository.GetPermissionsForUser(userId);
            _logger.LogInformation("Get permissions for user successful for user: {UserId}", userId);
            return permissions;
        }

        public bool UserHasPermission(int userId, string permissionCode)
        {
            _logger.LogInformation("Check permission attempt for user: {UserId} and permission: {PermissionCode}", userId, permissionCode);
            var user = _authRepository.GetUserById(userId);
            if (user == null)
            {
                _logger.LogWarning("Check permission failed: User not found for id {UserId}", userId);
                throw new UnauthorizedAccessException("Không tìm thấy người dùng.");
            }
            var permission = _authRepository.GetPermissionByCode(permissionCode);
            if (permission == null)
            {
                _logger.LogWarning("Check permission failed: Permission not found for code {PermissionCode}", permissionCode);
                return false;
            }
            var permissions = _authRepository.GetPermissionsForUser(userId);
            _logger.LogInformation("Check permission successful for user: {UserId} and permission: {PermissionCode}", userId, permissionCode);
            return permissions.Any(p => p.Code == permissionCode);
        }

        public bool UserHasPermission(int userId, string resource, string action)
        {
            _logger.LogInformation("Check permission attempt for user: {UserId} and resource: {Resource} and action: {Action}", userId, resource, action);
            var user = _authRepository.GetUserById(userId);
            if (user == null)
            {
                _logger.LogWarning("Check permission failed: User not found for id {UserId}", userId);
                throw new UnauthorizedAccessException("Không tìm thấy người dùng.");
            }
            var permission = _authRepository.GetPermissionByCode(resource);
            if (permission == null)
            {
                _logger.LogWarning("Check permission failed: Permission not found for resource {Resource} and action {Action}", resource, action);
                return false;
            }
            var permissions = _authRepository.GetPermissionsForUser(userId);
            _logger.LogInformation("Check permission successful for user: {UserId} and resource: {Resource} and action: {Action}", userId, resource, action);
            return permissions.Any(p => p.Resource == resource && p.Action == action);
        }
    }
}