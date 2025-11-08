using System;
using System.Linq;
using System.Collections.Generic;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.Infrastructure;
using FSI.BusinessProcessManagement.Infrastructure.Persistence;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure
{
    public class ServiceCollectionExtensionsTests
    {
        private static IConfiguration BuildConfiguration(string? connString)
        {
            var data = new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:BpmMySql", connString)
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(data!)
                .Build();
        }

        [Fact]
        public void AddInfrastructure_ShouldThrow_WhenConnectionStringMissing()
        {
            // Arrange
            var services = new ServiceCollection();
            var cfg = BuildConfiguration(null);

            // Act + Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                FSI.BusinessProcessManagement.Infrastructure.ServiceCollectionExtensions
                   .AddInfrastructure(services, cfg));

            Assert.Equal("Connection string 'BpmMySql' not found.", ex.Message);
        }

        [Fact]
        public void AddInfrastructure_ShouldRegister_AllExpectedServices_WithScopedLifetime()
        {
            // Arrange
            var services = new ServiceCollection();
            var cfg = BuildConfiguration("Server=localhost;Database=test;User=root;Password=123;");

            // Act
            FSI.BusinessProcessManagement.Infrastructure.ServiceCollectionExtensions
               .AddInfrastructure(services, cfg);

            // Assert – DbContext
            AssertContainsScoped<BpmDbContext>(services);

            // Repositórios concretos
            AssertContainsScoped<IDepartmentRepository, DepartmentRepository>(services);
            AssertContainsScoped<IUserRepository, UserRepository>(services);
            AssertContainsScoped<IRoleRepository, RoleRepository>(services);
            AssertContainsScoped<IUserRoleRepository, UserRoleRepository>(services);
            AssertContainsScoped<IScreenRepository, ScreenRepository>(services);
            AssertContainsScoped<IRoleScreenPermissionRepository, RoleScreenPermissionRepository>(services);
            AssertContainsScoped<IAuditLogRepository, AuditLogRepository>(services);
            AssertContainsScoped<IProcessRepository, ProcessRepository>(services);
            AssertContainsScoped<IProcessStepRepository, ProcessStepRepository>(services);
            AssertContainsScoped<IProcessExecutionRepository, ProcessExecutionRepository>(services);

            // UnitOfWork
            AssertContainsScoped<IUnitOfWork, UnitOfWork>(services);
        }

        // ------- Helpers -------

        private static void AssertContainsScoped<TService>(IServiceCollection services)
        {
            Assert.Contains(services, d =>
                d.ServiceType == typeof(TService) &&
                d.Lifetime == ServiceLifetime.Scoped);
        }

        private static void AssertContainsScoped<TService, TImpl>(IServiceCollection services)
        {
            Assert.Contains(services, d =>
                d.ServiceType == typeof(TService) &&
                d.ImplementationType == typeof(TImpl) &&
                d.Lifetime == ServiceLifetime.Scoped);
        }
    }
}
