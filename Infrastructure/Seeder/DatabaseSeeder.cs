using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ManageAccountWebAPI.Infrastructure.Seeder
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Seed()
        {
            SeedRoles();
            SeedPermissions();
            SeedRolePermissions();
            SeedAdminUser();
        }

        private void SeedRoles()
        {
            if (_context.Roles.Any()) return;

            var roles = new List<Role>
            {
                new Role { Name = "Admin", Description = "Quản trị viên - toàn quyền hệ thống" },
                new Role { Name = "User", Description = "Người dùng - quyền hạn chế" }
            };

            _context.Roles.AddRange(roles);
            _context.SaveChanges();
            _logger.LogInformation("Seeded {Count} roles", roles.Count);
        }

        private void SeedPermissions()
        {
            if (_context.Permissions.Any()) return;

            var permissions = new List<Permission>
            {
                new Permission { Code = "Account.Read", Resource = "Account", Action = "Read" },
                new Permission { Code = "Account.Create", Resource = "Account", Action = "Create" },
                new Permission { Code = "Account.Update", Resource = "Account", Action = "Update" },
                new Permission { Code = "Account.Delete", Resource = "Account", Action = "Delete" },
                new Permission { Code = "Account.ApplyInterest", Resource = "Account", Action = "ApplyInterest" },
                new Permission { Code = "Auth.ManagePermissions", Resource = "Auth", Action = "ManagePermissions" }
            };

            _context.Permissions.AddRange(permissions);
            _context.SaveChanges();
            _logger.LogInformation("Seeded {Count} permissions", permissions.Count);
        }

        private void SeedRolePermissions()
        {
            if (_context.RolePermissions.Any()) return;

            var adminRole = _context.Roles.First(r => r.Name == "Admin");
            var userRole = _context.Roles.First(r => r.Name == "User");
            var allPermissions = _context.Permissions.ToList();

            var rolePermissions = new List<RolePermission>();

            // Admin gets ALL permissions
            foreach (var permission in allPermissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }

            // User gets only Account.Read
            var readPermission = allPermissions.First(p => p.Code == "Account.Read");
            rolePermissions.Add(new RolePermission
            {
                RoleId = userRole.Id,
                PermissionId = readPermission.Id
            });

            _context.RolePermissions.AddRange(rolePermissions);
            _context.SaveChanges();
            _logger.LogInformation("Seeded {Count} role-permission mappings", rolePermissions.Count);
        }

        private void SeedAdminUser()
        {
            if (_context.Users.Any(u => u.Username == "admin")) return;

            var adminRole = _context.Roles.First(r => r.Name == "Admin");

            _context.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = "admin",
                Email = "admin@system.local",
                RoleId = adminRole.Id
            });

            _context.SaveChanges();
            _logger.LogInformation("Seeded admin user");
        }
    }
}
