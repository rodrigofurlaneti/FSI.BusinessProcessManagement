using System;
using System.Linq;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence
{
    public class BpmDbContextTests
    {
        private static BpmDbContext CreateInMemoryContext()
        {
            var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();

            var options = new DbContextOptionsBuilder<BpmDbContext>()
                .UseSqlite(conn)
                .Options;

            var ctx = new BpmDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }

        [Fact]
        public void Model_Should_Have_All_EntityTypes_Mapped()
        {
            using var ctx = CreateInMemoryContext();

            // Confirma que o modelo conhece todos os tipos esperados
            Assert.NotNull(ctx.Model.FindEntityType(typeof(Department)));
            Assert.NotNull(ctx.Model.FindEntityType(typeof(User)));
            Assert.NotNull(ctx.Model.FindEntityType(typeof(Role)));
            Assert.NotNull(ctx.Model.FindEntityType(typeof(UserRole)));
            Assert.NotNull(ctx.Model.FindEntityType(typeof(Screen)));
            Assert.NotNull(ctx.Model.FindEntityType(typeof(AuditLog)));
            Assert.NotNull(ctx.Model.FindEntityType(typeof(Process)));
            Assert.NotNull(ctx.Model.FindEntityType(typeof(ProcessStep)));
            Assert.NotNull(ctx.Model.FindEntityType(typeof(ProcessExecution)));
            Assert.NotNull(ctx.Model.FindEntityType(typeof(RoleScreenPermission)));
        }

        [Fact]
        public async Task Can_Create_And_Persist_All_Main_Aggregates_With_FKs()
        {
            using var ctx = CreateInMemoryContext();

            // Department
            var dept = new Department("Tecnologia", "TI");
            await ctx.AddAsync(dept);
            await ctx.SaveChangesAsync();

            // Users + Roles
            var user = new User("alice", "hash-alice", dept.Id, "alice@example.com", true);
            var role = new Role("Admin", "Acesso total");
            await ctx.AddRangeAsync(user, role);
            await ctx.SaveChangesAsync();

            // Screen
            var screen = new Screen("Dashboard", "Tela inicial");
            await ctx.AddAsync(screen);
            await ctx.SaveChangesAsync();

            // RoleScreenPermission (par RoleId, ScreenId é único)
            var perm = new RoleScreenPermission(role.Id, screen.Id, canView: true, canCreate: true, canEdit: false, canDelete: false);
            await ctx.AddAsync(perm);
            await ctx.SaveChangesAsync();

            // AuditLog (opcionalmente referenciando user e screen)
            var audit = new AuditLog("LOGIN", user.Id, screen.Id, "Sessão iniciada");
            await ctx.AddAsync(audit);
            await ctx.SaveChangesAsync();

            // Process
            var process = new Process("Onboarding", dept.Id, "Processo de onboarding", createdBy: user.Id);
            await ctx.AddAsync(process);
            await ctx.SaveChangesAsync();

            // Steps
            var step1 = new ProcessStep(process.Id, "Preenchimento de dados", 1, assignedRoleId: role.Id);
            var step2 = new ProcessStep(process.Id, "Aprovação gestor", 2, assignedRoleId: role.Id);
            await ctx.AddRangeAsync(step1, step2);
            await ctx.SaveChangesAsync();

            // Execution
            var exec = new ProcessExecution(process.Id, step1.Id, user.Id);
            await ctx.AddAsync(exec);
            await ctx.SaveChangesAsync();

            // Asserts básicos de persistência
            Assert.True(dept.Id > 0);
            Assert.True(user.Id > 0);
            Assert.True(role.Id > 0);
            Assert.True(screen.Id > 0);
            Assert.True(perm.Id > 0);
            Assert.True(audit.Id > 0);
            Assert.True(process.Id > 0);
            Assert.True(step1.Id > 0 && step2.Id > 0);
            Assert.True(exec.Id > 0);

            // Consulta para validar FKs
            var execDb = await ctx.ProcessExecutions
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(e => e.Id == exec.Id);
            Assert.NotNull(execDb);
            Assert.Equal(process.Id, execDb!.ProcessId);
            Assert.Equal(step1.Id, execDb.StepId);
            Assert.Equal(user.Id, execDb.UserId);
        }

        [Fact]
        public async Task Unique_Index_On_Department_Name_Should_Be_Enforced()
        {
            using var ctx = CreateInMemoryContext();

            await ctx.AddAsync(new Department("Financeiro", "Depto Fin"));
            await ctx.SaveChangesAsync();

            await ctx.AddAsync(new Department("Financeiro", "Duplicado"));
            await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
        }

        [Fact]
        public async Task Unique_Index_On_Screen_Name_Should_Be_Enforced()
        {
            using var ctx = CreateInMemoryContext();

            await ctx.AddAsync(new Screen("Relatórios", "Tela de relatórios"));
            await ctx.SaveChangesAsync();

            await ctx.AddAsync(new Screen("Relatórios", "Duplicada"));
            await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
        }

        [Fact]
        public async Task Unique_Index_On_RoleScreenPermission_Role_Screen_Should_Be_Enforced()
        {
            using var ctx = CreateInMemoryContext();

            var role = new Role("Editor", "Pode editar");
            var screen = new Screen("Configurações", "Preferências");
            await ctx.AddRangeAsync(role, screen);
            await ctx.SaveChangesAsync();

            var p1 = new RoleScreenPermission(role.Id, screen.Id, true, false, false, false);
            await ctx.AddAsync(p1);
            await ctx.SaveChangesAsync();

            var p2 = new RoleScreenPermission(role.Id, screen.Id, true, true, true, true);
            await ctx.AddAsync(p2);

            await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
        }

        [Fact]
        public async Task ApplyConfigurationsFromAssembly_Should_Set_Expected_ColumnTypes()
        {
            using var ctx = CreateInMemoryContext();

            var model = ctx.Model;

            var auditType = model.FindEntityType(typeof(AuditLog));
            Assert.NotNull(auditType);
            Assert.NotNull(auditType!.FindProperty(nameof(AuditLog.CreatedAt)));
            Assert.NotNull(auditType!.FindProperty(nameof(AuditLog.UpdatedAt)));

            var screenType = model.FindEntityType(typeof(Screen));
            Assert.NotNull(screenType);
            Assert.NotNull(screenType!.FindProperty(nameof(Screen.Name)));
            Assert.NotNull(screenType!.FindProperty(nameof(Screen.Description)));
        }
    }
}
