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

        public bool HasPermission(int roleId, string permissionCode)
            => _context.RolePermissions
                .Any(rp => rp.RoleId == roleId
                        && rp.Permission.Code == permissionCode);
    }
}