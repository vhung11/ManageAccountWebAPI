using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Controllers.Filters;
using ManageAccountWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("roles")]
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
        [AuthorizeFunction("Auth.ManagePermissions")]
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
    }
}