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

        public UserPermission? GetUserPermission(int userId, int permissionId) => _context.UserPermissions.FirstOrDefault(up => up.UserId == userId && up.PermissionId == permissionId);

        public UserPermission AddUserPermission(UserPermission userPermission)
        {
            _context.UserPermissions.Add(userPermission);
            _context.SaveChanges();
            return userPermission;
        }

        public void RemoveUserPermission(UserPermission userPermission)
        {
            _context.UserPermissions.Remove(userPermission);
            _context.SaveChanges();
        }

        public ICollection<Permission> GetPermissionsForUser(int userId)
        {
            return _context.UserPermissions
                .Include(up => up.Permission)
                .Where(up => up.UserId == userId)
                .Select(up => up.Permission)
                .ToList();
        }
    }
}