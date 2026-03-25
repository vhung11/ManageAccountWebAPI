using System.Configuration;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Context;
using ManageAccountWebAPI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ManageAccountWebAPI.Infrastructure.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;

        public AuthRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public User? GetUserByUsername(string username) => _context.Users.FirstOrDefault(u => u.Username == username);

        public User? GetUserById(int id) => _context.Users.FirstOrDefault(u => u.Id == id);

        public User? GetUserByEmail(string email) => _context.Users.FirstOrDefault(u => u.Email == email);

        public User AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public void DeleteUser(User user)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public Permission? GetPermissionById(int id) => _context.Permissions.FirstOrDefault(p => p.Id == id);

        public Permission? GetPermissionByCode(string code) => _context.Permissions.FirstOrDefault(p => p.Code == code);

        public ICollection<Permission> GetAllPermissions() => _context.Permissions.ToList();

        public Permission AddPermission(Permission permission)
        {
            _context.Permissions.Add(permission);
            _context.SaveChanges();
            return permission;
        }

        public Permission UpdatePermission(Permission permission)
        {
            _context.Permissions.Update(permission);
            _context.SaveChanges();
            return permission;
        }

        public void DeletePermission(Permission permission)
        {
            _context.Permissions.Remove(permission);
            _context.SaveChanges();
        }

        public Role? GetRoleByName(string roleName) 
            => _context.Roles.FirstOrDefault(r => r.Name == roleName);

        public ICollection<Role> GetAllRoles() => _context.Roles.ToList();
        
        public Role? GetRoleById(int id) => _context.Roles.FirstOrDefault(r => r.Id == id);
        
        public Role AddRole(Role role)
        {
            _context.Roles.Add(role);
            _context.SaveChanges();
            return role;
        }

        public Role UpdateRole(Role role)
        {
            _context.Roles.Update(role);
            _context.SaveChanges();
            return role;
        }

        public void DeleteRole(Role role)
        {
            _context.Roles.Remove(role);
            _context.SaveChanges();
        }

        public void AddUserRole(UserRole userRole)
        {
            _context.UserRoles.Add(userRole);
            _context.SaveChanges();
        }

        public UserRole? GetUserRole(int userId, int roleId)
            => _context.UserRoles.FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId);

        public void RemoveUserRole(UserRole userRole)
        {
            _context.UserRoles.Remove(userRole);
            _context.SaveChanges();
        }

        public void AddRolePermission(RolePermission rolePermission)
        {
            _context.RolePermissions.Add(rolePermission);
            _context.SaveChanges();
        }

        public RolePermission? GetRolePermission(int roleId, int permissionId)
            => _context.RolePermissions.FirstOrDefault(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        public void RemoveRolePermission(RolePermission rolePermission)
        {
            _context.RolePermissions.Remove(rolePermission);
            _context.SaveChanges();
        }

        public void AddUserPermission(UserPermission userPermission)
        {
            _context.UserPermissions.Add(userPermission);
            _context.SaveChanges();
        }

        public UserPermission? GetUserPermission(int userId, int permissionId)
            => _context.UserPermissions.FirstOrDefault(up => up.UserId == userId && up.PermissionId == permissionId);

        public void RemoveUserPermission(UserPermission userPermission)
        {
            _context.UserPermissions.Remove(userPermission);
            _context.SaveChanges();
        }

        public User? GetUserWithRolesByUsername(string username)
        {
            return _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Username == username);
        }

        public bool HasPermission(int userId, string permissionCode)
        {
            var hasDirectPermission = _context.UserPermissions
                .Any(up => up.UserId == userId && up.Permission.Code == permissionCode);
            if (hasDirectPermission)
            {
                return true;
            }

            var userRoles = _context.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToList();
            if (!userRoles.Any())
            {
                return false;
            }

            return _context.RolePermissions
                .Any(rp => userRoles.Contains(rp.RoleId) && rp.Permission.Code == permissionCode);
        }
    }
}