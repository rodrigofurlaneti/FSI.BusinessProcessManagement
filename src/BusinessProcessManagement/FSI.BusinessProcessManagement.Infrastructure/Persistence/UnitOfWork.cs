using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BpmDbContext __dbContext;

        public IDepartmentRepository Departments { get; }
        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IUserRoleRepository UserRoles { get; }
        public IScreenRepository Screens { get; }
        public IRoleScreenPermissionRepository RoleScreenPermissions { get; }
        public IAuditLogRepository AuditLogs { get; }
        public IProcessRepository Processes { get; }
        public IProcessStepRepository ProcessSteps { get; }
        public IProcessExecutionRepository ProcessExecutions { get; }

        public UnitOfWork(
            BpmDbContext ctx,
            IDepartmentRepository departments,
            IUserRepository users,
            IRoleRepository roles,
            IUserRoleRepository userRoles,
            IScreenRepository screens,
            IRoleScreenPermissionRepository roleScreenPermissions,
            IAuditLogRepository auditLogs,
            IProcessRepository processes,
            IProcessStepRepository processSteps,
            IProcessExecutionRepository processExecutions)
        {
            __dbContext = ctx;
            Departments = departments;
            Users = users;
            Roles = roles;
            UserRoles = userRoles;
            Screens = screens;
            RoleScreenPermissions = roleScreenPermissions;
            AuditLogs = auditLogs;
            Processes = processes;
            ProcessSteps = processSteps;
            ProcessExecutions = processExecutions;
        }

        public Task<int> CommitAsync() => __dbContext.SaveChangesAsync();

        // Caso sua interface tenha SaveChangesAsync com outro nome:
        // public Task<int> SaveChangesAsync() => __dbContext.SaveChangesAsync();

        public void Dispose() => __dbContext.Dispose();
    }
}
