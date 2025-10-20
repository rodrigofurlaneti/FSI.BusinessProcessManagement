namespace FSI.BusinessProcessManagement.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IDepartmentRepository Departments { get; }
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IUserRoleRepository UserRoles { get; }
        IScreenRepository Screens { get; }
        IRoleScreenPermissionRepository RoleScreenPermissions { get; }
        IAuditLogRepository AuditLogs { get; }
        IProcessRepository Processes { get; }
        IProcessStepRepository ProcessSteps { get; }
        IProcessExecutionRepository ProcessExecutions { get; }
        Task<int> CommitAsync();
    }
}
