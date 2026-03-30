using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Controllers.Filters;
using ManageAccountWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ManageAccountWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = _authService.Login(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("register")]
        public ActionResult Register([FromBody] RegisterRequest request)
        {
            try
            {
                _authService.Register(request);
                return Ok("Đăng ký thành công.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("permissions")]
        [AuthorizeFunction("Permission.Create")]
        public ActionResult<Permission> CreatePermission([FromBody] PermissionRequest request)
        {
            try
            {
                var permission = _authService.CreatePermission(request);
                return Ok(permission);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("permissions")]
        [AuthorizeFunction("Permission.Read")]
        public ActionResult<IEnumerable<Permission>> GetAllPermissions()
        {
            try
            {
                var permissions = _authService.GetAllPermissions();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("permissions/{id:int}")]
        [AuthorizeFunction("Permission.Read")]
        public ActionResult<Permission> GetPermissionById(int id)
        {
            try
            {
                var permission = _authService.GetPermissionById(id);
                if (permission == null) return NotFound();
                return Ok(permission);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("permissions/{id:int}")]
        [AuthorizeFunction("Permission.Update")]
        public ActionResult<Permission> UpdatePermission(int id, [FromBody] PermissionRequest request)
        {
            try
            {
                var permission = _authService.UpdatePermission(id, request);
                if (permission == null) return NotFound();
                return Ok(permission);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("permissions/{id:int}")]
        [AuthorizeFunction("Permission.Delete")]
        public ActionResult DeletePermission(int id)
        {
            try
            {
                _authService.DeletePermission(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user-has-permission")]
        public IActionResult UserHasPermission(int userId, string permissionCode)
        {
            try
            {
                var result = _authService.UserHasPermission(userId, permissionCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("roles")]
        [AuthorizeFunction("Role.Create")]
        public ActionResult<Role> CreateRole([FromBody] RoleRequest request)
        {
            try
            {
                var role = _authService.CreateRole(request);
                return Ok(role);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("roles")]
        [AuthorizeFunction("Role.Read")]
        public ActionResult<IEnumerable<Role>> GetAllRoles()
        {
            try
            {
                var roles = _authService.GetAllRoles();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("roles/{id:int}")]
        [AuthorizeFunction("Role.Read")]
        public ActionResult<Role> GetRoleById(int id)
        {
            try
            {
                var role = _authService.GetRoleById(id);
                if (role == null) return NotFound();
                return Ok(role);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("roles/{id:int}")]
        [AuthorizeFunction("Role.Update")]
        public ActionResult<Role> UpdateRole(int id, [FromBody] RoleRequest request)
        {
            try
            {
                var role = _authService.UpdateRole(id, request);
                if (role == null) return NotFound();
                return Ok(role);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("roles/{id:int}")]
        [AuthorizeFunction("Role.Delete")]
        public ActionResult DeleteRole(int id)
        {
            try
            {
                _authService.DeleteRole(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("users/assign-role")]
        [AuthorizeFunction("Role.Assign")]
        public ActionResult AssignRoleToUser([FromBody] AssignRoleToUserRequest request)
        {
            try
            {
                _authService.AssignRoleToUser(request);
                return Ok("Gán vai trò cho người dùng thành công.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("users/remove-role")]
        [AuthorizeFunction("Role.Assign")]
        public ActionResult RemoveRoleFromUser([FromBody] AssignRoleToUserRequest request)
        {
            try
            {
                _authService.RemoveRoleFromUser(request);
                return Ok("Thu hồi vai trò của người dùng thành công.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("roles/assign-permission")]
        [AuthorizeFunction("Permission.Assign")]
        public ActionResult AssignPermissionToRole([FromBody] AssignPermissionToRoleRequest request)
        {
            try
            {
                _authService.AssignPermissionToRole(request);
                return Ok("Gán quyền cho vai trò thành công.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("roles/remove-permission")]
        [AuthorizeFunction("Permission.Assign")]
        public ActionResult RemovePermissionFromRole([FromBody] AssignPermissionToRoleRequest request)
        {
            try
            {
                _authService.RemovePermissionFromRole(request);
                return Ok("Thu hồi quyền khỏi vai trò thành công.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("users/assign-permission")]
        [AuthorizeFunction("Permission.Assign")]
        public ActionResult AssignPermissionToUser([FromBody] AssignPermissionToUserRequest request)
        {
            try
            {
                _authService.AssignPermissionToUser(request);
                return Ok("Gán quyền cho người dùng thành công.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("users/remove-permission")]
        [AuthorizeFunction("Permission.Assign")]
        public ActionResult RemovePermissionFromUser([FromBody] AssignPermissionToUserRequest request)
        {
            try
            {
                _authService.RemovePermissionFromUser(request);
                return Ok("Thu hồi quyền của người dùng thành công.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("users/me")]
        public ActionResult<UserDTO> GetCurrentUser()
        {
            try
            {
                var userId = GetUserId();
                var userDto = _authService.GetCurrentUser(userId);
                return Ok(userDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("users/me/roles")]
        public ActionResult<IEnumerable<RoleDTO>> GetMyRoles()
        {
            try
            {
                var userId = GetUserId();
                var roles = _authService.GetRolesByUserId(userId);
                return Ok(roles);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("roles/{id:int}/permissions")]
        public ActionResult<IEnumerable<Permission>> GetPermissionsByRoleId(int id)
        {
            try
            {
                var permissions = _authService.GetPermissionsByRoleId(id);
                return Ok(permissions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("admin/users")]
        [AuthorizeFunction("User.ReadAll")]
        public ActionResult<IEnumerable<UserDTO>> GetAllUsers()
        {
            try
            {
                var users = _authService.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("users/{userId:int}/roles")]
        [AuthorizeFunction("Role.Read")]
        public ActionResult<IEnumerable<RoleDTO>> GetRolesByUserId(int userId)
        {
            try
            {
                var roles = _authService.GetRolesByUserId(userId);
                return Ok(roles);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("users/{userId:int}/permissions")]
        [AuthorizeFunction("Permission.Read")]
        public ActionResult<IEnumerable<Permission>> GetPermissionsByUserId(int userId)
        {
            try
            {
                var permissions = _authService.GetPermissionsByUserId(userId);
                return Ok(permissions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "userId");
            if (claim == null)
                throw new UnauthorizedAccessException("Token không chứa userId.");

            return int.Parse(claim.Value);
        }
    }
}