namespace ManageAccountWebAPI.Data.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }

    public class PermissionRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }

    public class RoleRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class AssignRoleToUserRequest
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }

    public class AssignPermissionToRoleRequest
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
    }

    public class AssignPermissionToUserRequest
    {
        public int UserId { get; set; }
        public int PermissionId { get; set; }
    }
}
