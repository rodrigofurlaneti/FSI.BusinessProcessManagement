using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using FSI.BusinessProcessManagement.Infrastructure.Persistence;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Api.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(BpmDbContext db)
        {
            await db.Database.MigrateAsync();

            var adminRole = await db.Set<Role>().AsNoTracking().FirstOrDefaultAsync(r => r.Name == "Administrador");
            if (adminRole == null)
            {
                adminRole = new Role("Administrador", "Acesso total ao sistema");
                await db.Set<Role>().AddAsync(adminRole);
                await db.SaveChangesAsync();
            }

            var adminUser = await db.Set<User>().FirstOrDefaultAsync(u => u.Username == "admin");
            if (adminUser == null)
            {
                var password = "Admin@123";
                var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

                adminUser = new User(
                    username: "admin",
                    passwordHash: hash,
                    departmentId: null,
                    email: "admin@local",
                    isActive: true);

                await db.Set<User>().AddAsync(adminUser);
                await db.SaveChangesAsync();
            }

            var hasLink = await db.Set<UserRole>()
                .AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);

            if (!hasLink)
            {
                var link = new UserRole(adminUser.Id, adminRole.Id);
                await db.Set<UserRole>().AddAsync(link);
                await db.SaveChangesAsync();
            }

            if (db.Model.FindEntityType(typeof(RoleScreenPermission)) != null)
            {
                var screens = await db.Set<Screen>().ToListAsync();
                if (!screens.Any())
                {
                    screens.Add(new Screen("Dashboard", "Visão geral"));
                    screens.Add(new Screen("Users", "Gestão de usuários"));
                    screens.Add(new Screen("Processes", "Gestão de processos"));
                    screens.Add(new Screen("Audit", "Log de auditoria"));
                    await db.Set<Screen>().AddRangeAsync(screens);
                    await db.SaveChangesAsync();
                }

                foreach (var s in screens)
                {
                    var exists = await db.Set<RoleScreenPermission>()
                        .AnyAsync(p => p.RoleId == adminRole.Id && p.ScreenId == s.Id);
                    if (!exists)
                    {
                        await db.Set<RoleScreenPermission>().AddAsync(
                            new RoleScreenPermission(adminRole.Id, s.Id, canView: true, canCreate: true, canEdit: true, canDelete: true));
                    }
                }
                await db.SaveChangesAsync();
            }
        }
    }
}
