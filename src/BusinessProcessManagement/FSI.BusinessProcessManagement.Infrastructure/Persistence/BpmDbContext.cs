using FSI.BusinessProcessManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence
{
    public class BpmDbContext : DbContext
    {
        public BpmDbContext(DbContextOptions<BpmDbContext> options) : base(options) { }
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Screen> Screens => Set<Screen>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Process> Processes => Set<Process>();
        public DbSet<ProcessStep> ProcessSteps => Set<ProcessStep>();
        public DbSet<ProcessExecution> ProcessExecutions => Set<ProcessExecution>();
        public DbSet<RoleScreenPermission> RoleScreenPermissions => Set<RoleScreenPermission>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BpmDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
