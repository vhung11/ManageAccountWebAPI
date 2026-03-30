using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Controllers.Filters;
using ManageAccountWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Xml;

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
    }
}