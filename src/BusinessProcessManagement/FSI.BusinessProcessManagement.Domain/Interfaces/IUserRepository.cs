namespace FSI.BusinessProcessManagement.Domain.Interfaces
{
    public interface IUserRepository : IRepository<Entities.User>
    {
        Task<Entities.User?> GetByUsernameAsync(string username);
        Task<IReadOnlyList<string>> GetRoleNamesAsync(long userId);
    }
}
