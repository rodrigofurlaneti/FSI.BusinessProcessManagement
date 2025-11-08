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
    public class DependencyInjectionTests
    {
        private static IConfiguration BuildConfigWith(string? connString)
        {
            var inMemory = new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:BpmMySql", connString)
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemory!)
                .Build();
        }

        [Fact]
        public void AddInfrastructure_ShouldThrow_WhenConnectionStringMissing()
        {
            var services = new ServiceCollection();
            var cfg = BuildConfigWith(null);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                // Chamada totalmente qualificada para evitar conflito com ServiceCollectionExtensions
                FSI.BusinessProcessManagement.Infrastructure.DependencyInjection
                    .AddInfrastructure(services, cfg)
            );

            Assert.Equal("Connection string 'BpmMySql' not found.", ex.Message);
        }

        [Fact]
        public void AddInfrastructure_ShouldRegister_AllExpectedServices_WithCorrectLifetimes()
        {
            // Arrange
            var services = new ServiceCollection();
            var cfg = BuildConfigWith("Server=localhost;Database=dummy;User=root;Password=pass;");

            // Act
            FSI.BusinessProcessManagement.Infrastructure.DependencyInjection
                .AddInfrastructure(services, cfg);

            // Assert – DbContext
            Assert.Contains(services, d =>
                d.ServiceType == typeof(DbContextOptions<BpmDbContext>));

            Assert.Contains(services, d =>
                d.ServiceType == typeof(BpmDbContext) &&
                d.Lifetime == ServiceLifetime.Scoped);

            Assert.Contains(services, d =>
                d.ServiceType == typeof(DbContext) &&
                d.ImplementationFactory != null &&
                d.Lifetime == ServiceLifetime.Scoped);

            // Generic Repository
            Assert.Contains(services, d =>
                d.ServiceType.IsGenericType &&
                d.ServiceType.GetGenericTypeDefinition() == typeof(IRepository<>) &&
                d.ImplementationType == typeof(GenericRepository<>) &&
                d.Lifetime == ServiceLifetime.Scoped);

            // Repositories concretos
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

        // -------------------------------
        // Helpers reutilizáveis
        // -------------------------------
        private static void AssertContainsScoped<TService, TImpl>(ServiceCollection services)
        {
            Assert.Contains(services, d =>
                d.ServiceType == typeof(TService) &&
                d.ImplementationType == typeof(TImpl) &&
                d.Lifetime == ServiceLifetime.Scoped);
        }
    }
}
