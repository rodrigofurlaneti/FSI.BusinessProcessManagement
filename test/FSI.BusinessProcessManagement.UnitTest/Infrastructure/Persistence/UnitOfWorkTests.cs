using System;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.Infrastructure.Persistence;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence
{
    public class UnitOfWorkTests
    {
        private static BpmDbContext CreateInMemoryContext(out SqliteConnection connection)
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<BpmDbContext>()
                .UseSqlite(connection)
                .Options;

            var ctx = new BpmDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }

        private static UnitOfWork CreateUow(BpmDbContext ctx, out (
            IDepartmentRepository dep,
            IUserRepository user,
            IRoleRepository role,
            IUserRoleRepository userRole,
            IScreenRepository screen,
            IRoleScreenPermissionRepository roleScreenPerm,
            IAuditLogRepository audit,
            IProcessRepository process,
            IProcessStepRepository step,
            IProcessExecutionRepository exec
        ) repos)
        {
            var dep = new DepartmentRepository(ctx);
            var user = new UserRepository(ctx);
            var role = new RoleRepository(ctx);
            var userRole = new UserRoleRepository(ctx);
            var screen = new ScreenRepository(ctx);
            var roleScreenPerm = new RoleScreenPermissionRepository(ctx);
            var audit = new AuditLogRepository(ctx);
            var process = new ProcessRepository(ctx);
            var step = new ProcessStepRepository(ctx);
            var exec = new ProcessExecutionRepository(ctx);

            repos = (dep, user, role, userRole, screen, roleScreenPerm, audit, process, step, exec);

            return new UnitOfWork(
                ctx, dep, user, role, userRole, screen, roleScreenPerm, audit, process, step, exec
            );
        }

        [Fact]
        public void Properties_ShouldExpose_InjectedRepositories()
        {
            var ctx = CreateInMemoryContext(out var conn);
            var uow = CreateUow(ctx, out var repos);

            Assert.Same(repos.dep, uow.Departments);
            Assert.Same(repos.user, uow.Users);
            Assert.Same(repos.role, uow.Roles);
            Assert.Same(repos.userRole, uow.UserRoles);
            Assert.Same(repos.screen, uow.Screens);
            Assert.Same(repos.roleScreenPerm, uow.RoleScreenPermissions);
            Assert.Same(repos.audit, uow.AuditLogs);
            Assert.Same(repos.process, uow.Processes);
            Assert.Same(repos.step, uow.ProcessSteps);
            Assert.Same(repos.exec, uow.ProcessExecutions);

            // cleanup
            uow.Dispose();
            conn.Dispose();
        }

        [Fact]
        public async Task CommitAsync_ShouldPersist_Changes()
        {
            var ctx = CreateInMemoryContext(out var conn);
            var uow = CreateUow(ctx, out var repos);

            // Arrange: insere um Department via repo e comita pelo UoW
            var dept = new Department("Tecnologia", "TI");
            await repos.dep.InsertAsync(dept);

            // Act
            var affected = await uow.CommitAsync();

            // Assert: entidade foi persistida e possui Id
            Assert.True(affected >= 1);
            Assert.True(dept.Id > 0);

            var exists = await ctx.Departments.AnyAsync(d => d.Id == dept.Id && d.Name == "Tecnologia");
            Assert.True(exists);

            // cleanup
            uow.Dispose();
            conn.Dispose();
        }

        [Fact]
        public void Dispose_ShouldDispose_DbContext()
        {
            var ctx = CreateInMemoryContext(out var conn);
            var uow = CreateUow(ctx, out _);

            // Act
            uow.Dispose();

            // Assert: tentar usar o contexto depois do Dispose deve lançar ObjectDisposedException
            Assert.Throws<ObjectDisposedException>(() =>
            {
                // Força acesso a algo do contexto para disparar a exceção
                var _ = ctx.Departments.Local;
            });

            conn.Dispose();
        }
    }
}
