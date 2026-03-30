using ManageAccountWebAPI.Data.Constants;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ManageAccountWebAPI.Infrastructure.Seeder
{
    /// <summary>
    /// Seed dữ liệu ban đầu cho cơ sở dữ liệu.
    /// Chỉ chèn dữ liệu nếu bảng tương ứng chưa có dòng nào (idempotent).
    /// </summary>
    public static class DatabaseSeeder
    {
        public static void Seed(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                logger.LogInformation("Bắt đầu seed dữ liệu...");

                SeedInterestTypes(context, logger);
                SeedPermissions(context, logger);
                SeedRoles(context, logger);
                SeedRolePermissions(context, logger);
                SeedUsers(context, logger);
                SeedUserRoles(context, logger);
                SeedAccounts(context, logger);

                logger.LogInformation("Seed dữ liệu hoàn tất.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi trong quá trình seed dữ liệu.");
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // 1. Loại lãi suất (InterestTypes)
        //    Đại diện cho các kỳ hạn gửi tiết kiệm thực tế tại ngân hàng Việt Nam
        // ─────────────────────────────────────────────────────────────────────
        private static void SeedInterestTypes(ApplicationDbContext ctx, ILogger logger)
        {
            if (ctx.InterestTypes.Any())
            {
                logger.LogInformation("InterestTypes đã có dữ liệu, bỏ qua.");
                return;
            }

            var interestTypes = new List<InterestType>
            {
                // Không kỳ hạn – lãi suất thấp, rút tiền bất kỳ lúc nào
                new() { Rate = 0.0010m },   // 0.10% / năm (không kỳ hạn)
                // Kỳ hạn ngắn
                new() { Rate = 0.0300m },   // 3.00% / năm (1 tháng)
                new() { Rate = 0.0330m },   // 3.30% / năm (2 tháng)
                new() { Rate = 0.0360m },   // 3.60% / năm (3 tháng)
                // Kỳ hạn trung bình
                new() { Rate = 0.0430m },   // 4.30% / năm (6 tháng)
                new() { Rate = 0.0490m },   // 4.90% / năm (9 tháng)
                new() { Rate = 0.0530m },   // 5.30% / năm (12 tháng)
                // Kỳ hạn dài
                new() { Rate = 0.0550m },   // 5.50% / năm (18 tháng)
                new() { Rate = 0.0570m },   // 5.70% / năm (24 tháng)
                new() { Rate = 0.0580m },   // 5.80% / năm (36 tháng)
            };

            ctx.InterestTypes.AddRange(interestTypes);
            ctx.SaveChanges();
            logger.LogInformation("Đã seed {Count} InterestType(s).", interestTypes.Count);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2. Quyền hệ thống (Permissions)
        //    Theo mô hình Resource:Action – phủ đầy đủ các nghiệp vụ
        // ─────────────────────────────────────────────────────────────────────
        private static void SeedPermissions(ApplicationDbContext ctx, ILogger logger)
        {
            if (ctx.Permissions.Any())
            {
                logger.LogInformation("Permissions đã có dữ liệu, bỏ qua.");
                return;
            }

            var permissions = new List<Permission>
            {
                // ── Quản lý người dùng ──────────────────────────────────────
                new() { Code = "User.ReadAll", Resource = "User", Action = "ReadAll" },
                new() { Code = "User.Read",    Resource = "User", Action = "Read"    },
                new() { Code = "User.Create",  Resource = "User", Action = "Create"  },
                new() { Code = "User.Update",  Resource = "User", Action = "Update"  },
                new() { Code = "User.Delete",  Resource = "User", Action = "Delete"  },

                // ── Quản lý tài khoản ngân hàng ─────────────────────────────
                new() { Code = "Account.ReadAll",     Resource = "Account", Action = "ReadAll"     },
                new() { Code = "Account.Read",        Resource = "Account", Action = "Read"        },
                new() { Code = "Account.Create",      Resource = "Account", Action = "Create"      },
                new() { Code = "Account.Update",      Resource = "Account", Action = "Update"      },
                new() { Code = "Account.Delete",      Resource = "Account", Action = "Delete"      },
                new() { Code = "Account.ApplyInterest", Resource = "Account", Action = "ApplyInterest" },

                // ── Giao dịch ────────────────────────────────────────────────
                new() { Code = "Transaction.Deposit",  Resource = "Transaction", Action = "Deposit"  },
                new() { Code = "Transaction.Withdraw", Resource = "Transaction", Action = "Withdraw" },
                new() { Code = "Transaction.Transfer", Resource = "Transaction", Action = "Transfer" },

                // ── Số dư ────────────────────────────────────────────────────
                new() { Code = "Balance.Read", Resource = "Balance", Action = "Read" },

                // ── Quản lý loại lãi suất ────────────────────────────────────
                new() { Code = "Interest.Read",   Resource = "Interest", Action = "Read"   },
                new() { Code = "Interest.Manage", Resource = "Interest", Action = "Manage" },

                // ── Quản lý vai trò ──────────────────────────────────────────
                new() { Code = "Role.Read",   Resource = "Role", Action = "Read"   },
                new() { Code = "Role.Create", Resource = "Role", Action = "Create" },
                new() { Code = "Role.Update", Resource = "Role", Action = "Update" },
                new() { Code = "Role.Delete", Resource = "Role", Action = "Delete" },
                new() { Code = "Role.Assign", Resource = "Role", Action = "Assign" },

                // ── Quản lý quyền ────────────────────────────────────────────
                new() { Code = "Permission.Read",   Resource = "Permission", Action = "Read"   },
                new() { Code = "Permission.Create", Resource = "Permission", Action = "Create" },
                new() { Code = "Permission.Update", Resource = "Permission", Action = "Update" },
                new() { Code = "Permission.Delete", Resource = "Permission", Action = "Delete" },
                new() { Code = "Permission.Assign", Resource = "Permission", Action = "Assign" },

                // ── Báo cáo / nhật ký hệ thống ──────────────────────────────
                new() { Code = "Log.Read", Resource = "Log", Action = "Read" },
            };

            ctx.Permissions.AddRange(permissions);
            ctx.SaveChanges();
            logger.LogInformation("Đã seed {Count} Permission(s).", permissions.Count);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3. Vai trò (Roles)
        // ─────────────────────────────────────────────────────────────────────
        private static void SeedRoles(ApplicationDbContext ctx, ILogger logger)
        {
            if (ctx.Roles.Any())
            {
                logger.LogInformation("Roles đã có dữ liệu, bỏ qua.");
                return;
            }

            var roles = new List<Role>
            {
                new() { Name = "Admin" },    // Quản trị hệ thống – toàn quyền
                new() { Name = "Manager" },  // Quản lý chi nhánh – quản lý tài khoản & giao dịch
                new() { Name = "Teller" },   // Giao dịch viên – thực hiện giao dịch
                new() { Name = "User" },     // Khách hàng – xem & thao tác tài khoản của mình
            };

            ctx.Roles.AddRange(roles);
            ctx.SaveChanges();
            logger.LogInformation("Đã seed {Count} Role(s).", roles.Count);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 4. Phân quyền cho từng vai trò (RolePermissions)
        // ─────────────────────────────────────────────────────────────────────
        private static void SeedRolePermissions(ApplicationDbContext ctx, ILogger logger)
        {
            if (ctx.RolePermissions.Any())
            {
                logger.LogInformation("RolePermissions đã có dữ liệu, bỏ qua.");
                return;
            }

            // Lấy map Code → Id
            var permMap  = ctx.Permissions.ToDictionary(p => p.Code, p => p.Id);
            var roleMap  = ctx.Roles.ToDictionary(r => r.Name, r => r.Id);

            var adminId   = roleMap["Admin"];
            var managerId = roleMap["Manager"];
            var tellerId  = roleMap["Teller"];
            var userId    = roleMap["User"];

            var rolePermissions = new List<RolePermission>();

            // Admin – toàn quyền hệ thống
            foreach (var code in permMap.Keys)
                rolePermissions.Add(new() { RoleId = adminId, PermissionId = permMap[code] });

            // Manager – quản lý người dùng, tài khoản, giao dịch, lãi suất, xem log
            var managerCodes = new[]
            {
                "User.ReadAll", "User.Read", "User.Create", "User.Update",
                "Account.ReadAll", "Account.Read", "Account.Create", "Account.Update", "Account.ApplyInterest",
                "Transaction.Deposit", "Transaction.Withdraw", "Transaction.Transfer",
                "Balance.Read",
                "Interest.Read", "Interest.Manage",
                "Role.Read", "Permission.Read",
                "Log.Read"
            };
            foreach (var code in managerCodes)
                rolePermissions.Add(new() { RoleId = managerId, PermissionId = permMap[code] });

            // Teller – thực hiện giao dịch, xem tài khoản & thông tin khách hàng
            var tellerCodes = new[]
            {
                "User.Read",
                "Account.Read",
                "Transaction.Deposit", "Transaction.Withdraw", "Transaction.Transfer",
                "Balance.Read",
                "Interest.Read"
            };
            foreach (var code in tellerCodes)
                rolePermissions.Add(new() { RoleId = tellerId, PermissionId = permMap[code] });

            // User (khách hàng) – xem & thao tác tài khoản của bản thân
            var userCodes = new[]
            {
                "Account.Read", "Account.Create", "Account.Delete",
                "Transaction.Deposit", "Transaction.Withdraw", "Transaction.Transfer",
                "Balance.Read",
                "Interest.Read"
            };
            foreach (var code in userCodes)
                rolePermissions.Add(new() { RoleId = userId, PermissionId = permMap[code] });

            ctx.RolePermissions.AddRange(rolePermissions);
            ctx.SaveChanges();
            logger.LogInformation("Đã seed {Count} RolePermission(s).", rolePermissions.Count);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 5. Người dùng mẫu (Users)
        //    Lưu ý: PasswordHash đang lưu plain-text cho mục đích demo.
        //    Trong production cần dùng BCrypt hoặc PBKDF2.
        // ─────────────────────────────────────────────────────────────────────
        private static void SeedUsers(ApplicationDbContext ctx, ILogger logger)
        {
            if (ctx.Users.Any())
            {
                logger.LogInformation("Users đã có dữ liệu, bỏ qua.");
                return;
            }

            var users = new List<User>
            {
                // Quản trị viên hệ thống
                new()
                {
                    FullName     = "Nguyễn Quản Trị",
                    Username     = "admin",
                    PasswordHash = "Admin@123",
                    Email        = "admin@vietbank.vn"
                },
                // Quản lý chi nhánh
                new()
                {
                    FullName     = "Trần Thị Quản Lý",
                    Username     = "manager01",
                    PasswordHash = "Manager@123",
                    Email        = "manager01@vietbank.vn"
                },
                // Giao dịch viên
                new()
                {
                    FullName     = "Lê Văn Giao Dịch",
                    Username     = "teller01",
                    PasswordHash = "Teller@123",
                    Email        = "teller01@vietbank.vn"
                },
                // Khách hàng mẫu 1
                new()
                {
                    FullName     = "Phạm Minh Tuấn",
                    Username     = "phamminhtuan",
                    PasswordHash = "User@123",
                    Email        = "phamminhtuan@gmail.com"
                },
                // Khách hàng mẫu 2
                new()
                {
                    FullName     = "Hoàng Thị Lan",
                    Username     = "hoangthilan",
                    PasswordHash = "User@123",
                    Email        = "hoangthilan@gmail.com"
                },
                // Khách hàng mẫu 3
                new()
                {
                    FullName     = "Vũ Đức Hùng",
                    Username     = "vuduchung",
                    PasswordHash = "User@123",
                    Email        = "vuduchung@gmail.com"
                },
            };

            ctx.Users.AddRange(users);
            ctx.SaveChanges();
            logger.LogInformation("Đã seed {Count} User(s).", users.Count);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 6. Gán vai trò cho người dùng (UserRoles)
        // ─────────────────────────────────────────────────────────────────────
        private static void SeedUserRoles(ApplicationDbContext ctx, ILogger logger)
        {
            if (ctx.UserRoles.Any())
            {
                logger.LogInformation("UserRoles đã có dữ liệu, bỏ qua.");
                return;
            }

            var userMap = ctx.Users.ToDictionary(u => u.Username, u => u.Id);
            var roleMap = ctx.Roles.ToDictionary(r => r.Name, r => r.Id);

            var userRoles = new List<UserRole>
            {
                new() { UserId = userMap["admin"],        RoleId = roleMap["Admin"]   },
                new() { UserId = userMap["manager01"],    RoleId = roleMap["Manager"] },
                new() { UserId = userMap["teller01"],     RoleId = roleMap["Teller"]  },
                new() { UserId = userMap["phamminhtuan"], RoleId = roleMap["User"]    },
                new() { UserId = userMap["hoangthilan"],  RoleId = roleMap["User"]    },
                new() { UserId = userMap["vuduchung"],    RoleId = roleMap["User"]    },
            };

            ctx.UserRoles.AddRange(userRoles);
            ctx.SaveChanges();
            logger.LogInformation("Đã seed {Count} UserRole(s).", userRoles.Count);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 7. Tài khoản & số dư mẫu (Accounts & AccountBalances)
        //    Mỗi khách hàng có 1 tài khoản thanh toán + tùy chọn tiết kiệm
        // ─────────────────────────────────────────────────────────────────────
        private static void SeedAccounts(ApplicationDbContext ctx, ILogger logger)
        {
            if (ctx.Accounts.Any())
            {
                logger.LogInformation("Accounts đã có dữ liệu, bỏ qua.");
                return;
            }

            var userMap = ctx.Users.ToDictionary(u => u.Username, u => u.Id);
            var interestTypes = ctx.InterestTypes.OrderBy(i => i.Id).ToList();

            // Id của từng kỳ hạn theo thứ tự seed ở bước 1
            var demandId = interestTypes[0].Id;  // 0.10% – không kỳ hạn
            var term1mId = interestTypes[1].Id;  // 3.00% – 1 tháng
            var term3mId = interestTypes[3].Id;  // 3.60% – 3 tháng
            var term6mId = interestTypes[4].Id;  // 4.30% – 6 tháng
            var term12mId = interestTypes[6].Id;  // 5.30% – 12 tháng

            var accounts = new List<Account>
            {
                // ── Phạm Minh Tuấn ──────────────────────────────────────────
                new()
                {
                    UserId = userMap["phamminhtuan"],
                    AccountBalances = new List<AccountBalance>
                    {
                        // Tài khoản thanh toán
                        new() { Type = AccountType.Checking, Balance = 15_500_000m, InterestTypeId = demandId  },
                        // Tiết kiệm 6 tháng
                        new() { Type = AccountType.Savings,  Balance = 50_000_000m, InterestTypeId = term6mId  },
                    }
                },

                // ── Hoàng Thị Lan ────────────────────────────────────────────
                new()
                {
                    UserId = userMap["hoangthilan"],
                    AccountBalances = new List<AccountBalance>
                    {
                        new() { Type = AccountType.Checking, Balance = 8_750_000m,   InterestTypeId = demandId  },
                        new() { Type = AccountType.Savings,  Balance = 100_000_000m, InterestTypeId = term12mId },
                    }
                },

                // ── Vũ Đức Hùng ─────────────────────────────────────────────
                new()
                {
                    UserId = userMap["vuduchung"],
                    AccountBalances = new List<AccountBalance>
                    {
                        new() { Type = AccountType.Checking, Balance = 3_200_000m,  InterestTypeId = demandId },
                        new() { Type = AccountType.Savings,  Balance = 20_000_000m, InterestTypeId = term3mId },
                        // Thêm một kỳ hạn 1 tháng cuốn chiếu
                        new() { Type = AccountType.Savings,  Balance = 10_000_000m, InterestTypeId = term1mId },
                    }
                },

                // ── Manager01 – thường cũng có tài khoản cá nhân ────────────
                new()
                {
                    UserId = userMap["manager01"],
                    AccountBalances = new List<AccountBalance>
                    {
                        new() { Type = AccountType.Checking, Balance = 25_000_000m,  InterestTypeId = demandId  },
                        new() { Type = AccountType.Savings,  Balance = 200_000_000m, InterestTypeId = term12mId },
                    }
                },
            };

            ctx.Accounts.AddRange(accounts);
            ctx.SaveChanges();
            logger.LogInformation("Đã seed {Count} Account(s) với AccountBalance(s) tương ứng.", accounts.Count);
        }
    }
}
