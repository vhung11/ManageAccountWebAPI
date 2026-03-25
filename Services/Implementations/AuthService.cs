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

        public Role CreateRole(RoleRequest request)
        {
            _logger.LogInformation("Creating role: {RoleName}", request.Name);

            if (_authRepository.GetRoleByName(request.Name) != null)
            {
                throw new InvalidOperationException($"Role '{request.Name}' already exists.");
            }

            var role = _authRepository.AddRole(new Role { Name = request.Name });
            _logger.LogInformation("Created role: {RoleName}", role.Name);
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
            if (existing == null)
            {
                return null;
            }

            var duplicated = _authRepository.GetRoleByName(request.Name);
            if (duplicated != null && duplicated.Id != id)
            {
                throw new InvalidOperationException($"Role '{request.Name}' already exists.");
            }

            existing.Name = request.Name;
            return _authRepository.UpdateRole(existing);
        }

        public void DeleteRole(int id)
        {
            var role = _authRepository.GetRoleById(id)
                ?? throw new KeyNotFoundException($"Role id={id} not found.");
            _authRepository.DeleteRole(role);
            _logger.LogInformation("Deleted role id={Id}", id);
        }

        public void AssignRoleToUser(AssignRoleToUserRequest request)
        {
            _logger.LogInformation("Assigning role {RoleId} to user {UserId}", request.RoleId, request.UserId);

            var user = _authRepository.GetUserById(request.UserId)
                ?? throw new KeyNotFoundException($"User id={request.UserId} not found.");
            var role = _authRepository.GetRoleById(request.RoleId)
                ?? throw new KeyNotFoundException($"Role id={request.RoleId} not found.");

            if (_authRepository.GetUserRole(request.UserId, request.RoleId) != null)
            {
                throw new InvalidOperationException($"User id={request.UserId} already has role id={request.RoleId}.");
            }

            _authRepository.AddUserRole(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });
        }

        public void RemoveRoleFromUser(AssignRoleToUserRequest request)
        {
            _logger.LogInformation("Removing role {RoleId} from user {UserId}", request.RoleId, request.UserId);

            var userRole = _authRepository.GetUserRole(request.UserId, request.RoleId)
                ?? throw new KeyNotFoundException($"User id={request.UserId} does not have role id={request.RoleId}.");

            _authRepository.RemoveUserRole(userRole);
        }

        public void AssignPermissionToRole(AssignPermissionToRoleRequest request)
        {
            _logger.LogInformation("Assigning permission {PermissionId} to role {RoleId}", request.PermissionId, request.RoleId);

            var role = _authRepository.GetRoleById(request.RoleId)
                ?? throw new KeyNotFoundException($"Role id={request.RoleId} not found.");
            var permission = _authRepository.GetPermissionById(request.PermissionId)
                ?? throw new KeyNotFoundException($"Permission id={request.PermissionId} not found.");

            if (_authRepository.GetRolePermission(request.RoleId, request.PermissionId) != null)
            {
                throw new InvalidOperationException($"Role id={request.RoleId} already has permission id={request.PermissionId}.");
            }

            _authRepository.AddRolePermission(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            });
        }

        public void RemovePermissionFromRole(AssignPermissionToRoleRequest request)
        {
            _logger.LogInformation("Removing permission {PermissionId} from role {RoleId}", request.PermissionId, request.RoleId);

            var rolePermission = _authRepository.GetRolePermission(request.RoleId, request.PermissionId)
                ?? throw new KeyNotFoundException($"Role id={request.RoleId} does not have permission id={request.PermissionId}.");

            _authRepository.RemoveRolePermission(rolePermission);
        }

        public void AssignPermissionToUser(AssignPermissionToUserRequest request)
        {
            _logger.LogInformation("Assigning permission {PermissionId} to user {UserId}", request.PermissionId, request.UserId);

            var user = _authRepository.GetUserById(request.UserId)
                ?? throw new KeyNotFoundException($"User id={request.UserId} not found.");
            var permission = _authRepository.GetPermissionById(request.PermissionId)
                ?? throw new KeyNotFoundException($"Permission id={request.PermissionId} not found.");

            if (_authRepository.GetUserPermission(request.UserId, request.PermissionId) != null)
            {
                throw new InvalidOperationException($"User id={request.UserId} already has permission id={request.PermissionId}.");
            }

            _authRepository.AddUserPermission(new UserPermission
            {
                UserId = user.Id,
                PermissionId = permission.Id
            });
        }

        public void RemovePermissionFromUser(AssignPermissionToUserRequest request)
        {
            _logger.LogInformation("Removing permission {PermissionId} from user {UserId}", request.PermissionId, request.UserId);

            var userPermission = _authRepository.GetUserPermission(request.UserId, request.PermissionId)
                ?? throw new KeyNotFoundException($"User id={request.UserId} does not have permission id={request.PermissionId}.");

            _authRepository.RemoveUserPermission(userPermission);
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