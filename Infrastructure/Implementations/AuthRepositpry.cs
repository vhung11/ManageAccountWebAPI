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

        // ── User ──────────────────────────────────────────────

        public User? GetUserByUsername(string username)
            => _context.Users.FirstOrDefault(u => u.Username == username);

        public User? GetUserById(int id)
            => _context.Users.FirstOrDefault(u => u.Id == id);

        public User? GetUserByEmail(string email)
            => _context.Users.FirstOrDefault(u => u.Email == email);

        public User? GetUserWithRole(int id)
            => _context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefault(u => u.Id == id);

        public User? GetUserWithRoleByUsername(string username)
            => _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Username == username);

        public User AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public User UpdateUser(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
            return user;
        }

        public void DeleteUser(User user)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        // ── Permission ───────────────────────────────────────

        public Permission? GetPermissionById(int id)
            => _context.Permissions.FirstOrDefault(p => p.Id == id);

        public Permission? GetPermissionByCode(string code)
            => _context.Permissions.FirstOrDefault(p => p.Code == code);

        public ICollection<Permission> GetAllPermissions()
            => _context.Permissions.ToList();

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

        // ── Role ──────────────────────────────────────────────

        public Role? GetRoleById(int id)
            => _context.Roles.FirstOrDefault(r => r.Id == id);

        public Role? GetRoleByName(string name)
            => _context.Roles.FirstOrDefault(r => r.Name == name);

        public ICollection<Role> GetAllRoles()
            => _context.Roles.ToList();

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

        // ── RolePermission ────────────────────────────────────

        public RolePermission? GetRolePermission(int roleId, int permissionId)
            => _context.RolePermissions
                .FirstOrDefault(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        public RolePermission AddRolePermission(RolePermission rolePermission)
        {
            _context.RolePermissions.Add(rolePermission);
            _context.SaveChanges();
            return rolePermission;
        }

        public void RemoveRolePermission(RolePermission rolePermission)
        {
            _context.RolePermissions.Remove(rolePermission);
            _context.SaveChanges();
        }

        public ICollection<Permission> GetPermissionsForRole(int roleId)
        {
            return _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission)
                .ToList();
        }
    }
}