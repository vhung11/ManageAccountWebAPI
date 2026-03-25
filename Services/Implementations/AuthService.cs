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

            var user = _authRepository.GetUserWithRolesByUsername(request.Username);
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
                _logger.LogWarning("Register failed: Username{Username} already exists", request.Username);
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");
            }

            if (_authRepository.GetUserByEmail(request.Email) != null)
            {
                _logger.LogWarning("Register failed: Email {Email} already exists", request.Email);
                throw new InvalidOperationException("Email đã tồn tại.");
            }

            var defaultRole = _authRepository.GetRoleByName("User")
                ?? throw new InvalidOperationException("Default role 'User' not found. Run database seeder first.");

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = request.Password,
                Email = request.Email,
            };

            newUser.UserRoles.Add(new UserRole { RoleId = defaultRole.Id });
            _authRepository.AddUser(newUser);
            _logger.LogInformation("Register successful for user {Username} with role 'User'", request.Username);
        }

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

        public bool UserHasPermission(int userId, string permissionCode)
        {
            _logger.LogInformation("Checking permission '{Code}' for user {UserId}", permissionCode, userId);
            var user = _authRepository.GetUserById(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return false;
            }
            var hasPermission = _authRepository.HasPermission(userId, permissionCode);
            _logger.LogInformation("User {UserId} {Result} permission '{Code}'",
                userId, hasPermission ? "has" : "lacks", permissionCode);
            return hasPermission;
        }
    }
}