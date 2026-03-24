using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Filters;
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
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
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
                return Ok(permission);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("permissions/{id:int}")]
        [AuthorizeFunction("Auth.ManagePermissions")]
        public ActionResult<Permission> UpdatePermission([FromBody] PermissionRequest request)
        {
            try
            {
                var permission = _authService.UpdatePermission(request);
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("users/{userId:int}/permissions/{permissionId:int}")]
        [AuthorizeFunction("Auth.ManagePermissions")]
        public IActionResult AssignPermissionToUser(int userId, int permissionId)
        {
            try
            {
                _authService.AssignPermissionToUser(userId, permissionId);
                return Ok("Gán quyền cho người dùng thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("users/{userId:int}/permissions/{permissionId:int}")]
        [AuthorizeFunction("Auth.ManagePermissions")]
        public IActionResult RemovePermissionFromUser(int userId, int permissionId)
        {
            try
            {
                _authService.RemovePermissionFromUser(userId, permissionId);
                return Ok("Xóa quyền khỏi người dùng thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("users/{userId:int}/permissions")]
        [AuthorizeFunction("Auth.ManagePermissions")]
        public IActionResult GetPermissionsForUser(int userId)
        {
            try
            {
                var permissions = _authService.GetPermissionsForUser(userId);
                return Ok(permissions);
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
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user-has-permission-by-resource-and-action")]
        public IActionResult UserHasPermissionByResourceAndAction(int userId, string resource, string action)
        {
            try
            {
                var result = _authService.UserHasPermission(userId, resource, action);
                return Ok(result);
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
    }
}