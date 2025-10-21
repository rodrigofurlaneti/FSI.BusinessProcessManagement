using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FSI.BusinessProcessManagement.Infrastructure.Persistence;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var cs = configuration.GetConnectionString("BpmMySql")
                     ?? throw new InvalidOperationException("Connection string 'BpmMySql' not found.");

            services.AddDbContext<BpmDbContext>(options =>
                options.UseMySql(cs, ServerVersion.AutoDetect(cs)));

            services.AddScoped<DbContext>(sp => sp.GetRequiredService<BpmDbContext>());
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IScreenRepository, ScreenRepository>();
            services.AddScoped<IRoleScreenPermissionRepository, RoleScreenPermissionRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IProcessRepository, ProcessRepository>();
            services.AddScoped<IProcessStepRepository, ProcessStepRepository>();
            services.AddScoped<IProcessExecutionRepository, ProcessExecutionRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
